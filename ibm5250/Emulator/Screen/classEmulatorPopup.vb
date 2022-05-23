'
' Copyright 2012, 2013 Alec Skelly
'
' This file is part of IBM5250, a VB.Net implementation of IBM's 5250 protocol.
'
' IBM5250 is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' IBM5250 is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with IBM5250. If not, see <http://www.gnu.org/licenses/>.
' 

Partial Class Emulator
    Partial Public Class EmulatorScreen
        <Serializable()> Public Class EmulatorPopup
            Private _Rows As Byte
            Private _Columns As Byte
            Private _Top As Byte
            Private _Left As Byte
            'Public Panel As Panel
            Public WindowTitle As String
            Public WindowTitleForeColor As String
            Public WindowFooter As String
            Public WindowFooterForeColor As String
            Public DefaultForeColor As String
            Public BackColor As String
            Public TextBuffer() As Byte
            Public Strings(255) As IBM5250.Emulator.EmulatorScreen.Field

            Public ReadOnly Property Rows As Byte
                Get
                    Return _Rows
                End Get
            End Property
            Public ReadOnly Property Columns As Byte
                Get
                    Return _Columns
                End Get
            End Property
            Public ReadOnly Property Top As Byte
                Get
                    Return _Top
                End Get
            End Property
            Public ReadOnly Property Left As Byte
                Get
                    Return _Left
                End Get
            End Property
            Private _Parent As Object
            Public ReadOnly Property Parent As EmulatorScreen
                Get
                    Return _Parent
                End Get
            End Property

            Public Sub New(ByVal WindowDepth As Byte, ByVal WindowWidth As Byte, ByVal Top As Byte, ByVal Left As Byte, ByVal Parent As EmulatorScreen) ', ByVal WindowTitleForeColor As Color, ByVal WindowFooterForeColor As Color)
                '@..........title..........@
                '@:@                     @:@
                '@:@                     @:@
                '@:@                     @:@
                '@:@                     @:@
                '@:@                     @:@
                '@:@                     @:@
                '@:.......................:@
                'Window Depth: If this parameter is used, it indicates the number of rows within the window. The minimum window depth value is 1. The
                'maximum window depth value is the number of rows remaining on the window minus 1 (for the bottom border row).
                '
                'Window Width: If this parameter is used, it indicates the number of columns within the window. The minimum window width value is 1. The
                'maximum window width value is the number of columns remaining on the window on the right minus 3 (for the right border).
                '
                'The AS/400 application is responsible for ensuring that all entry fields and data defined in the window do not extend beyond the window borders.
                'However, the exceptions are:
                '   A leading attribute (input or output data beginning with a display attribute) can overwrite the second left border attribute.
                '   An ending field attribute can overlap the first right border attribute. The 5494 suppresses an ending field attribute (SF order) if the ending
                '       attribute overlaps exactly the first attribute on the right border.
                '   ...
                '
                Me._Parent = Parent
                Me._Rows = WindowDepth
                Me._Columns = WindowWidth + 2 'to account for the overlappable attributes
                Me._Top = Top
                Me._Left = Left
                Me.BackColor = Parent.BackColor
                Me.DefaultForeColor = Parent.DefaultForeColor
                'Me.WindowTitleForeColor = WindowTitleForeColor
                'Me.WindowFooterForeColor = WindowFooterForeColor
                ReDim Me.TextBuffer((CInt(Rows) * CInt(Columns)) - 1)
                For i As Integer = 0 To Strings.Length - 1
                    Strings(i) = New IBM5250.Emulator.EmulatorScreen.Field
                Next

            End Sub

            Public Function GetTextBufferAddressFromScreenCoordinates(ByVal ScreenRow As Integer, ByVal ScreenCol As Integer) As Integer
                Return ((ScreenRow - Me.Top - 1) * Me.Columns) + ScreenCol - Me.Left - 2
            End Function
            Private Function GetTextBufferAddress(ByVal Row As Integer, ByVal Col As Integer) As Integer
                Return ((Row - 1) * Columns) + Col - 1
            End Function
            Private Function GetTextBufferRow(ByVal LinearAddress As Integer) As Byte
                Return (LinearAddress \ Me.Columns) + 1
            End Function
            Private Function GetTextBufferColumn(ByVal LinearAddress As Integer) As Byte
                Return (LinearAddress Mod Me.Columns) + 1
            End Function

            Public Sub WriteTextBuffer(ByVal ScreenRow As Byte, ByVal ScreenColumn As Byte, ByVal b As Byte)
                Dim n As Integer = Me.GetTextBufferAddressFromScreenCoordinates(ScreenRow, ScreenColumn)
                Me.TextBuffer(n) = b
            End Sub

            'Public Sub WriteTextBuffer(ByVal Address As Integer, ByVal b As Byte)
            '    WriteTextBuffer(Address, New Byte() {b})
            'End Sub
            'Public Sub WriteTextBuffer(ByVal Address As Integer, ByVal Bytes() As Byte)
            '    For i As Integer = 0 To Bytes.Length - 1
            '        Dim addr As Integer

            '        Me.TextBuffer(Address) = Bytes(i)
            '    Next
            'End Sub


            Public Sub UpdateStrings()

                'XXX this is lame
                For i As Integer = 0 To Me.Strings.Length - 1
                    With Me.Strings(i)
                        If .Allocated = True Then .Allocated = False Else Exit For
                    End With
                Next

                Dim attr As New FieldAttribute(FieldAttribute.ColorAttribute.Green, Me.DefaultForeColor, Me.BackColor)
                Dim TextStream As New System.IO.MemoryStream

                Dim StringIndex As Integer = 0

                Dim MaxBufferAddress As Integer = GetTextBufferAddress(Me.Rows, Me.Columns)
                Dim PriorRow As Integer = 1
                For i As Integer = 0 To MaxBufferAddress
                    Dim CurrentRow As Integer = GetTextBufferRow(i)
                    If IsAttribute(Me.TextBuffer(i)) Or (CurrentRow <> PriorRow) Or (i = MaxBufferAddress) Then

                        'record prior string, if any
                        Dim s As String = Nothing
                        If TextStream.Length > 0 Then
                            Dim a() As Byte = Me.Parent.Parent.EBCDIC_To_UTF8(TextStream.ToArray)
                            s = System.Text.Encoding.UTF8.GetString(a)
                        End If

                        'don't create a string if it's unprintable
                        If s IsNot Nothing Then
                            Dim FoundPrintableChar As Boolean = False
                            For idx As Integer = 0 To s.Length - 1
                                If s(idx) > " " And s(idx) <= "~" Then
                                    FoundPrintableChar = True
                                    Exit For
                                End If
                            Next
                            If Not FoundPrintableChar Then s = Nothing
                        End If

                        'remove unprintable characters and trailing whitespace
                        If s IsNot Nothing AndAlso s.Length > 0 Then
                            For idx As Integer = 0 To s.Length - 1
                                If s(idx) < " " OrElse s(idx) > "~" Then
                                    s = s.Replace(s(idx), " ")
                                End If
                            Next
                            s = s.TrimEnd()
                        End If

                        'allocate the string if it still exists
                        If s IsNot Nothing AndAlso s.Length > 0 Then
                            With Me.Strings(StringIndex)
                                .Text = s
                                .Attribute = attr
                                .Allocated = True

                                'XXX these positions are relative to the popup and probably should be absolute with respect to the screen
                                .Location.Position = i - TextStream.Length
                                .Location.Row = GetTextBufferRow(.Location.Position)
                                .Location.Column = GetTextBufferColumn(.Location.Position)
                                .Location.Length = TextStream.Length
                            End With
                            StringIndex += 1
                        End If

                        TextStream = New System.IO.MemoryStream

                        If IsAttribute(Me.TextBuffer(i)) Then
                            attr = New FieldAttribute(Me.TextBuffer(i), Me.DefaultForeColor, Me.BackColor)
                        Else
                            'add first byte to new string
                            TextStream.WriteByte(Me.TextBuffer(i))
                        End If
                    Else
                        'append to previous string
                        TextStream.WriteByte(Me.TextBuffer(i))
                    End If
                    PriorRow = CurrentRow
                Next

            End Sub

        End Class   'EmulatorPopup
    End Class   'EmulatorScreen
End Class   'Emulator
