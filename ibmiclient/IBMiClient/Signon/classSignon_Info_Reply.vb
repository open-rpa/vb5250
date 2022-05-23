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
        Friend Class Info_Reply
            Private Header As MessageHeader
            Friend ResultCode As UInt32
            Friend CurrentSignonDate As Date
            Friend LastSignonDate As Date
            Friend ExpirationDate As Date
            Friend ExpirationWarning As UInt32
            Friend UserID As String
            Friend Messages As New List(Of Signon.Info_Message)
            Friend ServerCCSID As UInt32

            'Derived fields
            Friend ResultText As String

            Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

            Public Sub New(ByVal MsgHeader As MessageHeader, ByVal DataBytes() As Byte)
                Logger.Trace("")
                If MsgHeader.RequestReplyID <> MessageType.InfoReply Then Throw New ArgumentException("The supplied header is not the correct message type")
                Me.Header = MsgHeader
                If DataBytes.Length < 4 Then Throw New ArgumentException("The supplied data buffer is too short to contain a result code")
                Dim Data As New System.IO.MemoryStream(DataBytes)
                Me.ResultCode = ReadUInt32(Data)

                Select Case Me.ResultCode
                    Case 0
                        Me.ResultText = "Profile logged on successfully."
                    Case &H20002
                        Me.ResultText = "Profile is disabled."
                    Case &H3000B
                        Me.ResultText = "Password incorrect."
                    Case &H3000C
                        Me.ResultText = "Password incorrect.  Another failure will disable the profile."
                    Case &H3000D
                        Me.ResultText = "Password expired."
                End Select

                Do While Data.Position < Data.Length - 5 'make sure we can get at least the length
                    Dim RecLen As Integer = ReadUInt32(Data)
                    If (Data.Position - 4 + RecLen) > Data.Length Then Exit Do
                    Dim CodePoint As CodePoints = ReadUInt16(Data)
                    Select Case CodePoint
                        Case CodePoints.CurrentSignonDate
                            Me.CurrentSignonDate = GetDate(Data)
                        Case CodePoints.LastSignonDate
                            Me.LastSignonDate = GetDate(Data)
                        Case CodePoints.ExpirationDate
                            Me.ExpirationDate = GetDate(Data)
                        Case CodePoints.PasswordExpirationWarning
                            Me.ExpirationWarning = ReadUInt32(Data) 'XXX how to interpret?
                        Case CodePoints.MessageCount
                            ReadUInt16(Data)
                            'We don't really care about this?
                        Case CodePoints.MessageList
                            Dim b(RecLen - 6 - 1) As Byte
                            Data.Read(b, 0, b.Length)
                            Dim Msg As New Signon.Info_Message(b)
                            Me.Messages.Add(Msg)
                        Case CodePoints.ServerCCSID
                            Me.ServerCCSID = ReadUInt32(Data)
                        Case Else
                            Logger.Debug("Unexpected codepoint: " & CodePoint.ToString)
                            'Read the rest of the record and throw it away
                            Dim b(RecLen - 6 - 1) As Byte
                            Data.Read(b, 0, b.Length)
                            'b = EBCDIC_To_UTF8(b)
                            'Debug.Print(CodePoint.ToString & ": " & System.Text.Encoding.UTF8.GetString(b))
                    End Select
                Loop


            End Sub

            Private Function GetDate(ByRef Data As System.IO.MemoryStream)
                Logger.Trace("")
                Dim Year As UInt16 = ReadUInt16(Data)
                Dim Month As Byte = Data.ReadByte
                Dim Day As Byte = Data.ReadByte
                Dim Hour As Byte = Data.ReadByte
                Dim Minute As Byte = Data.ReadByte
                Dim Second As Byte = Data.ReadByte
                Dim Unknown As Byte = Data.ReadByte 'XXX always zero?

                Dim d As Date = Date.MinValue
                Try
                    d = New Date(Year, Month, Day, Hour, Minute, Second, New System.Globalization.GregorianCalendar)
                Catch ex As Exception
                    Logger.Error(ex, "Error parsing date: " & ex.Message)
                End Try
                Return d
            End Function

        End Class
    End Class
End Class
