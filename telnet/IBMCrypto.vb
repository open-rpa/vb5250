'
' Copyright 2012, 2013 Alec Skelly
'
' This file is part of Telnet, a VB.Net implementation of the Telnet protocol.
'
' Telnet is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' Telnet is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with Telnet. If not, see <http://www.gnu.org/licenses/>.
' 

Public Class IBMCrypto_Deprecated
    Public Shared Function EncryptSHA1(ByVal Username As String, ByVal Password As String, ByVal ServerRSeed As String, ByVal ClientRSeed As String) As String
        'Test values from RFC4777
        'Dim Username As String = "user123"
        'Dim Password As String = "AbCdEfGh123?+"
        'Dim ServerSeed() As Byte = New Byte() {&H3E, &H3A, &H71, &HC7, &H87, &H95, &HE5, &HF5}  '3E 3A 71 C7 87 95 E5 F5
        'Dim ClientSeed() As Byte = New Byte() {&HB1, &HC8, 6, &HD5, &HD3, &H77, &HD9, &H94} 'B1 C8 06 D5 D3 77 D9 94

        If String.IsNullOrEmpty(Username) Then Throw New ArgumentNullException("Username")
        If String.IsNullOrEmpty(Password) Then Throw New ArgumentNullException("Password")
        If String.IsNullOrEmpty(ServerRSeed) Then Throw New ArgumentNullException("ServerRSeed")
        If String.IsNullOrEmpty(ClientRSeed) Then Throw New ArgumentNullException("ClientRSeed")
        If ServerRSeed.Length <> 16 Then Throw New ArgumentOutOfRangeException("ServerRSeed", "ServerRSeed must be 16 characters of 0 thru F")
        If ClientRSeed.Length <> 16 Then Throw New ArgumentOutOfRangeException("ClientRSeed", "ClientRSeed must be 16 characters of 0 thru F")

        Dim ServerSeed() As Byte = GetBytesFromHex(ServerRSeed)
        Dim ClientSeed() As Byte = GetBytesFromHex(ClientRSeed)

        Dim PwSEQs() As Byte = New Byte() {0, 0, 0, 0, 0, 0, 0, 1}

        Dim UsernamePad As String = Username.ToUpper.PadRight(10, " ")
        Dim UsernameUnicode() As Byte = System.Text.Encoding.BigEndianUnicode.GetBytes(UsernamePad)
        Dim PasswordUnicode() As Byte = System.Text.Encoding.BigEndianUnicode.GetBytes(Password)

        Dim SHA1 As New System.Security.Cryptography.SHA1Managed

        Dim mstream As System.IO.MemoryStream

        'PW_token = SHA-1(uppercase_unicode_userid, /* 20 bytes */
        '               unicode_password)           /* from 2 to 256 bytes */
        mstream = New System.IO.MemoryStream
        mstream.Write(UsernameUnicode, 0, UsernameUnicode.Length)
        mstream.Write(PasswordUnicode, 0, PasswordUnicode.Length)
        mstream.Position = 0
        Dim PW_token() As Byte = SHA1.ComputeHash(mstream)
        mstream.Close()

        'PW_SUB = SHA-1(PW_token,                   /* 20 bytes */
        '       serverseed,                         /*  8 bytes */
        '       clientseed,                         /*  8 bytes */
        '       uppercase_unicode_userid,           /* 20 bytes */
        '       PWSEQ)                              /*  8 bytes */
        mstream = New System.IO.MemoryStream
        mstream.Write(PW_token, 0, PW_token.Length)
        mstream.Write(ServerSeed, 0, ServerSeed.Length)
        mstream.Write(ClientSeed, 0, ClientSeed.Length)
        mstream.Write(UsernameUnicode, 0, UsernameUnicode.Length)
        mstream.Write(PwSEQs, 0, PwSEQs.Length)
        mstream.Position = 0
        Dim PW_SUB_Bytes() As Byte = SHA1.ComputeHash(mstream)
        mstream.Close()

        'MsgBox(BitConverter.ToString(PW_SUB_Bytes).Replace("-", "") & vbCr & "E7FAB5F034BEDA42E91F439DD07532A24140E3DD")
        Return BitConverter.ToString(PW_SUB_Bytes).Replace("-", "")
    End Function

    Public Shared Function EncryptDES(ByVal Username As String, ByVal Password As String, ByVal ServerRSeed As String, ByVal ClientRSeed As String) As String

        'XXX This function produces the wrong result when Password begins with a number, regardless of password length.  The reason is unknown.

        'Test values from RFC4777
        'ID:     USER123
        'Password: ABCDEFG
        'Server seed: '7D4C2319F28004B2'X
        'Client seed: '08BEF662D851F4B1'X
        'PWSEQs:       1     (PWSEQs is a sequence number needed in the
        '                     7-step encryption, and it is always one)
        '
        ' DES Encrypted Password should be: '5A58BD50E4DD9B5F'X
        '
        'Dim Username As String = "user123"
        'Dim Password As String = "ABCDEFG"
        'Dim ServerSeed() As Byte = New Byte() {&H7D, &H4C, &H23, &H19, &HF2, &H80, &H4, &HB2}
        'Dim ClientSeed() As Byte = New Byte() {&H8, &HBE, &HF6, &H62, &HD8, &H51, &HF4, &HB1}

        If String.IsNullOrEmpty(Username) Then Throw New ArgumentNullException("Username")
        If String.IsNullOrEmpty(Password) Then Throw New ArgumentNullException("Password")
        If String.IsNullOrEmpty(ServerRSeed) Then Throw New ArgumentNullException("ServerRSeed")
        If String.IsNullOrEmpty(ClientRSeed) Then Throw New ArgumentNullException("ClientRSeed")
        If ServerRSeed.Length <> 16 Then Throw New ArgumentOutOfRangeException("ServerRSeed", "ServerRSeed must be 16 characters of 0 thru F")
        If ClientRSeed.Length <> 16 Then Throw New ArgumentOutOfRangeException("ClientRSeed", "ClientRSeed must be 16 characters of 0 thru F")

        Dim ServerSeed() As Byte = GetBytesFromHex(ServerRSeed)
        Dim ClientSeed() As Byte = GetBytesFromHex(ClientRSeed)

        Dim PwSEQs() As Byte = New Byte() {0, 0, 0, 0, 0, 0, 0, 1}

        Dim Padded_UserID(7) As Byte
        If Username.Length > 8 Then
            Dim UserID() As Byte = System.Text.Encoding.GetEncoding(37).GetBytes(Username.ToUpper.PadRight(10)) 'US English uppercase is required by RFC4777
            UserID(0) = (UserID(0) And &H3F) Or ((UserID(0) And &HC0) Xor (UserID(8) And &HC0))
            UserID(1) = (UserID(1) And &H3F) Or ((UserID(1) And &HC0) Xor ((UserID(8) << 2) And &HC0))
            UserID(2) = (UserID(2) And &H3F) Or ((UserID(2) And &HC0) Xor ((UserID(8) << 4) And &HC0))
            UserID(3) = (UserID(3) And &H3F) Or ((UserID(3) And &HC0) Xor ((UserID(8) << 6) And &HC0))
            UserID(4) = (UserID(4) And &H3F) Or ((UserID(4) And &HC0) Xor (UserID(9) And &HC0))
            UserID(5) = (UserID(5) And &H3F) Or ((UserID(5) And &HC0) Xor ((UserID(9) << 2) And &HC0))
            UserID(6) = (UserID(6) And &H3F) Or ((UserID(6) And &HC0) Xor ((UserID(9) << 4) And &HC0))
            UserID(7) = (UserID(7) And &H3F) Or ((UserID(7) And &HC0) Xor ((UserID(9) << 6) And &HC0))
            Array.Copy(UserID, 0, Padded_UserID, 0, 8)
        Else
            Padded_UserID = System.Text.Encoding.GetEncoding(37).GetBytes(Username.ToUpper.PadRight(8))
        End If

        Dim Padded_PW() As Byte = System.Text.Encoding.GetEncoding(37).GetBytes(Password.ToUpper.PadRight(8))
        Dim PaddedPW As UInt64 = BitConverter.ToUInt64(Padded_PW, 0)
        Dim XorPW As UInt64 = PaddedPW Xor &H5555555555555555UL
        Dim ShiftResult As UInt64 = XorPW << 1
        Dim SHIFT_RESULT() As Byte = BitConverter.GetBytes(ShiftResult)
        Dim PW_TOKEN() As Byte = DES_ECB(SHIFT_RESULT, Padded_UserID)

        'Special handling for passwords of length 9 and 10.
        If Password.Length > 8 Then
            'Repeat generation of PW_TOKEN using only characters 9 and 10
            Dim Padded_PWb() As Byte = System.Text.Encoding.GetEncoding(37).GetBytes(Password.Substring(8).ToUpper.PadRight(8))
            Dim PaddedPWb As UInt64 = BitConverter.ToUInt64(Padded_PWb, 0)
            Dim XorPWb As UInt64 = PaddedPWb Xor &H5555555555555555UL
            Dim ShiftResultb As UInt64 = XorPWb << 1
            Dim SHIFT_RESULTb() As Byte = BitConverter.GetBytes(ShiftResultb)
            Dim PW_TOKENb() As Byte = DES_ECB(SHIFT_RESULTb, Padded_UserID)

            'XOR the token for characters 1-8 with the token for characters 9-10
            Dim PWTOKENa As UInt64 = BitConverter.ToUInt64(PW_TOKEN, 0)
            Dim PWTOKENb As UInt64 = BitConverter.ToUInt64(PW_TOKENb, 0)
            Dim PWTOKEN As UInt64 = PWTOKENa Xor PWTOKENb
            PW_TOKEN = BitConverter.GetBytes(PWTOKEN)
        End If

        Dim SSeed As UInt64 = BitConverter.ToUInt64(ServerSeed, 0)
        Dim Seq As UInt64 = BitConverter.ToUInt64(PwSEQs, 0)
        Dim RDrSEQ_UInt64 As UInt64 = SSeed + Seq
        Dim RDrSEQ() As Byte = BitConverter.GetBytes(RDrSEQ_UInt64)

        Dim Padded_UserID_16() As Byte = System.Text.Encoding.GetEncoding(37).GetBytes(Username.ToUpper.PadRight(16))
        Dim UserID_FirstHalf_UInt64 As UInt64 = BitConverter.ToUInt64(Padded_UserID_16, 0)
        Dim UserID_SecondHalf_UInt64 As UInt64 = BitConverter.ToUInt64(Padded_UserID_16, 8)

        'PW_SUB = DES_CBC_mode(PW_TOKEN,        /* key      */
        '                    (RDrSEQ,         /* 8 bytes  */
        '                     RDs,            /* 8 bytes  */
        '                     ID xor RDrSEQ,  /* 16 bytes */
        '                     PWSEQs,         /* 8 bytes  */
        '                     )               /* data     */
        '                    )

        Dim DES As System.Security.Cryptography.DESCryptoServiceProvider
        Dim mstream As System.IO.MemoryStream
        Dim cs As Security.Cryptography.CryptoStream

        DES = New System.Security.Cryptography.DESCryptoServiceProvider
        DES.Mode = Security.Cryptography.CipherMode.CBC
        DES.IV = New Byte() {0, 0, 0, 0, 0, 0, 0, 0}
        DES.Padding = Security.Cryptography.PaddingMode.None
        DES.Key = PW_TOKEN

        mstream = New System.IO.MemoryStream

        cs = New Security.Cryptography.CryptoStream(mstream, DES.CreateEncryptor, Security.Cryptography.CryptoStreamMode.Write)
        cs.Write(RDrSEQ, 0, RDrSEQ.Length)
        cs.Write(ClientSeed, 0, ClientSeed.Length)
        cs.Write(BitConverter.GetBytes(UserID_FirstHalf_UInt64 Xor RDrSEQ_UInt64), 0, 8)
        cs.Write(BitConverter.GetBytes(UserID_SecondHalf_UInt64 Xor RDrSEQ_UInt64), 0, 8)
        cs.Write(PwSEQs, 0, PwSEQs.Length)
        cs.FlushFinalBlock()

        DES.Clear()
        DES = Nothing

        Dim PW_SUB_Bytes(7) As Byte
        Array.Copy(mstream.ToArray, mstream.Length - 8, PW_SUB_Bytes, 0, 8)

        cs.Close()
        cs = Nothing
        mstream.Close()
        mstream = Nothing

        'MsgBox(BitConverter.ToString(PW_SUB_Bytes).Replace("-", "") & vbCr & "5A58BD50E4DD9B5F")
        Return BitConverter.ToString(PW_SUB_Bytes).Replace("-", "")
    End Function

    Private Shared Function DES_ECB(ByVal Key() As Byte, ByVal Data() As Byte) As Byte()
        Dim DES As System.Security.Cryptography.DESCryptoServiceProvider
        DES = New System.Security.Cryptography.DESCryptoServiceProvider
        DES.Mode = Security.Cryptography.CipherMode.ECB
        DES.Padding = Security.Cryptography.PaddingMode.None
        DES.Key = Key

        Dim mstream As System.IO.MemoryStream
        mstream = New System.IO.MemoryStream

        Dim cs As Security.Cryptography.CryptoStream
        cs = New Security.Cryptography.CryptoStream(mstream, DES.CreateEncryptor, Security.Cryptography.CryptoStreamMode.Write)
        cs.Write(Data, 0, Data.Length)
        cs.FlushFinalBlock()
        cs.Close()

        DES.Clear()

        DES_ECB = mstream.ToArray
        mstream.Close()

    End Function

    Private Shared Function GetBytesFromHex(HexString As String) As Byte()
        Dim b(-1) As Byte
        If (Not String.IsNullOrEmpty(HexString)) AndAlso (HexString.Length Mod 2 = 0) Then
            ReDim b((HexString.Length \ 2) - 1)
            For i As Integer = 0 To b.Length - 1
                b(i) = Convert.ToByte(HexString.Substring(i * 2, 2), 16)
            Next
        End If
        Return b
    End Function
End Class
