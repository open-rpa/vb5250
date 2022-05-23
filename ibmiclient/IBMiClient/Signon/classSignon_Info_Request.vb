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
        Friend Class Info_Request
            Private Header As MessageHeader
            Public AuthenticationScheme As AuthenticationScheme
            Friend AuthenticationBytes() As Byte
            Friend UserID As String
            Private ServerLevel As UInt16
            Private PasswordLevel As Byte

            Public Sub New(ByVal MsgHeader As MessageHeader, ClientSeed() As Byte, ServerSeed() As Byte, UserName As String, Password As String, PasswordLevel As Byte, ServerLevel As UInt16)
                If MsgHeader.RequestReplyID <> MessageType.InfoRequest Then Throw New ArgumentException("The supplied header is not the correct message type")
                Me.Header = MsgHeader
                Me.UserID = UserName.ToUpper
                Me.PasswordLevel = PasswordLevel
                Me.ServerLevel = ServerLevel

                Me.AuthenticationScheme = AuthenticationScheme.Password 'XXX

                AuthenticationBytes = Encryption.Encrypt(PasswordLevel, Me.UserID, Password, ServerSeed, ClientSeed)

            End Sub

            Public Function ToBytes() As Byte()
                Dim Data As New System.IO.MemoryStream
                Dim b() As Byte = Me.Header.ToBytes()
                Data.Write(b, 0, b.Length)

                Dim auth_type_code As Byte
                Dim auth_bytes_CP As CodePoints
                Select Case Me.AuthenticationScheme
                    Case AuthenticationScheme.Password
                        Select Case Me.PasswordLevel
                            Case Is < 2
                                auth_type_code = 1 'XXX Need an enum for these values once their meaning is understood.
                            Case Else
                                auth_type_code = 3
                        End Select
                    Case AuthenticationScheme.GSS_Token
                        auth_type_code = 5
                    Case AuthenticationScheme.Identity_Token
                        auth_type_code = 6
                    Case Else
                        auth_type_code = 2
                End Select
                Select Case Me.AuthenticationScheme
                    Case Client.AuthenticationScheme.Password
                        auth_bytes_CP = CodePoints.Password
                    Case Else
                        auth_bytes_CP = CodePoints.Auth_Token
                End Select

                ReDim b(0)
                b(0) = auth_type_code
                Data.Write(b, 0, b.Length)

                b = System.Text.Encoding.UTF8.GetBytes(Me.UserID)
                WriteCP(Data, CodePoints.UserID, UTF8_To_EBCDIC(b))

                If Me.AuthenticationScheme = Client.AuthenticationScheme.Password Then
                    WriteCP(Data, CodePoints.Password, AuthenticationBytes)
                Else
                    WriteCP(Data, CodePoints.Auth_Token, AuthenticationBytes)
                End If

                WriteCP(Data, CodePoints.Unknown1, CByte(3)) 'XXX What does it mean?

                WriteCP(Data, CodePoints.ClientCCSID, CCSID)

                If Me.ServerLevel > 4 Then WriteCP(Data, CodePoints.ReturnErrorMessages, CByte(1))

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
