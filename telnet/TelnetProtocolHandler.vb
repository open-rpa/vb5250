'
' Ported from Seva Petrov's .Net Telnet, which was ported from Matthias L. Jugel's and Marcus Meissner's JTA.
'
' Copyright 2012-2016 Alec Skelly
' (c) Seva Petrov 2002. All Rights Reserved.
' (c) Matthias L. Jugel, Marcus Mei�ner 1996-2002. All Rights Reserved.
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

Imports System
Imports System.Drawing

Namespace De.Mud.Telnet
    ''' <summary>
    ''' This is a telnet protocol handler. The handler needs implementations
    ''' for several methods to handle the telnet options and to be able to
    ''' read and write the buffer.
    ''' </summary>
    Public MustInherit Class TelnetProtocolHandler

#Region "Globals and properties"

        ''' <summary>
        ''' temporary buffer for data-telnetstuff-data transformation
        ''' </summary>
        Private tempbuf As Byte() = New Byte(-1) {}

        Private RecordStream As New System.IO.MemoryStream 'accumulates data until EOR is received

        ''' <summary>
        ''' the data sent on pressing [RETURN] \n
        ''' </summary>
        Private m_crlf As Byte() = New Byte(1) {}
        ''' <summary>
        ''' the data sent on pressing [LineFeed] \r
        ''' </summary>
        Private m_cr As Byte() = New Byte(1) {}

        ''' <summary>
        ''' Gets or sets the data sent on pressing [RETURN] \n
        ''' </summary>
        Public Property CRLF() As String
            Get
                Return System.Text.Encoding.ASCII.GetString(m_crlf)
            End Get
            Set(value As String)
                m_crlf = System.Text.Encoding.ASCII.GetBytes(value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the data sent on pressing [LineFeed] \r
        ''' </summary>
        Public Property CR() As String
            Get
                Return System.Text.Encoding.ASCII.GetString(m_cr)
            End Get
            Set(value As String)
                m_cr = System.Text.Encoding.ASCII.GetBytes(value)
            End Set
        End Property

        ''' <summary>
        ''' The current terminal type for TTYPE telnet option.
        ''' </summary>
        Protected terminalType As String = "dumb"

        'These are for NEW-ENVIRON negotiation
        Public Property VARs() As New Dictionary(Of String, String)
        Public Property USERVARs() As New Dictionary(Of String, String)
        Public Enum IBMAuthEncryptionType As Byte
            None = 0
            DES = 1
            SHA1 = 2
            Kerberos = 3
        End Enum
        Private _IBM_AuthEncryptionMethod As IBMAuthEncryptionType
        ''' <summary>
        ''' The encryption type configured on the IBM host for telnet authentication.
        ''' </summary>
        Public Property IBM_AuthEncryptionMethod As IBMAuthEncryptionType
            Get
                Return _IBM_AuthEncryptionMethod
            End Get
            Set(value As IBMAuthEncryptionType)
                _IBM_AuthEncryptionMethod = value
                Select Case value
                    Case IBMAuthEncryptionType.None, IBMAuthEncryptionType.Kerberos
                        If VARs.ContainsKey("IBMRSEED") Then
                            VARs("IBMRSEED") = Nothing
                        End If
                        'User must set VARS("IBMTICKET") to the kerberos security token as described in RFC 4777
                    Case IBMAuthEncryptionType.DES, IBMAuthEncryptionType.SHA1
                        Dim SeedIsValid As Boolean = False
                        If VARs.ContainsKey("IBMRSEED") Then
                            Dim Seed As String = VARs("IBMRSEED")
                            If Not String.IsNullOrEmpty(Seed) Then
                                If Seed.Length = 16 Then
                                    SeedIsValid = True
                                    For Each c As Char In Seed
                                        If Not "0123456789ABCDEF".Contains(c) Then
                                            SeedIsValid = False
                                            Exit For
                                        End If
                                    Next
                                End If
                            End If
                        End If
                        If Not SeedIsValid Then
                            'Dim RSeed(7) As Byte
                            'Dim rand As New Random
                            'rand.NextBytes(RSeed)
                            Dim RSeed() As Byte = IBMiClient.Client.Signon.Encryption.RandomSeed

                            If USERVARs.ContainsKey("IBMRSEED") Then
                                USERVARs("IBMRSEED") = BitConverter.ToString(RSeed).Replace("-", "")
                            Else
                                USERVARs.Add("IBMRSEED", BitConverter.ToString(RSeed).Replace("-", ""))
                            End If
                        End If
                End Select
            End Set
        End Property

        'XXX Public Property IBM_Password As System.Security.SecureString
        ''' <summary>
        ''' The password to use when bypassing the logon screen.
        ''' </summary>
        Public Property IBM_Password As String
        Private Property IBM_ServerSeed As String

        ''' <summary>
        ''' Send credentials during telnet option negotiation. Normally bypasses the logon screen on the host.
        ''' </summary>
        Public Property IBM_BypassLogonScreen As Boolean

        ''' <summary>
        ''' The window size of the terminal for the NAWS telnet option.
        ''' </summary>
        Protected windowSize As Size = Size.Empty

        ''' <summary>
        ''' Set the local echo option of telnet.
        ''' </summary>
        ''' <param name="echo">true for local echo, false for no local echo</param>
        Protected MustOverride Sub SetLocalEcho(echo As Boolean)

        ''' <summary>
        ''' Generate an EOR (end of record) request. For use by prompt displaying.
        ''' </summary>
        Protected MustOverride Sub NotifyEndOfRecord(ByVal Buf() As Byte)

        ''' <summary>
        ''' Send data to the remote host.
        ''' </summary>
        ''' <param name="b">array of bytes to send</param>
        Protected MustOverride Sub Write(b As Byte())

        ''' <summary>
        ''' Send one byte to the remote host.
        ''' </summary>
        ''' <param name="b">the byte to be sent</param>
        Private Sub Write(b As Byte)
            Write(New Byte() {b})
        End Sub

        ''' <summary>
        ''' Reset the protocol handler. This may be necessary after the
        ''' connection was closed or some other problem occured.
        ''' </summary>
        Protected Sub Reset()
            neg_state = 0
            receivedDX = New Byte(255) {}
            sentDX = New Byte(255) {}
            receivedWX = New Byte(255) {}
            sentWX = New Byte(255) {}
        End Sub

