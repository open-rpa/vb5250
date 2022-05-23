'
' Copyright 2013-2016 Alec Skelly
'
' This file is part of VB5250, a telnet client implementing IBM's 5250 protocol.
'
' VB5250 is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' VB5250 is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with VB5250. If not, see <http://www.gnu.org/licenses/>.
' 
Public Class frmConnectionManager
    Private Structure ConnectionFolder
        Dim ID As Integer
        Dim Parent As Integer
        Dim Name As String
    End Structure
    Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger
    Private RootNode As ConnectionTreeNode
    Private SelectedNode As ConnectionTreeNode
    Private Const Max_Number_Of_Nodes As Integer = 1000
    Private NodeIDs As List(Of Integer)
    Private SettingsFolder As String = My.Computer.FileSystem.CombinePath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VB5250")
    Private INI As String = My.Computer.FileSystem.CombinePath(SettingsFolder, "VB5250.INI")
    Private LocaleInfo As New IBM5250.Emulator.Localization
    Private FixedFonts As New List(Of String)

    Private Sub frmConnectionManager_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        RemoveINISection(INI, "EMULATOR.*")
        RemoveINISection(INI, "FOLDER.*")
        WriteFolderToINI(RootNode, -1)
    End Sub

    Private Sub frmConnectionManager_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If EmulatorView IsNot Nothing AndAlso EmulatorView.Count > 0 Then
            Dim Result As MsgBoxResult = MsgBox("Do you want to close all connections and exit?", MsgBoxStyle.OkCancel + MsgBoxStyle.Exclamation, "Confirm Exit")
            If Result <> MsgBoxResult.Ok Then e.Cancel = True
        End If
    End Sub

    Private Sub frmConnectionManager_Load(sender As Object, e As EventArgs) Handles Me.Load

        'For Each enc As System.Text.EncodingInfo In System.Text.Encoding.GetEncodings
        '    Debug.Print(enc.Name & vbTab & enc.DisplayName & vbTab & enc.CodePage)
        'Next
        For Each Loc As String In LocaleInfo.Locales.Keys
            Me.ComboBoxHostLocale.Items.Add(Loc)
        Next

        Try
            FixedFonts = ConnectionSettings.EnumFixedPitchFonts
            Me.ComboBoxFontFamilyName.Items.Clear()
            For Each fnt As String In FixedFonts
                Me.ComboBoxFontFamilyName.Items.Add(fnt)
            Next
        Catch ex As Exception
            Logger.Error("Error enumerating fixed pitch fonts", ex)
        End Try

        'initialize the pool of NodeIDs
        NodeIDs = New List(Of Integer)
        For i As Integer = 0 To Max_Number_Of_Nodes - 1
            NodeIDs.Add(i)
        Next

        ConnectionSettingsPanel.Visible = False
        EncryptionMethodComboBox.DataSource = System.Enum.GetValues(GetType(Telnet.De.Mud.Telnet.TelnetWrapper.IBMAuthEncryptionType))

        Try
            If Not My.Computer.FileSystem.DirectoryExists(SettingsFolder) Then My.Computer.FileSystem.CreateDirectory(SettingsFolder)
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
            MsgBox(ex.Message)
        End Try

        ConnectionsTreeView.LabelEdit = True
        ConnectionsTreeView.AllowDrop = True

        Dim Folders As List(Of ConnectionFolder) = ReadFoldersFromINI()
        If Folders.Count < 1 Then
            Dim root As ConnectionFolder
            root.ID = 0
            root.Name = "Connections"
            root.Parent = -1
            Folders.Add(root)
        End If
        CreateConnectionFolder(Folders, 0, Nothing)

        Dim Connections As List(Of ConnectionINIEntry) = ReadConnectionsFromINI()
        For Each con As ConnectionINIEntry In Connections
            Dim NewNode As New ConnectionTreeNode
            Dim ParentNode As ConnectionTreeNode = GetNodeByID(con.Parent, RootNode)
            If ParentNode Is Nothing Then ParentNode = RootNode
            With NewNode
                .NodeID = LeaseNodeID() 'ignore whatever ID it had in the INI file.
                .IsFolder = False
                .Text = con.Name
                .Connection = con.Settings
                .ImageKey = "Connection"
                .SelectedImageKey = "Connection"
            End With
            ParentNode.Nodes.Add(NewNode)
        Next

        RootNode.Expand()
    End Sub

    Private Function GetNodeByID(id As Integer, WithinNode As ConnectionTreeNode) As ConnectionTreeNode
        If WithinNode.NodeID = id Then Return WithinNode
        For Each Node As ConnectionTreeNode In WithinNode.Nodes
            Dim FoundNode As ConnectionTreeNode = GetNodeByID(id, Node)
            If FoundNode IsNot Nothing Then Return FoundNode
        Next
        Return Nothing
    End Function

    Private Sub CreateConnectionFolder(FolderList As List(Of ConnectionFolder), ID As Integer, ParentFolder As ConnectionTreeNode)
        Dim NewNode As ConnectionTreeNode = Nothing
        For Each folder As ConnectionFolder In FolderList
            If folder.ID = ID Then
                NewNode = New ConnectionTreeNode
                NewNode.NodeID = LeaseNodeID(folder.ID)
                NewNode.IsFolder = True
                NewNode.Text = folder.Name
                NewNode.ImageKey = "ClosedFolder"
                NewNode.SelectedImageKey = "ClosedFolder"
                If ParentFolder Is Nothing Then
                    If ID = 0 Then RootNode = NewNode
                    ConnectionsTreeView.Nodes.Add(RootNode)
                Else
                    ParentFolder.Nodes.Add(NewNode)
                End If
                Exit For
            End If
        Next
        If NewNode IsNot Nothing Then
            For Each folder As ConnectionFolder In FolderList
                If folder.Parent <> folder.ID Then 'protect from being added to self
                    If folder.Parent = ID Then
                        CreateConnectionFolder(FolderList, folder.ID, NewNode)
                    End If
                End If
            Next
        End If
    End Sub

    Private Function ReadFoldersFromINI() As List(Of ConnectionFolder)
        Dim Result As New List(Of ConnectionFolder)
        Dim Sections As List(Of String) = ReadIniSections(INI)
        For Each sect As String In Sections
            sect = sect.ToUpper
            If sect.Length > 7 Then
                If sect.Substring(0, 7) = "FOLDER." Then
                    Dim s As String = sect.Substring(7)
                    Dim n As Integer = -1
                    If Integer.TryParse(s, n) Then
                        Dim folder As ConnectionFolder
                        With folder
                            .ID = n
                            .Name = .ID
                            .Parent = -1
                            s = ReadIni(INI, sect, "Name")
                            If s IsNot Nothing Then .Name = s.Trim
                            s = ReadIni(INI, sect, "Parent")
                            If s IsNot Nothing Then
                                If Integer.TryParse(s, n) Then
                                    .Parent = n
                                End If
                            End If
                        End With
                        Result.Add(folder)
                    End If
                End If
            End If
        Next
        Return Result
    End Function

    Private Function ReadConnectionsFromINI() As List(Of ConnectionINIEntry)
        Dim Result As New List(Of ConnectionINIEntry)
        Dim Sections As List(Of String) = ReadIniSections(INI)
        For Each sect As String In Sections
            sect = sect.ToUpper
            If sect.Length > 9 Then
                If sect.Substring(0, 9) = "EMULATOR." Then
                    Dim s As String = sect.Substring(9)
                    Dim n As Integer = -1
                    If Integer.TryParse(s, n) Then
                        Dim con As New ConnectionINIEntry
                        With con
                            .ID = n
                            .Name = .ID
                            .Parent = -1
                            s = ReadIni(INI, sect, "Name")
                            If s IsNot Nothing Then .Name = s.Trim

                            s = ReadIni(INI, sect, "Parent")
                            If s IsNot Nothing Then
                                If Integer.TryParse(s, n) Then
                                    .Parent = n
                                End If
                            End If

                            s = ReadIni(INI, sect, "HostAddress")
                            'don't try to validate the address/hostname here.  The socket will do that later.
                            If s IsNot Nothing Then .Settings.HostAddress = s

                            s = ReadIni(INI, sect, "HostPort")
                            If s IsNot Nothing Then
                                If Integer.TryParse(s, n) Then
                                    .Settings.HostPort = n
                                End If
                            End If

                            s = ReadIni(INI, sect, "UseSSL")
                            .Settings.UseSSL = ToBool(s)

                            s = ReadIni(INI, sect, "StationName")
                            If s IsNot Nothing Then .Settings.StationName = s

                            s = ReadIni(INI, sect, "UserName")
                            If s IsNot Nothing Then .Settings.UserName = s

                            s = ReadIni(INI, sect, "Password")
                            If s IsNot Nothing Then
                                Try
                                    Dim crypt As New SimpleCrypto
                                    Dim dp As String = crypt.Decrypt(s)
                                    .Settings.Password = dp
                                Catch ex As SimpleCryptoExceptions.SuppliedStringNotEncryptedException
                                    .Settings.Password = s
                                Catch ex As Exception
                                    Debug.Print("Error decrypting password: " & ex.Message)
                                End Try
                            End If

                            s = ReadIni(INI, sect, "BypassLogin")
                            If s IsNot Nothing Then
                                'Dim b As Boolean = False
                                'Boolean.TryParse(s, b)
                                '.Settings.BypassLogin = b
                                .Settings.BypassLogin = ToBool(s)
                            End If

                            's = ReadIni(INI, sect, "GetServerInfo")
                            'If s IsNot Nothing Then
                            '    .Settings.GetServerInfo = ToBool(s)
                            'End If
                            's = ReadIni(INI, sect, "GetUserInfo")
                            'If s IsNot Nothing Then
                            '    .Settings.GetUserInfo = ToBool(s)
                            'End If
                            s = ReadIni(INI, sect, "PreAuthenticate")
                            If s IsNot Nothing Then
                                .Settings.PreAuthenticate = ToBool(s)
                            End If

                            s = ReadIni(INI, sect, "TimeoutMS")
                            Dim t As Integer = 0
                            Integer.TryParse(s, t)
                            If t >= 1000 Then .Settings.TimeoutMS = t

                            s = ReadIni(INI, sect, "AuthEncryptionMethod")
                            If s IsNot Nothing Then
                                Dim c As Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType = Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.None
                                [Enum].TryParse(Of Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType)(s, c)
                                .Settings.AuthEncryptionMethod = c
                            End If

                            .Settings.LocaleName = "Default"
                            s = ReadIni(INI, sect, "Locale")
                            If s IsNot Nothing Then
                                If LocaleInfo.Locales.ContainsKey(s) Then
                                    .Settings.LocaleName = s
                                End If
                            End If

                            .Settings.FontFamily = New FontFamily(Drawing.Text.GenericFontFamilies.Monospace)
                            s = ReadIni(INI, sect, "Font")
                            If s IsNot Nothing Then
                                If FixedFonts.Contains(s) Then
                                    .Settings.FontFamily = New FontFamily(s)
                                End If
                            End If

                            Dim Colors As New List(Of String)
                            For Each kvp As KeyValuePair(Of String, KnownColor) In .Settings.ColorMap
                                Colors.Add(kvp.Key)
                            Next
                            For Each c As String In Colors
                                s = ReadIni(INI, sect, c.ToString)
                                If s IsNot Nothing Then
                                    Try
                                        .Settings.ColorMap(c) = [Enum].Parse(GetType(KnownColor), s)
                                    Catch ex As Exception
                                        Debug.Print("Unable to interpret '" & s & "' as a known color")
                                    End Try
                                End If
                                Debug.Print("Connection " & .ID.ToString & " " & c.ToString & " --> " & .Settings.ColorMap(c).ToString)
                            Next

                            s = ReadIni(INI, sect, "AlternateCaretEnabled")
                            If s IsNot Nothing Then
                                .Settings.AlternateCaretEnabled = ToBool(s)
                            End If

                            s = ReadIni(INI, sect, "SavePassword")
                            If s IsNot Nothing Then
                                .Settings.SavePassword = ToBool(s)
                            End If

                        End With
                        Result.Add(con)
                    End If
                End If
            End If
        Next
        Return Result
    End Function

    Private Sub WriteFolderToINI(Node As ConnectionTreeNode, ParentNodeID As Integer)
        If Node.IsFolder Then
            Dim Section As String = "Folder." & Node.NodeID.ToString
            WriteIni(INI, Section, "Name", Node.Text)
            WriteIni(INI, Section, "Parent", ParentNodeID.ToString)
            For Each child As TreeNode In Node.Nodes
                WriteFolderToINI(child, Node.NodeID)
            Next
        Else
            Dim Section As String = "Emulator." & Node.NodeID.ToString
            WriteIni(INI, Section, "Name", Node.Text)
            WriteIni(INI, Section, "Parent", ParentNodeID.ToString)
            WriteIni(INI, Section, "HostAddress", Node.Connection.HostAddress)
            WriteIni(INI, Section, "HostPort", Node.Connection.HostPort.ToString)
            WriteIni(INI, Section, "TimeoutMS", Node.Connection.TimeoutMS.ToString)
            WriteIni(INI, Section, "UseSSL", Node.Connection.UseSSL.ToString)
            WriteIni(INI, Section, "StationName", Node.Connection.StationName)
            WriteIni(INI, Section, "UserName", Node.Connection.UserName)
            If Node.Connection.SavePassword Then
                Try
                    Dim crypt As New SimpleCrypto
                    WriteIni(INI, Section, "Password", crypt.Encrypt(Node.Connection.Password))
                Catch ex As Exception
                    Debug.Print("Error encrypting password: " & ex.Message)
                End Try
            Else
                WriteIni(INI, Section, "Password", "")
            End If
            WriteIni(INI, Section, "BypassLogin", Node.Connection.BypassLogin.ToString)
            'WriteIni(INI, Section, "GetServerInfo", Node.Connection.GetServerInfo.ToString)
            'WriteIni(INI, Section, "GetUserInfo", Node.Connection.GetUserInfo.ToString)
            WriteIni(INI, Section, "PreAuthenticate", Node.Connection.PreAuthenticate.ToString)

            WriteIni(INI, Section, "AuthEncryptionMethod", Node.Connection.AuthEncryptionMethod.ToString)
            WriteIni(INI, Section, "WideScreen", Node.Connection.Allow_ScreenSize_27x132.ToString)
            WriteIni(INI, Section, "Locale", Node.Connection.LocaleName)
            WriteIni(INI, Section, "Font", Node.Connection.FontFamily.Name)
            WriteIni(INI, Section, "AlternateCaretEnabled", Node.Connection.AlternateCaretEnabled.ToString)
            If Node.Connection.ColorMap IsNot Nothing Then
                For Each kvp As KeyValuePair(Of String, KnownColor) In Node.Connection.ColorMap
                    WriteIni(INI, Section, kvp.Key.ToString, kvp.Value.ToString)
                Next
            End If
        End If
    End Sub

    Private Sub OpenEmulator(ByVal WindowIndex As Short, Settings As ConnectionSettings)
        Logger.Trace("")
        If EmulatorView Is Nothing Then EmulatorView = New Dictionary(Of Integer, StructEmulatorView)
        If EmulatorView.ContainsKey(WindowIndex) Then
            If EmulatorView(WindowIndex).Form IsNot Nothing Then
                EmulatorView(WindowIndex).Form.BringToFront()
            Else
                Logger.Warn("Open window's form is Nothing!  Deleting.")
                EmulatorView.Remove(WindowIndex)
            End If
        Else
            If (Settings.BypassLogin Or Settings.PreAuthenticate) Then
                If Settings.AuthEncryptionMethod <> Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.Kerberos Then
                    'If String.IsNullOrEmpty(Settings.UserName) Then
                    '    MsgBox("Please enter a username before connecting.", MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly)
                    '    Exit Sub
                    'End If
                    'If String.IsNullOrEmpty(Settings.Password) Then
                    '    MsgBox("Please enter a password before connecting.", MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly)
                    '    Exit Sub
                    'End If
                    If String.IsNullOrWhiteSpace(Settings.UserName) Or String.IsNullOrWhiteSpace(Settings.Password) Then
                        Dim f As New frmLogon
                        f.StartPosition = FormStartPosition.CenterParent
                        f.UserNameTextBox.Text = Settings.UserName
                        If f.ShowDialog = DialogResult.OK Then
                            Settings.UserName = f.UserNameTextBox.Text
                            Settings.Password = f.PasswordTextBox.Text
                        Else
                            Exit Sub
                        End If
                    End If
                End If
                If (Settings.AuthEncryptionMethod = Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.DES) Then
                    If Not String.IsNullOrWhiteSpace(Settings.Password) Then
                        If IsNumeric(Settings.Password(0)) Then
                            MsgBox("The IBM host will not accept a DES encrypted password beginning with a number.  Expect an error message.", MsgBoxStyle.Exclamation, "IBM Limitation")
                        End If
                    End If
                End If
            End If

            'PreAuthenticate if we're not using Kerberos
            With Settings
                If .AuthEncryptionMethod <> Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.Kerberos Then
                    If .PreAuthenticate Then
                        Dim OriginalCursor As Cursor = Me.Cursor
                        Try
                            Me.Cursor = Cursors.WaitCursor
                            Dim HostPasswordLevel As Integer = 0
                            If Not PreAuthenticate(.HostAddress, .UseSSL, Settings.UserName, HostPasswordLevel, Settings.Password, Settings.TimeoutMS) Then Throw New Exception("PreAuthentication failed")
                            If HostPasswordLevel > 1 Then
                                .AuthEncryptionMethod = Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.SHA1
                            Else
                                .AuthEncryptionMethod = Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.DES
                            End If
                        Catch ex As Exception
                            Logger.Debug("Error during preauthentication: " & ex.Message)
                            If Not .SavePassword Then .Password = Nothing 'Avoid using a bad password multiple times.
                            Exit Sub
                        Finally
                            Me.Cursor = OriginalCursor
                        End Try
                    End If
                End If
            End With
            '

            Dim ev As New StructEmulatorView
            ev.Settings = Settings
            ev.Form = New frmEmulatorView(ev.Settings.Allow_ScreenSize_27x132)
            ev.Form.Tag = WindowIndex
            EmulatorView.Add(WindowIndex, ev)
            Try
                EmulatorView(WindowIndex).Form.Text = "VB5250: " & Settings.HostAddress & ", " & Settings.StationName
                EmulatorView(WindowIndex).Form.Show()
                AddHandler EmulatorView(WindowIndex).Form.FontFamilyChanged, AddressOf Me.FontFamilyUpdated
                AddHandler EmulatorView(WindowIndex).Form.FormClosed, AddressOf Me.EmulatorFormClosed
            Catch ex As Exception
                Logger.Error(ex.Message, ex)
                EmulatorView(WindowIndex).Form.Close()
                EmulatorView.Remove(WindowIndex)
                Throw
            End Try
        End If
    End Sub

    Private Sub EmulatorFormClosed(sender As Object, e As FormClosedEventArgs)
        Try
            Dim em As frmEmulatorView = DirectCast(sender, frmEmulatorView)
            RemoveHandler em.FontFamilyChanged, AddressOf Me.FontFamilyUpdated
            RemoveHandler em.FormClosed, AddressOf Me.EmulatorFormClosed
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub

    Private Sub NewFolderToolStripButton_Click(sender As Object, e As EventArgs) Handles NewFolderToolStripButton.Click
        CreateNode(True)
    End Sub

    Private Function LeaseNodeID(id As Integer) As Integer
        If NodeIDs.Remove(id) Then
            Return id
        Else
            Throw New Exception("Requested NodeID was not available")
        End If
    End Function
    Private Function LeaseNodeID() As Integer
        Dim id As Integer = NodeIDs.Min
        NodeIDs.Remove(id)
        Return id
    End Function

    Private Sub ReturnNodeID(id As Integer)
        If Not NodeIDs.Contains(id) Then NodeIDs.Add(id)
    End Sub

    Private Sub ConnectionsTreeView_AfterCollapse(sender As Object, e As TreeViewEventArgs) Handles ConnectionsTreeView.AfterCollapse
        e.Node.ImageKey = "ClosedFolder"
        e.Node.SelectedImageKey = e.Node.ImageKey
    End Sub

    Private Sub ConnectionsTreeView_AfterExpand(sender As Object, e As TreeViewEventArgs) Handles ConnectionsTreeView.AfterExpand
        e.Node.ImageKey = "OpenFolder"
        e.Node.SelectedImageKey = e.Node.ImageKey
    End Sub

    Private Sub DeleteNodeToolStripButton_Click(sender As Object, e As EventArgs) Handles DeleteNodeToolStripButton.Click
        If ConnectionsTreeView.SelectedNode IsNot Nothing Then
            If ConnectionsTreeView.SelectedNode IsNot RootNode Then
                RemoveNode(ConnectionsTreeView.SelectedNode)
            End If
        End If
    End Sub

    Private Sub RemoveNode(Node As ConnectionTreeNode)
        Dim children As New List(Of ConnectionTreeNode)
        For Each child As ConnectionTreeNode In Node.Nodes
            children.Add(child)
        Next
        For Each child As ConnectionTreeNode In children
            RemoveNode(child)
        Next
        children = Nothing
        ReturnNodeID(Node.NodeID)
        Node.Remove()
    End Sub

    Private Sub ConnectionsTreeView_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles ConnectionsTreeView.AfterSelect
        SelectedNode = DirectCast(ConnectionsTreeView.SelectedNode, ConnectionTreeNode)
        DeleteNodeToolStripButton.Enabled = Not (SelectedNode Is RootNode)
        ConnectionSettingsPanel.Visible = Not SelectedNode.IsFolder
        If Not SelectedNode.IsFolder Then
            With SelectedNode.Connection
                HostNameTextBox.Text = .HostAddress
                PortTextBox.Text = .HostPort
                SSLCheckBox.Checked = .UseSSL
                StationNameTextBox.Text = .StationName
                BypassLoginCheckBox.Checked = .BypassLogin
                UserNameTextBox.Text = .UserName
                PasswordTextBox.Text = .Password
                'PasswordTextBox.Enabled = .SavePassword
                If .Allow_ScreenSize_27x132 Then ScreenSizeComboBox.SelectedIndex = 1 Else ScreenSizeComboBox.SelectedIndex = 0
                'GetServerInfoCheckBox.Checked = .GetServerInfo
                PreAuthenticateCheckBox.Checked = .PreAuthenticate '.GetUserInfo
                EncryptionMethodComboBox.Text = .AuthEncryptionMethod.ToString
                'EncryptionMethodComboBox.Enabled = Not SelectedNode.Connection.UseLIPIServers
                'BypassLoginGroupBox.Enabled = BypassLoginCheckBox.Checked
                Try
                    ComboBoxHostLocale.SelectedItem = .LocaleName
                Catch ex As Exception
                    Logger.Error(ex.Message, ex)
                    MsgBox(ex.Message)
                End Try
                Try
                    ComboBoxFontFamilyName.SelectedItem = .FontFamily.Name
                Catch ex As Exception
                    Logger.Error(ex.Message, ex)
                    MsgBox(ex.Message)
                End Try
            End With
        End If
    End Sub

    Private Sub NewConnectionToolStripButton_Click(sender As Object, e As EventArgs) Handles NewConnectionToolStripButton.Click
        CreateNode(False)
    End Sub

    Private Sub CreateNode(IsFolder As Boolean)
        Dim SelectedFolder As ConnectionTreeNode = Nothing
        Dim NewNode As New ConnectionTreeNode
        If Me.ConnectionsTreeView.SelectedNode IsNot Nothing Then
            If ConnectionsTreeView.SelectedNode.GetType Is GetType(ConnectionTreeNode) Then
                SelectedFolder = DirectCast(ConnectionsTreeView.SelectedNode, ConnectionTreeNode)
                If Not SelectedFolder.IsFolder Then SelectedFolder = Nothing
            End If
        End If
        If SelectedFolder Is Nothing Then SelectedFolder = RootNode

        NewNode.NodeID = LeaseNodeID()
        NewNode.IsFolder = IsFolder

        If IsFolder Then
            NewNode.Text = "New Folder"
            NewNode.ImageKey = "ClosedFolder"
            NewNode.SelectedImageKey = "ClosedFolder"
        Else
            NewNode.Text = "New Connection"
            NewNode.ImageKey = "Connection"
            NewNode.SelectedImageKey = "Connection"
        End If

        SelectedFolder.Nodes.Add(NewNode)

        NewNode.EnsureVisible()
        ConnectionsTreeView.SelectedNode = NewNode
        NewNode.BeginEdit()
    End Sub

    Private Sub BypassLoginCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles BypassLoginCheckBox.CheckedChanged
        'BypassLoginGroupBox.Enabled = BypassLoginCheckBox.Checked
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            SelectedNode.Connection.BypassLogin = BypassLoginCheckBox.Checked
        End If
    End Sub

    Private Sub ColorMapButton_Click(sender As Object, e As EventArgs) Handles ColorMapButton.Click
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            Dim f As New frmColorPrefs(SelectedNode.NodeID, SelectedNode.Connection.ColorMap, "Color Preferences (" & SelectedNode.Connection.HostAddress & ", " & SelectedNode.Connection.StationName & ")")
            Dim result As DialogResult = f.ShowDialog
        End If
    End Sub

    Private Sub ConnectButton_Click(sender As Object, e As EventArgs) Handles ConnectButton.Click
        Try
            If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
                OpenEmulator(SelectedNode.NodeID, SelectedNode.Connection)
            End If
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub HostNameTextBox_TextChanged(sender As Object, e As EventArgs) Handles HostNameTextBox.TextChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            SelectedNode.Connection.HostAddress = HostNameTextBox.Text
        End If
    End Sub

    Private Sub PortTextBox_TextChanged(sender As Object, e As EventArgs) Handles PortTextBox.TextChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            Dim n As Integer = -1
            If Integer.TryParse(PortTextBox.Text, n) Then
                SelectedNode.Connection.HostPort = n
            End If
        End If
    End Sub

    Private Sub StationNameTextBox_TextChanged(sender As Object, e As EventArgs) Handles StationNameTextBox.TextChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            SelectedNode.Connection.StationName = StationNameTextBox.Text
        End If
    End Sub

    Private Sub UserNameTextBox_TextChanged(sender As Object, e As EventArgs) Handles UserNameTextBox.TextChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            SelectedNode.Connection.UserName = UserNameTextBox.Text
            PreAuthenticateCheckBox.Enabled = CanAuthenticate(SelectedNode.Connection)
            BypassLoginCheckBox.Enabled = CanAuthenticate(SelectedNode.Connection)
        End If
    End Sub

    Private Sub PasswordTextBox_TextChanged(sender As Object, e As EventArgs) Handles PasswordTextBox.TextChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            SelectedNode.Connection.Password = PasswordTextBox.Text 'XXX
            PreAuthenticateCheckBox.Enabled = CanAuthenticate(SelectedNode.Connection)
            BypassLoginCheckBox.Enabled = CanAuthenticate(SelectedNode.Connection)
        End If
    End Sub

    Private Sub EncryptionMethodComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles EncryptionMethodComboBox.SelectedIndexChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            Dim enc As Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType = Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.None
            If [Enum].TryParse(Of Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType)(EncryptionMethodComboBox.Text, enc) Then
                SelectedNode.Connection.AuthEncryptionMethod = enc
            End If
            'Me.UserNameTextBox.Enabled = Not (enc = Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.Kerberos)
            'Me.PasswordTextBox.Enabled = Me.UserNameTextBox.Enabled
            'PreAuthenticateCheckBox.Enabled = CanAuthenticate(SelectedNode.Connection)
            'BypassLoginCheckBox.Enabled = CanAuthenticate(SelectedNode.Connection)
            UpdateConnectionControlState()
        End If
    End Sub

    Private Sub ScreenSizeComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ScreenSizeComboBox.SelectedIndexChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            SelectedNode.Connection.Allow_ScreenSize_27x132 = (ScreenSizeComboBox.Text.Trim = "27x132")
        End If
    End Sub

    'Friend Function GetConnectionSettings(ByVal NodeID As Integer) As ConnectionSettings
    '    Dim node As ConnectionTreeNode = GetNodeByID(NodeID, RootNode)
    '    If node IsNot Nothing AndAlso (Not node.IsFolder) Then
    '        Return node.Connection
    '    Else
    '        Return Nothing
    '    End If
    'End Function

    Private Sub ConnectionsTreeView_DragDrop(sender As Object, e As DragEventArgs) Handles ConnectionsTreeView.DragDrop
        Dim loc As Point = (CType(sender, TreeView)).PointToClient(New Point(e.X, e.Y))
        Dim node As ConnectionTreeNode = e.Data.GetData(GetType(ConnectionTreeNode))
        Dim destNode As ConnectionTreeNode = (CType(sender, TreeView)).GetNodeAt(loc)
        If (destNode IsNot Nothing) AndAlso (destNode.IsFolder) AndAlso (destNode IsNot node) Then
            If node.Parent Is Nothing Then
                node.TreeView.Nodes.Remove(node)
            Else
                node.Parent.Nodes.Remove(node)
            End If
            destNode.Nodes.Add(node)
        End If
    End Sub

    Private Sub ConnectionsTreeView_DragEnter(sender As Object, e As DragEventArgs) Handles ConnectionsTreeView.DragEnter
        e.Effect = DragDropEffects.Move
    End Sub

    Private Sub ConnectionsTreeView_DragOver(sender As Object, e As DragEventArgs) Handles ConnectionsTreeView.DragOver
        Dim loc As Point = (CType(sender, TreeView)).PointToClient(New Point(e.X, e.Y))
        Dim node As ConnectionTreeNode = e.Data.GetData(GetType(ConnectionTreeNode))
        Dim destNode As ConnectionTreeNode = (CType(sender, TreeView)).GetNodeAt(loc)
        If (destNode IsNot Nothing) AndAlso (destNode IsNot node) AndAlso (destNode.IsFolder) Then
            e.Effect = DragDropEffects.Move
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub ConnectionsTreeView_ItemDrag(sender As Object, e As ItemDragEventArgs) Handles ConnectionsTreeView.ItemDrag
        sender.DoDragDrop(e.Item, DragDropEffects.Move)
    End Sub

    Private Sub ConnectionsTreeView_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles ConnectionsTreeView.NodeMouseDoubleClick
        Dim node As ConnectionTreeNode = DirectCast(e.Node, ConnectionTreeNode)
        If Not node.IsFolder Then
            Try
                OpenEmulator(node.NodeID, node.Connection)
            Catch ex As Exception
                Logger.Error(ex.Message, ex)
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Private Sub FontFamilyUpdated(NodeKey As Integer)
        Try
            Dim node As ConnectionTreeNode = DirectCast(GetNodeByID(NodeKey, RootNode), ConnectionTreeNode)
            If Me.SelectedNode Is node Then Me.ComboBoxFontFamilyName.SelectedItem = node.Connection.FontFamily.Name
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        Dim f As New Form
        f.Height = 160
        f.Width = 700
        f.Text = "About " & Application.ProductName
        f.Icon = Me.Icon

        Dim lblProductString As New Label
        With lblProductString
            Dim ver As FileVersionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location)
            .Text = ver.ProductName & " v" & ver.ProductVersion & " " & ver.LegalCopyright
            .Location = New Point(0, 10)
            .Left = 0
            .Width = f.Width
            .Anchor = AnchorStyles.Left + AnchorStyles.Right + AnchorStyles.Top
            .TextAlign = ContentAlignment.MiddleCenter
        End With
        f.Controls.Add(lblProductString)

        Dim lblTelnetString As New Label
        With lblTelnetString
            Dim ver As FileVersionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetAssembly(GetType(Telnet.De.Mud.Telnet.TelnetWrapper)).Location)
            .Text = ver.ProductName & " v" & ver.ProductVersion & " " & ver.LegalCopyright
            .Top = lblProductString.Bottom + 2
            .Left = 0
            .Width = f.Width
            .Anchor = AnchorStyles.Left + AnchorStyles.Right + AnchorStyles.Top
            .TextAlign = ContentAlignment.MiddleCenter
        End With
        f.Controls.Add(lblTelnetString)

        Dim lbl5250String As New Label
        With lbl5250String
            Dim ver As FileVersionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetAssembly(GetType(IBM5250.Emulator)).Location)
            .Text = ver.ProductName & " v" & ver.ProductVersion & " " & ver.LegalCopyright
            .Top = lblTelnetString.Bottom + 2
            .Left = 0
            .Width = f.Width
            .Anchor = AnchorStyles.Left + AnchorStyles.Right + AnchorStyles.Top
            .TextAlign = ContentAlignment.MiddleCenter
        End With
        f.Controls.Add(lbl5250String)

        Dim lbliClientString As New Label
        With lbliClientString
            Dim ver As FileVersionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetAssembly(GetType(IBMiClient.Client)).Location)
            .Text = ver.ProductName & " v" & ver.ProductVersion & " " & ver.LegalCopyright
            .Top = lbl5250String.Bottom + 2
            .Left = 0
            .Width = f.Width
            .Anchor = AnchorStyles.Left + AnchorStyles.Right + AnchorStyles.Top
            .TextAlign = ContentAlignment.MiddleCenter
        End With
        f.Controls.Add(lbliClientString)

        Dim lblNSspiString As New Label
        With lblNSspiString
            Dim ver As FileVersionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetAssembly(GetType(NSspi.Credentials.ClientCredential)).Location)
            .Text = ver.ProductName & " v" & ver.ProductVersion & " " & ver.LegalCopyright
            .Top = lbl5250String.Bottom + 2
            .Left = 0
            .Width = f.Width
            .Anchor = AnchorStyles.Left + AnchorStyles.Right + AnchorStyles.Top
            .TextAlign = ContentAlignment.MiddleCenter
        End With
        f.Controls.Add(lblNSspiString)

        f.StartPosition = FormStartPosition.CenterParent
        f.ShowDialog()
    End Sub

    Private Sub SSLCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles SSLCheckBox.CheckedChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            SelectedNode.Connection.UseSSL = SSLCheckBox.Checked
        End If

        If SSLCheckBox.Checked Then
            If String.IsNullOrWhiteSpace(PortTextBox.Text) OrElse PortTextBox.Text = "23" Then PortTextBox.Text = "992"
        Else
            If String.IsNullOrWhiteSpace(PortTextBox.Text) OrElse PortTextBox.Text = "992" Then PortTextBox.Text = "23"
        End If
    End Sub

    Private Sub ComboBoxHostLocale_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxHostLocale.SelectedIndexChanged
        If ComboBoxHostLocale.SelectedItem IsNot Nothing Then
            Dim Locale As IBM5250.Emulator.Localization.Locale = LocaleInfo.Locales(ComboBoxHostLocale.SelectedItem)
            Me.TextBoxCHARSET.Text = Locale.CHARSET
            Me.TextBoxCODEPAGE.Text = Locale.CODEPAGE
            Me.TextBoxKBDTYPE.Text = Locale.KBDTYPE
            If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
                SelectedNode.Connection.LocaleName = ComboBoxHostLocale.SelectedItem
            End If
        End If
    End Sub

    Private Sub ComboBoxFontFamilyName_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxFontFamilyName.SelectedIndexChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            SelectedNode.Connection.FontFamily = New FontFamily(ComboBoxFontFamilyName.Text)
        End If
    End Sub

    Private Sub GetServerInfoCheckBox_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles GetServerInfoCheckBox.CheckedChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            'SelectedNode.Connection.GetServerInfo = GetServerInfoCheckBox.Checked
            EncryptionMethodComboBox.Enabled = Not SelectedNode.Connection.PreAuthenticate 'SelectedNode.Connection.GetServerInfo
            UpdateConnectionControlState()
        End If
    End Sub

    Private Sub PreAuthenticateCheckBox_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles PreAuthenticateCheckBox.CheckedChanged
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            SelectedNode.Connection.PreAuthenticate = PreAuthenticateCheckBox.Checked
            EncryptionMethodComboBox.Enabled = Not SelectedNode.Connection.PreAuthenticate
        End If
    End Sub

    Private Function CanAuthenticate(Connection As ConnectionSettings) As Boolean
        If Connection.AuthEncryptionMethod = Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.Kerberos Then Return True

        'Always return True.  We may not be storing passwords but still want to be able to configure Pre-Authenticate and Bypass Logon.
        'Return Not (String.IsNullOrWhiteSpace(SelectedNode.Connection.UserName) Or String.IsNullOrWhiteSpace(SelectedNode.Connection.Password))
        Return True
        '
    End Function

    Private Sub UpdateConnectionControlState()
        If SelectedNode IsNot Nothing AndAlso (Not SelectedNode.IsFolder) Then
            'EncryptionMethodComboBox.Enabled = Not GetServerInfoCheckBox.Checked
            EncryptionMethodComboBox.Enabled = Not PreAuthenticateCheckBox.Checked

            UserNameTextBox.Enabled = Not (EncryptionMethodComboBox.SelectedValue = Telnet.De.Mud.Telnet.TelnetWrapper.IBMAuthEncryptionType.Kerberos) Or PreAuthenticateCheckBox.Checked 'GetServerInfoCheckBox.Checked

            'Dim enc As Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType = Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.None
            'If [Enum].TryParse(Of Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType)(EncryptionMethodComboBox.Text, enc) Then
            '    SelectedNode.Connection.AuthEncryptionMethod = enc
            'End If
            'Me.UserNameTextBox.Enabled = Not (enc = Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.Kerberos)


            PasswordTextBox.Enabled = UserNameTextBox.Enabled
            PreAuthenticateCheckBox.Enabled = CanAuthenticate(SelectedNode.Connection)
            BypassLoginCheckBox.Enabled = PreAuthenticateCheckBox.Enabled
        End If
    End Sub
End Class