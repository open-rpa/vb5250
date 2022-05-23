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
Public Class frmEmulatorPopup
    Public ParentEmulatorView As frmEmulatorView
    Public TopOffset, LeftOffset As Integer
    Public ReturnCursorTo As IBM5250.Emulator.EmulatorScreen.RowCol
    Dim Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

    Private Sub Panel1_Paint(sender As System.Object, e As System.Windows.Forms.PaintEventArgs)
        Try
            Me.DrawStrings(e)
        Catch ex As Exception
            Logger.Error(ex.Message, ex)
        End Try
    End Sub

    Private Sub DrawStrings(e As PaintEventArgs)
        If Me.Tag IsNot Nothing AndAlso Me.ParentEmulatorView IsNot Nothing Then

            Me.ParentEmulatorView.UpdateScreenMetrics()
            Me.Panel1.BackColor = Color.FromKnownColor(ParentEmulatorView.MappedColor(Me.ParentEmulatorView.Emulator.Screen.PopUps(Me.Tag).BackColor))

            'Strings are being updated on a different thread, so take a snapshot to work with here.
            Dim TempStrings(Me.ParentEmulatorView.Emulator.Screen.PopUps(Me.Tag).Strings.Length - 1) As IBM5250.Emulator.EmulatorScreen.Field
            Array.Copy(Me.ParentEmulatorView.Emulator.Screen.PopUps(Me.Tag).Strings, 0, TempStrings, 0, TempStrings.Length)

            For i As Integer = 0 To TempStrings.Length - 1
                With TempStrings(i)
                    If .Allocated Then
                        If .Text IsNot Nothing Then
                            Dim b As New SolidBrush(Color.FromKnownColor(ParentEmulatorView.MappedColor(.Attribute.ForeColor)))

                            Dim x As Single = (.Location.Column - 1) * Me.ParentEmulatorView.ScreenMetrics.Font_Character_Width
                            Dim y As Single = (.Location.Row - 1) * Me.ParentEmulatorView.ScreenMetrics.ControlHeight

                            e.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
                            e.Graphics.DrawString(.Text, Me.ParentEmulatorView.ScreenMetrics.Font_Regular, b, x, y)

                            b.Dispose()
                        End If
                    Else
                        Exit For
                    End If
                End With
            Next
        End If
    End Sub

    Private Sub frmEmulatorPopup_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        AddHandler Me.Panel1.Paint, AddressOf Me.Panel1_Paint
    End Sub

    Private Sub frmEmulatorPopup_SizeChanged(sender As Object, e As System.EventArgs) Handles Me.SizeChanged
        If Me.WindowState = FormWindowState.Maximized Then Me.WindowState = FormWindowState.Normal
    End Sub
End Class