#End Region

        ''' <summary>
        ''' Create a new telnet protocol handler.
        ''' </summary>
        Public Sub New()
            Reset()

            m_crlf(0) = 13
            m_crlf(1) = 10
            m_cr(0) = 13
            m_cr(1) = 0
        End Sub

        ''' <summary>
        ''' state variable for telnet negotiation reader
        ''' </summary>
        Private neg_state As Byte = 0

        ''' <summary>
        ''' What IAC SB we are handling right now
        ''' </summary>
        Private current_sb As Byte

#Region "Telnet protocol codes"

        ' constants for the negotiation state
        Private Const STATE_DATA As Byte = 0
        Private Const STATE_IAC As Byte = 1
        Private Const STATE_IACSB As Byte = 2
        Private Const STATE_IACWILL As Byte = 3
        Private Const STATE_IACDO As Byte = 4
        Private Const STATE_IACWONT As Byte = 5
        Private Const STATE_IACDONT As Byte = 6
        Private Const STATE_IACSBIAC As Byte = 7
        Private Const STATE_IACSBDATA As Byte = 8
        Private Const STATE_IACSBDATAIAC As Byte = 9

        ''' <summary>
        ''' IAC - init sequence for telnet negotiation.
        ''' </summary>
        Private Const IAC As Byte = 255
        ''' <summary>
        ''' [IAC] End Of Record
        ''' </summary>
        Private Const EOR As Byte = 239
        ''' <summary>
        ''' [IAC] WILL
        ''' </summary>
        Private Const WILL As Byte = 251
        ''' <summary>
        ''' [IAC] WONT
        ''' </summary>
        Private Const WONT As Byte = 252
        ''' <summary>
        ''' [IAC] DO
        ''' </summary>
        Private Const [DO] As Byte = 253
        ''' <summary>
        ''' [IAC] DONT
        ''' </summary>
        Private Const DONT As Byte = 254
        ''' <summary>
        ''' [IAC] Sub Begin
        ''' </summary>
        Private Const SB As Byte = 250
        ''' <summary>
        ''' [IAC] Sub End
        ''' </summary>
        Private Const SE As Byte = 240
        ''' <summary>
        ''' Telnet option: binary mode
        ''' </summary>
        Private Const TELOPT_BINARY As Byte = 0
        ' binary mode 
        ''' <summary>
        ''' Telnet option: echo text
        ''' </summary>
        Private Const TELOPT_ECHO As Byte = 1
        ' echo on/off 
        ''' <summary>
        ''' Telnet option: sga
        ''' </summary>
        Private Const TELOPT_SGA As Byte = 3
        ' supress go ahead 
        ''' <summary>
        ''' Telnet option: End Of Record
        ''' </summary>
        Private Const TELOPT_EOR As Byte = 25
        ' end of record 
        ''' <summary>
        ''' Telnet option: Negotiate About Window Size
        ''' </summary>
        Private Const TELOPT_NAWS As Byte = 31
        ' NA-WindowSize
        ''' <summary>
        ''' Telnet option: Terminal Type
        ''' </summary>
        Private Const TELOPT_TTYPE As Byte = 24
        ' terminal type 

        Private Const TELOPT_NEW_ENVIRON As Byte = 39

        Private Shared IACWILL As Byte() = {IAC, WILL}
        Private Shared IACWONT As Byte() = {IAC, WONT}
        Private Shared IACDO As Byte() = {IAC, [DO]}
        Private Shared IACDONT As Byte() = {IAC, DONT}
        Private Shared IACSB As Byte() = {IAC, SB}
        Private Shared IACSE As Byte() = {IAC, SE}

        ''' <summary>
        ''' Telnet option qualifier 'IS'
        ''' </summary>
        Private Shared TELQUAL_IS As Byte = CByte(0)
        ''' <summary>
        ''' Telnet option qualifier 'SEND'
        ''' </summary>
        Private Shared TELQUAL_SEND As Byte = CByte(1)

        Private Shared TELQUAL_INFO As Byte = CByte(2)
        Private Shared TELQUAL_VAR As Byte = CByte(0)
        Private Shared TELQUAL_VALUE As Byte = CByte(1)
        Private Shared TELQUAL_ESC As Byte = CByte(2)
        Private Shared TELQUAL_USERVAR As Byte = CByte(3)

#End Region

        ''' <summary>
        ''' What IAC DO(NT) request do we have received already ?
        ''' </summary>
        Private receivedDX As Byte()
        ''' <summary>
        ''' What IAC WILL/WONT request do we have received already ?
        ''' </summary>
        Private receivedWX As Byte()
        ''' <summary>
        ''' What IAC DO/DONT request do we have sent already ?
        ''' </summary>
        Private sentDX As Byte()
        ''' <summary>
        ''' What IAC WILL/WONT request do we have sent already ?
        ''' </summary>
        Private sentWX As Byte()

#Region "The actual negotiation handling for the telnet protocol"

        ''' <summary>
        ''' Send a Telnet Escape character
        ''' </summary>
        ''' <param name="code">IAC code</param>
        Protected Sub SendTelnetControl(code As Byte)
            Dim b As Byte() = New Byte(1) {}

            b(0) = IAC
            b(1) = code
            Write(b)
        End Sub

        ''' <summary>
        ''' Handle an incoming IAC SB type bytes IAC SE
        ''' </summary>
        ''' <param name="type">type of SB</param>
        ''' <param name="sbdata">byte array as bytes</param>
        ''' <param name="sbcount">nr of bytes. may be 0 too</param>
        Private Sub HandleSB(type As Byte, sbdata As Byte(), sbcount As Integer)
            Select Case type
                Case TELOPT_TTYPE
                    If sbcount > 0 AndAlso sbdata(0) = TELQUAL_SEND Then
                        Dim b(6 + terminalType.Length - 1) As Byte
                        b(0) = IAC
                        b(1) = SB
                        b(2) = TELOPT_TTYPE
                        b(3) = TELQUAL_IS
                        Dim tt() As Byte = System.Text.Encoding.ASCII.GetBytes(terminalType)
                        Array.Copy(tt, 0, b, 4, tt.Length)
                        b(4 + tt.Length) = IAC
                        b(4 + tt.Length + 1) = SE
                        Write(b)
                    End If
                    Exit Select
                Case TELOPT_NEW_ENVIRON
                    If sbcount > 0 AndAlso sbdata(0) = TELQUAL_SEND Then
                        Dim NE_STATE_NONE As Integer = -1 'arbitrary value to indicate no other state
                        Dim NE_STATE_VAR As Integer = TELQUAL_VAR
                        Dim NE_STATE_USERVAR As Integer = TELQUAL_USERVAR
                        Dim NE_STATE_VARNAME As Integer = -2
                        Dim NE_State As Integer = NE_STATE_NONE
                        Dim NE_Reply As New System.IO.MemoryStream
                        NE_Reply.WriteByte(IAC)
                        NE_Reply.WriteByte(SB)
                        NE_Reply.WriteByte(TELOPT_NEW_ENVIRON)
                        NE_Reply.WriteByte(TELQUAL_IS)
                        'Dim VarName As String = Nothing
                        Dim VarNameStream As New System.IO.MemoryStream
                        Dim Requested_Var_Type As Integer = TELQUAL_USERVAR
                        Dim Dict As Dictionary(Of String, String) = Nothing
                        For i As Integer = 1 To sbcount - 1
                            If (sbdata(i) = NE_STATE_VAR) Or (sbdata(i) = NE_STATE_USERVAR) Or (i = sbcount - 1) Then
                                Select Case NE_State
                                    Case NE_STATE_VAR, NE_STATE_USERVAR
                                        'the server is requesting all variables of the type specified in NE_State
                                        For Each env As KeyValuePair(Of String, String) In Dict
                                            NE_GetEnvironmentValueBytes(NE_State, env.Key, env.Value).WriteTo(NE_Reply)
                                        Next
                                        If (i = sbcount - 1) Then
                                            NE_State = sbdata(i)
                                            If NE_State = NE_STATE_VAR Then Dict = VARs Else Dict = USERVARs
                                            For Each env As KeyValuePair(Of String, String) In Dict
                                                NE_GetEnvironmentValueBytes(NE_State, env.Key, env.Value).WriteTo(NE_Reply)
                                            Next
                                        End If
                                    Case NE_STATE_VARNAME
                                        'the server is requesting a specific variable
                                        'If i = sbcount - 1 Then VarName += Chr(sbdata(i)) 'grab the last character
                                        If i = sbcount - 1 Then VarNameStream.WriteByte(sbdata(i)) 'grab the last character
                                        Dim VarName As String = System.Text.Encoding.UTF8.GetString(VarNameStream.ToArray) 'RFC4777 specifies US ASCII but RFC854 seems open to other character sets.
                                        If VarName IsNot Nothing Then

                                            If Me.IBM_BypassLogonScreen Then
                                                If VarName.Length > 8 Then
                                                    If VarName.Substring(0, 8) = "IBMRSEED" Then
                                                        If VarNameStream.Length = 16 Then
                                                            Dim b(7) As Byte
                                                            VarNameStream.Position = 8
                                                            VarNameStream.Read(b, 0, 8)
                                                            Me.IBM_ServerSeed = BitConverter.ToString(b).Replace("-", "")
                                                        Else
                                                            'this should never happen.
                                                            'authentication will fail.
                                                            Me.IBM_ServerSeed = "0000000000000000"
                                                        End If
                                                        Dim SubsPW As String = Nothing
                                                        Dim UserName As String = "NOBODY"
                                                        If VARs.ContainsKey("USER") Then UserName = VARs("USER")
                                                        Dim ClientSeed As String = "0000000000000000"
                                                        If USERVARs.ContainsKey("IBMRSEED") Then ClientSeed = USERVARs("IBMRSEED")
                                                        Select Case Me.IBM_AuthEncryptionMethod
                                                            Case IBMAuthEncryptionType.None
                                                                SubsPW = Me.IBM_Password
                                                            Case IBMAuthEncryptionType.DES
                                                                SubsPW = IBMiClient.Client.Signon.Encryption.DES.Encrypt(UserName, Me.IBM_Password, Me.IBM_ServerSeed, ClientSeed)
                                                            Case IBMAuthEncryptionType.SHA1
                                                                SubsPW = IBMiClient.Client.Signon.Encryption.SHA.Encrypt(UserName, Me.IBM_Password, Me.IBM_ServerSeed, ClientSeed)
                                                        End Select
                                                        If USERVARs.ContainsKey("IBMSUBSPW") Then
                                                            USERVARs("IBMSUBSPW") = SubsPW
                                                        Else
                                                            USERVARs.Add("IBMSUBSPW", SubsPW)
                                                        End If

                                                    End If
                                                End If
                                            End If

                                            If Dict.ContainsKey(VarName) Then
                                                NE_GetEnvironmentValueBytes(Requested_Var_Type, VarName, Dict(VarName)).WriteTo(NE_Reply)
                                            End If
                                            VarName = Nothing
                                            VarNameStream.Close()
                                            VarNameStream = New System.IO.MemoryStream
                                        End If
                                End Select
                                NE_State = sbdata(i)
                                If NE_State = NE_STATE_VAR Then Dict = VARs Else Dict = USERVARs
                            ElseIf NE_State = NE_STATE_VARNAME Then
                                'VarName += Chr(sbdata(i))
                                VarNameStream.WriteByte(sbdata(i))
                            Else
                                If (NE_State = NE_STATE_VAR) Or (NE_State = NE_STATE_USERVAR) Then
                                    'VarName += Chr(sbdata(i))
                                    VarNameStream.WriteByte(sbdata(i))
                                    Requested_Var_Type = NE_State
                                    NE_State = NE_STATE_VARNAME
                                End If
                            End If
                        Next
                        NE_Reply.WriteByte(IAC)
                        NE_Reply.WriteByte(SE)
                        Write(NE_Reply.ToArray)
                    End If
            End Select
        End Sub
        Private Function NE_EscapeEnvironmentValueBytes(ByVal Value() As Byte) As Byte()
            'RFC1572:
            '      If an IAC is contained between the IS and the IAC SE,
            'it must be sent as IAC IAC.  If a variable or a value contains a
            'VAR, it must be sent as ESC VAR.  If a variable or a value
            'contains a USERVAR, it must be sent as ESC USERVAR.  If a variable
            'or a value contains a VALUE, it must be sent as ESC VALUE.  If a
            'variable or a value contains an ESC, it must be sent as ESC ESC.

            'XXX The AS400 doesn't seem to tolerate escaping of IBMRSEED.  
            '    Make sure the seed doesn't contain any reserved bytes.

            Dim s As New System.IO.MemoryStream
            For Each b As Byte In Value
                Select Case b
                    Case IAC
                        s.WriteByte(IAC)
                        s.WriteByte(IAC)
                    Case TELQUAL_VAR, TELQUAL_USERVAR, TELQUAL_VALUE, TELQUAL_ESC
                        s.WriteByte(TELQUAL_ESC)
                        s.WriteByte(b)
                    Case Else
                        s.WriteByte(b)
                End Select
            Next
            NE_EscapeEnvironmentValueBytes = s.ToArray
            s.Close()
        End Function
        Private Function NE_GetEnvironmentValueBytes(ByVal VarType As Byte, ByVal VarName As String, ByVal VarValue As String) As System.IO.MemoryStream
            Dim ReplyStream As New System.IO.MemoryStream
            ReplyStream.WriteByte(VarType)
            Dim b() As Byte = StringToBytes(VarName)
            ReplyStream.Write(b, 0, b.Length)
            ReplyStream.WriteByte(TELQUAL_VALUE)
            If VarValue IsNot Nothing Then
                Dim c() As Byte
                Select Case VarName
                    Case "IBMRSEED", "IBMTICKET" 'variables with hexadecimal values
                        c = HexToBytes(VarValue)
                    Case "IBMSUBSPW" 'data format depends on encryption method
                        Select Case Me.IBM_AuthEncryptionMethod
                            Case IBMAuthEncryptionType.None
                                c = StringToBytes(VarValue)
                            Case IBMAuthEncryptionType.DES, IBMAuthEncryptionType.SHA1
                                c = HexToBytes(VarValue)
                        End Select
                    Case Else 'variables with string values
                        c = StringToBytes(VarValue)
                End Select
                c = NE_EscapeEnvironmentValueBytes(c)
                ReplyStream.Write(c, 0, c.Length)
            End If
            Return ReplyStream
        End Function
        Private Function StringToBytes(ByVal s As String) As Byte()
            Dim b(s.Length - 1) As Byte
            For i As Integer = 0 To s.Length - 1
                b(i) = Asc(s.Chars(i))
            Next
            Return b
        End Function
        Private Function HexToBytes(ByVal HexString As String) As Byte()
            Dim b(-1) As Byte
            If (Not String.IsNullOrEmpty(HexString)) AndAlso (HexString.Length Mod 2 = 0) Then
                ReDim b((HexString.Length \ 2) - 1)
                For i As Integer = 0 To b.Length - 1
                    b(i) = Convert.ToByte(HexString.Substring(i * 2, 2), 16)
                Next
            End If
            Return b
        End Function

        ''' <summary>
        ''' Transpose special telnet codes like 0xff or newlines to values
        ''' that are compliant to the protocol. This method will also send
        ''' the buffer immediately after transposing the data.
        ''' </summary>
        ''' <param name="buf">the data buffer to be sent</param>
        Protected Sub Transpose(buf As Byte())
            Dim i As Integer

            Dim nbuf As Byte(), xbuf As Byte()
            Dim nbufptr As Integer = 0
            nbuf = New Byte(buf.Length * 2 - 1) {}

            For i = 0 To buf.Length - 1
                Select Case buf(i)
                    ' Escape IAC twice in stream ... to be telnet protocol compliant
                    ' this is there in binary and non-binary mode.
                    Case IAC
                        nbuf(System.Math.Max(System.Threading.Interlocked.Increment(nbufptr), nbufptr) - 1) = IAC
                        nbuf(System.Math.Max(System.Threading.Interlocked.Increment(nbufptr), nbufptr) - 1) = IAC
                        Exit Select
                        ' We need to heed RFC 854. LF (\n) is 10, CR (\r) is 13
                        ' we assume that the Terminal sends \n for lf+cr and \r for just cr
                        ' linefeed+carriage return is CR LF  
                    Case 10
                        ' \n
                        If receivedDX(TELOPT_BINARY + 128) <> [DO] Then
                            While nbuf.Length - nbufptr < m_crlf.Length
                                xbuf = New Byte(nbuf.Length * 2 - 1) {}
                                Array.Copy(nbuf, 0, xbuf, 0, nbufptr)
                                nbuf = xbuf
                            End While
                            For j As Integer = 0 To m_crlf.Length - 1
                                nbuf(System.Math.Max(System.Threading.Interlocked.Increment(nbufptr), nbufptr) - 1) = m_crlf(j)
                            Next
                            Exit Select
                        Else
                            ' copy verbatim in binary mode.
                            nbuf(System.Math.Max(System.Threading.Interlocked.Increment(nbufptr), nbufptr) - 1) = buf(i)
                        End If
                        Exit Select
                        ' carriage return is CR NUL */ 
                    Case 13
                        ' \r
                        If receivedDX(TELOPT_BINARY + 128) <> [DO] Then
                            While nbuf.Length - nbufptr < m_cr.Length
                                xbuf = New Byte(nbuf.Length * 2 - 1) {}
                                Array.Copy(nbuf, 0, xbuf, 0, nbufptr)
                                nbuf = xbuf
                            End While
                            For j As Integer = 0 To m_cr.Length - 1
                                nbuf(System.Math.Max(System.Threading.Interlocked.Increment(nbufptr), nbufptr) - 1) = m_cr(j)
                            Next
                        Else
                            ' copy verbatim in binary mode.
                            nbuf(System.Math.Max(System.Threading.Interlocked.Increment(nbufptr), nbufptr) - 1) = buf(i)
                        End If
                        Exit Select
                    Case Else
                        ' all other characters are just copied
                        nbuf(System.Math.Max(System.Threading.Interlocked.Increment(nbufptr), nbufptr) - 1) = buf(i)
                        Exit Select
                End Select
            Next

            If receivedDX(TELOPT_EOR + 128) = [DO] Then
                xbuf = New Byte(nbufptr - 1 + 2) {}
                Array.Copy(nbuf, 0, xbuf, 0, nbufptr)
                xbuf(nbufptr) = IAC
                xbuf(nbufptr + 1) = EOR
                Write(xbuf)
                'Write(New Byte(1) {IAC, EOR})
            Else
                xbuf = New Byte(nbufptr - 1) {}
                Array.Copy(nbuf, 0, xbuf, 0, nbufptr)
                Write(xbuf)
            End If
        End Sub


        ''' <summary>
        ''' Handle telnet protocol negotiation. The buffer will be parsed
        ''' and necessary actions are taken according to the telnet protocol.
        ''' <see cref="RFC-Telnet"/>
        ''' </summary>
        ''' <param name="nbuf">the byte buffer used for negotiation</param>
        ''' <returns>a new buffer after negotiation</returns>
        Protected Function Negotiate(nbuf As Byte()) As Integer
            Dim count As Integer = tempbuf.Length
            If count = 0 Then
                ' buffer is empty.
                Return -1
            End If

            Dim sendbuf As Byte() = New Byte(2) {}
            Dim SBbuf As Byte() = New Byte(tempbuf.Length - 1) {}
            Dim buf As Byte() = tempbuf

            Dim b As Byte
            Dim reply As Byte

            Dim SBcount As Integer = 0
            Dim boffset As Integer = 0
            Dim noffset As Integer = 0

            Dim done As Boolean = False
            Dim foundSE As Boolean = False

            'bool dobreak = false;

            While Not done AndAlso (boffset < count AndAlso noffset < nbuf.Length)
                b = buf(System.Math.Max(System.Threading.Interlocked.Increment(boffset), boffset) - 1)
                Select Case neg_state
                    Case STATE_DATA
                        If b = IAC Then
                            'dobreak = true; // leave the loop so we can sync.
                            neg_state = STATE_IAC
                        Else
                            nbuf(System.Math.Max(System.Threading.Interlocked.Increment(noffset), noffset) - 1) = b
                            RecordStream.WriteByte(b)
                        End If
                        Exit Select

                    Case STATE_IAC
                        Select Case b
                            Case IAC
                                neg_state = STATE_DATA
                                nbuf(System.Math.Max(System.Threading.Interlocked.Increment(noffset), noffset) - 1) = IAC
                                RecordStream.WriteByte(b)
                                ' got IAC, IAC: set option to IAC
                                Exit Select

                            Case WILL
                                neg_state = STATE_IACWILL
                                Exit Select

                            Case WONT
                                neg_state = STATE_IACWONT
                                Exit Select

                            Case DONT
                                neg_state = STATE_IACDONT
                                Exit Select

                            Case [DO]
                                neg_state = STATE_IACDO
                                Exit Select

                            Case EOR
                                NotifyEndOfRecord(RecordStream.ToArray)
                                RecordStream = New System.IO.MemoryStream
                                'dobreak = true; // leave the loop so we can sync.
                                neg_state = STATE_DATA
                                Exit Select

                            Case SB
                                neg_state = STATE_IACSB
                                SBcount = 0
                                Exit Select
                            Case Else

                                neg_state = STATE_DATA
                                Exit Select
                        End Select
                        Exit Select

                    Case STATE_IACWILL
                        Select Case b
                            Case TELOPT_ECHO
                                reply = [DO]
                                SetLocalEcho(False)
                                Exit Select

                            Case TELOPT_SGA
                                reply = [DO]
                                Exit Select

                            Case TELOPT_EOR
                                reply = [DO]
                                Exit Select

                            Case TELOPT_BINARY
                                reply = [DO]
                                Exit Select
                            Case Else

                                reply = DONT
                                Exit Select
                        End Select

                        If reply <> sentDX(b + 128) OrElse WILL <> receivedWX(b + 128) Then
                            sendbuf(0) = IAC
                            sendbuf(1) = reply
                            sendbuf(2) = b
                            Write(sendbuf)

                            sentDX(b + 128) = reply
                            receivedWX(b + 128) = WILL
                        End If

                        neg_state = STATE_DATA
                        Exit Select

                    Case STATE_IACWONT

                        Select Case b
                            Case TELOPT_ECHO
                                SetLocalEcho(True)
                                reply = DONT
                                Exit Select

                            Case TELOPT_SGA
                                reply = DONT
                                Exit Select

                            Case TELOPT_EOR
                                reply = DONT
                                Exit Select

                            Case TELOPT_BINARY
                                reply = DONT
                                Exit Select
                            Case Else

                                reply = DONT
                                Exit Select
                        End Select

                        If reply <> sentDX(b + 128) OrElse WONT <> receivedWX(b + 128) Then
                            sendbuf(0) = IAC
                            sendbuf(1) = reply
                            sendbuf(2) = b
                            Write(sendbuf)

                            sentDX(b + 128) = reply
                            receivedWX(b + 128) = WILL
                        End If

                        neg_state = STATE_DATA
                        Exit Select

                    Case STATE_IACDO
                        Select Case b
                            Case TELOPT_EOR
                                reply = WILL
                                Exit Select

                            Case TELOPT_NEW_ENVIRON
                                reply = WILL
                                Exit Select

                            Case TELOPT_ECHO
                                reply = WILL
                                SetLocalEcho(True)
                                Exit Select

                            Case TELOPT_SGA
                                reply = WILL
                                Exit Select

                            Case TELOPT_TTYPE
                                reply = WILL
                                Exit Select

                            Case TELOPT_BINARY
                                reply = WILL
                                Exit Select

                            Case TELOPT_NAWS
                                Dim size As Size = windowSize
                                receivedDX(b) = [DO]

                                If size.[GetType]() <> GetType(Size) Then
                                    ' this shouldn't happen
                                    Write(IAC)
                                    Write(WONT)
                                    Write(TELOPT_NAWS)
                                    reply = WONT
                                    sentWX(b) = WONT
                                    Exit Select
                                End If

                                reply = WILL
                                sentWX(b) = WILL
                                sendbuf(0) = IAC
                                sendbuf(1) = WILL
                                sendbuf(2) = TELOPT_NAWS
                                Write(sendbuf)
                                Write(IAC)
                                Write(SB)
                                Write(TELOPT_NAWS)
                                'XXX These values should be cooked to escape the IAC value (255) by doubling: <IAC><IAC>.
                                Write(CByte(size.Width >> 8))
                                Write(CByte(size.Width And &HFF))
                                Write(CByte(size.Height >> 8))
                                Write(CByte(size.Height And &HFF))
                                Write(IAC)
                                Write(SE)
                                Exit Select
                            Case Else

                                reply = WONT
                                Exit Select
                        End Select

                        If reply <> sentWX(128 + b) OrElse [DO] <> receivedDX(128 + b) Then
                            sendbuf(0) = IAC
                            sendbuf(1) = reply
                            sendbuf(2) = b
                            Write(sendbuf)

                            sentWX(b + 128) = reply
                            receivedDX(b + 128) = [DO]
                        End If

                        neg_state = STATE_DATA
                        Exit Select

                    Case STATE_IACDONT
                        Select Case b
                            Case TELOPT_ECHO
                                reply = WONT
                                SetLocalEcho(False)
                                Exit Select

                            Case TELOPT_SGA
                                reply = WONT
                                Exit Select

                            Case TELOPT_NAWS
                                reply = WONT
                                Exit Select

                            Case TELOPT_BINARY
                                reply = WONT
                                Exit Select
                            Case Else

                                reply = WONT
                                Exit Select
                        End Select

                        If reply <> sentWX(b + 128) OrElse DONT <> receivedDX(b + 128) Then
                            sendbuf(0) = IAC
                            sendbuf(1) = reply
                            sendbuf(2) = b
                            Write(sendbuf)

                            sentWX(b + 128) = reply
                            receivedDX(b + 128) = DONT
                        End If

                        neg_state = STATE_DATA
                        Exit Select

                    Case STATE_IACSBIAC

                        ' If SE not found in this buffer, move on until we get it
                        For i As Integer = boffset To tempbuf.Length - 1
                            If tempbuf(i) = SE Then
                                foundSE = True
                            End If
                        Next

                        If Not foundSE Then
                            boffset -= 1
                            done = True
                            Exit Select
                        End If

                        foundSE = False

                        If b = IAC Then
                            SBcount = 0
                            current_sb = b
                            neg_state = STATE_IACSBDATA
                        Else
                            neg_state = STATE_DATA
                        End If
                        Exit Select

                    Case STATE_IACSB

                        ' If SE not found in this buffer, move on until we get it
                        For i As Integer = boffset To tempbuf.Length - 1
                            If tempbuf(i) = SE Then
                                foundSE = True
                            End If
                        Next

                        If Not foundSE Then
                            boffset -= 1
                            done = True
                            Exit Select
                        End If

                        foundSE = False

                        Select Case b
                            Case IAC
                                neg_state = STATE_IACSBIAC
                                Exit Select
                            Case Else

                                current_sb = b
                                SBcount = 0
                                neg_state = STATE_IACSBDATA
                                Exit Select
                        End Select
                        Exit Select

                    Case STATE_IACSBDATA

                        ' If SE not found in this buffer, move on until we get it
                        For i As Integer = boffset To tempbuf.Length - 1
                            If tempbuf(i) = SE Then
                                foundSE = True
                            End If
                        Next

                        If Not foundSE Then
                            boffset -= 1
                            done = True
                            Exit Select
                        End If

                        foundSE = False

                        Select Case b
                            Case IAC
                                neg_state = STATE_IACSBDATAIAC
                                Exit Select
                            Case Else
                                SBbuf(System.Math.Max(System.Threading.Interlocked.Increment(SBcount), SBcount) - 1) = b
                                Exit Select
                        End Select
                        Exit Select

                    Case STATE_IACSBDATAIAC
                        Select Case b
                            Case IAC
                                neg_state = STATE_IACSBDATA
                                SBbuf(System.Math.Max(System.Threading.Interlocked.Increment(SBcount), SBcount) - 1) = IAC
                                Exit Select
                            Case SE
                                HandleSB(current_sb, SBbuf, SBcount)
                                current_sb = 0
                                neg_state = STATE_DATA
                                Exit Select
                            Case SB
                                HandleSB(current_sb, SBbuf, SBcount)
                                neg_state = STATE_IACSB
                                Exit Select
                            Case Else
                                neg_state = STATE_DATA
                                Exit Select
                        End Select
                        Exit Select
                    Case Else

                        neg_state = STATE_DATA
                        Exit Select
                End Select
            End While

            ' shrink tempbuf to new processed size.
            Dim xb As Byte() = New Byte(count - boffset - 1) {}
            Array.Copy(tempbuf, boffset, xb, 0, count - boffset)
            tempbuf = xb

            Return noffset
        End Function

#End Region

        ''' <summary>
        ''' Adds bytes to the input buffer we'll parse for codes.
        ''' </summary>
        ''' <param name="b">Bytes array from which to add.</param>
        ''' <param name="len">Number of bytes to add.</param>
        Protected Sub InputFeed(b As Byte(), len As Integer)
            Dim bytesTmp As Byte() = New Byte(tempbuf.Length + (len - 1)) {}

            Array.Copy(tempbuf, 0, bytesTmp, 0, tempbuf.Length)
            Array.Copy(b, 0, bytesTmp, tempbuf.Length, len)

            tempbuf = bytesTmp
        End Sub
    End Class
End Namespace
