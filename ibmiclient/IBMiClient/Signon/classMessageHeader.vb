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
        Friend Class MessageHeader
            Public MessageLength As UInt32
            Public HeaderID As UInt16
            Public ServerID As UInt16
            Public CSInstance As UInt32
            Public CorrelationID As UInt32
            Public TemplateLength As UInt16
            Public RequestReplyID As MessageType
            Public Shared HeaderLength As Integer = 20

            Public Sub New(HeaderBytes() As Byte)
                If HeaderBytes.Length <> HeaderLength Then Throw New ArgumentException("Incorrect number of bytes to interpret as a message header")
                Dim Data As New System.IO.MemoryStream(HeaderBytes)
                Me.MessageLength = ReadUInt32(Data)
                Me.HeaderID = ReadUInt16(Data)
                Me.ServerID = ReadUInt16(Data)
                Me.CSInstance = ReadUInt32(Data)
                Me.CorrelationID = ReadUInt32(Data)
                Me.TemplateLength = ReadUInt16(Data)
                Me.RequestReplyID = ReadUInt16(Data)
            End Sub

            Public Sub New()

            End Sub

            Friend Sub New(MsgType As MessageType)
                Me.MessageLength = 0
                Me.HeaderID = 0
                Me.ServerID = &HE009 'XXX what does this mean?
                Me.CSInstance = 0
                Me.CorrelationID = 0
                Select Case MsgType
                    Case MessageType.InfoRequest, MessageType.ChangePasswordRequest
                        Me.TemplateLength = 1
                    Case MessageType.EndServerRequest
                        Me.MessageLength = MessageHeader.HeaderLength
                    Case Else
                        Me.TemplateLength = 0
                End Select
                Me.RequestReplyID = MsgType
                'Select Case MsgType
                '    Case MessageType.ExchangeAttributeRequest
                '        MessageLength = HeaderLength + Signon_Exchange_Attribute_Request.DataLength
                '    Case Else
                '        MessageLength = 0 'XXX
                'End Select
            End Sub

            Public Function ToBytes() As Byte()
                Dim Data As New System.IO.MemoryStream
                Dim b() As Byte = ToBigEndianBytes(Me.MessageLength)
                Data.Write(b, 0, b.Length)
                b = ToBigEndianBytes(Me.HeaderID)
                Data.Write(b, 0, b.Length)
                b = ToBigEndianBytes(Me.ServerID)
                Data.Write(b, 0, b.Length)
                b = ToBigEndianBytes(Me.CSInstance)
                Data.Write(b, 0, b.Length)
                b = ToBigEndianBytes(Me.CorrelationID)
                Data.Write(b, 0, b.Length)
                b = ToBigEndianBytes(Me.TemplateLength)
                Data.Write(b, 0, b.Length)
                b = ToBigEndianBytes(Me.RequestReplyID)
                Data.Write(b, 0, b.Length)
                Return Data.ToArray
            End Function
        End Class
    End Class
End Class
