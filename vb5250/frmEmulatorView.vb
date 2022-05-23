'
' Copyright 2013 Alec Skelly
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
Imports System.Xml

Public Class frmEmulatorView
    Dim WithEvents Telnet As Telnet.De.Mud.Telnet.TelnetWrapper
    Friend WithEvents Emulator As IBM5250.Emulator
    Private SuppressCursorPositionChanges As Boolean = False
    Private SuppressFieldModifiedFlags As Boolean = False
    Private SuppressScreenBufferUpdates As Boolean = False
    Private SuppressInsertKeyEvents As Boolean = False
    Private InputReady As Boolean = False
    Private PriorFieldIndex As Integer = -1

    Private InputFields(255) As EmulatorTextBox

    Private ErrorLines As New System.Collections.Generic.Stack(Of String)

    'these are used by child popups also
    Public Structure Screen_Metrics
        Dim ScaleFactor As Single
        Dim Font_Regular As Drawing.Font
        Dim Font_Bold As Drawing.Font
        Dim Font_Underscore As Drawing.Font
        Dim Font_Character_Width As Single
        Dim ControlHeight As Integer
        Dim EmulatorSize As Size
    End Structure
    Public ScreenMetrics As Screen_Metrics

    Private Structure MouseDragCoordinates
        Dim Origin As Point
        Dim Destination As Point
        Dim Rectangle As Rectangle
        Dim xIncrement As Integer
        Dim yIncrement As Integer
    End Structure
    Private MouseDragCoords As MouseDragCoordinates
    Private MouseDragging As Boolean = False

    Public Enum PrintOrientation As Integer
        Auto = 0
        Portrait = 1
        Landscape = 2
    End Enum

    Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger
    Public Event FontFamilyChanged(EmulatorView_Key As Integer)

    Public Sub New(Allow_ScreenSize_27x132 As Boolean)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Logger.Trace("")

        For i As Integer = 0 To InputFields.Length - 1
            InputFields(i) = New EmulatorTextBox
        Next

        'IBM-5555-C01   24 x 80 Double-Byte Character Set color display
        'IBM-5555-B01   24 x 80 Double-Byte Character Set (DBCS)
        'IBM-3477-FC    27 x 132 color display
        'IBM-3477-FG    27 x 132 monochrome display
        'IBM-3180-2     27 x 132 monochrome display
        'IBM-3179-2     24 x 80 color display
        'IBM-3196-A1    24 x 80 monochrome display
        'IBM-5292-2     24 x 80 color display
        'IBM-5291-1     24 x 80 monochrome display
        'IBM-5251-11    24 x 80 monochrome display

        Telnet = New Telnet.De.Mud.Telnet.TelnetWrapper
        Emulator = New IBM5250.Emulator(Allow_ScreenSize_27x132)

        If Allow_ScreenSize_27x132 Then
            Telnet.Terminal_Type = "IBM-3477-FC"
            Emulator.TerminalType = "3477"
            Emulator.TerminalModel = "FC"
        Else
            Telnet.Terminal_Type = "IBM-3179-2"
            Emulator.TerminalType = "3179"
            Emulator.TerminalModel = "2"
        End If

    End Sub

    Protected Overrides Sub Finalize()
        Logger.Trace("WindowIndex=" & Me.Tag)

        MyBase.Finalize()
    End Sub

    Private Sub frmEmulatorView_Activated(sender As Object, e As System.EventArgs) Handles Me.Activated
        For Each kvp As KeyValuePair(Of String, IBM5250.Emulator.EmulatorScreen.EmulatorPopup) In Emulator.Screen.PopUps
            Dim frm As Form = GetOpenPopupForm(kvp.Key)
            If frm IsNot Nothing Then frm.Activate()
        Next
    End Sub

    Private Sub frmEmulatorView_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        Emulator.Screen.Clear()

        'RemoveHandler Emulator.CursorPositionSet, AddressOf Me._OnCursorPositionSet


        RemoveHandler Telnet.DataAvailable, AddressOf Me.TelnetDataAvailable
        RemoveHandler Telnet.Disconnected, AddressOf Me.Disco
        'Telnet.Disconnect()
        Telnet.Dispose() 'this will handle disconnection

        If Me.Tag IsNot Nothing Then
            EmulatorView.Remove(CInt(Me.Tag))
        End If

        Me.Text = ""
    End Sub

    Private Sub frmEmulatorView_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Logger.Trace("WindowIndex=" & Me.Tag)
        If Me.Tag IsNot Nothing Then

            For Each Orientation As String In System.Enum.GetNames(GetType(PrintOrientation))
                Me.ToolStripComboBoxPrintOrientation.Items.Add(Orientation)
            Next
            Me.ToolStripComboBoxPrintOrientation.Text = PrintOrientation.Auto.ToString

            With Me.PanelGreenScreen
                .Top = Me.ToolStripMain.Bottom + 1
                '.Left = .Parent.ClientRectangle.Left
                .Anchor = AnchorStyles.Top + AnchorStyles.Bottom + AnchorStyles.Left + AnchorStyles.Right
            End With
            'frmConnectionManager.Cursor = Cursors.WaitCursor

            AddHandler Me.PanelGreenScreen.Paint, AddressOf Me.PanelGreenScreen_Paint

            Me.KeyPreview = True
            AddHandler Me.PreviewKeyDown, AddressOf Control_PreviewKeyDown
            AddHandler Me.KeyDown, AddressOf frmEmulatorView_KeyDown
            AddHandler Me.KeyUp, AddressOf frmEmulatorView_KeyUp
            AddHandler Me.KeyPress, AddressOf frmEmulatorView_KeyPress

            Me.ToolStripStatusLabelStatus.Text = ""
            Me.StatusStripStatus.ImageList = Me.ImageListConnectionStatus
            Me.SetConnectionStatus(False, False, Nothing)

            Try
                If EmulatorView(CInt(Me.Tag)).Settings.LocaleName IsNot Nothing Then Emulator.Locale = Emulator.LocaleInfo.Locales(EmulatorView(CInt(Me.Tag)).Settings.LocaleName)
            Catch ex As Exception
                Logger.Error("Error setting Locale: " & ex.Message, ex)
            End Try

            Telnet.UseSSL = EmulatorView(CInt(Me.Tag)).Settings.UseSSL


            'If EmulatorView(CInt(Me.Tag)).Settings.AuthEncryptionMethod


            Telnet.IBM_AuthEncryptionMethod = EmulatorView(CInt(Me.Tag)).Settings.AuthEncryptionMethod

            Telnet.IBM_BypassLogonScreen = EmulatorView(CInt(Me.Tag)).Settings.BypassLogin
            If Telnet.IBM_BypassLogonScreen Then
                If Telnet.IBM_AuthEncryptionMethod = Global.Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.Kerberos Then
                    'IBMTICKET user variable will be set prior to each connection rather than here
                    '   since kerberos replay protection prevents re-using a ticket.
                Else
                    Telnet.IBM_Password = EmulatorView(CInt(Me.Tag)).Settings.Password
                    Telnet.VARs.Add("USER", EmulatorView(CInt(Me.Tag)).Settings.UserName)
                End If
            End If

            UseAlternateCaretToolStripMenuItem.Checked = EmulatorView(CInt(Me.Tag)).Settings.AlternateCaretEnabled

            Telnet.USERVARs.Add("IBMCURLIB", Nothing)
            Telnet.USERVARs.Add("IBMIMENU", Nothing)
            Telnet.USERVARs.Add("IBMPROGRAM", Nothing)
            Telnet.USERVARs.Add("DEVNAME", EmulatorView(CInt(Me.Tag)).Settings.StationName)
            'Telnet.USERVARs.Add("KBDTYPE", "USB")
            'Telnet.USERVARs.Add("CODEPAGE", "37")
            'Telnet.USERVARs.Add("CHARSET", "697")
            Telnet.USERVARs.Add("KBDTYPE", Emulator.Locale.KBDTYPE)
            Telnet.USERVARs.Add("CODEPAGE", Emulator.Locale.CODEPAGE)
            Telnet.USERVARs.Add("CHARSET", Emulator.Locale.CHARSET)
            Telnet.USERVARs.Add("IBMSENDCONFREC", "YES")
            Telnet.USERVARs.Add("IBMASSOCPRT", Nothing) 'Printer associated with display device
            PushErrorLine("")
        End If

    End Sub

    Private Function GetKerberosTicket() As String
        Dim Credential As NSspi.Credentials.ClientCredential = Nothing
        Dim Context As NSspi.Contexts.ClientContext = Nothing
        Dim hostname As String = EmulatorView(CInt(Me.Tag)).Settings.HostAddress
        If Not String.IsNullOrWhiteSpace(hostname) Then hostname = hostname.ToLower
        Dim ServerPrinciple As String = "krbsvr400/" & hostname
        Try
            Credential = New NSspi.Credentials.ClientCredential(NSspi.PackageNames.Kerberos)
            'RFC 4777 has an error: correct SPN begins with "krbsvr400/", not "krbsrv400/"!
            Context = New NSspi.Contexts.ClientContext(Credential, _
                                                        ServerPrinciple, _
                                                        NSspi.Contexts.ContextAttrib.Connection Or _
                                                        NSspi.Contexts.ContextAttrib.Delegate Or _
                                                        NSspi.Contexts.ContextAttrib.MutualAuth Or _
                                                        NSspi.Contexts.ContextAttrib.Confidentiality)
            Dim OutToken() As Byte = Nothing
            Dim ServerToken() As Byte = Nothing
            Dim ClientStatus As NSspi.SecurityStatus
            ClientStatus = Context.Init(ServerToken, OutToken)
            'Ordinarily in a kerberos handshake we would need to call Context.Init multiple times until ClientStatus <> Continue, but the AS400 just wants this first token.
            If OutToken IsNot Nothing Then
                Return BitConverter.ToString(OutToken).Replace("-", "")
            Else
                Throw New Exception("Kerberos initialization returned a null security token")
            End If
        Catch ex As NSspi.SSPIException
            Dim msg As String = Nothing
            Select Case ex.ErrorCode
                Case NSspi.SecurityStatus.NoCredentials
                    msg = "Unable to find a Kerberos credential for the current user.  This may indicate that the user is not a member of an Active Directory domain."
                Case NSspi.SecurityStatus.TargetUnknown
                    msg = "'" & ServerPrinciple & "' is unknown to Kerberos.  Please ensure that you have specified the fully qualified host name (foo.bar.local)" _
                        & " and that the IBM host has been properly configured for Kerberos in your Active Directory domain."
                Case Else
                    msg = ex.Message
            End Select
            Logger.Warn("Error getting Kerberos ticket: " & msg)
            MsgBox(msg, MsgBoxStyle.OkOnly Or MsgBoxStyle.Exclamation, "Kerberos Error")
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
            MsgBox(ex.Message, MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, "Kerberos Error")
        Finally
            If Credential IsNot Nothing Then Credential.Dispose()
            If Context IsNot Nothing Then Context.Dispose()
        End Try
        Return Nothing
    End Function

    Private Delegate Sub PushErrorLineDelegate(Text As String)
    Private dPushErrorLine = New PushErrorLineDelegate(AddressOf PushErrorLine)
    Private Sub PushErrorLine(Text As String)
        With Me.ErrorLines
            If .Count > 1 Then .Pop()
            .Push(Text)
            Me.ToolStripStatusLabelStatus.Text = .Peek
        End With
    End Sub

    Private Delegate Sub PopErrorLineDelegate()
    Private dPopErrorLine = New PopErrorLineDelegate(AddressOf PopErrorLine)
    Private Sub PopErrorLine()
        With Me.ErrorLines
            If .Count > 1 Then .Pop()
            Me.ToolStripStatusLabelStatus.Text = .Peek
        End With
    End Sub

    Private Delegate Sub SetConnectionStatusDelegate(Status As Boolean, Secured As Boolean, ProtocolInfo As String)
    Private dSetConnectionStatus = New SetConnectionStatusDelegate(AddressOf SetConnectionStatus)
    Private Sub SetConnectionStatus(Status As Boolean, Secured As Boolean, ProtocolInfo As String)
        If Status Then
            Me.ToolStripStatusLabelConnectionState.ImageKey = "Connected"
            Me.ToolStripStatusLabelConnectionState.ToolTipText = "Connected"
        Else
            Me.ToolStripStatusLabelConnectionState.ImageKey = "Disconnected"
            Me.ToolStripStatusLabelConnectionState.ToolTipText = "Disconnected"
        End If
        If Secured Then
            Me.ToolStripStatusLabelSSL.ImageKey = "Secured"
            Me.ToolStripStatusLabelSSL.ToolTipText = "Connection Secured"
            If ProtocolInfo IsNot Nothing Then Me.ToolStripStatusLabelSSL.ToolTipText += vbCr & ProtocolInfo
        Else
            Me.ToolStripStatusLabelSSL.ImageKey = "Unsecured"
            Me.ToolStripStatusLabelSSL.ToolTipText = "Connection not secured"
        End If
    End Sub

    Private Sub SendBytes(ByRef buf() As Byte, ByVal OpCode As IBM5250.TN5250.OpCodes)
        SendBytes(buf, OpCode, 0)
    End Sub

    Private Sub SendBytes(ByRef buf() As Byte, ByVal OpCode As IBM5250.TN5250.OpCodes, ByVal Flags As UInt16)
        Logger.Trace("")

        Dim hdr As New IBM5250.TN5250.Header(buf.Length)
        hdr.OpCode = OpCode
        hdr.Flags = Flags
        Dim b(hdr.RecLen - 1) As Byte
        Array.Copy(hdr.ToBytes, 0, b, 0, hdr.Length)
        Array.Copy(buf, 0, b, hdr.Length, buf.Length)

        Try
            Telnet.Send(b)
            If OpCode = IBM5250.TN5250.OpCodes.PutOrGet Then Me.SetInputReady(False) 'wait for a reply before we send anything else
        Catch ex As Exception
            'XXX
            Logger.Error(ex.Message, ex)
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Error sending data")
        End Try

    End Sub

    Private Sub SetFieldFocus()
        Me.SetFieldFocus(Emulator.Screen.Row, Emulator.Screen.Column)
    End Sub
    Private Sub SetFieldFocus(ByVal Row As Byte, ByVal Column As Byte)
        Try
            Dim findex As Integer = Emulator.Screen.FieldIndexOfAddress(Row, Column)
            If findex > -1 Then
                InputFields(findex).Focus()
                Me.ApplyHighlightFCW(findex, True)
            End If
        Catch ex As Exception
            Logger.Error("Failed to set focus on active field", ex)
        End Try
    End Sub

    Private Delegate Sub OnKeyboardStateChangedDelegate()
    Private dKeyboardStateChanged = New OnKeyboardStateChangedDelegate(AddressOf OnKeyboardStateChanged)
    Private Sub _OnKeyboardStateChange() Handles Emulator.KeyboardStateChanged
        Try
            Me.Invoke(dKeyboardStateChanged)
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub OnKeyboardStateChanged()
        If Emulator.Keyboard.Locked Then
            Me.ToolStripStatusLabelError.ForeColor = Color.Red
            Me.ToolStripStatusLabelError.Enabled = True
        Else
            'Me.ToolStripStatusLabelError.ForeColor = Color.Black
            Me.ToolStripStatusLabelError.Enabled = False
        End If
    End Sub

    Private Delegate Sub OnInsertChangedDelegate(ByVal NewValue As Boolean)
    Private dInsertChanged = New OnInsertChangedDelegate(AddressOf OnInsertChanged)
    Private Sub _OnInsertChanged(ByVal NewValue As Boolean) Handles Emulator.InsertChanged
        Try
            Me.Invoke(dInsertChanged, New Object() {NewValue})
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub OnInsertChanged(ByVal NewValue As Boolean)
        Me.ToolStripStatusLabelInsert.Enabled = NewValue
    End Sub

    Private Delegate Sub OnScreenClearedDelegate()
    Private dScreenCleared = New OnScreenClearedDelegate(AddressOf OnScreenCleared)
    Private Sub _OnScreenCleared() Handles Emulator.ScreenCleared
        Try
            Me.Invoke(dScreenCleared)
        Catch ex As Exception

        End Try
    End Sub
    Private Sub OnScreenCleared()
        Me.UpdateScreenMetrics()
        Me.ClearScreen()
        Me.OnResizeEnd(New System.EventArgs)
    End Sub

    Private Delegate Sub OnStringsChangedDelegate()
    Private dStringsChanged = New OnStringsChangedDelegate(AddressOf OnStringsChanged)
    Private Sub _OnStringsChanged() Handles Emulator.StringsChanged
        Try
            Me.Invoke(dStringsChanged)
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub OnStringsChanged()
        Logger.Trace("Started")

        Me.PanelGreenScreen.Invalidate(False)
        For Each kvp As KeyValuePair(Of String, IBM5250.Emulator.EmulatorScreen.EmulatorPopup) In Emulator.Screen.PopUps
            Dim frm As frmEmulatorPopup = GetOpenPopupForm(kvp.Key)
            If frm IsNot Nothing Then
                frm.Panel1.Invalidate(False)
            End If
        Next

    End Sub

    Private Delegate Sub OnFieldsChangedDelegate()
    Private dFieldsChanged = New OnFieldsChangedDelegate(AddressOf OnFieldsChanged)
    Private Sub _OnFieldsChanged() Handles Emulator.FieldsChanged
        Try
            Me.Invoke(dFieldsChanged)
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub OnFieldsChanged()
        Logger.Trace("Started")

        Me.FieldsChanged()

        Logger.Trace("Finished")
    End Sub

    Private Delegate Sub OnCursorPositionSetDelegate(ByVal Row As Byte, ByVal Column As Byte)
    Private dCursorPositionSet = New OnCursorPositionSetDelegate(AddressOf OnCursorPositionSet)
    Private Sub _OnCursorPositionSet(ByVal Row As Byte, ByVal Column As Byte) Handles Emulator.CursorPositionSet
        Try
            Me.Invoke(dCursorPositionSet, New Object() {Row, Column})
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub OnCursorPositionSet(ByVal Row As Byte, ByVal Column As Byte)
        Me.ToolStripStatusLabelCoordinates.Text = Emulator.Screen.Row.ToString & "," & Emulator.Screen.Column.ToString
        Me.SetFieldFocus(Row, Column)
    End Sub

    Private Delegate Sub OnErrorTextChangedDelegate()
    Private dErrorTextChanged = New OnErrorTextChangedDelegate(AddressOf OnErrorTextChanged)
    Private Sub _OnErrorTextChanged() Handles Emulator.ErrorTextChanged
        Try
            Me.Invoke(dErrorTextChanged)
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub OnErrorTextChanged()
        Me.PushErrorLine(Emulator.Screen.ErrorText)
    End Sub

    Private Delegate Sub OnPopupAddedDelegate(ByVal Key As String)
    Private dPopupAdded = New OnPopupAddedDelegate(AddressOf OnPopupAdded)
    Private Sub _OnPopupAdded(ByVal Key As String) Handles Emulator.PopupAdded
        Try
            Me.Invoke(dPopupAdded, New Object() {Key})
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub OnPopupAdded(ByVal Key As String)

        Me.UpdateScreenMetrics()

        Dim frm As New frmEmulatorPopup
        frm.MdiParent = Me.MdiParent

        frm.Tag = Key
        frm.ParentEmulatorView = Me
        frm.KeyPreview = True
        'frm.Panel1.BackColor = Color.FromKnownColor(MappedColor(Emulator.Screen.PopUps(Key).BackColor))

        AddHandler frm.KeyDown, AddressOf frmEmulatorView_KeyDown
        AddHandler frm.KeyUp, AddressOf frmEmulatorView_KeyUp
        AddHandler frm.KeyPress, AddressOf frmEmulatorView_KeyPress

        frm.StartPosition = FormStartPosition.Manual
        frm.TopOffset = (Emulator.Screen.PopUps(Key).Top * Me.ScreenMetrics.ControlHeight)
        frm.LeftOffset = (Emulator.Screen.PopUps(Key).Left * Me.ScreenMetrics.Font_Character_Width)
        frm.Top = Me.Top + frm.TopOffset
        frm.Left = Me.Left + frm.LeftOffset

        Dim BordersHeight As Integer = frm.Height - frm.Panel1.Height
        Dim BordersWidth As Integer = frm.Width - frm.Panel1.Width

        frm.Height = (Emulator.Screen.PopUps(Key).Rows * Me.ScreenMetrics.ControlHeight) + BordersHeight
        frm.Width = ((Emulator.Screen.PopUps(Key).Columns + 2) * Me.ScreenMetrics.Font_Character_Width) + BordersWidth
        frm.Text = Emulator.Screen.PopUps(Key).WindowTitle
        frm.ControlBox = False
        frm.ShowInTaskbar = False
        frm.FormBorderStyle = Windows.Forms.FormBorderStyle.Fixed3D
        frm.MaximizeBox = False

        'frm.ReturnCursorTo.Row = Emulator.Screen.Row
        'frm.ReturnCursorTo.Column = Emulator.Screen.Column

        frm.BackColor = Me.PanelGreenScreen.BackColor 'stops brief flash of grey when form opens

        frm.Show()

    End Sub

    Private Delegate Sub OnPopupRemovedDelegate(ByVal Key As String)
    Private dPopupRemoved = New OnPopupRemovedDelegate(AddressOf OnPopupRemoved)
    Private Sub _OnPopupRemoved(ByVal Key As String) Handles Emulator.PopupRemoved
        Try
            Me.Invoke(dPopupRemoved, New Object() {Key})
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub OnPopupRemoved(ByVal Key As String)
        Dim pop As frmEmulatorPopup = Nothing
        For Each frm As Form In Application.OpenForms
            If frm.Tag = Key Then
                If frm.GetType() Is GetType(frmEmulatorPopup) Then
                    pop = DirectCast(frm, frmEmulatorPopup)
                    If pop.ParentEmulatorView Is Me Then
                        Exit For
                    Else
                        pop = Nothing
                    End If
                End If
            End If
        Next
        If pop IsNot Nothing Then
            pop.Controls.Clear() 'this prevents the textboxes from being disposed when the form closes.
            pop.Close()
        End If
    End Sub

    Private Function GetOpenPopupForm(ByVal Key As String) As frmEmulatorPopup
        Dim pop As frmEmulatorPopup = Nothing
        For Each frm As Form In Application.OpenForms
            If frm.Tag = Key Then
                If frm.GetType() Is GetType(frmEmulatorPopup) Then
                    pop = DirectCast(frm, frmEmulatorPopup)
                    If pop.ParentEmulatorView Is Me Then
                        Exit For
                    Else
                        pop = Nothing
                    End If
                End If
            End If
        Next
        Return pop
    End Function

    Private Sub OnDataReady(ByVal Buffer() As Byte, ByVal OpCode As IBM5250.TN5250.OpCodes) Handles Emulator.DataReady
        SendBytes(Buffer, OpCode)

    End Sub

    Private Sub Disco(ByVal sender As Object, ByVal e As Telnet.Net.Graphite.Telnet.DisconnectEventArgs) Handles Telnet.Disconnected
        Try
            Dim s As String = "Disconnected"
            If e.Reason IsNot Nothing Then s += ": " & e.Reason
            s += "."
            If Me.InvokeRequired Then
                Me.Invoke(dPushErrorLine, New Object() {s})
                Me.Invoke(dSetConnectionStatus, New Object() {Telnet.Connected, Telnet.Secured, Telnet.SecurityProtocolInfo})
            Else
                Me.PushErrorLine(s)
                Me.SetConnectionStatus(Telnet.Connected, Telnet.Secured, Telnet.SecurityProtocolInfo)
            End If
            Emulator.Screen.Clear()
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub

    Private Delegate Sub OnStartupResponseReceivedDelegate(ByVal ResponseCode As String)
    Private dStartupResponseReceived = New OnStartupResponseReceivedDelegate(AddressOf OnStartupResponseReceived)
    Private Sub _OnStartupResponseReceived(ByVal ResponseCode As String) Handles Emulator.StartupResponseReceived
        Try
            Me.Invoke(dStartupResponseReceived, New Object() {ResponseCode})
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub OnStartupResponseReceived(ResponseCode As String)
        Dim Response As New IBM5250.Emulator.StartupResponse(ResponseCode, Telnet.IBM_AuthEncryptionMethod = Global.Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.Kerberos)
        Me.PushErrorLine(Response.Description)
        Logger.Info("Message from iSeries: " & Response.Code & ": " & Response.Description)
        If Not Response.Success Then
            MsgBox(Response.Code & ": " & Response.Description, MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Message from iSeries")
            Disconnect(Response.Description)
        End If
    End Sub

    Private Sub OnDataStreamError(NegativeResponse As IBM5250.Emulator.NegativeResponse) Handles Emulator.DataStreamError
        Logger.Warn("Negative response: " & NegativeResponse.ToString)

        Dim b(4) As Byte
        b(0) = NegativeResponse >> 24
        b(1) = (NegativeResponse >> 16) And &HFF
        b(2) = (NegativeResponse >> 8) And &HFF
        b(3) = NegativeResponse And &HFF
        SendBytes(b, IBM5250.TN5250.OpCodes.None, IBM5250.TN5250.Flag.ERR)
    End Sub

    Private Delegate Sub OnMessageWaitingChangedDelegate(Lit As Boolean)
    Private dMessageWaitingChanged = New OnMessageWaitingChangedDelegate(AddressOf OnMessageWaitingChanged)
    Private Sub _OnMessageWaitingChanged(Lit As Boolean) Handles Emulator.MessageWaitingChanged
        Try
            Me.Invoke(dMessageWaitingChanged, New Object() {Lit})
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub OnMessageWaitingChanged(ByVal Lit As Boolean)
        SetMessageLight(Lit)
    End Sub

    Private Delegate Sub SetMessageLightDelegate(Lit As Boolean)
    Private dSetMessageLight = New SetMessageLightDelegate(AddressOf SetMessageLight)
    Private Sub SetMessageLight(Lit As Boolean)
        Me.ToolStripStatusLabelMsg.ForeColor = Color.Green
        Me.ToolStripStatusLabelMsg.Enabled = Lit
    End Sub

    Private Delegate Sub UpdateFieldAttributeDelegate(ByVal FieldIndex As Integer, ByVal RegisterEventHandlers As Boolean)
    Private dUpdateFieldAttribute = New UpdateFieldAttributeDelegate(AddressOf UpdateFieldAttribute)
    Private Sub UpdateFieldAttribute(ByVal FieldIndex As Integer, ByVal RegisterEventHandlers As Boolean)
        Me.UpdateFieldAttributeX(FieldIndex, Emulator.Screen.Fields(FieldIndex).Attribute, RegisterEventHandlers)
    End Sub
    Private Sub UpdateFieldAttributeX(ByVal FieldIndex As Integer, ByVal Attribute As IBM5250.Emulator.EmulatorScreen.FieldAttribute, ByVal RegisterEventHandlers As Boolean)
        With Me.InputFields(FieldIndex)
            Me.RegisterTextBoxEventHandlers(FieldIndex, False)

            .ForeColor = Color.FromKnownColor(MappedColor(Attribute.ForeColor))
            .BackColor = Color.FromKnownColor(MappedColor(Attribute.BackColor))

            'Exception for Windows color scheme
            If Not Attribute.NonDisplay Then
                If Not Attribute.Attribute.ToString.Contains("Reverse") Then
                    .BackColor = Color.FromKnownColor(MappedColor("FieldBackground"))
                End If
            End If

            .UseSystemPasswordChar = Attribute.NonDisplay

            If Attribute.Underscore And Not Emulator.Screen.Fields(FieldIndex).Flags.Bypass Then
                .BorderStyle = BorderStyle.FixedSingle
            Else
                .BorderStyle = BorderStyle.None
            End If

            If RegisterEventHandlers Then Me.RegisterTextBoxEventHandlers(FieldIndex, True)
        End With
    End Sub
    Private Sub _OnFieldAttributeUpdated(ByVal FieldIndex As Integer) Handles Emulator.FieldAttributeChanged
        Try
            Me.Invoke(dUpdateFieldAttribute, New Object() {FieldIndex, True})
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub
    Private Sub ApplyHighlightFCW(ByVal FieldIndex As Integer, ByVal RegisterEventHandlers As Boolean)
        For Each fcw As IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FCW In Emulator.Screen.Fields(FieldIndex).ControlFlags
            If fcw.Type = IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FCW_Type.Highlighted Then
                Dim attr As IBM5250.Emulator.EmulatorScreen.FieldAttribute = New IBM5250.Emulator.EmulatorScreen.FieldAttribute(fcw.Data, Emulator.Screen.DefaultForeColor, Emulator.Screen.BackColor)
                Me.UpdateFieldAttributeX(FieldIndex, attr, RegisterEventHandlers)
                Exit For
            End If
        Next
    End Sub

    Private Sub TelnetDataAvailable(ByVal sender As Object, ByVal e As Telnet.Net.Graphite.Telnet.DataAvailableEventArgs) Handles Telnet.DataAvailable
        Logger.Trace("")
        Dim Buffer() As Byte = e.Data
        Dim h As New IBM5250.TN5250.Header(Buffer)
        Logger.Debug("RCVD IBM5250(Variable Record Length: " & h.RecLen.ToString & ", Record Type: " & h.RecType.ToString & ", Header Length: 0x" & Hex(h.HeaderLen) & ", Flags: 0x" & Hex(h.Flags) & ", OpCode: " & h.OpCode.ToString & ")" & vbLf)
        Select Case h.RecType
            Case IBM5250.TN5250.RecordType.GDS
                Dim Success As Boolean
                Select Case h.OpCode
                    Case IBM5250.TN5250.OpCodes.TurnOnMessageLight

                        'SetMessageLight(True)
                        Try
                            Me.Invoke(dSetMessageLight, New Object() {True})
                        Catch ex As Exception
                            Logger.Error(ex.Message, ex)
                        End Try

                        Success = Emulator.ReadDataBuffer(Buffer, h.Length, h.RecLen - h.Length)


                    Case IBM5250.TN5250.OpCodes.TurnOffMessageLight

                        'SetMessageLight(False)
                        Try
                            Me.Invoke(dSetMessageLight, New Object() {False})
                        Catch ex As Exception
                            Logger.Error(ex.Message, ex)
                        End Try

                        Success = Emulator.ReadDataBuffer(Buffer, h.Length, h.RecLen - h.Length)

                    Case IBM5250.TN5250.OpCodes.None
                        'When USERVAR IBMSENDCONFREC=YES is sent during telnet option negotiation, the AS400 will send a connection
                        '   confirmation record with h.Reserved = &H9000 and h.Flags = &H6006.
                        Success = Emulator.ReadDataBuffer(Buffer, h.Length, h.RecLen - h.Length)

                    Case IBM5250.TN5250.OpCodes.CancelInvite
                        Emulator.Invited = False
                        SendBytes(New Byte() {}, IBM5250.TN5250.OpCodes.CancelInvite)
                        Success = Emulator.ReadDataBuffer(Buffer, h.Length, h.RecLen - h.Length)

                    Case IBM5250.TN5250.OpCodes.Invite
                        Emulator.Invited = True
                        Success = Emulator.ReadDataBuffer(Buffer, h.Length, h.RecLen - h.Length)

                    Case IBM5250.TN5250.OpCodes.Put, IBM5250.TN5250.OpCodes.PutOrGet, IBM5250.TN5250.OpCodes.SaveScreen, IBM5250.TN5250.OpCodes.RestoreScreen, IBM5250.TN5250.OpCodes.ReadScreen ', IBM5250.TN5250.OpCodes.ReadImmediate

                        'Me.SetInputReady(False)
                        Try
                            Me.Invoke(dSetInputReady, New Object() {False})
                        Catch ex As Exception
                            Logger.Error(ex.Message, ex)
                        End Try

                        Success = Emulator.ReadDataBuffer(Buffer, h.Length, h.RecLen - h.Length)

                        'Me.SetInputReady(True)
                        Try
                            Me.Invoke(dSetInputReady, New Object() {True})
                        Catch ex As Exception
                            Logger.Error(ex.Message, ex)
                        End Try

                        'Me.ProcessScreenMatch(Me.MatchedScreenDescription)
                        Try
                            Me.Invoke(dProcessScreenMatch, New Object() {Me.MatchedScreenDescription})
                        Catch ex As Exception
                            Logger.Error(ex.Message, ex)
                        End Try


                    Case Else
                        'XXX more opcodes to handle here instead of hitting the Else?

                        Logger.Warn("Unknown TN5250 Opcode: &H" & Hex(h.OpCode))
                        MsgBox("Unknown TN5250 Opcode: &H" & Hex(h.OpCode), MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, "Emulator Error")
                End Select
                If Not Success Then
                    Logger.Error("There was an error while parsing the 5250 data stream")
                End If
            Case Else
                Throw New ApplicationException("Record Type is invalid: 0x" & Hex(h.RecType))
        End Select

        Logger.Trace("Finished")
    End Sub

    Private Sub ClearScreen()
        Me.PriorFieldIndex = -1
        Me.PanelGreenScreen.Controls.Clear()

        'Resize the window if we switched from 80x24 to 132x27
        Static Dim PriorEmulatorWidth As Integer
        If PriorEmulatorWidth < 80 Then PriorEmulatorWidth = 80
        If Emulator.Screen.Columns <> PriorEmulatorWidth Then
            'frmEmulatorView_SizeChanged(Me, New System.EventArgs)
            Me.WindowState = FormWindowState.Maximized
            PriorEmulatorWidth = Emulator.Screen.Columns
        End If

        'Me.Graphics.Clear(Emulator.Screen.BackColor)
    End Sub

    Private Sub DrawStrings(e As PaintEventArgs)
        Logger.Trace("Started")

        Me.UpdateScreenMetrics()

        Me.PanelGreenScreen.BackColor = Color.FromKnownColor(MappedColor(Emulator.Screen.BackColor))

        ''Dump the string data for debugging:
        'For Each fld As IBM5250.Emulator.EmulatorScreen.Field In Emulator.Screen.Strings
        '    If fld.Allocated Then
        '        Debug.Print("String: Location=(" & fld.Location.Row.ToString & "," & fld.Location.Column.ToString & "), Length=" & fld.Location.Length.ToString & ", Text='" & fld.Text & "'")
        '    Else
        '        Exit For
        '    End If
        'Next

        Me.DrawStringsToGraphicsObject(e.Graphics, Emulator.Screen.Strings, False)

        Logger.Trace("Finished")
        'If Me.Parent.Cursor = Cursors.WaitCursor Then Me.Parent.Cursor = Cursors.Default
    End Sub

    Private Sub DrawStringsToGraphicsObject(ByRef g As Graphics, ByRef Strings() As IBM5250.Emulator.EmulatorScreen.Field, ByVal Desaturate As Boolean)
        'Strings are being updated on a different thread, so take a snapshot to work with here.
        Dim TempStrings(Strings.Length - 1) As IBM5250.Emulator.EmulatorScreen.Field
        Array.Copy(Strings, 0, TempStrings, 0, TempStrings.Length)
        '
        For i As Integer = 0 To TempStrings.Length - 1
            With TempStrings(i)
                If .Allocated Then
                    'If (.Text IsNot Nothing) Or (.IsInputField) Then
                    If .Text Is Nothing Then .Text = ""

                    Dim fb, bb As SolidBrush
                    If Desaturate Then
                        fb = New SolidBrush(Color.Black)
                        bb = New SolidBrush(Color.White)
                    Else
                        fb = New SolidBrush(Color.FromKnownColor(MappedColor(.Attribute.ForeColor)))
                        bb = New SolidBrush(Color.FromKnownColor(MappedColor(.Attribute.BackColor)))
                    End If

                    Dim x As Single = (.Location.Column - 1) * Me.ScreenMetrics.Font_Character_Width
                    Dim y As Single = (.Location.Row - 1) * Me.ScreenMetrics.ControlHeight
                    g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

                    If Not (Desaturate And (.Attribute.ForeColor = .Attribute.BackColor)) Then

                        'Draw the backcolor if it's different from the background
                        If .Attribute.BackColor <> "Background" Then
                            Dim w As Single = .Location.Length * Me.ScreenMetrics.Font_Character_Width + 1
                            g.FillRectangle(bb, x + 2, y, w, Me.ScreenMetrics.ControlHeight)
                        End If

                        If .Attribute.Underscore Then
                            'Debug.Print(.Text.Length & ":[" & .Text & "]")
                            Dim s As String = .Text

                            If .IsInputField Then 'XXX we shouldn't need to check this.  There's a problem with trailing attributes.
                                s = s.PadRight(.Location.Length, " ") & ChrW(&H200B) 'Zero-Width Space character
                            End If

                            g.DrawString(s, Me.ScreenMetrics.Font_Underscore, fb, x, y)
                        Else
                            g.DrawString(.Text, Me.ScreenMetrics.Font_Regular, fb, x, y)
                        End If
                    End If

                    fb.Dispose()
                    bb.Dispose()
                    'End If
                Else
                    Exit For
                End If
            End With
        Next
        TempStrings = Nothing
    End Sub

    Private Sub FieldsChanged()
        Logger.Trace("Started")

        Static Running As Boolean
        If Not Running Then
            Try
                Running = True
                Me.UpdateScreenMetrics()

                Dim MaxX, MaxY As Integer

                For Each kvp As KeyValuePair(Of String, IBM5250.Emulator.EmulatorScreen.EmulatorPopup) In Emulator.Screen.PopUps
                    Dim frm As frmEmulatorPopup = GetOpenPopupForm(kvp.Key)
                    frm.Panel1.Controls.Clear()
                Next
                Me.PanelGreenScreen.Controls.Clear()

                Dim FieldIndexToFocus As Integer = -1
                Dim FieldCursorOffset As Integer = 0
                Me.SuppressCursorPositionChanges = True
                Me.SuppressFieldModifiedFlags = True
                Me.SuppressScreenBufferUpdates = True
                For i As Integer = 0 To Emulator.Screen.Fields.Length - 1

                    InputFields(i).Dispose()
                    InputFields(i) = New EmulatorTextBox

                    With InputFields(i)

                        Dim fld As IBM5250.Emulator.EmulatorScreen.Field = Emulator.Screen.Fields(i)

                        If Emulator.Screen.Fields(i).Allocated Then
                            Me.UpdateFieldAttribute(i, False)

                            .Multiline = False
                            .TabStop = Not Emulator.Screen.Fields(i).Flags.Bypass
                            .MaxLength = Emulator.Screen.Fields(i).Location.Length
                            .Name = i.ToString
                            .Index = i
                            .Text = Emulator.Screen.Fields(i).Text

                            .AlternateCaretEnabled = EmulatorView(CInt(Me.Tag)).Settings.AlternateCaretEnabled

                            .Font = Me.ScreenMetrics.Font_Regular

                            .AutoSize = False
                            .Height = Me.ScreenMetrics.ControlHeight '- (Me.ScreenMetrics.Character_Height_Fudge \ 2)
                            .Width = (fld.Location.Length + 1) * Me.ScreenMetrics.Font_Character_Width

                            Dim PopupKey As String = Emulator.Screen.Fields(i).PopupKey

                            If PopupKey IsNot Nothing Then
                                Dim frm As Form = GetOpenPopupForm(PopupKey)

                                .Left = (fld.Location.Column - 1 - Emulator.Screen.PopUps(PopupKey).Left - 1) * Me.ScreenMetrics.Font_Character_Width
                                .Top = (fld.Location.Row - 1 - Emulator.Screen.PopUps(PopupKey).Top) * Me.ScreenMetrics.ControlHeight

                                .Parent = frm.Controls("Panel1")

                                '*** must not alter the cursor position here ***
                                'Try
                                '    If .Parent.Controls(0) Is InputFields(i) Then
                                '        Emulator.Screen.Row = fld.Location.Row
                                '        Emulator.Screen.Column = fld.Location.Column
                                '    End If
                                'Catch
                                'End Try

                                frm = Nothing
                            Else
                                .Left = (fld.Location.Column - 1) * Me.ScreenMetrics.Font_Character_Width
                                .Top = (fld.Location.Row - 1) * Me.ScreenMetrics.ControlHeight

                                .Parent = Me.PanelGreenScreen
                            End If
                            .BringToFront()

                            If .Right > MaxX Then MaxX = .Right
                            If .Bottom > MaxY Then MaxY = .Bottom

                            'If FieldIndexToFocus < 0 Then
                            '    If Emulator.Screen.Row = ctl.IBM.Location.Row Then
                            '        If Emulator.Screen.Column >= ctl.IBM.Location.Column Then
                            '            If Emulator.Screen.Column < (ctl.IBM.Location.Column + ctl.IBM.Location.Length) Then
                            '                FieldIndexToFocus = i
                            '                FieldCursorOffset = Emulator.Screen.Column - ctl.IBM.Location.Column 'set cursor position with field
                            '            End If
                            '        End If
                            '    End If
                            'End If

                            'Do this last to prevent excessive events from firing.
                            Me.RegisterTextBoxEventHandlers(i, True)

                        Else
                            Exit For
                        End If
                    End With
                Next

                'If FieldIndexToFocus >= 0 Then
                '    Emulator.Screen.Fields(FieldIndexToFocus).Focus()
                '    If FieldCursorOffset > 0 Then
                '        If Emulator.Screen.Fields(FieldIndexToFocus).TextLength >= FieldCursorOffset Then
                '            Emulator.Screen.Fields(FieldIndexToFocus).SelectionStart = FieldCursorOffset
                '        End If
                '    End If
                '    Emulator.Screen.Fields(FieldIndexToFocus).SelectionLength = 0
                '    'Emulator.Screen.Fields(FieldIndexToFocus).Select(FieldCursorOffset, 0)
                '    'Emulator.Screen.Row = sender.IBM.Location.Row
                '    'Emulator.Screen.Column = sender.IBM.Location.Column + sender.SelectionStart
                'End If

            Catch ex As Exception
                Logger.Error(ex.Message, ex)
            Finally
                Me.SuppressCursorPositionChanges = False
                Me.SuppressFieldModifiedFlags = False
                Me.SuppressScreenBufferUpdates = False
                Running = False
                Me.SetFieldFocus()
            End Try
        End If

        Logger.Trace("Finished")

        'If frmConnectionManager.Cursor = Cursors.WaitCursor Then frmConnectionManager.Cursor = Cursors.Default

    End Sub

    Private Sub RegisterTextBoxEventHandlers(ByVal InputFieldIndex As Integer, ByVal Register As Boolean)
        If (InputFieldIndex >= 0) And (InputFieldIndex < InputFields.Length) Then
            If InputFields(InputFieldIndex) IsNot Nothing Then
                With InputFields(InputFieldIndex)
                    If Register Then
                        If Not .EventHandlersRegistered Then
                            AddHandler .PreviewKeyDown, AddressOf Me.Control_PreviewKeyDown
                            AddHandler .Enter, AddressOf Me.EmulatorField_Enter
                            AddHandler .Leave, AddressOf Me.EmulatorField_Leave
                            AddHandler .KeyDown, AddressOf Me.EmulatorField_KeyDown
                            AddHandler .KeyUp, AddressOf Me.EmulatorField_KeyUp
                            AddHandler .KeyPress, AddressOf Me.EmulatorField_KeyPress
                            AddHandler .TextChanged, AddressOf Me.EmulatorField_TextChanged
                            AddHandler .SelectionChanged, AddressOf Me.EmulatorField_SelectionChanged
                            AddHandler .InsertChanged, AddressOf Me.EmulatorField_InsertChanged
                            AddHandler .MouseDown, AddressOf Me.EmulatorField_MouseDown
                            .EventHandlersRegistered = True
                        End If
                    Else
                        If .EventHandlersRegistered Then
                            RemoveHandler .PreviewKeyDown, AddressOf Me.Control_PreviewKeyDown
                            RemoveHandler .Enter, AddressOf Me.EmulatorField_Enter
                            RemoveHandler .Leave, AddressOf Me.EmulatorField_Leave
                            RemoveHandler .KeyDown, AddressOf Me.EmulatorField_KeyDown
                            RemoveHandler .KeyUp, AddressOf Me.EmulatorField_KeyUp
                            RemoveHandler .KeyPress, AddressOf Me.EmulatorField_KeyPress
                            RemoveHandler .TextChanged, AddressOf Me.EmulatorField_TextChanged
                            RemoveHandler .SelectionChanged, AddressOf Me.EmulatorField_SelectionChanged
                            RemoveHandler .InsertChanged, AddressOf Me.EmulatorField_InsertChanged
                            RemoveHandler .MouseDown, AddressOf Me.EmulatorField_MouseDown
                            .EventHandlersRegistered = False
                        End If
                    End If
                End With
            End If
        End If
    End Sub

    Public Function GetTextWidth(ByVal Text As String, ByVal DestinationControl As Control) As Integer
        Dim g As Graphics = Graphics.FromHwnd(DestinationControl.Handle)
        'g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Dim f As SizeF = g.MeasureString(Text, DestinationControl.Font, 5000, StringFormat.GenericTypographic)
        Return f.Width
    End Function
    Public Function GetTextSize(ByVal Text As String, ByVal DestinationControl As Control) As SizeF
        Dim g As Graphics = Graphics.FromHwnd(DestinationControl.Handle)
        'g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Return g.MeasureString(Text, DestinationControl.Font, 5000, StringFormat.GenericTypographic)
    End Function

    Private Sub EmulatorField_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = Windows.Forms.MouseButtons.Right Then
            Dim txt As EmulatorTextBox = DirectCast(sender, EmulatorTextBox)
            Dim ctx As New ContextMenuStrip
            Dim mi_c As New ToolStripMenuItem("Copy")
            Dim mi_p As New ToolStripMenuItem("Paste")
            ctx.ImageList = Me.ImageListFieldContextMenu
            mi_c.ImageIndex = 0
            mi_p.ImageIndex = 1
            AddHandler mi_c.Click, AddressOf EmulatorField_ContextMenuCopy
            AddHandler mi_p.Click, AddressOf EmulatorField_ContextMenuPaste
            mi_c.Enabled = (txt.SelectionLength > 0)
            mi_p.Enabled = Clipboard.ContainsText
            ctx.Items.Add(mi_c)
            ctx.Items.Add(mi_p)
            ctx.Show(txt, txt.PointToClient(MousePosition))
        End If
    End Sub
    Private Sub EmulatorField_ContextMenuCopy(ByVal sender As Object, ByVal e As EventArgs)
        Dim mi As ToolStripMenuItem = DirectCast(sender, ToolStripMenuItem)
        Dim ctx As ContextMenuStrip = DirectCast(mi.Owner, ContextMenuStrip)
        Dim txt As EmulatorTextBox = DirectCast(ctx.SourceControl, EmulatorTextBox)
        txt.Copy()
        txt = Nothing
        ctx = Nothing
        mi = Nothing
    End Sub
    Private Sub EmulatorField_ContextMenuPaste(ByVal sender As Object, ByVal e As EventArgs)
        Dim mi As ToolStripMenuItem = DirectCast(sender, ToolStripMenuItem)
        Dim ctx As ContextMenuStrip = DirectCast(mi.Owner, ContextMenuStrip)
        Dim txt As EmulatorTextBox = DirectCast(ctx.SourceControl, EmulatorTextBox)
        Me.TryUnformatClipboardContents()
        txt.Paste()
        txt = Nothing
        ctx = Nothing
        mi = Nothing
    End Sub

    Private Sub EmulatorField_InsertChanged(ByVal NewValue As Boolean)
        Emulator.Keyboard.Insert = NewValue
    End Sub

    Private Sub EmulatorField_SelectionChanged(ByVal sender As Object, ByVal e As EventArgs)
        Logger.Trace("")
        UpdateCoordinates(sender)
    End Sub

    Public Sub SelectNextVerticalField(ByVal RelativeToIndex As Integer, ByVal Reverse As Boolean)
        Dim idx As Integer = Emulator.Screen.NextVerticalFieldIndex(RelativeToIndex, Reverse)
        If idx > -1 Then
            Try
                Me.InputFields(idx).Focus()
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Sub TryUnformatClipboardContents()
        Try
            'strip any formatting out of the text on the clipboard
            Dim clip As String = Clipboard.GetText(TextDataFormat.Text)
            System.Threading.Thread.Sleep(250)
            If clip IsNot Nothing Then Clipboard.SetText(clip)
        Catch ex As Exception
        End Try
    End Sub
    Private Sub EmulatorField_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        Logger.Trace("")

        Dim txt As EmulatorTextBox = DirectCast(sender, EmulatorTextBox)
        If Not Emulator.Keyboard.Locked Then
            'XXX respect flags for "field exit required", "mandatory enter", "auto enter", "mandatory fill"

            Select Case e.KeyCode
                Case Keys.Insert
                    If e.Shift Then
                        Me.TryUnformatClipboardContents()
                    End If
                Case Keys.V
                    If e.Control Then
                        Me.TryUnformatClipboardContents()
                    End If
                Case Keys.End 'ERASE EOF
                    If e.Modifiers = Keys.None Then
                        If txt.Text IsNot Nothing Then
                            Dim CursorPos As Integer = txt.SelectionStart
                            txt.Text = txt.Text.Substring(0, txt.SelectionStart) + New String(" ", txt.MaxLength - txt.SelectionStart)
                            txt.SelectionStart = CursorPos
                        End If
                        e.SuppressKeyPress = True
                    End If
                Case Keys.F1 To Keys.F24
                    e.SuppressKeyPress = True
                Case Keys.Right
                    Try
                        If (txt.SelectionLength = 0) And (txt.SelectionStart = txt.MaxLength) Then
                            txt.Parent.SelectNextControl(sender, True, True, False, True)
                            e.SuppressKeyPress = True
                        End If
                    Catch ex As Exception
                    End Try
                Case Keys.Left
                    Try
                        If (txt.SelectionLength = 0) And (txt.SelectionStart = 0) Then
                            txt.Parent.SelectNextControl(sender, False, True, False, True)
                            e.SuppressKeyPress = True
                        End If
                    Catch ex As Exception
                    End Try
                Case Keys.Enter
                    'e.Handled = True
                    e.SuppressKeyPress = True
                Case Keys.Up
                    Try
                        'sender.Parent.SelectNextControl(sender, False, True, False, True)
                        Me.SelectNextVerticalField(txt.Index, True)
                    Catch ex As Exception
                    End Try
                    'e.Handled = True
                    e.SuppressKeyPress = True
                Case Keys.Down
                    Try
                        'sender.Parent.SelectNextControl(sender, True, True, False, True)
                        Me.SelectNextVerticalField(txt.Index, False)
                    Catch ex As Exception
                    End Try
                    'e.Handled = True
                    e.SuppressKeyPress = True
                Case Keys.Add, Keys.Subtract 'numpad only: Field+, Field-
                    Dim s As String = txt.Text
                    'Client Access clears the field from the cursor position forward
                    s = s.Substring(0, txt.SelectionStart) + New String(" ", txt.MaxLength - txt.SelectionStart)
                    '
                    Select Case Emulator.Screen.Fields(txt.Index).Flags.Shift_Edit_Spec
                        Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.NumericOnly
                            If s.Length < txt.MaxLength Then
                                s += New String(" ", txt.MaxLength - s.Length)
                            End If

                            'If (txt.SelectionStart = txt.MaxLength - 1) And _
                            '    (txt.IBM.Flags.Mandatory_Fill <> IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_MandatoryFill.None) Then
                            '    'special handling to match Client Access and avoid losing the last digit
                            'Else
                            'End If
                            Dim RightAdjust As Boolean = False
                            Dim FillChar As Char = " "
                            Dim SpecialLastCharHandling As Boolean = False
                            Select Case Emulator.Screen.Fields(txt.Index).Flags.Mandatory_Fill
                                Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_MandatoryFill.RightAdjust_ZeroFill
                                    RightAdjust = True
                                    FillChar = "0"
                                    SpecialLastCharHandling = True
                                Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_MandatoryFill.RightAdjust_BlankFill
                                    RightAdjust = True
                                    FillChar = " "
                                    SpecialLastCharHandling = True
                                Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_MandatoryFill.MandatoryFill
                                    RightAdjust = False
                                    FillChar = "" 'XXX?
                                    SpecialLastCharHandling = True
                            End Select

                            If (txt.SelectionStart = txt.MaxLength - 1) And SpecialLastCharHandling Then
                                'We're on the last character; don't erase it.
                            Else
                                'Erase from cursor position to end of field
                                s = s.Substring(0, txt.SelectionStart) + New String(" ", txt.MaxLength - txt.SelectionStart)
                            End If

                            If RightAdjust Then
                                Dim ss As String = s
                                For i As Integer = 0 To ss.Length - 1
                                    If Not Char.IsWhiteSpace(ss(i)) Then
                                        s = s.Substring(i)
                                        Exit For
                                    End If
                                Next
                                s = s.TrimEnd
                                s = s.PadLeft(txt.MaxLength, FillChar)
                            End If

                            If e.KeyCode = Keys.Add Then
                                If SpecialLastCharHandling Then
                                    'leave the last character alone
                                Else
                                    s = s.Substring(0, s.Length - 1) & " " 'make sure the last character is empty to indicate a positive number
                                End If
                            Else
                                If SpecialLastCharHandling Then
                                    Dim LastChar As Char = s.Substring(s.Length - 1)
                                    If Char.IsDigit(LastChar) Then
                                        Dim LastByte As Byte = AscW(LastChar)
                                        LastByte = Emulator.UTF8_To_EBCDIC(New Byte() {LastByte})(0) - &H20 'change Fx to Dx to indicate a negative #
                                        LastByte = Emulator.EBCDIC_To_UTF8(New Byte() {LastByte})(0)
                                        LastChar = ChrW(LastByte)
                                        s = s.Substring(0, s.Length - 1) & LastChar
                                    Else
                                        'XXX throw an error

                                    End If
                                Else
                                    s = s.Substring(0, s.Length - 1) & "}" 'change the last char to "}", which will become D0 in EBCDIC to indicate a negative #
                                End If
                            End If

                        Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.SignedNumeric
                            s = s.TrimEnd
                            s = s.PadLeft(txt.MaxLength - 1)
                            If e.KeyCode = Keys.Add Then
                                s += " " 'make sure the last character is empty to indicate a positive number
                            Else
                                s += "-" 'make sure the last character is "-" to indicate a negative number
                                'This "-" will be removed and the last digit will be modified by setting its "zone" to "D" prior to sending to the 400.
                            End If
                        Case Else
                    End Select
                    Dim oldpos As Integer = txt.SelectionStart
                    txt.Text = s
                    txt.SelectionStart = oldpos
                    Try
                        'Dim ctl As Control = txt.Parent.GetNextControl(txt, True) 'get next tabstop control; returns Nothing if at end of chain.
                        'If ctl Is Nothing Then ctl = txt.Parent.GetNextControl(Nothing, True) 'get first tabstop control.
                        'ctl.Focus()
                        txt.Parent.SelectNextControl(txt, True, True, False, True)
                    Catch ex As Exception
                        'XXX
                    End Try
                    'e.Handled = True
                    e.SuppressKeyPress = True

                Case Else
                    If Emulator.Screen.Fields(txt.Index).Flags.Bypass Then
                        'XXX should throw an error here instead of just ignoring the keypress.
                        'e.Handled = True
                        e.SuppressKeyPress = True
                    End If
            End Select
            'e.Handled = True
        Else
            e.Handled = False
        End If
    End Sub
    Private Sub EmulatorField_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        Logger.Trace("")
    End Sub

    'Private Sub SetNextControl(ctl As Control, Forward As Boolean)
    '    Dim FoundControl As Boolean = False
    '    Try
    '        Dim origin As Control = ctl
    '        Dim c As Control = ctl
    '        Do
    '            c = c.Parent.GetNextControl(c, Forward)
    '            SelectNextControl()
    '            If c Is Nothing Then c = c.Parent.GetNextControl(Nothing, Forward)
    '        Loop While Not FoundControl
    '    Catch ex As Exception

    '    End Try
    'End Sub


    Public Sub EmulatorField_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs)
        Logger.Trace("")

        Try
            Dim txt As EmulatorTextBox = DirectCast(sender, EmulatorTextBox)
            'XXX handle "Dup key" here and respect the "Dup Enable" flag on fields.
            If Char.IsControl(e.KeyChar) Then
                Select Case AscW(e.KeyChar)
                    'Case Keys.Tab
                    'XXX handle all non-data keypresses here

                    Case Else

                End Select
            Else
                If Not Emulator.Screen.Fields(txt.Index).Flags.Bypass Then
                    Select Case Emulator.Screen.Fields(txt.Index).Flags.Shift_Edit_Spec
                        Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.AlphaOnly
                            Select Case e.KeyChar
                                Case "A" To "Z", "a" To "z", ",", "-", ".", " "
                                Case Else
                                    e.Handled = True
                            End Select
                        Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.AlphaShift
                            'Accepts all characters, and the Shift keys are acknowledged.
                        Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.NumericOnly
                            Select Case e.KeyChar
                                Case "0" To "9", "-", "+", ",", ".", " "
                                Case Else
                                    e.Handled = True
                            End Select
                        Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.NumericShift
                            'Accepts all characters.
                        Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.SignedNumeric
                            Select Case e.KeyChar
                                Case "0" To "9"
                                    If txt.SelectionStart = txt.MaxLength - 1 Then
                                        e.Handled = True 'not allowed to type anything in the sign position
                                    End If
                                Case Else
                                    e.Handled = True
                            End Select
                        Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.DigitsOnly
                            Select Case e.KeyChar
                                Case "0" To "9"
                                Case Else
                                    e.Handled = True
                            End Select
                        Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.KatakanaShift
                            'XXX hopefully we'll never encounter this.
                        Case IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.IO
                            'Only a magnetic stripe reader or selector light pen can enter data into an I/O field without causing an error.
                            e.Handled = True
                    End Select

                    If Emulator.Screen.Fields(txt.Index).Flags.UpperCase Then
                        If Char.IsLetter(e.KeyChar) Then e.KeyChar = Char.ToUpper(e.KeyChar)
                    End If

                    'This works, but it doesn't catch programmatic changes to field values.
                    'Emulator.Screen.WriteTextBuffer(IBM5250.Emulator.UTF8_To_EBCDIC(New Byte() {AscW(e.KeyChar)}))
                Else
                    'XXX
                End If
            End If
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub

    Private Sub EmulatorField_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
        Logger.Trace("")
        Dim txt As EmulatorTextBox = DirectCast(sender, EmulatorTextBox)

        Emulator.Screen.Fields(txt.Index).Text = txt.Text
        If Not Me.SuppressFieldModifiedFlags Then
            Emulator.Screen.Fields(txt.Index).Flags.Modified = txt.Modified
        End If
        If Not Me.SuppressScreenBufferUpdates Then
            '    'Emulator.Screen.Fields(sender.Index).IBM.Location.
            'Dim b(txt.IBM.Location.Length - 1) As Byte
            'Array.Copy(Emulator.Screen.TextBuffer, txt.IBM.Location.Position, b, 0, b.Length) 'get the existing data from the screen buffer
            'Dim c() As Byte = System.Text.Encoding.ASCII.GetBytes(txt.Text)
            'Array.Copy(c, 0, b, 0, c.Length) 'overwrite only as many bytes as we have in the field
            'b = IBM5250.Emulator.UTF8_To_EBCDIC(b)

            Dim s As String = New String(" ", Emulator.Screen.Fields(txt.Index).Location.Length)
            Dim b() As Byte = System.Text.Encoding.UTF8.GetBytes(s)
            If txt.Text IsNot Nothing Then
                s = txt.Text
                If s.Length > Emulator.Screen.Fields(txt.Index).Location.Length Then s = s.Substring(0, Emulator.Screen.Fields(txt.Index).Location.Length)
                Dim c() As Byte = System.Text.Encoding.UTF8.GetBytes(s)
                Array.Copy(c, 0, b, 0, c.Length)
            End If
            b = Emulator.UTF8_To_EBCDIC(b)

            Emulator.Screen.WriteTextBuffer(Emulator.Screen.Fields(txt.Index).Location.Row, Emulator.Screen.Fields(txt.Index).Location.Column, b)
            Emulator.Screen.UpdateStrings(True)
        End If

        'Auto-tab during data entry
        If txt.SelectionStart = txt.MaxLength Then
            Try
                If txt.Parent.SelectNextControl(sender, True, True, False, True) Then
                    Dim Container As ContainerControl = txt.GetContainerControl
                    If Container IsNot Nothing Then
                        If Container.ActiveControl.GetType = GetType(EmulatorTextBox) Then
                            Dim txt2 As EmulatorTextBox = Container.ActiveControl
                            If txt2 IsNot txt Then txt2.SelectionStart = 0
                            txt2 = Nothing
                        End If
                    End If
                End If
            Catch ex As Exception
                Logger.Warn("Error during auto-tab attempt: " & ex.Message, ex)
            End Try
        End If
        txt = Nothing
    End Sub

    Public Sub SimulateKeypress(e As KeyEventArgs)
        Logger.Trace("")
        Me.WaitForInputReady(500)
        MyBase.OnKeyDown(e)
        If e.KeyCode >= &H20 And e.KeyCode <= &H7E Then 'space thru tilde
            MyBase.OnKeyPress(New KeyPressEventArgs(ChrW(e.KeyCode)))
        End If
        MyBase.OnKeyUp(e)

        Logger.Debug("Calling DoEvents()...")
        Application.DoEvents()
        Logger.Debug("Returned from DoEvents()")

    End Sub

    Public Sub SendKeys(Text As String)
        SendKeys(Text, Emulator.Screen.Row, Emulator.Screen.Column)
    End Sub
    Public Sub SendKeys(Text As String, Row As Integer, Column As Integer)
        Logger.Debug("'" & Text & "'==>(" & Row.ToString & "," & Column.ToString & ")")

        Dim fidx As Integer = Emulator.Screen.FieldIndexOfAddress(Row, Column)

        'Allow mnemonics, but only alone
        Dim txt As String = Text.Trim
        If txt.Length > 2 Then '[] 
            If txt.Substring(0, 1) = "[" Then
                If txt.Substring(txt.Length - 1, 1) = "]" Then
                    Dim mnemonic As String = txt.Substring(1, txt.Length - 2)
                    Dim e As KeyEventArgs = Nothing
                    Select Case mnemonic.ToUpper
                        Case "ENTER"
                            e = New KeyEventArgs(Keys.Enter)
                        Case "RESET"
                            e = New KeyEventArgs(Keys.ControlKey)
                            'Case "PF1" To "PF24" 'doesn't work because it uses a string comparison, sorting PF3 after PF24.
                        Case "PF1", "PF2", "PF3", "PF4", "PF5", "PF6", "PF7", "PF8", "PF9", "PF10", "PF11", "PF12", _
                             "PF13", "PF14", "PF15", "PF16", "PF17", "PF18", "PF19", "PF20", "PF21", "PF22", "PF23", "PF24"
                            Dim n As Integer
                            If Integer.TryParse(mnemonic.Substring(2), n) Then
                                If n > 0 And n < 25 Then
                                    If n > 12 Then
                                        e = New KeyEventArgs((Keys.F1 + (n - 12)) Or Keys.Shift)
                                    Else
                                        e = New KeyEventArgs(Keys.F1 + (n - 1))
                                    End If
                                End If
                            End If
                            If e Is Nothing Then
                                Logger.Error("Failed to parse [PFxx] mnemonic from string '" & Text & "'")
                            End If
                        Case "ERASE EOF"
                            e = New KeyEventArgs(Keys.End)
                        Case Else
                            Logger.Warn("Unimplemented mnemonic: '" & Text & "'")
                    End Select

                    If e IsNot Nothing Then
                        Me.SimulateKeypress(e)
                        If Not e.Handled Then
                            If fidx > -1 Then
                                InputFields(fidx).SimulateKeypress(e)
                            End If
                        End If
                    End If
                    Exit Sub
                End If
            End If
        End If

        'XXX this effectively overwrites the entire field.  Usually that's what we want, but it's not correct.
        If fidx > -1 Then
            Dim offs As Integer = Row - Emulator.Screen.Fields(fidx).Location.Row
            Dim s As New String(" ", offs) 'pad left
            s += Text
            If s.Length > Emulator.Screen.Fields(fidx).Location.Length Then s = s.Substring(0, Emulator.Screen.Fields(fidx).Location.Length) 'truncate if needed
            'InputFields(fidx).Text = s 'This works but doesn't honor any special formatting flags on the field
            Dim ss As String = Nothing
            For i As Integer = 0 To s.Length - 1
                Dim e As New KeyPressEventArgs(s(i))
                Me.frmEmulatorView_KeyPress(InputFields(fidx), e)
                Me.EmulatorField_KeyPress(InputFields(fidx), e)
                If Not e.Handled Then
                    'character may have been modified by event handlers during keypress event handlers above.
                    ss += e.KeyChar
                End If
            Next

            'Me.SuppressCursorPositionChanges = True
            'Dim OriginalText As String = InputFields(fidx).Text
            InputFields(fidx).Text = ss
            'Me.SuppressCursorPositionChanges = False
            'If ss <> OriginalText Then Me.EmulatorField_TextChanged(InputFields(fidx), New EventArgs)

        End If
    End Sub

    Private Sub frmEmulatorView_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        'For fields this fires _before_ the field's own KeyDown event.
        Logger.Trace("")

        If Not Emulator.Keyboard.Locked Then

            PopErrorLine()

            'Submit keypress events to the old code used by Client Access
            Dim PassItOn As Boolean
            Select Case e.KeyCode
                Case Keys.F1 To Keys.F24
                    Dim k As Keys = e.KeyCode
                    If (k < Keys.F13) AndAlso e.Shift Then k += 12
                    Dim s As String = "[P" & k.ToString & "]"
                    ProcessKeypress(CInt(Me.Tag), "M", s, PassItOn)
                Case Else
                    PassItOn = True
            End Select
            If Not PassItOn Then
                e.SuppressKeyPress = True
                e.Handled = True
                Exit Sub
            End If

            Select Case e.KeyCode
                'Case Keys.Insert
                '    If Not SuppressInsertKeyEvents Then
                '        SuppressInsertKeyEvents = True
                '        Emulator.Screen.Insert = Not Emulator.Screen.Insert
                '        SuppressInsertKeyEvents = False
                '    End If
                '    'e.Handled = True
                '    '    e.SuppressKeyPress = True
                Case Keys.Escape

                    If Emulator.Screen.PopupKeyOfAddress(Emulator.Screen.Row, Emulator.Screen.Column) <> Nothing Then
                        'XXX this is a lazy bug workaround.  To fix it right, remove this code and figure out how to serialize popups in SAVE_SCREEN.
                        MsgBox("Pressing ESC from a popup window usually ends badly.  You should probably press F12 or F3 instead.")
                    Else
                        Select Case e.Modifiers
                            Case Keys.Shift
                                'System Request.  User types up to 78 characters and it's sent raw to the AS400.
                                'SC30-3533-04 pg. 15.1-1.

                                'XXX should display a line for the user to enter up to 78 characters to be sent to the AS400.
                                'Dim b() As Byte = System.Text.Encoding.Default.GetBytes("Something the user typed")
                                'b = IBM5250.Emulator.UTF8_To_EBCDIC(b)
                                Dim b() As Byte = New Byte() {}
                                SendBytes(b, IBM5250.TN5250.OpCodes.None, IBM5250.TN5250.Flag.SRQ) 'system request

                            Case Keys.None
                                'Attention key.
                                SendBytes(New Byte() {}, IBM5250.TN5250.OpCodes.None, IBM5250.TN5250.Flag.ATN)

                        End Select
                    End If
                Case Keys.Enter, Keys.PageDown, Keys.PageUp, Keys.F1 To Keys.F12
                    If Emulator.Screen.Read.Pending Then
                        'XXX need to also handle READ_INPUT_FIELDS, READ_MDT_ALTERNATE, READ_IMMEDIATE, READ_IMMEDIATE_ALTERNATE.

                        If Emulator.Screen.Header.Starting_Field_For_Reads > 0 Then
                            'XXX need to handle resequencing of fields if screen.header.starting_field_for_reads is > 0.  Sequence will be in the FCW for each field.
                            Me.OnDataStreamError(IBM5250.Emulator.NegativeResponse.Format_Table_Resequencing_Error)
                        End If

                        If Emulator.Screen.Read.Command = IBM5250.Emulator.Command.READ_MDT_FIELDS Then
                            Dim b(2) As Byte
                            b(0) = Emulator.Screen.Row
                            b(1) = Emulator.Screen.Column
                            Select Case e.KeyCode
                                Case Keys.Enter
                                    b(2) = IBM5250.Emulator.AID.Enter
                                Case Keys.PageDown
                                    b(2) = IBM5250.Emulator.AID.RollUp
                                Case Keys.PageUp
                                    b(2) = IBM5250.Emulator.AID.RollDown
                                Case Keys.F1 To Keys.F12 'XXX check if these are masked in SOH Order
                                    If e.Shift Then
                                        'AID.PF13 - AID.PF24 are 65 more than Keys.F1 - Keys.F12
                                        b(2) = e.KeyCode + 65
                                    Else
                                        'AID.PF1 - AID.PF12 are 63 less than Keys.F1 - Keys.F12
                                        b(2) = e.KeyCode - 63
                                    End If
                            End Select
                            If Not Emulator.Screen.Header.Inhibited_AID_Codes.Contains(b(2)) Then
                                For i As Integer = 0 To Emulator.Screen.Fields.Length - 1
                                    If Emulator.Screen.Fields(i).Allocated Then
                                        If Emulator.Screen.Fields(i).Flags.Modified Then
                                            'XXX should read from the screenbuffer instead
                                            Dim FieldStart As Integer = b.Length
                                            Dim NewMax As Integer = b.Length - 1 'set to existing max
                                            NewMax += 1 'SBA byte
                                            NewMax += 2 'Row & Column

                                            Dim s As String = Emulator.Screen.Fields(i).Text
                                            Dim DoNegativeZoneConversion As Boolean = False
                                            If Emulator.Screen.Fields(i).Flags.Shift_Edit_Spec = IBM5250.Emulator.EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.SignedNumeric Then
                                                If (s.Length > 0) AndAlso (s.Substring(s.Length - 1) = "-") Then
                                                    DoNegativeZoneConversion = True
                                                    s = s.Substring(0, s.Length - 1) 'remove the "-"
                                                End If
                                            End If

                                            NewMax += s.Length
                                            ReDim Preserve b(NewMax)
                                            b(FieldStart) = IBM5250.Emulator.EmulatorScreen.WTD_Order.Set_Buffer_Address
                                            b(FieldStart + 1) = Emulator.Screen.Fields(i).Location.Row
                                            b(FieldStart + 2) = Emulator.Screen.Fields(i).Location.Column

                                            Dim c() As Byte = System.Text.Encoding.UTF8.GetBytes(s)
                                            c = Emulator.UTF8_To_EBCDIC(c)

                                            If (c.Length > 0) And DoNegativeZoneConversion Then
                                                c(c.Length - 1) = c(c.Length - 1) - &H20 'change from Fx to Dx to indicate negative number
                                            End If

                                            Array.Copy(c, 0, b, FieldStart + 3, c.Length)
                                        End If
                                    Else
                                        Exit For
                                    End If
                                Next
                            End If
                            SendBytes(b, IBM5250.TN5250.OpCodes.PutOrGet)
                            Emulator.Screen.Read.Pending = False
                            e.Handled = True
                            e.SuppressKeyPress = True
                        Else

                            'XXX

                        End If
                    Else

                        'XXX

                    End If

            End Select

        Else
            Select Case e.KeyCode
                Case Keys.ControlKey
                    'restore error line and unlock keyboard
                    Emulator.Keyboard.State = IBM5250.Emulator.EmulatorKeyboard.Keyboard_State.Normal_Unlocked
                    'Emulator.Screen.ErrorText = Emulator.Screen.PriorErrorText
                    'XXX PopStatus
                Case Else
            End Select
        End If
    End Sub

    Private Sub frmEmulatorView_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        Logger.Trace("")
        'This is to catch the case where fields were created while the a modifier key was pressed, like when a user uses F13-F24 to switch screens.
        'In that case the insert mode will not have been set correctly on the textbox.
        If e.Modifiers <> Keys.None Then
            For Each txt As EmulatorTextBox In Me.InputFields
                txt.Insert = Emulator.Keyboard.Insert
            Next
        End If

        ''For some reason we don't get a KeyDown event for the PrtScn key, so handle it here.
        'If e.Modifiers = Keys.None Then
        '    If e.KeyCode = Keys.PrintScreen Then
        '        Me.PrintScreen(False, False)
        '        e.Handled = True
        '    End If
        'End If
    End Sub

    Private Sub frmEmulatorView_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs)
        'For fields this fires _before_ the field's own KeyPress event.
        Logger.Trace("")
    End Sub

    Private Sub Control_PreviewKeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.PreviewKeyDownEventArgs)
        Logger.Debug("KeyCode=" & e.KeyCode.ToString & ", Control=" & e.Control.ToString)
        Select Case e.KeyCode
            Case Keys.PageDown
                e.IsInputKey = True
            Case Keys.PageUp
                e.IsInputKey = True
            Case Keys.C, Keys.V, Keys.Z 'Allow cut, paste, undo with the usual keys
                If e.Control Then
                    e.IsInputKey = True
                End If
            Case Else
        End Select
    End Sub

    Private Sub EmulatorField_Enter(ByVal sender As Object, ByVal e As EventArgs)
        Logger.Trace("")

        'Dim txt As EmulatorTextBox = DirectCast(sender, TN5250.TN5250_TextBox)
        sender.Insert = Emulator.Keyboard.Insert
        UpdateCoordinates(sender)

        If Me.PriorFieldIndex > -1 Then
            If Me.PriorFieldIndex <> sender.Index Then
                Try
                    Me.UpdateFieldAttribute(Me.PriorFieldIndex, True)
                    sender.Focus() 'changing the border style of the prior field will make it steal the focus
                    Me.ApplyHighlightFCW(sender.Index, True)
                Catch ex As Exception

                End Try
            End If
        End If

        If Me.InputReady Then Me.ProcessScreenMatch(Me.MatchedScreenDescription)

    End Sub
    Private Sub EmulatorField_Leave(ByVal sender As Object, ByVal e As EventArgs)
        Logger.Trace("")

        'Me.UpdateFieldAttribute(sender.Index) 'this seems to prevent proper deletion of the field when using PGUP/PGDN (at least).
        Me.PriorFieldIndex = sender.Index
    End Sub

    Private Sub UpdateCoordinates(ByVal sender As Object)
        Logger.Trace("")

        'If sender.Focused Then
        If Not Me.SuppressCursorPositionChanges Then
            Emulator.Screen.Row = Emulator.Screen.Fields(sender.Index).Location.Row
            Emulator.Screen.Column = Emulator.Screen.Fields(sender.Index).Location.Column + sender.SelectionStart
            ' End If

            ''need to track locally so we can return to the right field after a popup closes.
            'Me.Row = Emulator.Screen.Row
            'Me.Column = Emulator.Screen.Column

            Me.ToolStripStatusLabelCoordinates.Text = Emulator.Screen.Row.ToString & "," & Emulator.Screen.Column.ToString

            '***** Don't do this here.  UpdateCoordinates() is called from RichTextBox.SelectionChanged(), which is
            '      is fired before RichTextBox.TextChanged().  If we do screen matching here we miss the last character
            '      typed by the user, which has dire consequences for zip codes (at least).
            'If Me.InputReady Then Me.ProcessScreenMatch(Me.MatchedScreenDescription)
            '*****

        End If
    End Sub

    Private Sub PanelGreenScreen_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs)
        Try
            Me.DrawStrings(e)
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub

    Private Function MatchedScreenDescription() As Integer
        'Dim MethodName As String = Me.Name & "(" & Me.Tag & "):" & System.Reflection.MethodBase.GetCurrentMethod().Name

        'For i As Integer = 0 To CurrentSettings.Emulator.ScreenDescs.Length - 1
        '    Dim Matched As Boolean = True
        '    Dim j As Integer
        '    For j = 0 To CurrentSettings.Emulator.ScreenDescs(i).Elements.Length - 1
        '        Select Case CurrentSettings.Emulator.ScreenDescs(i).Elements(j).Type
        '            Case ScreenDescTypeEnum.CursorPosition
        '                If CurrentSettings.Emulator.ScreenDescs(i).Elements(j).Row <> Me.Emulator.Screen.Row Then
        '                    Matched = False
        '                Else
        '                    If CurrentSettings.Emulator.ScreenDescs(i).Elements(j).Col <> Me.Emulator.Screen.Column Then
        '                        Matched = False
        '                    End If
        '                End If
        '            Case ScreenDescTypeEnum.Text
        '                If CurrentSettings.Emulator.ScreenDescs(i).Elements(j).Text <> Me.Emulator.Screen.GetText( _
        '                    CurrentSettings.Emulator.ScreenDescs(i).Elements(j).Row, _
        '                    CurrentSettings.Emulator.ScreenDescs(i).Elements(j).Col, _
        '                    CurrentSettings.Emulator.ScreenDescs(i).Elements(j).Text.Length) Then
        '                    Matched = False
        '                End If
        '            Case Else
        '                DebugWrite(MethodName, DebugLevel.Warn, False, "Unsupported screen description type: '" & CurrentSettings.Emulator.ScreenDescs(i).Elements(j).Type.ToString & "'")
        '        End Select
        '        If Not Matched Then Exit For
        '    Next
        '    If Matched Then Return i
        'Next

        Return -1
    End Function

    Private Delegate Sub ProcessScreenMatchDelegate(ByVal ScreenDescriptionIndex As Integer)
    Private dProcessScreenMatch = New ProcessScreenMatchDelegate(AddressOf ProcessScreenMatch)
    Private Sub ProcessScreenMatch(ScreenDescriptionIndex As Integer)
        'Dim MethodName As String = Me.Name & "(" & Me.Tag & ")" & ":" & System.Reflection.MethodBase.GetCurrentMethod().Name
        'If ScreenDescriptionIndex < 0 Then Exit Sub
        'DebugWrite(MethodName, DebugLevel.Debug, False, "Matched screen: '" & CurrentSettings.Emulator.ScreenDescs(ScreenDescriptionIndex).Name & "'")
        'Dim j As Integer = ScreenDescriptionIndex
        'Select Case CurrentSettings.Emulator.ScreenDescs(j).Name.ToUpper
        '    '    Case "CUSTOMERADDED"
        '    '        If CurrentSettings.ChexSystems.AutoRun Then
        '    '            DebugWrite(MethodName, DebugLevel.Debug, False, "ChexSystems autorun is enabled; running...")
        '    '            Dim CISNo As String = Nothing
        '    '            Try
        '    '                For k As Integer = 0 To CurrentSettings.Emulator.ScreenDescs(j).Elements.GetUpperBound(0)
        '    '                    If CurrentSettings.Emulator.ScreenDescs(j).Elements(k).Name.ToUpper = "RESPONSETEXT" Then
        '    '                        Dim row As Integer = 0
        '    '                        Dim col As Integer = 0
        '    '                        Dim len As Integer = 0
        '    '                        row = CurrentSettings.Emulator.ScreenDescs(j).Elements(k).DataRow
        '    '                        col = CurrentSettings.Emulator.ScreenDescs(j).Elements(k).DataCol
        '    '                        len = CurrentSettings.Emulator.ScreenDescs(j).Elements(k).DataLen
        '    '                        If row > 0 AndAlso col > 0 AndAlso len > 0 Then
        '    '                            CISNo = Me.Emulator.Screen.GetText(row, col, len).ToString.Trim
        '    '                        End If
        '    '                        Exit For
        '    '                    End If
        '    '                Next
        '    '            Catch ex As Exception
        '    '                DebugWrite(MethodName, DebugLevel.Error_, False, "Error scraping CIS number: " & ex.Message)
        '    '            End Try
        '    '            If CISNo IsNot Nothing Then
        '    '                'Make sure we only do this once...
        '    '                Static Dim AddedCustomers() As String
        '    '                If AddedCustomers Is Nothing Then ReDim AddedCustomers(-1)
        '    '                For z As Integer = 0 To AddedCustomers.GetUpperBound(0)
        '    '                    If AddedCustomers(z) = CISNo Then
        '    '                        Exit Sub
        '    '                    End If
        '    '                Next
        '    '                'If MsgBox("Do you want to run QualiFile for this customer?", MsgBoxStyle.YesNo + MsgBoxStyle.Question + MsgBoxStyle.ApplicationModal) = MsgBoxResult.Yes Then
        '    '                ReDim AddedCustomers(AddedCustomers.GetUpperBound(0) + 1)
        '    '                AddedCustomers(AddedCustomers.GetUpperBound(0)) = CISNo
        '    '                '
        '    '                Try
        '    '                    DebugWrite(MethodName, DebugLevel.Audit, False, "Launching Qualifile query for CISNO '" & CISNo & "'")
        '    '                    QueryCustomer(CISNo)
        '    '                Catch ex As Exception
        '    '                    DebugWrite(MethodName, DebugLevel.Error_, True, ex.Message)
        '    '                End Try
        '    '                'End If
        '    '            End If
        '    '        Else
        '    '            DebugWrite(MethodName, DebugLevel.Debug, False, "ChexSystems autorun is disabled; skipping")
        '    '        End If
        '    '    Case "ACCOUNTADDED"
        '    '        'DebugWrite(MethodName, DebugLevel.Debug, False, "Account added")
        '    '        Dim AcctNo As String = Nothing
        '    '        Try
        '    '            For k As Integer = 0 To CurrentSettings.Emulator.ScreenDescs(j).Elements.GetUpperBound(0)
        '    '                If CurrentSettings.Emulator.ScreenDescs(j).Elements(k).Name.ToUpper = "RESPONSETEXT" Then
        '    '                    Dim row As Integer = 0
        '    '                    Dim col As Integer = 0
        '    '                    Dim len As Integer = 0
        '    '                    row = CurrentSettings.Emulator.ScreenDescs(j).Elements(k).DataRow
        '    '                    col = CurrentSettings.Emulator.ScreenDescs(j).Elements(k).DataCol
        '    '                    len = CurrentSettings.Emulator.ScreenDescs(j).Elements(k).DataLen
        '    '                    If row > 0 AndAlso col > 0 AndAlso len > 0 Then
        '    '                        AcctNo = Me.Emulator.Screen.GetText(row, col, len).ToString.Trim
        '    '                    End If
        '    '                    Exit For
        '    '                End If
        '    '            Next
        '    '        Catch ex As Exception
        '    '            DebugWrite(MethodName, DebugLevel.Error_, False, "Error scraping account number: " & ex.Message)
        '    '        End Try
        '    '        If AcctNo IsNot Nothing Then
        '    '            'Debug.Print("Account added in Client Access window " & i.ToString & ": " & AcctNo)
        '    '            frmMain.ShowNewAccount(AcctNo)
        '    '        End If
        '    '    Case "LOGON"
        '    '        'DebugWrite(MethodName, DebugLevel.Debug, False, "At the logon screen")
        '    '        If CurrentSettings.Emulator.AutoLogonEnabled Then
        '    '            If Not EmulatorView(CInt(Me.Tag)).SentCredentials Then
        '    '                Try
        '    '                    DebugWrite(MethodName, DebugLevel.Audit, False, "Sending credentials")
        '    '                    Dim row As Integer = 0
        '    '                    Dim col As Integer = 0
        '    '                    For k As Integer = 0 To CurrentSettings.Emulator.ScreenDescs(j).Elements.GetUpperBound(0)
        '    '                        If CurrentSettings.Emulator.ScreenDescs(j).Elements(k).Name.ToUpper = "USERNAME" Then
        '    '                            row = CurrentSettings.Emulator.ScreenDescs(j).Elements(k).InputRow
        '    '                            col = CurrentSettings.Emulator.ScreenDescs(j).Elements(k).InputCol
        '    '                            If row > 0 AndAlso col > 0 Then
        '    '                                DebugWrite(MethodName, DebugLevel.Audit, False, "   Username: '" & CurrentSettings.User.Name & "' (" & row & "," & col & ")")
        '    '                                Me.SendKeys(CurrentSettings.User.Name, row, col)
        '    '                            End If
        '    '                            Exit For
        '    '                        End If
        '    '                    Next
        '    '                    For k As Integer = 0 To CurrentSettings.Emulator.ScreenDescs(j).Elements.GetUpperBound(0)
        '    '                        If CurrentSettings.Emulator.ScreenDescs(j).Elements(k).Name.ToUpper = "PASSWORD" Then
        '    '                            row = CurrentSettings.Emulator.ScreenDescs(j).Elements(k).InputRow
        '    '                            col = CurrentSettings.Emulator.ScreenDescs(j).Elements(k).InputCol
        '    '                            If row > 0 AndAlso col > 0 Then
        '    '                                DebugWrite(MethodName, DebugLevel.Audit, False, "   Password: '*****' (" & row & "," & col & ")")
        '    '                                Me.SendKeys(CurrentSettings.User.Password, row, col)
        '    '                            End If
        '    '                            Me.SendKeys("[enter]")
        '    '                            EmulatorView(CInt(Me.Tag)).SentCredentials = True
        '    '                            Exit For
        '    '                        End If
        '    '                    Next
        '    '                Catch ex As Exception
        '    '                    DebugWrite(MethodName, DebugLevel.Error_, False, "Error sending credentials: " & ex.Message)
        '    '                End Try
        '    '            End If
        '    '        End If
        '    '    Case "ADDCISCUSTOMER", "UPDATECISCUSTOMER", "UPDATEALTERNATEADDRESS", "UPDATESEASONALADDRESS"
        '    '        If CurrentSettings.Geocoder.Enabled Then
        '    '            Me.Cursor = Cursors.WaitCursor
        '    '            TryGeocode(CInt(Me.Tag), j)
        '    '            Me.Cursor = Cursors.Default
        '    '        End If
        'End Select

    End Sub

    Public Sub WaitForInputReady(Timeout_ms As Double)
        Logger.Debug("Waiting " & Timeout_ms.ToString & "ms for input to become ready...")

        Dim start As Date = Now
        Dim deadline As Date = start.AddMilliseconds(Timeout_ms)
        Do While ((Me.InputReady = False) Or (Emulator.Keyboard.Locked = True)) And (deadline >= Now)
            Logger.Debug("InputReady=" & Me.InputReady.ToString & ", KeyboardLocked=" & Emulator.Keyboard.Locked.ToString)

            Logger.Debug("Calling DoEvents()...")

            Application.DoEvents()

            Logger.Debug("Returned from DoEvents()")
        Loop
        Logger.Trace("Finished")
    End Sub

    Private Delegate Sub SetInputReadyDelegate(ByVal NewState As Boolean)
    Private dSetInputReady = New SetInputReadyDelegate(AddressOf SetInputReady)
    Private Sub SetInputReady(NewState As Boolean)
        Static Dim PriorState As Boolean
        PriorState = Me.InputReady
        Me.InputReady = NewState
        Logger.Debug(PriorState.ToString & " ==> " & Me.InputReady.ToString)
    End Sub

    Private Sub frmEmulatorView_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown

        'Me.WindowState = FormWindowState.Maximized

        Try
            Dim WindowOffset As Integer = CInt(Me.Tag) * 20
            Me.Location = New Point(WindowOffset, WindowOffset)
        Catch
        End Try

        frmEmulatorView_SizeChanged(sender, e)

        Me.Connect()
    End Sub

    Private Sub frmEmulatorView_SizeChanged(sender As Object, e As System.EventArgs) Handles Me.SizeChanged
        'Debug.Print("SizeChanged()")
        If Emulator IsNot Nothing Then
            Static Dim Running As Boolean
            If (Not Running) Then
                Try
                    Running = True
                    Select Case Me.WindowState
                        Case FormWindowState.Maximized
                            'Me.WindowState = FormWindowState.Normal
                            'Me.Location = Screen.FromPoint(MousePosition).WorkingArea.Location
                            Me.FitToWindow(Screen.FromControl(Me).WorkingArea)
                            Me.Redraw()
                        Case FormWindowState.Normal
                            If Screen.FromControl(Me).WorkingArea.Contains(RectangleToScreen(Me.DisplayRectangle)) Then
                                Me.FitToWindow(Rectangle.Inflate(Me.DisplayRectangle, 20, 20)) 'XXX 20 is arbitrary fudge
                            Else
                                Me.FitToWindow(Screen.FromControl(Me).WorkingArea)
                            End If
                            Me.Redraw()
                    End Select
                Catch ex As Exception
                    'Debug.Print(ex.Message)
                    Logger.Error(ex.Message, ex)
                Finally
                    Running = False
                End Try
            End If
        End If
    End Sub

    Private Sub FitToWindow(ByVal rect As Rectangle)
        'Debug.Print("FitToWindow: In: ScaleFactor=" & Me.ScreenMetrics.ScaleFactor.ToString & ", Me.DisplayRectangle=" & Me.DisplayRectangle.ToString & ", Rect=" & rect.ToString)

        Dim BordersHeight As Integer = Me.Height - Me.PanelGreenScreen.Height
        Dim BordersWidth As Integer = Me.Width - Me.PanelGreenScreen.Width

        Dim MinimumScaleFactor As Single = 0.25F

        Dim sm As New Screen_Metrics

        CalculateScreenMetrics(sm)
        Dim EmulatorAspectRatio As Single = CSng(sm.EmulatorSize.Width + BordersWidth) / CSng(sm.EmulatorSize.Height + BordersHeight)
        Dim WindowAspectRatio As Single = CSng(rect.Width) / CSng(rect.Height)

        'Calculate an approximation of the new size
        If WindowAspectRatio > EmulatorAspectRatio Then 'Height is the limiting dimension
            sm.ScaleFactor = CSng(rect.Height) / CSng(sm.EmulatorSize.Height + BordersHeight)
        Else 'Width is the limiting dimension
            sm.ScaleFactor = CSng(rect.Width) / CSng(sm.EmulatorSize.Width + BordersWidth)
        End If

        'Set a lower limit on scaling
        sm.ScaleFactor = Math.Max(sm.ScaleFactor, MinimumScaleFactor)

        'Fudge the size down until it fits.
        Do While sm.ScaleFactor > MinimumScaleFactor
            CalculateScreenMetrics(sm)
            Dim NewSize As Size
            NewSize.Height = sm.EmulatorSize.Height + BordersHeight
            NewSize.Width = sm.EmulatorSize.Width + BordersWidth
            If NewSize.Height < rect.Height AndAlso NewSize.Width < rect.Width Then Exit Do
            sm.ScaleFactor -= 0.01F
        Loop
        Me.ScreenMetrics.ScaleFactor = sm.ScaleFactor
        CalculateScreenMetrics(Me.ScreenMetrics)

        'Debug.Print("Before: Me.Height=" & Me.Height.ToString & ", Me.Width=" & Me.Width.ToString)
        'Me.Height = Me.ScreenMetrics.EmulatorSize.Height + BordersHeight
        'Me.Width = Me.ScreenMetrics.EmulatorSize.Width + BordersWidth
        'Debug.Print("After: Me.Height=" & Me.Height.ToString & ", Me.Width=" & Me.Width.ToString)

        'resize popup forms to fit new text size
        For Each kvp As KeyValuePair(Of String, IBM5250.Emulator.EmulatorScreen.EmulatorPopup) In Emulator.Screen.PopUps
            Dim frm As frmEmulatorPopup = GetOpenPopupForm(kvp.Key)
            If frm IsNot Nothing Then
                Dim popBordersHeight As Integer = frm.Height - frm.Panel1.Height
                Dim popBordersWidth As Integer = frm.Width - frm.Panel1.Width
                frm.Height = (Emulator.Screen.PopUps(kvp.Key).Rows * Me.ScreenMetrics.ControlHeight) + popBordersHeight
                frm.Width = ((Emulator.Screen.PopUps(kvp.Key).Columns + 2) * Me.ScreenMetrics.Font_Character_Width) + popBordersWidth
                frm = Nothing
            End If
        Next

        'Debug.Print("FitToWindow: Out: ScaleFactor=" & Me.ScreenMetrics.ScaleFactor.ToString & ", Me.DisplayRectangle=" & Me.DisplayRectangle.ToString & ", Rect=" & rect.ToString)

    End Sub

    'Protected Overrides Function ProcessCmdKey(ByRef msg As System.Windows.Forms.Message, keyData As System.Windows.Forms.Keys) As Boolean
    '    If msg.Msg = WinAPI.WM_KEYDOWN Then
    '        If msg.WParam = Keys.PrintScreen Then
    '            'Me.PrintPanel(Me.PanelGreenScreen)
    '            Me.PrintScreen(False, False)
    '        End If
    '    End If
    '    Return MyBase.ProcessCmdKey(msg, keyData)
    'End Function

    'Protected Overrides Function ProcessKeyEventArgs(ByRef msg As System.Windows.Forms.Message) As Boolean
    '    If msg.Msg = WinAPI.WM_KEYUP Then
    '        If msg.WParam = Keys.PrintScreen Then
    '            'Me.PrintPanel(Me.PanelGreenScreen)
    '            Me.PrintScreen(False, False)
    '        End If
    '    End If
    '    Return MyBase.ProcessKeyEventArgs(msg)
    'End Function

    Dim ScreenCap As Bitmap

    Private Sub PrintDocument1_PrintPage(sender As Object, e As System.Drawing.Printing.PrintPageEventArgs) Handles PrintDocument1.PrintPage
        'e.Graphics.DrawImage(ScreenCap, 0, 0, PrintDocument1.DefaultPageSettings.PaperSize.Height, PrintDocument1.DefaultPageSettings.PaperSize.Width)

        Dim WidthRatio As Single = e.MarginBounds.Width / Me.ScreenMetrics.EmulatorSize.Width
        Dim HeightRatio As Single = e.MarginBounds.Height / Me.ScreenMetrics.EmulatorSize.Height
        Dim LimitingRatio As Single = Math.Min(WidthRatio, HeightRatio)
        e.Graphics.PageScale = LimitingRatio

        e.Graphics.Clear(Color.White)

        Me.DrawStringsToGraphicsObject(e.Graphics, Emulator.Screen.Strings, True)
        Me.DrawStringsToGraphicsObject(e.Graphics, Emulator.Screen.Fields, True)

    End Sub

    Private Sub PrintScreen(ByVal ShowDialog As Boolean, ByVal Preview As Boolean)
        Dim LogPrefix As String = "(Tag=" & Me.Tag & ") "
        Try
            Dim myPrintDialog As PrintDialog = New PrintDialog()
            'Dim values As System.Drawing.Printing.PrinterSettings
            'values = myPrintDialog.PrinterSettings

            myPrintDialog.Document = PrintDocument1
            PrintDocument1.OriginAtMargins = True

            Select Case Me.ToolStripComboBoxPrintOrientation.SelectedItem
                Case PrintOrientation.Portrait.ToString
                    PrintDocument1.DefaultPageSettings.Landscape = False
                Case PrintOrientation.Landscape.ToString
                    PrintDocument1.DefaultPageSettings.Landscape = True
                Case PrintOrientation.Auto.ToString
                    'Dim AspectRatio As Single = Me.ScreenMetrics.EmulatorSize.Width / Me.ScreenMetrics.EmulatorSize.Height
                    'PrintDocument1.DefaultPageSettings.Landscape = (AspectRatio > 1.4)
                    PrintDocument1.DefaultPageSettings.Landscape = Me.Emulator.Screen.Columns > 80
            End Select

            If Preview Then
                Dim PreviewDialog As New PrintPreviewDialog
                PreviewDialog.Document = PrintDocument1
                PreviewDialog.Height = Me.Height
                PreviewDialog.Width = Me.Width
                PreviewDialog.PrintPreviewControl.Zoom = 1
                PreviewDialog.ShowDialog()
            Else
                Dim Result As DialogResult
                If ShowDialog Then Result = myPrintDialog.ShowDialog Else Result = DialogResult.OK
                If Result = DialogResult.OK Then PrintDocument1.Print()
            End If
        Catch ex As Exception
            Dim msg As String = "Error printing screen: " & ex.Message
            Logger.Error(LogPrefix & msg, ex)
            MsgBox(msg, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub

    'Private Sub PrintPanel(ByRef pnl As Panel)
    '    Dim MethodName As String = Me.Name & "(" & Me.Tag & ")" & ":" & System.Reflection.MethodBase.GetCurrentMethod().Name
    '    Dim myPrintDialog As PrintDialog = New PrintDialog()
    '    Dim PanelGraphics As Graphics = PanelGreenScreen.CreateGraphics()
    '    ScreenCap = New System.Drawing.Bitmap(pnl.Width, pnl.Height, PanelGraphics)
    '    Dim BackColor As Color = pnl.BackColor
    '    pnl.BackColor = Color.White
    '    pnl.DrawToBitmap(ScreenCap, pnl.ClientRectangle)
    '    pnl.BackColor = BackColor
    '    'Me.ConvertToBW(ScreenCap)

    '    If myPrintDialog.ShowDialog() = DialogResult.OK Then
    '        Dim values As System.Drawing.Printing.PrinterSettings
    '        values = myPrintDialog.PrinterSettings
    '        myPrintDialog.Document = PrintDocument1
    '        PrintDocument1.DefaultPageSettings.Landscape = True
    '        PrintDocument1.Print()
    '    End If
    '    PrintDocument1.Dispose()
    'End Sub

    'Private Function ConvertToBW(ByVal Original As Bitmap) As Bitmap
    '    Dim MethodName As String = Me.Name & "(" & Me.Tag & ")" & ":" & System.Reflection.MethodBase.GetCurrentMethod().Name
    '    Dim Source As Bitmap = Nothing
    '    'If original bitmap is not already in 32 BPP, ARGB format, then convert
    '    If Original.PixelFormat <> System.Drawing.Imaging.PixelFormat.Format32bppArgb Then
    '        Source = New Bitmap(Original.Width, Original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
    '        Source.SetResolution(Original.HorizontalResolution, Original.VerticalResolution)
    '        Using g As Graphics = Graphics.FromImage(Source)
    '            g.DrawImageUnscaled(Original, 0, 0)
    '        End Using
    '    Else
    '        Source = Original
    '    End If

    '    'Lock source bitmap in memory
    '    Dim sourceData As System.Drawing.Imaging.BitmapData = Source.LockBits(New Rectangle(0, 0, Source.Width, Source.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb)

    '    'Copy image data to binary array
    '    Dim imageSize As Integer = sourceData.Stride * sourceData.Height
    '    Dim sourceBuffer(imageSize - 1) As Byte

    '    System.Runtime.InteropServices.Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, imageSize)

    '    'Unlock source bitmap
    '    Source.UnlockBits(sourceData)

    '    'Create destination bitmap
    '    Dim Destination As Bitmap = New Bitmap(Source.Width, Source.Height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed)

    '    'Lock destination bitmap in memory
    '    Dim destinationData As System.Drawing.Imaging.BitmapData = Destination.LockBits(New Rectangle(0, 0, Destination.Width, Destination.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed)

    '    'Create destination buffer
    '    imageSize = destinationData.Stride * destinationData.Height
    '    Dim destinationBuffer(imageSize - 1) As Byte

    '    Dim sourceIndex As Integer = 0
    '    Dim destinationIndex As Integer = 0
    '    Dim pixelTotal As Integer = 0
    '    Dim destinationValue As Byte = 0
    '    Dim pixelValue As Integer = 128
    '    Dim height As Integer = Source.Height
    '    Dim width As Integer = Source.Width
    '    Dim threshold As Integer = 500


    '    'Iterate lines
    '    For y As Integer = 0 To height - 1

    '        sourceIndex = y * sourceData.Stride
    '        destinationIndex = y * destinationData.Stride
    '        destinationValue = 0
    '        pixelValue = 128

    '        'Iterate pixels
    '        For x As Integer = 0 To width - 1

    '            'Compute pixel brightness (i.e. total of Red, Green, and Blue values)
    '            pixelTotal = CInt(sourceBuffer(sourceIndex + 1)) + CInt(sourceBuffer(sourceIndex + 2)) + CInt(sourceBuffer(sourceIndex + 3))
    '            If (pixelTotal > threshold) Then
    '                destinationValue += CByte(pixelValue)
    '            End If
    '            If pixelValue = 1 Then
    '                destinationBuffer(destinationIndex) = destinationValue
    '                destinationIndex += 1
    '                destinationValue = 0
    '                pixelValue = 128
    '            Else
    '                pixelValue >>= 1
    '            End If
    '            sourceIndex += 4
    '        Next
    '        If pixelValue <> 128 Then
    '            destinationBuffer(destinationIndex) = destinationValue
    '        End If
    '    Next

    '    'Copy binary image data to destination bitmap
    '    System.Runtime.InteropServices.Marshal.Copy(destinationBuffer, 0, destinationData.Scan0, imageSize)

    '    'Unlock destination bitmap
    '    Destination.UnlockBits(destinationData)

    '    Return Destination
    'End Function

    Public Sub UpdateScreenMetrics()
        CalculateScreenMetrics(Me.ScreenMetrics)
    End Sub

    Private Sub CalculateScreenMetrics(ByRef sm As Screen_Metrics)
        Try
            With sm
                'Debug.Print(MethodName & ": In: ScaleFactor=" & .ScaleFactor.ToString & ", FontCharWidth=" & .Font_Character_Width.ToString & ", EmulatorSize=" & .EmulatorSize.ToString)
                If .ScaleFactor <= 0.0F Then .ScaleFactor = 1.0F

                Dim FontFam As FontFamily = FontFamily.GenericMonospace
                If Me.Tag IsNot Nothing Then
                    If EmulatorView(CInt(Me.Tag)).Settings.FontFamily IsNot Nothing Then
                        FontFam = EmulatorView(CInt(Me.Tag)).Settings.FontFamily
                    End If
                End If

                .Font_Regular = New Drawing.Font(FontFam, 20 * .ScaleFactor, FontStyle.Regular, GraphicsUnit.Pixel)
                .Font_Bold = New Drawing.Font(FontFam, 20 * .ScaleFactor, FontStyle.Bold, GraphicsUnit.Pixel)
                .Font_Underscore = New Drawing.Font(FontFam, 20 * .ScaleFactor, FontStyle.Underline, GraphicsUnit.Pixel)

                'If .Font_Regular.Name <> "Consolas" Then 'Windows XP doesn't come with Consolas, so fall back to Lucida Console
                '    .Font_Regular = New Drawing.Font("Lucida Console", 20 * .ScaleFactor, FontStyle.Regular, GraphicsUnit.Pixel)
                '    .Font_Bold = New Drawing.Font("Lucida Console", 20 * .ScaleFactor, FontStyle.Bold, GraphicsUnit.Pixel)
                '    .Font_Underscore = New Drawing.Font("Lucida Console", 20 * .ScaleFactor, FontStyle.Underline, GraphicsUnit.Pixel)
                'End If

                Dim lb As New Label
                lb.Font = .Font_Bold
                Dim SampleString As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"

                Dim TextSizeF As SizeF = GetTextSize(SampleString, lb)
                .Font_Character_Width = TextSizeF.Width / CSng(SampleString.Length)

                Dim Character_Width_Fudge As Single = 0.333F * .ScaleFactor 'MeasureString() in GetTextSize() is inaccurate.
                .Font_Character_Width += Character_Width_Fudge

                lb.Height = TextSizeF.Height

                Dim Character_Height_Fudge As Integer = 4 * .ScaleFactor
                .ControlHeight = lb.Height + Character_Height_Fudge

                Dim Screen_Width_Fudge As Integer = 1 '= 2
                'If Emulator.Screen.Columns > 80 Then Screen_Width_Fudge += 2

                .EmulatorSize = New Size(CInt(Math.Round((Emulator.Screen.Columns + Screen_Width_Fudge) * .Font_Character_Width, 0, MidpointRounding.AwayFromZero)), Emulator.Screen.Rows * .ControlHeight)
                '.EmulatorSize = New Size(Emulator.Screen.Columns * .Font_Character_Width, Emulator.Screen.Rows * .ControlHeight)
                lb.Dispose()
            End With
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub

    Private Sub ToolStripButtonConnect_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButtonConnect.Click
        Me.ToggleTelnetConnection()
    End Sub

    Private Sub Disconnect(Reason As String)
        Logger.Trace("")
        Telnet.Disconnect(Reason)
        If Telnet.Connected Then
            Me.ToolStripMenuItemConnect.Text = "Disconnect"
        Else
            Me.ToolStripMenuItemConnect.Text = "Connect"
        End If
    End Sub

    Private Sub Connect()
        Logger.Trace("")
        Try
            PushErrorLine("Connecting...")
            Me.Cursor = Cursors.WaitCursor
            Application.DoEvents() 'let the cursor draw
            If String.IsNullOrWhiteSpace(EmulatorView(CInt(Me.Tag)).Settings.HostAddress) OrElse (EmulatorView(CInt(Me.Tag)).Settings.HostPort < 1) Then
                Dim s As String = "You must provide a hostname or IP address and a port number before connecting."
                'MsgBox(s, MsgBoxStyle.Critical, "Connection Error")
                Throw New Exception(s)
            Else
                If Telnet.IBM_AuthEncryptionMethod = Global.Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.Kerberos Then
                    Dim Ticket As String = GetKerberosTicket()
                    If String.IsNullOrWhiteSpace(Ticket) Then
                        If Telnet.USERVARs.ContainsKey("IBMTICKET") Then
                            Telnet.USERVARs.Remove("IBMTICKET")
                        End If
                    Else
                        If Telnet.USERVARs.ContainsKey("IBMTICKET") Then
                            Telnet.USERVARs("IBMTICKET") = Ticket
                        Else
                            Telnet.USERVARs.Add("IBMTICKET", Ticket)
                        End If
                    End If
                End If
                Telnet.Connect(EmulatorView(CInt(Me.Tag)).Settings.HostAddress, EmulatorView(CInt(Me.Tag)).Settings.HostPort)
            End If
        Catch ex As Exception
            Me.PushErrorLine("ERROR: " & ex.Message)
        Finally
            Me.Cursor = Cursors.Default
        End Try
        If Telnet.Connected Then
            Me.ToolStripMenuItemConnect.Text = "Disconnect"
        Else
            Me.ToolStripMenuItemConnect.Text = "Connect"
        End If
    End Sub

    Private Sub ToggleTelnetConnection()
        If Telnet.Connected Then
            Disconnect("User request")
        Else
            Connect()
        End If
    End Sub

    Private Sub OnConnectionAttemptCompleted(sender As Object, e As EventArgs) Handles Telnet.ConnectionAttemptCompleted
        Try
            If Telnet.Connected Then
                If Me.InvokeRequired Then
                    Me.Invoke(dPushErrorLine, New Object() {"Connected."})
                Else
                    Me.PushErrorLine("Connected.")
                End If
                Telnet.Receive()
            Else
                If Me.InvokeRequired Then
                    Me.Invoke(dPushErrorLine, New Object() {"ERROR: Failed to connect to host"})
                Else
                    Me.PushErrorLine("ERROR: Failed to connect to host")
                End If
            End If
            If Me.InvokeRequired Then
                Me.Invoke(dSetConnectionStatus, New Object() {Telnet.Connected, Telnet.Secured, Telnet.SecurityProtocolInfo})
            Else
                Me.SetConnectionStatus(Telnet.Connected, Telnet.Secured, Telnet.SecurityProtocolInfo)
            End If
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub

    Public Function MappedColor(ColorOrScreenElement As String) As KnownColor
        Try
            If Me.Tag Is Nothing Then Throw New Exception("Tag is Nothing")
            Return EmulatorView(CInt(Me.Tag)).Settings.ColorMap(ColorOrScreenElement)
        Catch ex As Exception
            Logger.Error("Key='" & ColorOrScreenElement & "':" & ex.Message, ex)
            Return KnownColor.Orange 'XXX
        End Try
    End Function

    Private Sub ToolStripMenuItemConnect_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripMenuItemConnect.Click
        Me.ToggleTelnetConnection()
    End Sub

    Private Sub ToolStripMenuItemColors_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripMenuItemColors.Click
        If Me.Tag IsNot Nothing Then
            Me.Cursor = Cursors.WaitCursor
            Dim f As New frmColorPrefs(Me.Tag, EmulatorView(CInt(Me.Tag)).Settings.ColorMap, "Color Preferences (" & EmulatorView(CInt(Me.Tag)).Settings.HostAddress & ", " & EmulatorView(CInt(Me.Tag)).Settings.StationName & ")")
            f.StartPosition = FormStartPosition.CenterParent
            f.ShowDialog()
            f = Nothing
            Me.Cursor = Cursors.Default
        End If
    End Sub

    Friend Sub Redraw()

        Debug.Print("ReDraw()")

        Me.OnStringsChanged()
        Me.OnFieldsChanged()
    End Sub

    Private Sub ToolStripButton1_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton1.Click
        Me.PrintScreen(False, False)
    End Sub

    Private Sub ToolStripButtonPrintPreview_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButtonPrintPreview.Click
        Me.PrintScreen(False, True)
    End Sub

    Private Sub ToolStripMenuItemPrint_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripMenuItemPrint.Click
        Me.PrintScreen(True, False)
    End Sub

    Private Sub PanelGreenScreen_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles PanelGreenScreen.MouseDown
        'Debug.Print("Name=" & Me.ActiveControl.Name & ", Focused=" & Me.ActiveControl.Focused.ToString)

        If e.Button = Windows.Forms.MouseButtons.Left Then
            Me.MouseDragging = True
            sender.Capture = True
            Me.MouseDragCoords = New MouseDragCoordinates
            With Me.MouseDragCoords
                .xIncrement = Me.ScreenMetrics.Font_Character_Width
                .yIncrement = Me.ScreenMetrics.ControlHeight
                .Origin = Me.MouseDrag_SnapToPoint(sender, e.Location)
                .Destination = New Point(-1, -1)
            End With
        End If

        Try
            If Me.ActiveControl IsNot Nothing Then
                Me.ActiveControl.Focus()
            Else
                Me.PanelGreenScreen.Focus()
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub PanelGreenScreen_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles PanelGreenScreen.MouseMove
        If Me.MouseDragging Then
            With Me.MouseDragCoords
                If .Destination <> New Point(-1, -1) Then
                    'erase previous rectangle
                    ControlPaint.DrawReversibleFrame(sender.RectangleToScreen(.Rectangle), Color.Black, FrameStyle.Dashed)
                End If

                Dim pt As Point = e.Location
                If pt.X > sender.Width Then pt.X = sender.Width
                If pt.X < 0 Then pt.X = 0
                If pt.Y > sender.Height Then pt.Y = sender.Height
                If pt.Y < 0 Then pt.Y = 0

                .Destination = Me.MouseDrag_SnapToPoint(sender, pt)
                .Rectangle.X = Math.Min(.Origin.X, .Destination.X)
                .Rectangle.Y = Math.Min(.Origin.Y, .Destination.Y)
                .Rectangle.Width = Math.Abs(.Origin.X - .Destination.X)
                .Rectangle.Height = Math.Abs(.Origin.Y - .Destination.Y)

                'draw new rectangle
                ControlPaint.DrawReversibleFrame(sender.RectangleToScreen(.Rectangle), Color.Black, FrameStyle.Dashed)
            End With
        End If
    End Sub

    Private Sub PanelGreenScreen_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles PanelGreenScreen.MouseUp
        If e.Button = Windows.Forms.MouseButtons.Left Then
            Me.MouseDragging = False
            sender.Capture = False

            With Me.MouseDragCoords
                'erase final rectangle
                ControlPaint.DrawReversibleFrame(sender.RectangleToScreen(.Rectangle), Color.Black, FrameStyle.Dashed)

                'Debug.Print("Selected rectangle: " & .Rectangle.ToString)
                Dim Row As Integer = (.Rectangle.Top \ Me.ScreenMetrics.ControlHeight) + 1
                Row = Math.Min(Row, Emulator.Screen.Rows)
                Row = Math.Max(Row, 1)
                Dim Col As Integer = (.Rectangle.Left \ Me.ScreenMetrics.Font_Character_Width) + 1
                Col = Math.Min(Col, Emulator.Screen.Columns)
                Col = Math.Max(Col, 1)
                Dim Len As Integer = .Rectangle.Width \ Me.ScreenMetrics.Font_Character_Width
                Len = Math.Min(Len, Emulator.Screen.Columns - Col) '+ 1
                Dim Rows As Integer = (.Rectangle.Bottom \ Me.ScreenMetrics.ControlHeight) + 1 - Row
                Rows = Math.Min(Rows, Emulator.Screen.Rows - Row) ' + 1

                Dim s As String = Nothing
                For i As Integer = Row To Row + Rows - 1
                    If s IsNot Nothing Then s += vbCrLf
                    s += Emulator.Screen.GetText(i, Col, Len, True)
                Next
                'Debug.Print(Rows.ToString & " rows, " & Len.ToString & " columns" & vbCrLf & vbCrLf & s)

                Try
                    '**** WARNING: Clipboard.Clear() will make explorer.exe crash eventually! (May be resolved in Windows 8).
                    'https://bugzilla.mozilla.org/show_bug.cgi?id=518412
                    'http://social.technet.microsoft.com/Forums/windows/en-US/038ee435-4631-4cc4-b429-e9d28d3bb309/clipboardclear-plus-a-right-click-on-the-desktop-causes-crashes-in-explorerexe-and-mmcexe
                    'Use Clipboard.SetText("") instead of Clipboard.Clear()!
                    'If s Is Nothing Then s = ""
                    If s IsNot Nothing Then Clipboard.SetText(s)
                Catch ex As Exception
                    Logger.Error(ex.Message, ex)
                End Try
            End With
        End If
    End Sub

    Private Function MouseDrag_SnapToPoint(sender As Object, ByVal pt As Point) As Point
        Dim yOffs As Integer = pt.Y Mod MouseDragCoords.yIncrement
        pt.Y = (pt.Y \ MouseDragCoords.yIncrement) * MouseDragCoords.yIncrement
        If yOffs > (MouseDragCoords.yIncrement \ 2) Then pt.Y += MouseDragCoords.yIncrement
        Dim hOffs As Integer = pt.X Mod MouseDragCoords.xIncrement
        pt.X = (pt.X \ MouseDragCoords.xIncrement) * MouseDragCoords.xIncrement
        If hOffs > (MouseDragCoords.xIncrement \ 2) Then pt.X += MouseDragCoords.xIncrement
        If pt.Y > sender.Height Then pt.Y -= MouseDragCoords.yIncrement
        If pt.X > sender.Width Then pt.X -= MouseDragCoords.xIncrement
        Return pt
    End Function

    Private Sub ToolStripMenuItemFont_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItemFont.Click
        If Me.Tag IsNot Nothing Then
            Dim f As New frmFontPrefs(Me.Tag, EmulatorView(CInt(Me.Tag)).Settings.FontFamily, ConnectionSettings.EnumFixedPitchFonts, "Font Preferences (" & EmulatorView(CInt(Me.Tag)).Settings.HostAddress & IIf(EmulatorView(CInt(Me.Tag)).Settings.StationName IsNot Nothing, ", " & EmulatorView(CInt(Me.Tag)).Settings.StationName, "") & ")")
            f.StartPosition = FormStartPosition.CenterParent
            If f.ShowDialog() = Windows.Forms.DialogResult.OK Then
                Redraw()
                RaiseEvent FontFamilyChanged(Me.Tag)
            End If
            f = Nothing
        End If
    End Sub

    Private Sub ChangePasswordToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ChangePasswordToolStripMenuItem.Click
        With EmulatorView(CInt(Me.Tag)).Settings
            ChangePassword(.HostAddress, .UseSSL, .UserName, .Password, .TimeoutMS)
        End With
    End Sub

    Private Sub UseAlternateCaretToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UseAlternateCaretToolStripMenuItem.Click
        EmulatorView(CInt(Me.Tag)).Settings.AlternateCaretEnabled = UseAlternateCaretToolStripMenuItem.Checked
        Me.Redraw()
    End Sub

    Private Sub ToolStripStatusLabelSSL_Click(sender As Object, e As EventArgs) Handles ToolStripStatusLabelSSL.Click
        If Telnet.Secured Then
            Try
                System.Security.Cryptography.X509Certificates.X509Certificate2UI.DisplayCertificate(Telnet.Certificate, Me.Handle)
            Catch ex As Exception
                Logger.Error(ex)
                MsgBox("Unable to display certificate", vbOKOnly + vbExclamation, "Error")
            End Try
        End If
    End Sub
End Class