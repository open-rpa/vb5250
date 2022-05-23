'
' Copyright 2016 Alec Skelly
'
' This file is part of IBMiClient.
'
' IBMiClient is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' IBMiClient is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with IBMiClient. If not, see <http://www.gnu.org/licenses/>.
' 
Partial Public Class Client
    Partial Public Class Signon
        Friend Class Info_Message
            Private TextCCSID As UInt32
            Private SubstitutionDataCCSID As UInt32
            Friend Severity As UInt16
            Private MessageType() As Byte
            Private MessageID() As Byte
            Friend FileName As String
            Friend LibraryName As String
            Friend Text As String
            Private SubstitutionData() As Byte
            Friend Help As String

            'Derived fields
            Friend ReasonCode As UInt32 'SubstitionData seems to contain the Reason Code for a failed logon
            Friend ReasonText As String

            Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

            Public Sub New(ByVal DataBytes() As Byte)
                Logger.Trace("")
                If DataBytes.Length < 15 Then Throw New Exception("Insufficient data to construct a Signon_Info_Message")
                Dim Data As New System.IO.MemoryStream(DataBytes)
                Me.TextCCSID = ReadUInt32(Data)
                Logger.Debug("TextCCSID: " & Me.TextCCSID.ToString)
                Me.SubstitutionDataCCSID = ReadUInt32(Data)
                Logger.Debug("SubstitutionDataCCSID: " & Me.SubstitutionDataCCSID.ToString)
                Me.Severity = ReadUInt16(Data)
                Dim l As UInt32 = ReadUInt32(Data)
                ReDim Me.MessageType(l - 1)
                Data.Read(Me.MessageType, 0, Me.MessageType.Length)

                l = ReadUInt32(Data)
                ReDim Me.MessageID(l - 1)
                Data.Read(Me.MessageID, 0, Me.MessageID.Length)

                l = ReadUInt32(Data)
                Dim b(l - 1) As Byte
                Data.Read(b, 0, b.Length)
                b = EBCDIC_To_UTF8(b) 'XXX should translate according to TextCCSID?
                Me.FileName = System.Text.Encoding.UTF8.GetString(b).Trim

                l = ReadUInt32(Data)
                ReDim b(l - 1)
                Data.Read(b, 0, b.Length)
                b = EBCDIC_To_UTF8(b) 'XXX should translate according to TextCCSID?
                Me.LibraryName = System.Text.Encoding.UTF8.GetString(b).Trim

                l = ReadUInt32(Data)
                ReDim b(l - 1)
                Data.Read(b, 0, b.Length)
                b = EBCDIC_To_UTF8(b) 'XXX should translate according to TextCCSID?
                Me.Text = System.Text.Encoding.UTF8.GetString(b).Trim

                l = ReadUInt32(Data)
                ReDim Me.SubstitutionData(l - 1)
                Data.Read(Me.SubstitutionData, 0, Me.SubstitutionData.Length)

                'After a failed logon SubstitionData seems to contain the Reason Code.
                If Me.SubstitutionData.Length = 2 Then
                    ReDim b(Me.SubstitutionData.Length - 1)
                    Array.Copy(Me.SubstitutionData, b, Me.SubstitutionData.Length)
                    If BitConverter.IsLittleEndian Then Array.Reverse(b)
                    Me.ReasonCode = BitConverter.ToUInt16(b, 0)
                    Select Case Me.ReasonCode
                        Case 2
                            Me.ReasonText = "The password was created or last changed on a release earlier than Version 2 Release 2 or on a System/38. The method used for storing user passwords was changed in Version 2 Release 2 and passwords last changed before this release are not valid for use as protected passwords."
                        Case 713
                            Me.ReasonText = "User profile not found. It does not exist on the system."
                        Case 723
                            Me.ReasonText = "Password *NONE not allowed for protected password."
                        Case 725
                            Me.ReasonText = "Incorrect user profile name. The user profile name is not valid."
                        Case 726
                            Me.ReasonText = "User profile disabled."
                    End Select
                End If

                l = ReadUInt32(Data)
                ReDim b(l - 1)
                Data.Read(b, 0, b.Length)
                b = EBCDIC_To_UTF8(b) 'XXX should translate according to TextCCSID?
                Me.Help = System.Text.Encoding.UTF8.GetString(b).Trim

                If Data.Position <> Data.Length Then
                    Logger.Debug("!!! Some data remains in the buffer after parsing message!") 'XXX should throw here
                End If

            End Sub
        End Class
    End Class
End Class


