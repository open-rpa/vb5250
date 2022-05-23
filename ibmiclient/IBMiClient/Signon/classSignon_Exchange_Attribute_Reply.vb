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
        Friend Class Exchange_Attribute_Reply
            Private Header As MessageHeader
            Public ResultCode As UInt32
            Private ServerVersion As UInt32
            Public ServerLevel As UInt16
            Public ServerSeed(7) As Byte
            Public PasswordLevel As Byte
            Public JobName As String

            'Derived fields
            Public ServerVersion_Version As Integer
            Public ServerVersion_Release As Integer
            Public ServerVersion_Modification As Integer

            Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

            Public Sub New(ByVal MsgHeader As MessageHeader, ByVal DataBytes() As Byte)
                Logger.Trace("")
                If MsgHeader.RequestReplyID <> MessageType.ExchangeAttributeReply Then Throw New ArgumentException("The supplied header is not the correct message type")
                Me.Header = MsgHeader
                If DataBytes.Length < 4 Then Throw New ArgumentException("The supplied data buffer is too short to contain a result code")
                Dim Data As New System.IO.MemoryStream(DataBytes)
                Me.ResultCode = ReadUInt32(Data)
                If Me.ResultCode <> 0 Then
                    Logger.Debug("!!! Non-zero result code: " & Me.ResultCode.ToString) 'XXX
                End If
                Do While Data.Position < Data.Length - 5 'make sure we can get at least the length
                    Dim RecLen As Integer = ReadUInt32(Data)
                    If (Data.Position - 4 + RecLen) > Data.Length Then Exit Do
                    Dim CodePoint As CodePoints = ReadUInt16(Data)
                    Select Case CodePoint
                        Case CodePoints.Version
                            If RecLen <> (6 + 4) Then Throw New Exception("Server version record length incorrect")
                            Me.ServerVersion = ReadUInt32(Data)
                            Me.ServerVersion_Version = (Me.ServerVersion >> 16) And &HFFFF
                            Me.ServerVersion_Release = (Me.ServerVersion >> 8) And &HFF
                            Me.ServerVersion_Modification = Me.ServerVersion And &HFF
                        Case CodePoints.Level
                            If RecLen <> (6 + 2) Then Throw New Exception("Server level record length incorrect")
                            Me.ServerLevel = ReadUInt16(Data)
                        Case CodePoints.Seed
                            If RecLen <> (6 + Me.ServerSeed.Length) Then Throw New Exception("Server seed record length incorrect")
                            Data.Read(Me.ServerSeed, 0, Me.ServerSeed.Length)
                        Case CodePoints.PasswordLevel
                            If RecLen <> (6 + 1) Then Throw New Exception("Server PasswordLevel record length incorrect")
                            Me.PasswordLevel = Data.ReadByte
                        Case CodePoints.JobName
                            Dim b(RecLen - 6 - 1) As Byte
                            Data.Read(b, 0, b.Length)
                            b = EBCDIC_To_UTF8(b)
                            Me.JobName = System.Text.Encoding.UTF8.GetString(b)
                            Me.JobName = Me.JobName.Replace(Chr(0), " ").Trim
                        Case Else
                            Logger.Debug("Unexpected codepoint: " & CodePoint.ToString)
                            'Read the rest of the record and throw it away
                            Dim b(RecLen - 6 - 1) As Byte
                            Data.Read(b, 0, b.Length)
                    End Select
                Loop
            End Sub
        End Class
    End Class
End Class
