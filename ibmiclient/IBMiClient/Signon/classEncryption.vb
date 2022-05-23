'
' Copyright 2012-2016 Alec Skelly
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
        Public Class Encryption
            Public Shared Function RandomSeed() As Byte()
                Dim Seed(7) As Byte
                Dim rand As New Random
                rand.NextBytes(Seed)

                'Avoid the need to escape the seed during Telnet negotiation.
                For i As Integer = 0 To Seed.Length - 1
                    If Seed(i) < 10 Then Seed(i) += 10
                    If Seed(i) = &HFF Then Seed(i) -= 10
                Next

                Return Seed
            End Function

            Private Shared Function ToBigEndianBytes(ByVal n As Object) As Byte()
                Dim b() As Byte = BitConverter.GetBytes(n)
                If BitConverter.IsLittleEndian Then Array.Reverse(b)
                Return b
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

            Public Shared Function Encrypt(ByVal PasswordLevel As Byte, ByVal Username As String, ByVal Password As String, ByVal ServerRSeed As String, ByVal ClientRSeed As String) As String
                If PasswordLevel > 1 Then
                    Return SHA.Encrypt(Username, Password, ServerRSeed, ClientRSeed)
                Else
                    Return DES.Encrypt(Username, Password, ServerRSeed, ClientRSeed)
                End If
            End Function

            Public Shared Function Encrypt(ByVal PasswordLevel As Byte, ByVal Username As String, ByVal Password As String, ByVal ServerSeed() As Byte, ByVal ClientSeed() As Byte) As Byte()
                If PasswordLevel > 1 Then
                    Return SHA.Encrypt(Username, Password, ServerSeed, ClientSeed)
                Else
                    Return DES.Encrypt(Username, Password, ServerSeed, ClientSeed)
                End If
            End Function

            Public Shared Function Encrypt(ByVal PasswordLevel As Byte, ByVal Username As String, ByVal Password_Token() As Byte, ByVal ServerSeed() As Byte, ByVal ClientSeed() As Byte, ByVal Sequence As UInt64) As Byte()
                If PasswordLevel > 1 Then
                    Return SHA.Encrypt(Username, Password_Token, ServerSeed, ClientSeed, Sequence)
                Else
                    Return DES.Encrypt(Username, Password_Token, ServerSeed, ClientSeed, Sequence)
                End If
            End Function

            Public Class SHA
                Public Shared Function Encrypt(ByVal Username As String, ByVal Password As String, ByVal ServerRSeed As String, ByVal ClientRSeed As String) As String
                    If String.IsNullOrEmpty(ServerRSeed) Then Throw New ArgumentNullException("ServerRSeed")
                    If String.IsNullOrEmpty(ClientRSeed) Then Throw New ArgumentNullException("ClientRSeed")
                    If ServerRSeed.Length <> 16 Then Throw New ArgumentOutOfRangeException("ServerRSeed", "ServerRSeed must be 16 characters of 0 thru F")
                    If ClientRSeed.Length <> 16 Then Throw New ArgumentOutOfRangeException("ClientRSeed", "ClientRSeed must be 16 characters of 0 thru F")
                    Dim ServerSeed() As Byte = GetBytesFromHex(ServerRSeed)
                    Dim ClientSeed() As Byte = GetBytesFromHex(ClientRSeed)
                    Return BitConverter.ToString(Encrypt(Username, Password, ServerSeed, ClientSeed)).Replace("-", "")
                End Function
                Public Shared Function Encrypt(ByVal Username_Unicode_Bytes() As Byte, ByVal Password_Token() As Byte, ByVal Sequence As UInt64, ByVal ServerSeed() As Byte, ByVal ClientSeed() As Byte) As Byte()
                    'Dim PwSEQs() As Byte = New Byte() {0, 0, 0, 0, 0, 0, 0, 1}
                    Dim PwSEQs() As Byte = ToBigEndianBytes(Sequence)

                    'PW_SUB = SHA-1(PW_token,                   /* 20 bytes */
                    '       serverseed,                         /*  8 bytes */
                    '       clientseed,                         /*  8 bytes */
                    '       uppercase_unicode_userid,           /* 20 bytes */
                    '       PWSEQ)                              /*  8 bytes */
                    Dim mstream As New System.IO.MemoryStream
                    mstream.Write(Password_Token, 0, Password_Token.Length)
                    mstream.Write(ServerSeed, 0, ServerSeed.Length)
                    mstream.Write(ClientSeed, 0, ClientSeed.Length)
                    mstream.Write(Username_Unicode_Bytes, 0, Username_Unicode_Bytes.Length)
                    mstream.Write(PwSEQs, 0, PwSEQs.Length)
                    mstream.Position = 0
                    Dim SHA1 As New System.Security.Cryptography.SHA1Managed
                    Dim PW_SUB_Bytes() As Byte = SHA1.ComputeHash(mstream)
                    mstream.Close()

                    Return PW_SUB_Bytes
                End Function

                Public Shared Function Encrypt(ByVal Username As String, ByVal Password_Token() As Byte, ByVal ServerSeed() As Byte, ByVal ClientSeed() As Byte, ByVal Sequence As UInt64) As Byte()
                    If String.IsNullOrEmpty(Username) Then Throw New ArgumentNullException("Username")

                    Dim UsernamePad As String = Username.ToUpper.PadRight(10, " ")
                    Dim UsernameUnicode() As Byte = System.Text.Encoding.BigEndianUnicode.GetBytes(UsernamePad)

                    Return Encrypt(UsernameUnicode, Password_Token, Sequence, ServerSeed, ClientSeed)
                End Function
                Public Shared Function Encrypt(ByVal Username As String, ByVal Password_Token() As Byte, ByVal ServerSeed() As Byte, ByVal ClientSeed() As Byte) As Byte()
                    If String.IsNullOrEmpty(Username) Then Throw New ArgumentNullException("Username")
                    Return Encrypt(Username, Password_Token, ServerSeed, ClientSeed, 1UL)
                End Function
                Public Shared Function Encrypt(ByVal Username As String, ByVal Password As String, ByVal ServerSeed() As Byte, ByVal ClientSeed() As Byte) As Byte()
                    'Test values from RFC4777
                    'Dim Username As String = "user123"
                    'Dim Password As String = "AbCdEfGh123?+"
                    'Dim ServerSeed() As Byte = New Byte() {&H3E, &H3A, &H71, &HC7, &H87, &H95, &HE5, &HF5}  '3E 3A 71 C7 87 95 E5 F5
                    'Dim ClientSeed() As Byte = New Byte() {&HB1, &HC8, 6, &HD5, &HD3, &H77, &HD9, &H94} 'B1 C8 06 D5 D3 77 D9 94

                    If String.IsNullOrEmpty(Username) Then Throw New ArgumentNullException("Username")
                    If String.IsNullOrEmpty(Password) Then Throw New ArgumentNullException("Password")

                    Return Encrypt(Username, GetToken(Username, Password), ServerSeed, ClientSeed)
                End Function

                Public Shared Function GetToken(UserName As String, Password As String) As Byte()
                    'PW_token = SHA-1(uppercase_unicode_userid, /* 20 bytes */
                    '               unicode_password)           /* from 2 to 256 bytes */
                    Dim SHA1 As New System.Security.Cryptography.SHA1Managed
                    Dim UsernamePad As String = UserName.ToUpper.PadRight(10, " ")
                    Dim UsernameUnicode() As Byte = System.Text.Encoding.BigEndianUnicode.GetBytes(UsernamePad)
                    Dim PasswordUnicode() As Byte = System.Text.Encoding.BigEndianUnicode.GetBytes(Password)
                    Dim mstream As System.IO.MemoryStream
                    mstream = New System.IO.MemoryStream
                    mstream.Write(UsernameUnicode, 0, UsernameUnicode.Length)
                    mstream.Write(PasswordUnicode, 0, PasswordUnicode.Length)
                    mstream.Position = 0
                    Dim PW_token() As Byte = SHA1.ComputeHash(mstream)
                    mstream.Close()
                    Return PW_token
                End Function

                Public Shared Function Encrypt_For_PasswordChangeRequest(ByVal UserName As String, ByVal Password As String, ByVal SHA1_Token As Byte(), ByVal ServerSeed As Byte(), ByVal ClientSeed As Byte(), ByRef Sequence As UInt64) As Byte()
                    'XXX check params

                    Dim UsernamePad As String = UserName.ToUpper.PadRight(10, " ")
                    Dim UsernameUnicode() As Byte = System.Text.Encoding.BigEndianUnicode.GetBytes(UsernamePad)
                    Dim PasswordUnicode() As Byte = System.Text.Encoding.BigEndianUnicode.GetBytes(Password.Trim)

                    Dim mstream As New System.IO.MemoryStream

                    Dim RemainingBytes As Integer = PasswordUnicode.Length
                    Dim Offset As Integer = 0
                    Do While RemainingBytes > 0
                        Sequence += 1
                        Dim b() As Byte = Encrypt(UsernameUnicode, SHA1_Token, Sequence, ServerSeed, ClientSeed)
                        For i As Integer = 0 To b.Length - 1
                            If (Offset + i) < PasswordUnicode.Length Then
                                mstream.WriteByte(b(i) Xor PasswordUnicode(Offset + i))
                            Else
                                mstream.WriteByte(b(i))
                            End If
                        Next
                        Offset += b.Length
                        RemainingBytes -= b.Length
                    Loop
                    Return mstream.ToArray

                End Function

            End Class

            Public Class DES
                Private Shared Function ValidPassword(ByVal Password As String) As Boolean
                    'http://www-01.ibm.com/support/docview.wss?uid=nas8N1016481
                    'The system supports user profile passwords with a length of 1-10 characters. The allowable characters are A-Z, 0-9 and characters $, @, # and underline. 
                    If Not String.IsNullOrEmpty(Password) Then
                        Password = Password.TrimEnd
                        If Password.Length > 0 AndAlso Password.Length < 11 Then
                            For Each c As Char In Password.ToUpper
                                If Not "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$@#_".Contains(c) Then Return False
                            Next
                            Return True
                        End If
                    End If
                    Return False
                End Function

                Public Shared Function Encrypt_For_PasswordChangeRequest(ByVal UserName As String, ByVal Password As String, ByVal DES_Token As Byte(), ByVal ServerSeed As Byte(), ByVal ClientSeed As Byte(), ByRef Sequence As UInt64) As Byte()
                    'XXX check params

                    If Not ValidPassword(Password) Then Throw New Exception("Password is not valid")

                    Dim UsernamePad As String = UserName.ToUpper.PadRight(10, " ")
                    Dim PasswordPad As String = Password.ToUpper.PadRight(10, " ")
                    Dim PasswordBytes() As Byte = System.Text.Encoding.GetEncoding(37).GetBytes(PasswordPad)

                    Dim TempHash() As Byte = Encrypt(UserName, DES_Token, ServerSeed, ClientSeed, Sequence)
                    Dim Result() As Byte = BitConverter.GetBytes(BitConverter.ToUInt64(TempHash, 0) Xor BitConverter.ToUInt64(PasswordBytes, 0))

                    If Password.Length > 8 Then
                        Sequence += 1
                        TempHash = Encrypt(UserName, DES_Token, ServerSeed, ClientSeed, Sequence)
                        Dim s As String = Password.ToUpper.Substring(8).PadRight(8)
                        PasswordBytes = System.Text.Encoding.GetEncoding(37).GetBytes(s)
                        Dim b() As Byte = BitConverter.GetBytes(BitConverter.ToUInt64(TempHash, 0) Xor BitConverter.ToUInt64(PasswordBytes, 0))
                        ReDim Preserve Result(Result.Length + b.Length - 1)
                        Array.Copy(b, 0, Result, 8, b.Length)
                    End If

                    Return Result
                End Function

                Public Shared Function GetToken(UserName As String, Password As String) As Byte()
                    If String.IsNullOrEmpty(UserName) Then Throw New ArgumentNullException("Username")
                    If String.IsNullOrEmpty(Password) Then Throw New ArgumentNullException("Password")

                    Dim Padded_UserID(7) As Byte
                    If UserName.Length > 8 Then
                        Dim UserID() As Byte = System.Text.Encoding.GetEncoding(37).GetBytes(UserName.ToUpper.PadRight(10))
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
                        Padded_UserID = System.Text.Encoding.GetEncoding(37).GetBytes(UserName.ToUpper.PadRight(8))
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

                    Return PW_TOKEN
                End Function

                Public Shared Function Encrypt(ByVal Username As String, ByVal Password As String, ByVal ServerRSeed As String, ByVal ClientRSeed As String) As String
                    If Not ValidPassword(Password) Then Throw New Exception("Password is not valid")
                    If String.IsNullOrEmpty(ServerRSeed) Then Throw New ArgumentNullException("ServerRSeed")
                    If String.IsNullOrEmpty(ClientRSeed) Then Throw New ArgumentNullException("ClientRSeed")
                    If ServerRSeed.Length <> 16 Then Throw New ArgumentOutOfRangeException("ServerRSeed", "ServerRSeed must be 16 characters of 0 thru F")
                    If ClientRSeed.Length <> 16 Then Throw New ArgumentOutOfRangeException("ClientRSeed", "ClientRSeed must be 16 characters of 0 thru F")
                    Dim ServerSeed() As Byte = GetBytesFromHex(ServerRSeed)
                    Dim ClientSeed() As Byte = GetBytesFromHex(ClientRSeed)
                    Return BitConverter.ToString(Encrypt(Username, Password, ServerSeed, ClientSeed)).Replace("-", "")
                End Function
                Public Shared Function Encrypt(ByVal Username As String, PW_TOKEN() As Byte, ByVal ServerSeed() As Byte, ByVal ClientSeed() As Byte, Sequence As UInt64) As Byte()
                    'XXX The AS400 requires DES encrypted passwords to begin with a letter.  A password beginning with a number will fail to authenticate.
                    '       JTOpen prepends "Q" to a password beginning with a number when using DES encryption.
                    '       If the resulting password is longer than 10 characters, JTOpen throws an exception.
                    '       http://grepcode.com/file/repo1.maven.org/maven2/net.sf.jt400/jt400/8.5/com/ibm/as400/access/AS400ImplRemote.java

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
                    If Sequence < 1 Then Throw New ArgumentException("Sequence cannot be less than one")

                    'Dim PwSEQs() As Byte = New Byte() {0, 0, 0, 0, 0, 0, 0, 1}
                    Dim PwSEQs() As Byte = ToBigEndianBytes(Sequence)

                    'Dim PW_TOKEN() As Byte = GetToken(Username, Password)

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

                    Return PW_SUB_Bytes

                End Function
                Public Shared Function Encrypt(ByVal Username As String, ByVal Password As String, ByVal ServerSeed() As Byte, ByVal ClientSeed() As Byte) As Byte()
                    If Not ValidPassword(Password) Then Throw New Exception("Password is not valid")
                    Return Encrypt(Username, GetToken(Username, Password), ServerSeed, ClientSeed, 1UL)
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
            End Class
        End Class
    End Class
End Class

