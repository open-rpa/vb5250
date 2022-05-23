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
Module modEmulator
    Structure StructEmulatorView
        Dim Settings As ConnectionSettings
        Dim Form As frmEmulatorView
    End Structure

    'Public Enum ScreenDescTypeEnum
    '    Text = 0
    '    TextInRectangle = 1
    '    Attribute = 2
    '    CursorPosition = 3
    '    Invalid = 666
    'End Enum

    'Public Structure EmulatorScreenDescElements
    '    Dim Name As String
    '    Dim Type As ScreenDescTypeEnum
    '    Dim Text As String
    '    Dim Row As Integer
    '    Dim Col As Integer
    '    Dim InputRow As Integer
    '    Dim InputCol As Integer
    '    Dim Input2Row As Integer
    '    Dim Input2Col As Integer
    '    Dim DataRow As Integer
    '    Dim DataCol As Integer
    '    Dim DataLen As Integer
    '    Dim Data2Row As Integer
    '    Dim Data2Col As Integer
    '    Dim Data2Len As Integer
    '    Dim Data3Row As Integer
    '    Dim Data3Col As Integer
    '    Dim Data3Len As Integer
    '    Dim Data4Row As Integer
    '    Dim Data4Col As Integer
    '    Dim Data4Len As Integer
    'End Structure

    'Public Structure EmulatorScreenDesc
    '    Dim Name As String
    '    Dim Elements() As EmulatorScreenDescElements
    'End Structure

    Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

    Public Structure EmulatorConnection
        Dim StationName As String
        Dim HostAddress As String
        Dim HostPort As Integer
        Dim UseSSL As Boolean
        Dim BypassLogin As Boolean 'send credentials during telnet handshake; bypass login screen.
        Dim AuthEncryptionMethod As Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType 'Crypto method for BypassLogin.
        Dim Allow_ScreenSize_27x132 As Boolean
        Dim ColorMap As System.Collections.Generic.Dictionary(Of String, KnownColor)
    End Structure

    Public EmulatorView As Dictionary(Of Integer, StructEmulatorView)

    'Public Sub GetEmulatorOptions()
    '    Dim MethodName As String = System.Reflection.MethodBase.GetCurrentMethod().Name

    '    DebugWrite(MethodName, DebugLevel.Debug, False, "Reading screen descriptions...")
    '    Dim ScreenCount As Integer = -1
    '    Dim ScreenNames() As String
    '    ReDim ScreenNames(-1)
    '    If Db IsNot Nothing Then
    '        Try
    '            Dim ScreenDescINIEntries As System.Collections.Specialized.NameValueCollection = Db.ReadSettingSection("EmulatorScreenDescriptions")
    '            If ScreenDescINIEntries Is Nothing Then
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_UserName_Type", "Text")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_UserName_Text", "User")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_UserName_Row", "6")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_UserName_Col", "17")
    '                'Coordinates to put username:
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_UserName_InputRow", "6")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_UserName_InputCol", "53")
    '                '
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_Password_Type", "Text")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_Password_Text", "Password")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_Password_Row", "7")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_Password_Col", "17")
    '                'Coordinates to put password:
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_Password_InputRow", "7")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "Logon_Password_InputCol", "53")
    '                '
    '                Db.WriteSetting("EmulatorScreenDescriptions", "CustomerAdded_ResponseText_Type", "Text")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "CustomerAdded_ResponseText_Text", "Customer has been added")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "CustomerAdded_ResponseText_Row", "21")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "CustomerAdded_ResponseText_Col", "8")
    '                'CIS number location & length:
    '                Db.WriteSetting("EmulatorScreenDescriptions", "CustomerAdded_ResponseText_DataRow", "4")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "CustomerAdded_ResponseText_DataCol", "69")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "CustomerAdded_ResponseText_DataLen", "8")
    '                '
    '                Db.WriteSetting("EmulatorScreenDescriptions", "AccountAdded_ResponseText_Type", "Text")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "AccountAdded_ResponseText_Text", "Account has been added")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "AccountAdded_ResponseText_Row", "21")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "AccountAdded_ResponseText_Col", "2")
    '                'Account number location & length:
    '                Db.WriteSetting("EmulatorScreenDescriptions", "AccountAdded_ResponseText_DataRow", "4")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "AccountAdded_ResponseText_DataCol", "12")
    '                Db.WriteSetting("EmulatorScreenDescriptions", "AccountAdded_ResponseText_DataLen", "11")
    '                '
    '                'Else
    '                ScreenDescINIEntries = Db.ReadSettingSection("EmulatorScreenDescriptions")
    '            End If
    '            'Parse items
    '            ScreenDescINIEntries = Db.ReadSettingSection("EmulatorScreenDescriptions")
    '            For Each Item As Object In ScreenDescINIEntries
    '                Dim Arry As String() = Split(Item, "_")
    '                If Arry(0) IsNot Nothing Then
    '                    Dim ScreenExists As Boolean = False
    '                    For i As Integer = 0 To ScreenNames.GetUpperBound(0)
    '                        If Arry(0).Trim = ScreenNames(i) Then
    '                            ScreenExists = True
    '                            Exit For
    '                        End If
    '                    Next
    '                    If Not ScreenExists Then
    '                        ScreenCount += 1
    '                        ReDim Preserve ScreenNames(ScreenCount)
    '                        ScreenNames(ScreenCount) = Arry(0).Trim
    '                    End If
    '                End If
    '            Next
    '            'End If

    '            ReDim CurrentSettings.Emulator.ScreenDescs(ScreenCount)
    '            For i As Integer = 0 To ScreenCount
    '                CurrentSettings.Emulator.ScreenDescs(i).Name = ScreenNames(i)
    '                ReDim CurrentSettings.Emulator.ScreenDescs(i).Elements(-1)
    '                For Each Item As Object In ScreenDescINIEntries
    '                    Dim Arry As String() = Split(Item, "_")
    '                    If Arry.GetUpperBound(0) = 2 Then
    '                        If Arry(0) IsNot Nothing Then
    '                            If Arry(0).Trim = CurrentSettings.Emulator.ScreenDescs(i).Name Then
    '                                If Arry(1).Trim IsNot Nothing Then
    '                                    Dim CurrentElementIndex As Integer = -1
    '                                    For j As Integer = 0 To CurrentSettings.Emulator.ScreenDescs(i).Elements.GetUpperBound(0)
    '                                        If CurrentSettings.Emulator.ScreenDescs(i).Elements(j).Name = Arry(1).Trim Then
    '                                            CurrentElementIndex = j
    '                                            Exit For
    '                                        End If
    '                                    Next
    '                                    If CurrentElementIndex < 0 Then
    '                                        CurrentElementIndex = CurrentSettings.Emulator.ScreenDescs(i).Elements.GetUpperBound(0) + 1
    '                                        ReDim Preserve CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex)
    '                                        CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Name = Arry(1).Trim
    '                                    End If
    '                                    Select Case Arry(2).Trim.ToUpper
    '                                        Case "TEXT"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Text = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "ROW"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Row = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "COL"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Col = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "INPUTROW"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).InputRow = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "INPUTCOL"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).InputCol = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "INPUT2ROW"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Input2Row = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "INPUT2COL"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Input2Col = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATAROW"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).DataRow = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATACOL"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).DataCol = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATALEN"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).DataLen = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATA2ROW"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Data2Row = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATA2COL"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Data2Col = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATA2LEN"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Data2Len = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATA3ROW"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Data3Row = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATA3COL"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Data3Col = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATA3LEN"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Data3Len = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATA4ROW"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Data4Row = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATA4COL"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Data4Col = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "DATA4LEN"
    '                                            CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Data4Len = Replace(ScreenDescINIEntries(Item), Chr(34), "")
    '                                        Case "TYPE"
    '                                            Select Case ScreenDescINIEntries(Item).ToString.ToUpper.Trim
    '                                                Case "TEXT"
    '                                                    CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Type = ScreenDescTypeEnum.Text
    '                                                Case "TEXTINRECTANGLE"
    '                                                    CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Type = ScreenDescTypeEnum.TextInRectangle
    '                                                Case "ATTRIBUTE"
    '                                                    CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Type = ScreenDescTypeEnum.Attribute
    '                                                Case "CURSORPOSITION"
    '                                                    CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Type = ScreenDescTypeEnum.CursorPosition
    '                                                Case Else
    '                                                    CurrentSettings.Emulator.ScreenDescs(i).Elements(CurrentElementIndex).Type = ScreenDescTypeEnum.Invalid
    '                                            End Select
    '                                    End Select
    '                                End If
    '                            End If
    '                        End If
    '                    End If
    '                Next
    '            Next
    '            DebugWrite(MethodName, DebugLevel.Debug, False, "Complete; see output from RegisterScreenDescs() for details")
    '        Catch ex As Exception
    '            DebugWrite(MethodName, DebugLevel.Error_, False, "Error getting screen description configuration: " & ex.Message)
    '        End Try
    '    Else
    '        DebugWrite(MethodName, DebugLevel.Warn, False, "Some settings could not be obtained because the database is not initialized.")
    '    End If

    'End Sub

    Public Function GetString(ByVal WindowIndex As Short, ByVal intRow As Integer, ByVal IntCol As Integer, ByVal intLength As Integer) As String
        GetString = ""
        Try
            GetString = EmulatorView(WindowIndex).Form.Emulator.Screen.GetText(intRow, IntCol, intLength)
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "Error reading text from screen")
        End Try
    End Function

    Public Sub PutString(ByVal WindowIndex As Short, ByVal intRow As Integer, ByVal IntCol As Integer, ByVal strValue As String)
        Try
            EmulatorView(WindowIndex).Form.WaitForInputReady(250)
            EmulatorView(WindowIndex).Form.SendKeys(strValue, intRow, IntCol)
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "Error writing text to screen")
        End Try
    End Sub

    Public Sub ProcessKeypress(ByVal WindowIndex As Integer, ByVal KeyType As String, ByVal KeyString As String, ByRef PassItOn As Boolean)
        PassItOn = True
        If WindowIndex > -1 Then
            Select Case UCase(KeyType)
                Case "A"  'ASCII keys
                    Logger.Debug("KeyType = '" & KeyType & "', Keystring = '" & "*" & "'")
                Case "M"  'Mnemonic keys
                    Logger.Debug("KeyType = '" & KeyType & "', Keystring = '" & KeyString & "'")
                    Select Case UCase(KeyString)
                        '    Case "[PF5]"
                    End Select
                Case Else
                    Logger.Debug("KeyType = '" & KeyType & "', Keystring = '" & KeyString & "'")
            End Select
        End If
    End Sub

End Module
