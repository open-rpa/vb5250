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
Public Class Client
    Partial Public Class Signon
        Friend Class Change_Password_Request
            Private Header As MessageHeader
            'Private Const CCSID As UInt32 = 13488    'https://www-01.ibm.com/software/globalization/ccsid/ccsid13488.html
            'Public AuthenticationScheme As AuthenticationScheme
            Private PasswordBytes() As Byte
            Private OldPasswordBytes() As Byte
            Private NewPasswordBytes() As Byte
            Friend UserID As String
            Private Password As String
            Private NewPassword As String
            Private ServerLevel As UInt16
            Private PasswordLevel As Byte
            Private ServerSeed() As Byte
            Private ClientSeed() As Byte

            Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

            Public Sub New(ByVal MsgHeader As MessageHeader, ClientSeed() As Byte, ServerSeed() As Byte, UserName As String, Password As String, NewPassword As String, PasswordLevel As Byte, ServerLevel As UInt16)
                Logger.Trace("")
                If MsgHeader.RequestReplyID <> MessageType.ChangePasswordRequest Then Throw New ArgumentException("The supplied header is not the correct message type")
                Me.Header = MsgHeader
                Me.UserID = UserName.ToUpper
                Me.Password = Password
                Me.NewPassword = NewPassword
                Me.PasswordLevel = PasswordLevel
                Me.ServerLevel = ServerLevel
                Me.ServerSeed = ServerSeed
                Me.ClientSeed = ClientSeed

                If PasswordLevel > 1 Then
                    Dim Sequence As UInt64 = 1
                    Dim token() As Byte = Encryption.SHA.GetToken(UserName, Password)
                    PasswordBytes = Encryption.SHA.Encrypt(UserName, token, ServerSeed, ClientSeed, Sequence)
                    NewPasswordBytes = Encryption.SHA.Encrypt_For_PasswordChangeRequest(UserName, NewPassword, token, ServerSeed, ClientSeed, Sequence)
                    token = Encryption.SHA.GetToken(UserName, NewPassword)
                    OldPasswordBytes = Encryption.SHA.Encrypt_For_PasswordChangeRequest(UserName, Password, token, ServerSeed, ClientSeed, Sequence)
                Else
                    Dim Sequence As UInt64 = 1
                    Dim token() As Byte = Encryption.DES.GetToken(UserName, Password)
                    PasswordBytes = Encryption.DES.Encrypt(UserName, token, ServerSeed, ClientSeed, Sequence)
                    Sequence += 1
                    NewPasswordBytes = Encryption.DES.Encrypt_For_PasswordChangeRequest(UserName, NewPassword, token, ServerSeed, ClientSeed, Sequence)
                    token = Encryption.DES.GetToken(UserName, NewPassword)
                    Sequence += 1
                    OldPasswordBytes = Encryption.DES.Encrypt_For_PasswordChangeRequest(UserName, Password, token, ServerSeed, ClientSeed, Sequence)
                End If

            End Sub

            Public Function ToBytes() As Byte()
                Logger.Trace("")
                Dim Data As New System.IO.MemoryStream
                Dim b() As Byte = Me.Header.ToBytes()
                Data.Write(b, 0, b.Length)

                Dim auth_type_code As Byte
                Select Case Me.PasswordLevel
                    Case Is < 2 'DES
                        auth_type_code = 1 'XXX Need an enum for these values once their meaning is understood.
                    Case Else 'SHA1
                        auth_type_code = 3
                End Select
                ReDim b(0)
                b(0) = auth_type_code
                Data.Write(b, 0, b.Length)

                b = System.Text.Encoding.UTF8.GetBytes(Me.UserID)
                WriteCP(Data, CodePoints.UserID, UTF8_To_EBCDIC(b))

                WriteCP(Data, CodePoints.Password, PasswordBytes)
                WriteCP(Data, CodePoints.OldPassword, OldPasswordBytes)
                WriteCP(Data, CodePoints.NewPassword, NewPasswordBytes)

                If Me.PasswordLevel > 1 Then 'SHA1 then
                    WriteCP(Data, CodePoints.OldPasswordLength, CUInt(Me.Password.Length * 2)) '*2 because the strings are Unicode
                    WriteCP(Data, CodePoints.NewPasswordLength, CUInt(Me.NewPassword.Length * 2))
                    'WriteCP(Data, CodePoints.PasswordCCSID, CCSID)
                End If

                If Me.ServerLevel >= 5 Then
                    WriteCP(Data, CodePoints.ReturnErrorMessages, CByte(1))
                End If

                'Overwrite the header with the final message length
                Me.Header.MessageLength = Data.Length
                b = Me.Header.ToBytes()
                Data.Position = 0
                Data.Write(b, 0, b.Length)
                '

                Return Data.ToArray
            End Function

        End Class
    End Class
End Class
