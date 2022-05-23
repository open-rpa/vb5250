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
        <Serializable()> Public Class StartOfHeader_Header
            Public Length As Byte
            Public Flag As Byte
            Public Reserved As Byte
            Public Starting_Field_For_Reads As Integer
            Public Error_Row As Byte
            Private PF_Data_Inhibit As UInt32
            Public Inhibited_AID_Codes As New List(Of AID)

            Public Sub New()

            End Sub
            Public Sub New(ByRef Buf() As Byte, ByVal Offset As Integer)
                If Buf.Length - Offset >= 1 Then
                    Me.Length = Buf(Offset)
                    If (Me.Length > 0) And (Me.Length < 8) Then
                        For i As Integer = 1 To Me.Length
                            Select Case i
                                Case 1
                                    Me.Flag = Buf(Offset + i)
                                Case 2
                                    Me.Reserved = Buf(Offset + i)
                                    If Me.Reserved <> 0 Then
                                        'XXX
                                    End If
                                Case 3
                                    Me.Starting_Field_For_Reads = Buf(Offset + i)
                                Case 4
                                    Me.Error_Row = Buf(Offset + i)
                                Case 5
                                    Me.PF_Data_Inhibit = CUInt(Buf(Offset + i)) << 16
                                Case 6
                                    Me.PF_Data_Inhibit = Me.PF_Data_Inhibit + (CUInt(Buf(Offset + i)) << 8)
                                Case 7
                                    Me.PF_Data_Inhibit = Me.PF_Data_Inhibit + CUInt(Buf(Offset + i))
                            End Select
                        Next
                        For i As Integer = 0 To 23
                            Dim Inhibited As Integer = (Me.PF_Data_Inhibit >> i) And 1
                            If Inhibited > 0 Then
                                If i < 12 Then
                                    Me.Inhibited_AID_Codes.Add(AID.PF1 + i)
                                Else
                                    Me.Inhibited_AID_Codes.Add(AID.PF13 + i - 12)
                                End If
                            End If
                        Next
                    Else
                        'The following line was removed to allow sending a Negative Response from the data stream parser.
                        '       XXX It would be better to create a custom exception and catch it in the data stream parser.
                        'Throw New ApplicationException("The length parameter is outside the allowable range for a 'Start Of Header' header.")
                    End If
                Else
                    Throw New ApplicationException("The buffer is too short to contain a complete 'Start Of Header' header.")
                End If
            End Sub
        End Class 'StartOfHeader_Header
    End Class 'EmulatorScreen
End Class 'Emulator

