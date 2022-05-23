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
    'Private Const CCSID As UInt32 = 13488    'https://www-01.ibm.com/software/globalization/ccsid/ccsid13488.html
    Private Const CCSID As UInt32 = 37       'https://www-01.ibm.com/software/globalization/ccsid/ccsid37.html

    Friend Enum MessageType As UInt16
        Unknown1_Client = &H7001
        Unknown1_Server = &HF001
        Unknown2_Client = &H7002
        Unknown2_Server = &HF002
        Unknown3_Client = &H1001
        Unknown3_Server = &H8001

        Unknown4_Client = &H1003 'these two were observed twice in the same session.  Negotiation?
        Unknown4_Server = &H8003 '

        Unknown5_Client = &H1004 'terminates conversation

        ExchangeAttributeRequest = &H7003   'http://grepcode.com/file/repo1.maven.org/maven2/net.sf.jt400/jt400/8.5/com/ibm/as400/access/SignonExchangeAttributeReq.java
        ExchangeAttributeReply = &HF003     'http://grepcode.com/file/repo1.maven.org/maven2/net.sf.jt400/jt400/8.5/com/ibm/as400/access/SignonExchangeAttributeRep.java
        InfoRequest = &H7004                'http://grepcode.com/file/repo1.maven.org/maven2/net.sf.jt400/jt400/8.5/com/ibm/as400/access/SignonInfoReq.java
        InfoReply = &HF004                  'http://grepcode.com/file/repo1.maven.org/maven2/net.sf.jt400/jt400/8.5/com/ibm/as400/access/SignonInfoRep.java
        ChangePasswordRequest = &H7005      'http://grepcode.com/file/repo1.maven.org/maven2/net.sf.jt400/jt400/8.5/com/ibm/as400/access/ChangePasswordReq.java
        ChangePasswordReply = &HF005        'http://grepcode.com/file/repo1.maven.org/maven2/net.sf.jt400/jt400/8.5/com/ibm/as400/access/ChangePasswordRep.java
        EndServerRequest = &H7006           'http://grepcode.com/file/repo1.maven.org/maven2/net.sf.jt400/jt400/8.5/com/ibm/as400/access/SignonEndServerReq.java

        GenAuthTokenRequestDS = &H7008      'http://grepcode.com/file/repo1.maven.org/maven2/net.sf.jt400/jt400/8.5/com/ibm/as400/access/SignonGenAuthTokenRequestDS.java
        PingRequest = &H7FFE                'http://grepcode.com/file/repo1.maven.org/maven2/net.sf.jt400/jt400/8.5/com/ibm/as400/access/SignonPingReq.java

    End Enum

    Friend Enum CodePoints As UInt16
        Version = &H1101
        Level = &H1102
        Seed = &H1103
        UserID = &H1104
        Password = &H1105
        CurrentSignonDate = &H1106
        LastSignonDate = &H1107
        ExpirationDate = &H1108
        Unknown2 = &H1109 'UInt16 = 1 in Info_Reply
        Unknown3 = &H110A 'UInt16 = 3 in Info_Reply
        OldPassword = &H110C 'Current (old) password hash in Change_Password_Request
        NewPassword = &H110D 'New password hash in Change_Password_Request
        Unknown4 = &H110E 'Byte = 3 in Info_Reply
        Unknown1 = &H110F 'Byte = 3 in Info_Request
        Unknown5 = &H1110 '8 bytes = all zeros in Info_Reply
        Unknown6 = &H1111 '8 bytes = non-zero in Info_Reply | <-----
        Unknown7 = &H1112 '8 bytes = non-zero in Info_Reply | <----- These two match
        ClientCCSID = &H1113
        ServerCCSID = &H1114
        Auth_Token = &H1115
        PasswordLevel = &H1119
        Unknown14 = &H111A '20 bytes in Info_Reply.  SHA1 hash?
        OldPasswordLength = &H111C 'Length of old password in Change_Password_Request when using SHA1
        NewPasswordLength = &H111D 'Length of new password in Change_Password_Request when using SHA1
        PasswordCCSID = &H111E 'CCSID of password in Change_Password_Request when using SHA1
        JobName = &H111F
        Unknown8 = &H1120 'Byte = 0 in Info_Reply
        Unknown9 = &H1121 'Byte = 0 in Info_Reply
        Unknown10 = &H1122 '10 bytes = all spaces in Info_Reply
        Unknown11 = &H1123 '8 bytes = all zeros in Info_Reply
        Unknown12 = &H1124 '10 bytes = all spaces in Info_Reply
        Unknown13 = &H1125 '8 bytes = all zeros in Info_Reply
        MessageCount = &H112A
        MessageList = &H112B
        ReturnErrorMessages = &H1128
        PasswordExpirationWarning = &H112C
    End Enum

    Friend Enum AuthenticationScheme As Integer
        Password = 0
        GSS_Token = 1
        Profile_Token = 2
        Identity_Token = 3
    End Enum

    Private Shared EBCDIC_Encoding As System.Text.Encoding = System.Text.Encoding.GetEncoding("IBM037")

    Public Shared Function EBCDIC_To_UTF8(ByVal b() As Byte) As Byte()
        Return System.Text.Encoding.Convert(EBCDIC_Encoding, System.Text.Encoding.UTF8, b)
    End Function

    Public Shared Function UTF8_To_EBCDIC(ByVal b() As Byte) As Byte()
        Return System.Text.Encoding.Convert(System.Text.Encoding.UTF8, EBCDIC_Encoding, b)
    End Function

    Private Shared Function ReadUInt32(ByRef Data As System.IO.Stream) As UInt32
        Dim b(3) As Byte
        Data.Read(b, 0, 4)
        If BitConverter.IsLittleEndian Then Array.Reverse(b)
        Return BitConverter.ToUInt32(b, 0)
    End Function

    Private Shared Function ReadUInt16(ByRef Data As System.IO.Stream) As UInt16
        Dim b(1) As Byte
        Data.Read(b, 0, 2)
        If BitConverter.IsLittleEndian Then Array.Reverse(b)
        Return BitConverter.ToUInt16(b, 0)
    End Function

    Private Shared Function ToBigEndianBytes(ByVal n As Object) As Byte()
        Dim b() As Byte = BitConverter.GetBytes(n)
        If BitConverter.IsLittleEndian Then Array.Reverse(b)
        Return b
    End Function

    Private Shared Function WriteCP(ByRef s As System.IO.MemoryStream, ByVal CodePoint As CodePoints, ByVal Data As Object) As UInt32
        Dim RecLen As UInt32 = 6 'LL = 4 bytes, CP = 2 bytes.
        Dim b() As Byte

        If Data.GetType Is GetType(UInt32) Or Data.GetType Is GetType(UInt16) Then
            b = ToBigEndianBytes(Data)
        ElseIf Data.GetType Is GetType(Byte) Then
            ReDim b(0)
            b(0) = Data
        ElseIf Data.GetType Is GetType(Byte()) Then
            ReDim b(Data.Length - 1)
            Array.Copy(Data, b, Data.Length)
        Else
            Throw New Exception("Unexpected data type: " & Data.GetType.Name)
        End If
        RecLen += b.Length

        Dim LL() As Byte = ToBigEndianBytes(RecLen)
        s.Write(LL, 0, LL.Length)
        Dim CP() As Byte = ToBigEndianBytes(CodePoint)
        s.Write(CP, 0, CP.Length)
        s.Write(b, 0, b.Length)
        Debug.Print("Wrote " & RecLen.ToString & " bytes for codepoint '" & CodePoint.ToString & "'.")
        Return RecLen
    End Function

    Public Sub New()

    End Sub

End Class
