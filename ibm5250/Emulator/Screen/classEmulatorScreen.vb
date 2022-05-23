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
    <Serializable()> Partial Public Class EmulatorScreen
        Implements ICloneable
        <NonSerialized()> _
        Public Event Cleared()
        <NonSerialized()> _
        Public Event PopupAdded(ByVal key As String)
        <NonSerialized()> _
        Public Event PopupRemoved(ByVal key As String)
        <NonSerialized()> _
        Public Event FieldsChanged()
        <NonSerialized()> _
        Public Event StringsChanged()
        <NonSerialized()> _
        Public Event FieldAttributeChanged(ByVal FieldIndex As Integer)

        '5250 Write To Display Orders
        Public Enum WTD_Order As Byte
            Start_Of_Header = 1
            Repeat_To_Address = 2
            Erase_To_Address = 3
            Transparent_Data = &H10
            Set_Buffer_Address = &H11
            Write_Extended_Attribute = &H12
            Insert_Cursor = &H13
            Move_Cursor = &H14
            Write_To_Display_Structured_Field = &H15
            Start_Of_Field = &H1D
        End Enum
        Public Enum Erase_To_Address_Attribute_Types As Byte
            Display_Screen = 0
            Extended_Primary_Attributes = 1
            Extended_Text_Attributes = 2
            Extended_Foreground_Color_Attributes = 3
            Extended_Ideographic_Attributes = 5
            All = &HFF
        End Enum
        <Serializable()> Public Structure RowCol
            Dim Row As Byte
            Dim Column As Byte
        End Structure
        <Serializable()> Public Structure PendingRead
            Dim Pending As Boolean
            Dim Command As Emulator.Command
        End Structure

        Public DefaultForeColor As String
        Public BackColor As String
        Public Fields(255) As Field
        Public Strings(255) As Field
        Public ErrorText As String
        Public TextBuffer() As Byte
        Private _Rows, _Columns, _FinalFieldAddress As Integer ', _CurrentFieldIndex As Integer
        Public HomeCoordinates As RowCol
        Public Read As PendingRead
        Public Row, Column As Integer
        Public PopUps As New System.Collections.Generic.Dictionary(Of String, EmulatorPopup)
        Public Header As StartOfHeader_Header

        Public Complete As Boolean 'set by the Emulator class while processing the data stream from the server.

        Public ReadOnly Property Rows As Integer
            Get
                Return _Rows
            End Get
        End Property
        Public ReadOnly Property Columns As Integer
            Get
                Return _Columns
            End Get
        End Property
        Private _Parent As Object
        Public ReadOnly Property Parent As Emulator
            Get
                Return _Parent
            End Get
        End Property

        Public Sub New(ByVal Rows As Integer, ByVal Columns As Integer, ByVal Parent As Emulator)
            _Rows = Rows
            _Columns = Columns
            _Parent = Parent

            Me.DefaultForeColor = "Green"
            Me.BackColor = "Background"

            Me.Clear()

        End Sub

        Public Sub Clear()
            For Each kvp As KeyValuePair(Of String, EmulatorPopup) In Me.PopUps
                RaiseEvent PopupRemoved(kvp.Key)
            Next
            Me.PopUps.Clear()

            Me.ClearFields(False)
            Me.ClearStrings(False)

            Me.Row = 1
            Me.Column = 1
            Me.HomeCoordinates.Row = 1
            Me.HomeCoordinates.Column = 1

            Me.Header = New StartOfHeader_Header

            Me.Read = New PendingRead

            ReDim Me.TextBuffer((_Rows * _Columns) - 1)

            RaiseEvent Cleared()

        End Sub

        Public Function GetTextBufferAddress() As Integer
            Return GetTextBufferAddress(Me.Row, Me.Column)
        End Function
        Public Function GetTextBufferAddress(ByVal Row As Integer, ByVal Col As Integer) As Integer
            If Row > Me.Rows OrElse Col > Me.Columns Then Throw New ArgumentException("Coordinates out of range: " & Row & "," & Column)
            Return ((Row - 1) * Columns) + Col - 1
        End Function

        Public Function GetTextBufferRow(ByVal LinearAddress As Integer) As Byte
            Return (LinearAddress \ Columns) + 1
        End Function
        Public Function GetTextBufferColumn(ByVal LinearAddress As Integer) As Byte
            Return (LinearAddress Mod Columns) + 1
        End Function

        Public Sub WriteTextBuffer(ByVal b As Byte)
            WriteTextBuffer(Me.Row, Me.Column, b)
            Me.IncrementCursorPosition(1)
        End Sub
        Public Sub WriteTextBuffer(ByVal Row As Byte, ByVal Column As Byte, ByVal b As Byte)
            If Row > Me.Rows OrElse Column > Me.Columns Then Throw New ArgumentException("Coordinates out of range: " & Row & "," & Column)
            WriteTextBuffer(Row, Column, New Byte() {b})
        End Sub
        Public Sub WriteTextBuffer(ByVal Bytes() As Byte)
            WriteTextBuffer(Me.Row, Me.Column, Bytes)
            Me.IncrementCursorPosition(Bytes.Length)
        End Sub
        Public Sub WriteTextBuffer(ByVal Row As Byte, ByVal Column As Byte, ByVal Bytes() As Byte)
            If Row > Me.Rows OrElse Column > Me.Columns Then Throw New ArgumentException("Coordinates out of range: " & Row & "," & Column)
            Dim addr As Integer = GetTextBufferAddress(Row, Column)
            For i As Integer = 0 To Bytes.Length - 1
                Dim popkey As String = PopupKeyOfAddress(addr)
                If popkey IsNot Nothing Then
                    Me.PopUps(popkey).WriteTextBuffer(Me.GetTextBufferRow(addr), Me.GetTextBufferColumn(addr), Bytes(i))
                Else
                    Me.TextBuffer(addr) = Bytes(i)
                End If
                addr += 1
            Next
        End Sub

        Public Sub RepeatToAddress(ByVal Row As Integer, ByVal Column As Integer, ByVal Byt As Byte, SuppressEvents As Boolean)
            If Row > Me.Rows OrElse Column > Me.Columns Then Throw New ArgumentException("Coordinates out of range: " & Row & "," & Column)
            Dim finish As Integer = GetTextBufferAddress(Row, Column)
            Dim LastAddressWritten As Integer = 0
            Dim FieldRemoved As Boolean = False
            Do
                LastAddressWritten = GetTextBufferAddress()
                Me.WriteTextBuffer(Byt)
                If Byt = 0 Then
                    If Me.RemoveField(LastAddressWritten, True, False) Then FieldRemoved = True
                End If
            Loop While LastAddressWritten <> finish
            If Not SuppressEvents Then
                If FieldRemoved Then RaiseEvent FieldsChanged()
                UpdateStrings(False)
            End If
        End Sub

        Public Sub EraseToAddress(ByVal Row As Integer, ByVal Column As Integer, ByVal AttributeTypes() As Erase_To_Address_Attribute_Types, SuppressEvents As Boolean)
            If Row > Me.Rows OrElse Column > Me.Columns Then Throw New ArgumentException("Coordinates out of range: " & Row & "," & Column)
            'XXX Since we don't yet handle extended attributes, always erase everything.
            Dim finish As Integer = GetTextBufferAddress(Row, Column)
            Dim LastAddressWritten As Integer = 0
            Dim FieldRemoved As Boolean = False
            Do
                LastAddressWritten = GetTextBufferAddress()
                Me.WriteTextBuffer(0)
                If Me.RemoveField(LastAddressWritten, True, True) Then FieldRemoved = True
            Loop While LastAddressWritten <> finish
            If Not SuppressEvents Then
                If FieldRemoved Then RaiseEvent FieldsChanged()
                UpdateStrings(False)
            End If
        End Sub

        <Serializable()> Public Class Field
            <Serializable()> Public Structure EmulatorFieldLocation
                Dim Position As Integer '0 based ordinal position in data stream, regardless of rows and columns
                Dim Column As Integer   '1 based
                Dim Row As Integer      '1 based
                Dim Length As Integer
            End Structure
            Public Allocated As Boolean 'Is the field a used member of the array?
            Public Location As EmulatorFieldLocation
            Public IsInputField As Boolean
            Public Flags As StartOfField_Header.FFW
            Public ControlFlags() As StartOfField_Header.FCW
            Public Attribute As FieldAttribute
            Public Text As String
            Public PopupKey As String
            Public Sub New()
                ReDim Me.ControlFlags(-1)
            End Sub
        End Class 'Field

        Public Sub IncrementCursorPosition(ByVal n As Integer)
            Dim Offs As Integer = GetTextBufferAddress()
            Dim MaxOffs As Integer = (Me.Rows * Me.Columns) - 1
            Offs += n
            If Offs > MaxOffs Then Offs -= MaxOffs 'wrap back to top of screen
            Me.Row = (Offs \ Me.Columns) + 1
            Me.Column = (Offs Mod Me.Columns) + 1
        End Sub

        Public Sub AddPopup(ByVal Header As StructuredField.CreateWindowHeader)
            Dim key As String = Me.Row & "," & Me.Column & "," & Header.Rows & "," & Header.Columns
            Dim p As EmulatorPopup
            If Me.PopUps.ContainsKey(key) Then
                'Overwrite the existing popup having the same coordinates and size.
                Me.PopUps.Remove(key)
            End If
            'Create a new popup
            p = New EmulatorPopup(Header.Rows, Header.Columns, Me.Row, Me.Column, Me)
            For Each ms As Object In Header.MinorStructures
                Select Case ms.GetType
                    Case GetType(StructuredField.CreateWindowHeader.WindowTitle_Or_Footer)
                        Select Case ms.Element
                            Case StructuredField.CreateWindowHeader.WindowTitle_Or_Footer.WindowElement.Title
                                p.WindowTitle = ms.Text
                                p.WindowTitleForeColor = ms.ColorAttribute.ForeColor
                            Case StructuredField.CreateWindowHeader.WindowTitle_Or_Footer.WindowElement.Footer
                                p.WindowFooter = ms.Text
                                p.WindowFooterForeColor = ms.ColorAttribute.ForeColor
                        End Select
                    Case GetType(StructuredField.CreateWindowHeader.BorderPresentation)
                        'XXX

                End Select

            Next
            Me.PopUps.Add(key, p)
            RaiseEvent PopupAdded(key)
        End Sub

        Public Sub RemovePopup(ByVal Key As String)
            If Me.PopUps.ContainsKey(Key) Then Me.PopUps.Remove(Key)
            RaiseEvent PopupRemoved(Key)
        End Sub

        Public Sub RemovePopup(ByVal Row As Integer, ByVal Column As Integer, ByVal Height As Integer, ByVal Width As Integer)
            If Row > Me.Rows OrElse Column > Me.Columns Then Throw New ArgumentException("Coordinates out of range: " & Row & "," & Column)
            Dim key As String = Row & "," & Column & "," & Height & "," & Width
            RemovePopup(key)
        End Sub

        Public Function PopupKeyOfAddress(ByVal Address As Integer) As String
            Return Me.PopupKeyOfAddress(Me.GetTextBufferRow(Address), Me.GetTextBufferColumn(Address))
        End Function
        Public Function PopupKeyOfAddress(ByVal Row As Integer, ByVal Column As Integer) As String
            'return the key of the popup which contains the given address _inside_ its borders.
            If Row > Me.Rows OrElse Column > Me.Columns Then Throw New ArgumentException("Coordinates out of range: " & Row & "," & Column)
            For Each kvp As KeyValuePair(Of String, EmulatorPopup) In Me.PopUps
                Dim p As EmulatorPopup = kvp.Value
                If (Row >= p.Top) And (Row <= (p.Top + p.Rows + 1)) Then 'add 1 for the border character
                    If (Column >= p.Left) And (Column <= (p.Left + p.Columns + 2)) Then 'add 2 for the leading attribute and border character
                        Return kvp.Key
                    End If
                End If
            Next
            Return Nothing
        End Function

        Public Sub ClearStrings(ByVal SuppressEvent As Boolean)
            For i As Integer = 0 To Strings.Length - 1
                Strings(i) = New Field
            Next
            If Not SuppressEvent Then RaiseEvent StringsChanged()
        End Sub

        Public Sub UpdateStrings(SuppressEvent As Boolean)

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
                        Dim a() As Byte = Me.Parent.EBCDIC_To_UTF8(TextStream.ToArray)
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
            For Each kvp As KeyValuePair(Of String, EmulatorPopup) In Me.PopUps
                kvp.Value.UpdateStrings()
            Next
            If Not SuppressEvent Then RaiseEvent StringsChanged()
        End Sub

        Public Function GetText(Row As Integer, Column As Integer, Length As Integer) As String
            Return Me.GetText(Row, Column, Length, False)
        End Function
        Public Function GetText(Row As Integer, Column As Integer, Length As Integer, ExcludeNonDisplay As Boolean) As String
            If (Row < 1) Or (Row > _Rows) Then
                Throw New ArgumentException("Row must be between 1 and " & _Rows.ToString & ", inclusive")
            End If
            If (Column < 1) Or (Column > _Columns) Then
                Throw New ArgumentException("Column must be between 1 and " & _Columns.ToString & ", inclusive")
            End If
            If Column + Length - 1 > _Columns Then
                Throw New ArgumentException("Cannot read past column " & _Columns.ToString)
            End If

            Dim attr As New FieldAttribute()

            Dim addr As Integer = Me.GetTextBufferAddress(Row, Column)

            If ExcludeNonDisplay Then
                'look backward in buffer to find current attribute
                For i As Integer = addr To 0 Step -1
                    If IsAttribute(Me.TextBuffer(i)) Then
                        attr = New FieldAttribute(Me.TextBuffer(i))
                        Exit For
                    End If
                Next
            End If

            Dim b(Length - 1) As Byte
            Array.Copy(Me.TextBuffer, addr, b, 0, Length)
            For i As Integer = 0 To Length - 1
                If IsAttribute(b(i)) Then attr = New FieldAttribute(b(i))
                If IsAttribute(b(i)) Or (b(i) = 0) Or (ExcludeNonDisplay And attr.NonDisplay) Then
                    b(i) = &H40 'convert attribute or null to space, which is how it would be displayed on the screen.
                End If
            Next
            b = Me.Parent.EBCDIC_To_UTF8(b)
            Return System.Text.Encoding.UTF8.GetString(b)
        End Function

        Public Function WaitForString(Text As String, Row As Integer, Column As Integer, Timeout_ms As Double, WaitForInputReady As Boolean, CaseSensitive As Boolean) As Boolean
            Dim StartTime As Date = Now
            Dim Deadline As Date = StartTime.AddMilliseconds(Timeout_ms)
            If Timeout_ms = 0 Then Timeout_ms = Double.MaxValue 'pseudo-infinite wait time to match Client Access
            Do While Now <= Deadline
                If Me.Complete Then
                    If Row = 0 And Column = 0 Then 'search the entire screen
                        For r As Integer = 1 To _Rows
                            Dim s As String = Me.GetText(r, 1, _Columns)
                            If Not CaseSensitive Then
                                Text = Text.ToUpper
                                s = s.ToUpper
                            End If
                            If s.Contains(Text) Then Return True
                        Next
                    Else 'search the specified coordinates
                        Dim s As String = Me.GetText(Row, Column, Text.Length)
                        If Not CaseSensitive Then
                            Text = Text.ToUpper
                            s = s.ToUpper
                        End If
                        If s = Text Then Return True
                    End If
                End If
                System.Windows.Forms.Application.DoEvents()
            Loop
            Return False
        End Function

        Public Function FieldIndexOfAddress(ByVal Row As Integer, ByVal Column As Integer) As Integer
            If Row > Me.Rows OrElse Column > Me.Columns Then Throw New ArgumentException("Coordinates out of range: " & Row & "," & Column)
            Return FieldIndexOfAddress(GetTextBufferAddress(Row, Column))
        End Function
        Public Function FieldIndexOfAddress(ByVal LinearAddress As Integer) As Integer

            Dim pkey As String = Me.PopupKeyOfAddress(LinearAddress)

            For i As Integer = 0 To Me.Fields.Length - 1
                If Me.Fields(i).Allocated Then
                    Dim start As Integer = Fields(i).Location.Position
                    Dim finish As Integer = start + Fields(i).Location.Length - 1
                    If LinearAddress >= start AndAlso LinearAddress <= finish Then
                        If pkey = Me.Fields(i).PopupKey Then Return i 'will usually be Nothing = Nothing
                    End If
                Else
                    Exit For
                End If
            Next
            Return -1
        End Function

        Public Function NextVerticalFieldIndex(RelativeToIndex As Integer, Reverse As Boolean) As Integer
            Dim r As Integer = Fields(RelativeToIndex).Location.Row
            Dim RowMin, RowMax As Integer
            Dim popkey As String = Fields(RelativeToIndex).PopupKey
            If popkey Is Nothing Then
                RowMin = 1
                RowMax = Me.Rows
            Else
                'search the interior of the window, excluding top & bottom frames
                RowMin = Me.PopUps(popkey).Top + 1
                RowMax = Me.PopUps(popkey).Top + Me.PopUps(popkey).Rows
            End If
            Do
                If Reverse Then
                    r -= 1
                    If r < RowMin Then r = RowMax
                Else
                    r += 1
                    If r > RowMax Then r = RowMin
                End If
                Dim idx As Integer = Me.FieldIndexOfAddress(r, Fields(RelativeToIndex).Location.Column)
                If idx > -1 Then
                    If Me.Fields(idx).Flags.Bypass Then idx = -1
                End If
                If idx > -1 Then Return idx
            Loop While r <> Fields(RelativeToIndex).Location.Row
            Return -1
        End Function

        Public Sub ClearFields(ByVal SuppressEvent As Boolean)
            For i As Integer = 0 To Fields.Length - 1
                Fields(i) = New Field
            Next
            If Not SuppressEvent Then RaiseEvent FieldsChanged()
        End Sub

        Public Sub UpdateFieldValues()
            For i As Integer = 0 To Me.Fields.Length - 1
                If Me.Fields(i).Allocated Then
                    Dim start As Integer = Fields(i).Location.Position
                    Dim finish As Integer = start + Fields(i).Location.Length - 1
                    Dim pkey As String = PopupKeyOfAddress(start)

                    If finish <= (Me.TextBuffer.Length - 1) AndAlso (pkey = Fields(i).PopupKey) Then
                        Dim b(Fields(i).Location.Length - 1) As Byte

                        If pkey IsNot Nothing Then
                            Dim pop As EmulatorPopup = Me.PopUps(pkey)
                            Dim addr As Integer = pop.GetTextBufferAddressFromScreenCoordinates(Fields(i).Location.Row, Fields(i).Location.Column)
                            Array.Copy(pop.TextBuffer, addr, b, 0, Fields(i).Location.Length)
                        Else
                            Array.Copy(Me.TextBuffer, start, b, 0, Fields(i).Location.Length)
                        End If

                        b = Me.Parent.EBCDIC_To_UTF8(b)

                        'XXX is it always ok to convert nulls to spaces?
                        'Dim s As String = Replace(System.Text.Encoding.Default.GetString(b), Chr(0), " ")
                        's = s.TrimEnd 'This allows insert mode to work properly.

                        'remove unprintable characters and trailing whitespace
                        Dim s As String = System.Text.Encoding.UTF8.GetString(b)
                        If s IsNot Nothing AndAlso s.Length > 0 Then
                            For idx As Integer = 0 To s.Length - 1
                                If s(idx) < " " OrElse s(idx) > "~" Then
                                    s = s.Replace(s(idx), " ")
                                End If
                            Next
                            s = s.TrimEnd()
                        End If


                        Fields(i).Text = s
                    End If
                Else
                    Exit For
                End If
            Next
        End Sub

        Public Sub UpdateFieldAttribute(FieldIndex As Integer, Attribute As Emulator.EmulatorScreen.FieldAttribute, ByVal SuppressEvent As Boolean)
            Fields(FieldIndex).Attribute = Attribute
            If Not SuppressEvent Then RaiseEvent FieldAttributeChanged(FieldIndex)
        End Sub

        Public Sub UpdateField(ByVal Header As EmulatorScreen.StartOfField_Header, ByVal Text As String, SuppressEvent As Boolean)
            Dim FieldIndex As Integer = -1
            Dim LastIndex As Integer = -1
            For i As Integer = 0 To Me.Fields.Length - 1
                If Me.Fields(i).Allocated Then
                    LastIndex = i
                    If (FieldIndex < 0) And (Me.Fields(i).Location.Position = GetTextBufferAddress()) Then
                        FieldIndex = i
                    End If
                Else
                    Exit For
                End If
            Next
            If (FieldIndex < 0) And (GetTextBufferAddress() > _FinalFieldAddress) Then 'XXX _FinalFieldAddress is never set anywhere!
                FieldIndex = LastIndex + 1
                If FieldIndex < Fields.Length Then
                    'allocate new field
                    Fields(FieldIndex).Allocated = True
                End If
            End If
            If FieldIndex >= 0 Then
                'update field properties
                Fields(FieldIndex).Location.Row = Me.Row
                Fields(FieldIndex).Location.Column = Me.Column
                Fields(FieldIndex).Location.Position = GetTextBufferAddress()
                Fields(FieldIndex).Location.Length = Header.FieldLength
                Fields(FieldIndex).IsInputField = Header.IsInputField
                Fields(FieldIndex).Flags = Header.FieldFormatWord

                ReDim Fields(FieldIndex).ControlFlags(Header.FieldControlWords.Length - 1)
                If Fields(FieldIndex).ControlFlags.Length > 0 Then
                    Array.Copy(Header.FieldControlWords, 0, Fields(FieldIndex).ControlFlags, 0, Fields(FieldIndex).ControlFlags.Length)
                End If

                UpdateFieldAttribute(FieldIndex, Header.LeadingFieldAttribute, True)

                Fields(FieldIndex).PopupKey = PopupKeyOfAddress(Fields(FieldIndex).Location.Position)

                If Not SuppressEvent Then RaiseEvent FieldsChanged()
            End If
        End Sub

        Public Function RemoveField(Row As Byte, Column As Byte, SuppressEvent As Boolean, Greedy As Boolean) As Boolean
            Return RemoveField(GetTextBufferAddress(Row, Column), SuppressEvent, Greedy)
        End Function
        Private Function RemoveField(Position As Integer, SuppressEvent As Boolean, Greedy As Boolean) As Boolean
            'Greedy parameter added to accomodate observed behavior of ERASE_TO_ADDRESS command.
            'Normally we only want to delete a field when the leading attribute byte is overwritten with a Null,
            'but ERASE_TO_ADDRESS seems to expect field deletion even when the leading attribute is not nulled.

            Dim RemoveIndex As Integer = -1
            For i As Integer = 0 To Fields.Length - 1
                If Fields(i).Allocated Then
                    If Fields(i).PopupKey = PopupKeyOfAddress(Fields(i).Location.Position) Then
                        If Position = (Fields(i).Location.Position - 1) Then '-1 is the attribute position
                            RemoveIndex = i
                            Exit For
                        ElseIf Greedy Then
                            If (Position >= Fields(i).Location.Position) And (Position <= Fields(i).Location.Position + Fields(i).Location.Length - 1) Then
                                RemoveIndex = i
                                Exit For
                            End If
                        End If
                    End If
                Else
                    Exit For
                End If

            Next
            If RemoveIndex > -1 Then
                For i As Integer = RemoveIndex To Fields.Length - 2
                    Fields(i) = Fields(i + 1)
                Next
                Fields(Fields.Length - 1) = New Field
                If Not SuppressEvent Then RaiseEvent FieldsChanged()
                Return True
            Else
                Return False
            End If
        End Function

        Public Function Clone() As Object Implements System.ICloneable.Clone
            Dim m As New System.IO.MemoryStream()
            Dim f As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            f.Serialize(m, Me)
            m.Seek(0, System.IO.SeekOrigin.Begin)
            Return f.Deserialize(m)
        End Function

    End Class 'Screen
End Class 'Emulator

