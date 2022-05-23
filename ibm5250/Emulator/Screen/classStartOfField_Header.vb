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
    Partial Class EmulatorScreen
        <Serializable()> Public Class StartOfField_Header
            Public FieldFormatWord As FFW
            Public FieldControlWords() As FCW
            Public LeadingFieldAttribute As Emulator.EmulatorScreen.FieldAttribute
            Public FieldLength As UInt16
            Public Length As Integer
            Public IsInputField As Boolean 'input fields become textboxes, output fields become labels

            Public Enum FCW_Type As Byte
                EntryFieldResequencing = &H80
                MagneticStripeOrLightPen = &H81
                Ideographic = &H82
                Transparency = &H84
                ForwardEdgeTrigger = &H85
                ContinuedEntry = &H86
                CursorProgression = &H88
                Highlighted = &H89
                PointerDevice = &H8A
                SelfCheck = &HB1
            End Enum
            Public Structure FCW
                Dim Raw As UInt16
                Dim Type As FCW_Type
                Dim Data As Byte 'meaning varies according to Type
            End Structure
            Private Enum HighlightField_DisplayType As Byte
                Both = 0
                Monochrome = 1
                Color = 2
                Reserved = 3
            End Enum

            Public Enum FFW_ShiftEditSpec As Byte
                AlphaShift = 0
                'Accepts all characters, and the Shift keys are acknowledged.

                AlphaOnly = 1
                'Accepts only characters A--Z (both uppercase and lowercase plus the comma (,), period (.), dash (-), and blank characters). Other
                'characters cause operator errors. Some special characters for World Trade countries are also acceptable.

                NumericShift = 2
                'Accepts all characters.

                NumericOnly = 3
                'Accepts only the 0-9, plus (+), comma (,), period (.), and blank ( ) characters. Any other character causes an operator error. The 
                'unit position carries the sign digit for the field. Use either the Field +, the Field -, or the Field Exit key to exit this field. If 
                'you use the Field - key to exit the field, the 5494 changes the zone of the low-order digit to X'D', 
                'unless it is one of the symbols (+,-, . or blank); in this case, an error results.

                KatakanaShift = 4
                'This is the same as the alphabetic shift except that the keyboard is Katakana and is placed in Katakana shift.

                DigitsOnly = 5
                'Allows digits 0-9 only from keyboard. Also allows the Dup key if the Dup enabled bit is in the FFW.

                IO = 6
                'Only a magnetic stripe reader or selector light pen can enter data into an I/O field without causing an error.

                SignedNumeric = 7
                'Allows only characters 0-9. An attempt to enter any other character causes an error. The signed numeric field must be at least 2 bytes
                'long. The last (rightmost) position is reserved for the sign display (- for negative and blank for positive). You cannot type
                'characters into this position. To exit this field, use either the Field +, the Field -, or the Field Exit key. If you use the Field -
                'key to exit this field, the 5494 right-adjusts the field and inserts a negative sign in the rightmost position. If you use Field +, the
                '5494 right-adjusts the field and inserts a blank in the rightmost position.
                '
                'If the last character is negative, the zone of the low-order digit is set to X'D' before the character is sent to the AS/400 system.
                'If the last character is positive, the low-order digit is not changed. The last sign position is not sent to the AS/400 system in 
                'response to either the READ MDT or READ INPUT commands.
            End Enum
            Public Enum FFW_MandatoryFill
                None = 0
                'Accepts characters without either position adjustment or insertion of fill characters.

                Reserved1 = 1
                Reserved2 = 2
                Reserved3 = 3
                Reserved4 = 4

                'these next two are incorrectly documented on page 15.6.12.1-3 of SC30-3533-04.
                RightAdjust_ZeroFill = 5
                RightAdjust_BlankFill = 6
                'Fills all leftmost unoccupied positions of a field with the specified character; characters are right-adjust and spaces are
                'blank-filled or zero-filled. The operator must specify this as either blank or 0. The fill character appears on the workstation
                'screen.

                MandatoryFill = 7
                'If the operator enters any character into this type of field, the field must be completely filled before exiting. Any attempt to 
                'leave an unfilled field causes an error. The workstation operator can use the Dup key to fill the field. If the field is nulled when 
                'the workstation operator exits from the first position using the Field Exit or Erase Input key and the MDT bit is on, the null 
                'characters can be sent back to the AS/400 system in response to a READ command.
            End Enum

            <Serializable()> Public Structure FFW 'Field Format Word
                Dim Raw As UInt16
                Dim Bypass As Boolean
                'Entries are not allowed in this field. If the workstation operator tries to enter something into this field, an error results.

                Dim Dup_Or_Field_Mark_Enabled As Boolean
                'The 5494 repeats X'1C' from the cursor position to the end of the field when the workstation operator presses the Dup key; this 
                'displays on the workscreen as an overstruck asterisk.

                Dim Modified As Boolean

                Dim Shift_Edit_Spec As FFW_ShiftEditSpec

                Dim Auto_Enter_On_Exit As Boolean
                'The contents of all fields, including the modified READ MDT fields, are sent to the AS/400 system. The operator begins by pressing one
                'of the field exit keys or by entering the last character in last required field.

                Dim Field_Exit_Key_Required As Boolean
                'Requires the workstation operator to exit the field with a nondata key. When the operator has entered the last character, the cursor
                'remains under the character and blinks, indicating that a Field Exit key is required.

                Dim UpperCase As Boolean
                'Convert all characters to uppercase

                Dim Mandatory_Enter As Boolean
                'Requires the workstation operator to enter something in the file before the 5494 allows the Enter key to be active. The 5494
                'recognizes the state of these fields by checking the MDT bit for the field. If the workstation operator tries to bypass the field using 
                'a Field +, Field -, or Field Exit key, an error occurs.

                Dim Mandatory_Fill As FFW_MandatoryFill

            End Structure

            Private Enum FieldType As Byte
                Unknown = 0
                FieldFormatWord = 1
                FieldControlWord = 2
                Attribute = 3
            End Enum

            Private Function GetFieldType(ByVal b As Byte) As FieldType
                If (b And &H80) = &H80 Then Return FieldType.FieldControlWord 'high bit is 1
                If (b And &HC0) = &H40 Then Return FieldType.FieldFormatWord 'high 2 bits are 01
                If (b And &HE0) = &H20 Then Return FieldType.Attribute 'high 3 bits are 001
                Return FieldType.Unknown
            End Function

            Public Sub New(ByRef buf() As Byte, ByVal Offset As Integer, DefaultForeColor As String, BackColor As String)
                ReDim Me.FieldControlWords(-1)
                Me.IsInputField = False
                Dim offs As Integer = Offset
                If Me.GetFieldType(buf(offs)) = FieldType.FieldFormatWord Then
                    Me.IsInputField = True
                    Dim FFW As UInt16 = (CUShort(buf(offs)) << 8) + buf(offs + 1)
                    With Me.FieldFormatWord
                        .Raw = FFW
                        .Bypass = CBool(FFW And &H2000) 'bit 2
                        .Dup_Or_Field_Mark_Enabled = CBool(FFW And &H1000) 'bit 3
                        .Modified = CBool(FFW And &H800) 'bit 4
                        .Shift_Edit_Spec = CType((FFW And &H700) >> 8, FFW_ShiftEditSpec) 'bits 5-7
                        .Auto_Enter_On_Exit = CBool(FFW And &H80) 'bit 8
                        .Field_Exit_Key_Required = CBool(FFW And &H40) 'bit 9
                        .UpperCase = CBool(FFW And &H20) 'bit 10
                        .Mandatory_Enter = CBool(FFW And 8) 'bit 12
                        .Mandatory_Fill = CType((FFW And 7), FFW_MandatoryFill) 'bits 13-15
                    End With
                    offs += 2
                    Do While Me.GetFieldType(buf(offs)) = FieldType.FieldControlWord
                        Dim Keep_This_FCW As Boolean = True
                        Dim _fcw As FCW
                        With _fcw
                            .Raw = (CUShort(buf(offs)) << 8) + buf(offs + 1)
                            offs += 2
                            .Type = .Raw >> 8
                            Select Case .Type
                                Case FCW_Type.Highlighted
                                    Dim DisplayType As HighlightField_DisplayType = (.Raw And &HFF) >> 6
                                    Dim Cursor_Is_Visible As Boolean = CBool((.Raw And &H20) >> 5) 'XXX we're currently throwing this away
                                    Select Case DisplayType
                                        Case HighlightField_DisplayType.Both, HighlightField_DisplayType.Color
                                            .Data = (.Raw And &H1F) Or &H20 '.Data is now the attribute for the highlighted field
                                        Case Else
                                            'ignore this FCW
                                            Keep_This_FCW = False
                                    End Select
                                Case Else
                                    'XXX lots of work to do here for other data types
                                    .Data = .Raw And &HFF
                            End Select
                        End With
                        If Keep_This_FCW Then
                            ReDim Preserve Me.FieldControlWords(Me.FieldControlWords.Length)
                            Me.FieldControlWords(Me.FieldControlWords.Length - 1) = _fcw
                        End If
                    Loop
                End If
                If Me.GetFieldType(buf(offs)) = FieldType.Attribute Then
                    Me.LeadingFieldAttribute = New Emulator.EmulatorScreen.FieldAttribute(buf(offs), DefaultForeColor, BackColor)
                    offs += 1
                Else
                    Throw New ApplicationException("Unexpected byte encountered while parsing Start-of-Field header")
                End If
                Me.FieldLength = (CUShort(buf(offs)) << 8) + buf(offs + 1)
                offs += 2
                Me.Length = offs - Offset
            End Sub

        End Class 'StartOfField_Header
    End Class 'EmulatorScreen
End Class 'Emulator

