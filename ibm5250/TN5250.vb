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

Imports System

'See RFC 1205
'000A 12A0 0000 0400 000B < > FFEF
'|    |    |    | |    |   |  |
'|    |    |    | |    |   |  End Of Record marker <IAC><EOR>
'|    |    |    | |    |   |
'|    |    |    | |    |   Optional data stream
'|    |    |    | |    |  
'|    |    |    | |    Opcode = Turn On Message Light ('0B'X)
'|    |    |    | |
'|    |    |    | Flags = '0000'X
'|    |    |    |
'|    |    |    Variable Header Length = '04'X
'|    |    |
'|    |    Reserved - Set to '0000'X
'|    |
'|    Record Type = General Data Stream ('12A0'X)
'|
'Logical Record Length = '000A'X for this record

Namespace TN5250
    Public Enum RecordType As UInt16
        GDS = &H12A0 'General Data Stream
    End Enum
    Public Enum Flag As UInt16
        ERR = &H8000         'bit 0.  Data stream output error.
        ATN = &H4000         'bit 1.  Attention key was pressed.
        'RESERVED2 = &h2000   'bit 2.
        'RESERVED3 = &h1000   'bit 3.
        'RESERVED4 = &h800  'bit 4.
        SRQ = &H400          'bit 5.  System Request key was pressed.
        TRQ = &H200           'bit 6.  Test Request key was pressed.
        HLP = &H100           'bit 7.  Help in Error State.  Error code is "packed decimal number" following the header.
        'RESERVED8 = &h80
        'RESERVED9 = &h40
        '...
    End Enum
    Public Enum OpCodes As Byte
        'Some of these opcodes are enumerated in RFC1205, but not described.  Others are taken from a discussion amongst the Linux tn5250 developers.
        'Apparently no documentation exists.
        None = 0
        Invite = 1
        Put = 2 'Output
        PutOrGet = 3
        SaveScreen = 4
        RestoreScreen = 5
        ReadImmediate = 6
        ReadModImmediateAlt = 7
        ReadScreen = 8
        InternallyGeneratedPut = 9
        CancelInvite = &HA
        TurnOnMessageLight = &HB
        TurnOffMessageLight = &HC
        ReadScreenWithExtendedAttributes = &HD
        ReadScreenToPrint = &HE
        ReadScreenToPrintWithExtendedAttributes = &HF
        ReadScreenToPrintWithGridlines = &H10
        ReadScreenToPrintWithGridlinesAndExtendedAttributes = &H11
        'XXX are there more?  Many of these are from http://archive.midrange.com/linux5250/199912/msg00054.html, circa 1999.
    End Enum
    Public Class Header
        Public RecLen As UInt16 'length in bytes of record, including header but before doubling (escaping) IACs and excluding terminating <IAC><EOR>.
        Public RecType As RecordType = RecordType.GDS
        Public Reserved As UInt16 'unused per RFC 1205.  &H9000 used in connection result record per RFC4777.
        Public HeaderLen As Byte 'length in bytes of variable portion of header, which starts with this byte.  Always &H04 per RFC1205.  &H05 for connection result record per RFC4777.
        Public Flags As UInt16 'see Flags enum
        Public OpCode As OpCodes 'see OpCode enum

        Public Sub New(ByRef buf As Byte())
            If buf.Length >= 10 Then
                Me.RecLen = CUShort(CInt(buf(0)) << 8) + buf(1)
                Me.RecType = CType(CUShort(CInt(buf(2)) << 8) + buf(3), RecordType)
                Me.Reserved = CUShort(CInt(buf(4)) << 8) + buf(5)
                Me.HeaderLen = buf(6)
                Me.Flags = CUShort(CInt(buf(7)) << 8) + buf(8)
                Me.OpCode = CType(buf(9), OpCodes)
            Else
                Throw New ApplicationException("The buffer is too short to contain a complete TN5250 header.")
            End If
        End Sub

        Public Sub New(ByVal DataLength As Integer)
            Me.HeaderLen = 4
            Me.RecType = RecordType.GDS
            Me.RecLen = DataLength + Me.Length 'leave this last since me.length is calculated from me.headerlen
        End Sub

        Public Function ToBytes() As Byte()
            Dim b(Me.Length - 1) As Byte
            b(0) = Me.RecLen >> 8
            'b(0) = (Me.RecLen And &HFF00) >> 8
            b(1) = Me.RecLen And &HFF
            b(2) = Me.RecType >> 8
            'b(2) = (Me.RecType And &HFF00) >> 8
            b(3) = Me.RecType And &HFF
            b(6) = Me.HeaderLen
            b(7) = Me.Flags >> 8
            b(8) = Me.Flags And &HFF
            b(9) = Me.OpCode
            Return b
        End Function

        'Private Sub AddWithDoubledIAC(ByRef buffer As NetBuffer, ByVal ch As Byte)
        '    buffer.Add(ch)
        '    If ch = 255 Then
        '        ' IAC
        '        buffer.Add(ch)
        '    End If
        'End Sub

        'Private Sub AddWithDoubledIAC(ByRef buffer As NetBuffer, ByVal n As UInt16)
        '    Dim msb As Byte = CByte(n >> 8)
        '    Dim lsb As Byte = CByte(n And &HFF)
        '    AddWithDoubledIAC(buffer, msb)
        '    AddWithDoubledIAC(buffer, lsb)
        'End Sub

        'Public Sub AddToNetBuffer(ByRef buffer As NetBuffer, ByVal DataLen As UInt16)
        '    AddWithDoubledIAC(buffer, DataLen)
        '    AddWithDoubledIAC(buffer, RecordType.GDS)
        '    AddWithDoubledIAC(buffer, CUShort(0))
        '    AddWithDoubledIAC(buffer, CByte(4))
        '    AddWithDoubledIAC(buffer, Me.Flags)
        '    AddWithDoubledIAC(buffer, Me.OpCode)
        'End Sub

        Public Function Length() As Integer
            Return 6 + Me.HeaderLen
        End Function
    End Class


End Namespace
