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
        Friend Class Exchange_Attribute_Request
            Private Header As MessageHeader
            Friend ClientVersion As UInt32
            Friend ClientLevel As UInt16
            Friend ClientSeed(7) As Byte

            Public Sub New(ByVal MsgHeader As MessageHeader)
                Me.New(MsgHeader, Encryption.RandomSeed)
            End Sub

            Public Sub New(ByVal MsgHeader As MessageHeader, ByVal ClientSeed() As Byte)
                Me.New(MsgHeader, 1, 2, ClientSeed)
            End Sub

            Public Sub New(ByVal MsgHeader As MessageHeader, ByVal ClientVersion As UInt32, ByVal ClientLevel As UInt16, ByVal ClientSeed() As Byte)
                If ClientSeed.Length <> 8 Then Throw New ArgumentException("ClientSeed must be an array of 8 bytes")
                If MsgHeader.RequestReplyID <> MessageType.ExchangeAttributeRequest Then Throw New ArgumentException("The supplied header is not the correct message type")
                Me.Header = MsgHeader
                Me.ClientVersion = ClientVersion
                Me.ClientLevel = ClientLevel
                Me.ClientSeed = ClientSeed
            End Sub

            Public Function ToBytes() As Byte()
                Dim Data As New System.IO.MemoryStream
                Dim b() As Byte = Me.Header.ToBytes()
                Data.Write(b, 0, b.Length)
                WriteCP(Data, CodePoints.Version, Me.ClientVersion)
                WriteCP(Data, CodePoints.Level, Me.ClientLevel)
                WriteCP(Data, CodePoints.Seed, Me.ClientSeed)
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

