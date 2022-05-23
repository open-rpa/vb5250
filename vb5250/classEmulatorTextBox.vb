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
Public Class EmulatorTextBox
    Inherits RichTextBox
    Public Index As Integer
    Private _IsPasswordBox As Boolean
    Private _Insert As Boolean = True 'RichTextBox defaults to insert mode
    Public PopupKey As String 'The key of the popup window this textbox should be on.
    Public EventHandlersRegistered As Boolean = False
    Public Event InsertChanged(ByVal NewState As Boolean)
    Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger
    Public AlternateCaretEnabled As Boolean = False

    Public Property Insert As Boolean
        Set(value As Boolean)
            If _Insert <> value Then
                'When the RichTextBox processes WM_KEYDOWN it will use the current state of the SHIFT key, which may cause a paste operation (SHIFT+INS). 
                If Control.ModifierKeys = Keys.None Then
                    Dim Result As Integer
                    Result = WinAPI.SendMessage(Me.Handle, WinAPI.WM_KEYDOWN, WinAPI.VK_INSERT, &H510001)
                    Result = WinAPI.SendMessage(Me.Handle, WinAPI.WM_KEYUP, WinAPI.VK_INSERT, &HC0510001)
                    'If Result = 0 Then
                    '    _Insert = Not _Insert
                    '    RaiseEvent InsertChanged(_Insert)
                    'End If
                End If
            End If
        End Set
        Get
            Return _Insert
        End Get
    End Property

    Public Property UseSystemPasswordChar As Boolean
        Set(ByVal value As Boolean)
            Me._IsPasswordBox = value
            Dim PasswordChar As Integer = 0
            If value Then PasswordChar = Asc("*") Else PasswordChar = 0
            Dim result As Integer = WinAPI.SendMessage(Me.Handle, WinAPI.EM_SETPASSWORDCHAR, PasswordChar, 0)
            If result Then
                'XXX
            End If
        End Set
        Get
            Return Me._IsPasswordBox
        End Get
    End Property

    Private Sub KeyUpHandler(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyUp
        If e.KeyCode = Keys.Insert Then
            If e.Modifiers = Keys.None Then
                _Insert = Not _Insert
                RaiseEvent InsertChanged(_Insert)
            End If
        End If
    End Sub

    Private Sub KeyPressHandler(ByVal sender As Object, ByVal e As KeyPressEventArgs) Handles MyBase.KeyPress
        'Increase length of field by 1 if we're in overwrite mode and at the end of the string.
        If Not _Insert Then
            Select Case e.KeyChar
                Case " " To "~" 'UTF-8 printable character range
                    If (Me.SelectionStart = Me.Text.Length) And (Me.SelectionStart < Me.MaxLength) Then
                        Dim p As Integer = Me.SelectionStart
                        Me.Text += " "
                        Me.SelectionStart = p
                    End If
            End Select
        End If
    End Sub

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub SimulateKeypress(e As KeyEventArgs)
        Logger.Trace("")
        MyBase.OnKeyDown(e)
        If e.KeyCode >= &H20 And e.KeyCode <= &H7E Then 'space thru tilde
            MyBase.OnKeyPress(New KeyPressEventArgs(ChrW(e.KeyCode)))
        End If
        MyBase.OnKeyUp(e)
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        Static CreateNewCaret As Boolean = True
        MyBase.WndProc(m)
        Try
            If Me.AlternateCaretEnabled Then
                Select Case m.Msg
                    Case WinAPI.WM_PAINT
                        If Me.Focused Then
                            If CreateNewCaret Then
                                Debug.Print("Create Caret: hWnd=" & m.HWnd.ToString)
                                WinAPI.CreateCaret(m.HWnd, Nothing, SystemInformation.BorderSize.Width * 3, Me.Height - 1)
                                WinAPI.ShowCaret(m.HWnd)
                                CreateNewCaret = False
                            End If
                        End If
                    Case WinAPI.WM_KILLFOCUS
                        Debug.Print("Destroy Caret: hWnd=" & m.HWnd.ToString)
                        WinAPI.DestroyCaret()
                        CreateNewCaret = True
                End Select
            End If
        Catch ex As Exception
            Debug.Print(ex.Message)
        End Try
    End Sub
End Class



