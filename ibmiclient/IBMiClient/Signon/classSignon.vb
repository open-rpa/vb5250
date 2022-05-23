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
Imports System.Security.Cryptography.X509Certificates
Imports System.Security
Imports System.Net.Security
Imports System.Threading
Imports System.Windows.Forms

Partial Public Class Client
    Partial Public Class Signon
        Public Structure ServerInfo
            Dim Host As String
            Dim Version As Integer
            Dim Release As Integer
            Dim Modification As Integer
            Dim VersionString As String
            Dim Level As Integer
            Dim PasswordLevel As Integer
            Dim JobName As String
        End Structure

        Public Structure SignonInfo
            Dim Code As UInteger
            Dim Text As String
            Dim CurrentSignonDate As Date
            Dim LastSignonDate As Date
            Dim ExpirationDate As Date
            Dim ExpirationWarning As UInteger
            Dim Messages As List(Of SignonInfoMessage)
        End Structure

        Public Structure SignonInfoMessage
            Dim Severity As UInt16
            Dim ReasonCode As UInt32
            Dim ReasonText As String
            Dim FileName As String
            Dim LibraryName As String
            Dim Text As String
            Dim Help As String
        End Structure

        Public Structure ChangePasswordInfo
            Dim Code As UInteger
            Dim Text As String
            Dim Messages As List(Of SignonInfoMessage)
        End Structure

        'Public Enum PasswordComplexityRequirement As Integer
        '    MinLength = 1
        '    MaxLength = 2
        '    ValidChars = 3
        '    CannotStartWithChars = 4
        '    UppercaseAllowed = 5
        '    LowercaseAllowed = 6
        '    NumbersAllowed = 7
        '    SymbolsAllowed = 8
        '    MinCharSets = 9
        'End Enum

        Private tc As System.Net.Sockets.TcpClient

        Private stream As System.IO.Stream
        Private UseSSL_ As Boolean
        Private ConnectDone As New ManualResetEvent(False)
        Private CertActionDone As New ManualResetEvent(False)
        Private ConnectException As Exception

        Private Host_ As String
        Private Port_ As Integer
        Private ClientSeed_(7) As Byte 'Most recently used client seed

        Public Sub New(Host As String, Port As Integer, UseSSL As Boolean)
            If String.IsNullOrWhiteSpace(Host) Then Throw New ArgumentException("Host cannot be null")
            If Port < 1 Then Throw New Exception("The Port parameter is not valid")
            Host_ = Host
            Port_ = Port
            UseSSL_ = UseSSL
        End Sub

        'Friend Shared Function GetPasswordComplexityRequirement(Requirement As PasswordComplexityRequirement, PasswordLevel As Integer) As Object
        '    'These are the basic requirements for each password level, but there are gobs of other requirements that can be configured on the AS400.
        '    Select Case Requirement
        '        Case PasswordComplexityRequirement.MinLength
        '            Return 1
        '        Case PasswordComplexityRequirement.MaxLength
        '            If PasswordLevel > 1 Then
        '                Return 128
        '            Else
        '                Return 10
        '            End If
        '        Case PasswordComplexityRequirement.ValidChars
        '            If PasswordLevel > 1 Then
        '                Return Nothing
        '            Else
        '                Return "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$@#_"
        '            End If
        '        Case PasswordComplexityRequirement.CannotStartWithChars
        '            If PasswordLevel > 1 Then
        '                Return "*"
        '            Else
        '                Return "0123456789"
        '            End If
        '        Case PasswordComplexityRequirement.UppercaseAllowed
        '            Return True
        '        Case PasswordComplexityRequirement.LowercaseAllowed
        '            Return (PasswordLevel > 1)
        '        Case PasswordComplexityRequirement.NumbersAllowed
        '            Return True
        '        Case PasswordComplexityRequirement.SymbolsAllowed
        '            Return True
        '        Case PasswordComplexityRequirement.MinCharSets
        '            Return 1
        '        Case Else
        '            Return Nothing
        '    End Select
        'End Function

        Private Function Connect(TimeoutMS As Integer) As Boolean
            If String.IsNullOrWhiteSpace(Host_) Then Throw New Exception("The Host parameter has not been set")
            If Port_ < 1 Then Throw New Exception("The Port parameter is not valid")
            Return Connect(Host_, Port_, TimeoutMS)
        End Function
        Private Function Connect(Host As String, Port As Integer, TimeoutMS As Integer) As Boolean
            If String.IsNullOrWhiteSpace(Host) Then Throw New ArgumentException("Host cannot be null")
            If Port_ < 1 Then Throw New Exception("The Port parameter is not valid")
            Try
                tc = New System.Net.Sockets.TcpClient
                stream = Nothing
                ConnectDone.Reset()
                CertActionDone.Reset()
                ConnectException = Nothing
                Dim Result As IAsyncResult = tc.BeginConnect(Host, Port, New AsyncCallback(AddressOf ConnectCallback), Nothing)
                Dim Success As Object = Result.AsyncWaitHandle.WaitOne(TimeoutMS)
                If Success Then
                    If UseSSL_ Then CertActionDone.WaitOne() 'Wait for SSL negotiation to complete.  We may be waiting for the user to accept the remote certificate, so don't time out.
                    ConnectDone.WaitOne() 'Wait for ConnectCallback to complete.
                    If ConnectException IsNot Nothing Then Throw ConnectException
                    Return True
                Else
                    Throw New Exception("Failed to connect to host '" & Host & "' on port " & Port.ToString)
                End If
            Catch ex As Exception
                Try
                    tc.Close()
                Catch exx As ObjectDisposedException
                    'Do nothing; this happens after we've already disconnected.
                Catch exx As Exception
                End Try
                Throw
            End Try
            Return False
        End Function

        Private Sub ConnectCallback(ByVal ar As IAsyncResult)
            Try
                ' Complete the connection.
                tc.EndConnect(ar)

                If UseSSL_ Then
                    Dim ssl_stream As New SslStream(tc.GetStream, True, New RemoteCertificateValidationCallback(AddressOf ServerCertificateValidationCallback))
                    ssl_stream.AuthenticateAsClient(Host_, Nothing, Authentication.SslProtocols.None, False)
                    stream = ssl_stream
                Else
                    stream = tc.GetStream
                End If
                ConnectDone.Set() 'Signal that the connection attempt has completed.
            Catch ex As Exception
                ConnectException = ex
                CertActionDone.Set() 'Signal that SSL authentication has completed so we don't wait forever.
                ConnectDone.Set() 'Signal that the connection attempt has completed. Don't move this to a Finally block or Disconnect() will hang waiting for the signal.
                Disconnect()
            End Try
        End Sub

        Private Function ServerCertificateValidationCallback(ByVal sender As Object, ByVal certificate As X509Certificate, ByVal chain As X509Chain, ByVal sslPolicyErrors As SslPolicyErrors) As Boolean
            Try
                If sslPolicyErrors = SslPolicyErrors.None Then
                    Return True
                Else
                    Dim Store As New X509Store(StoreName.TrustedPeople, StoreLocation.CurrentUser)

                    'Check for a certificate the user has stored, even if it's invalid.
                    Store.Open(OpenFlags.ReadOnly)
                    Dim certs As X509Certificate2Collection = Store.Certificates.Find(X509FindType.FindByThumbprint, certificate.GetCertHashString, False)
                    Store.Close()
                    If certs.Count Then Return True

                    'We didn't find the cert in the store, so prompt the user.
                    Dim Reasons As New List(Of String)
                    If sslPolicyErrors And SslPolicyErrors.RemoteCertificateNotAvailable Then Reasons.Add("Remote certificate not available")
                    If sslPolicyErrors And SslPolicyErrors.RemoteCertificateNameMismatch Then Reasons.Add("Remote certificate name mismatch")
                    If sslPolicyErrors And SslPolicyErrors.RemoteCertificateChainErrors Then Reasons.Add("Remote certificate chain errors")
                    Dim tmpDate As Date
                    If Date.TryParse(certificate.GetEffectiveDateString, tmpDate) Then
                        If tmpDate > Now Then Reasons.Add("Remote certificate is not yet valid")
                    End If
                    If Date.TryParse(certificate.GetExpirationDateString, tmpDate) Then
                        If tmpDate < Now Then Reasons.Add("Remote certificate is expired")
                    End If
                    If Reasons.Count Then
                        Dim frm As New FormSSLCertErrors
                        For Each s As String In Reasons
                            frm.CertErrorsListBox.Items.Add(s)
                        Next
                        With frm.CertPropsListView.Items
                            '.Add(New ListViewItem(New String() {"Format", certificate.GetFormat}))
                            .Add(New ListViewItem(New String() {"Subject", certificate.Subject}))
                            .Add(New ListViewItem(New String() {"Issuer", certificate.Issuer}))
                            .Add(New ListViewItem(New String() {"Effective Date", certificate.GetEffectiveDateString}))
                            .Add(New ListViewItem(New String() {"Expiration Date", certificate.GetExpirationDateString}))
                            .Add(New ListViewItem(New String() {"Serial Number", certificate.GetSerialNumberString}))
                            .Add(New ListViewItem(New String() {"Fingerprint", certificate.GetCertHashString}))
                            '.Add(New ListViewItem(New String() {"Public Key", certificate.GetPublicKeyString}))
                            '.Add(New ListViewItem(New String() {"Key Algorithm", certificate.GetKeyAlgorithm}))
                            '.Add(New ListViewItem(New String() {"Key Algorithm Parameters", certificate.GetKeyAlgorithmParametersString}))
                        End With
                        frm.StartPosition = Windows.Forms.FormStartPosition.CenterParent
                        frm.AcceptAlwaysButton.Enabled = Not (sslPolicyErrors And System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable)
                        Dim Result As Windows.Forms.DialogResult = frm.ShowDialog
                        Select Case frm.Action
                            Case FormSSLCertErrors.CertAction.Reject
                                Return False
                            Case FormSSLCertErrors.CertAction.Accept
                                Return True
                            Case FormSSLCertErrors.CertAction.AcceptAndStore
                                Store.Open(OpenFlags.ReadWrite)
                                Store.Add(certificate)
                                Store.Close()
                                Return True
                            Case Else
                                'Should never get here
                                Return False
                        End Select
                    Else
                        'Should never get here
                        Return True
                    End If
                End If
            Catch ex As Exception
                Return False
            Finally
                CertActionDone.Set()
            End Try
        End Function

        Private Sub SayGoodBye()
            If tc IsNot Nothing Then
                Dim Bye As New MessageHeader(MessageType.EndServerRequest)
                Dim b() As Byte = Bye.ToBytes
                stream.Write(b, 0, b.Length)
            End If
        End Sub

        Private Sub Disconnect()
            Try
                If tc IsNot Nothing Then tc.Close()
            Catch ex As Exception
            End Try
        End Sub

        Public Function Get_Server_Attributes(TimeoutMS As Integer) As ServerInfo
            Connect(TimeoutMS)
            Dim AttributeReply As Signon.Exchange_Attribute_Reply = Get_Server_AttributesX(TimeoutMS)
            SayGoodBye()
            Disconnect()
            Dim Result As New ServerInfo
            Result.Host = Me.Host_
            Result.Version = AttributeReply.ServerVersion_Version
            Result.Release = AttributeReply.ServerVersion_Release
            Result.Modification = AttributeReply.ServerVersion_Modification
            Result.VersionString = "V" & AttributeReply.ServerVersion_Version.ToString & "R" & AttributeReply.ServerVersion_Release.ToString & "M" & AttributeReply.ServerVersion_Modification.ToString
            Result.Level = CInt(AttributeReply.ServerLevel)
            Result.PasswordLevel = CInt(AttributeReply.PasswordLevel)
            Result.JobName = AttributeReply.JobName.Trim
            Return Result
        End Function

        Private Function Get_Server_AttributesX(TimeoutMS As Integer) As Signon.Exchange_Attribute_Reply
            Dim MsgHeader As New Signon.MessageHeader(MessageType.ExchangeAttributeRequest)
            Dim AttributeRequest As New Signon.Exchange_Attribute_Request(MsgHeader)
            Array.Copy(AttributeRequest.ClientSeed, ClientSeed_, ClientSeed_.Length)
            Dim b() As Byte = AttributeRequest.ToBytes
            stream.Write(b, 0, b.Length)

            Dim o As Object = GetReply(TimeoutMS)
            If o IsNot Nothing Then
                If o.GetType Is GetType(Signon.Exchange_Attribute_Reply) Then
                    Return o
                Else
                    Throw New Exception("Server replied with an unexpected message type: " & o.GetType.ToString)
                End If
            Else
                Throw New Exception("Communication error")
            End If

        End Function

        Public Function Get_Signon_Info(UserName As String, Password As String, TimeoutMS As Integer) As SignonInfo
            Connect(TimeoutMS)
            Dim InfoReply As Signon.Info_Reply = Get_Signon_InfoX(UserName, Password, TimeoutMS)
            SayGoodBye()
            Disconnect()
            Dim Result As New SignonInfo
            Result.Code = InfoReply.ResultCode
            Result.Text = InfoReply.ResultText
            Result.CurrentSignonDate = InfoReply.CurrentSignonDate
            Result.LastSignonDate = InfoReply.LastSignonDate
            Result.ExpirationDate = InfoReply.ExpirationDate
            Result.ExpirationWarning = InfoReply.ExpirationWarning
            'XXX more props here?
            Result.Messages = New List(Of SignonInfoMessage)
            For Each Msg As Signon.Info_Message In InfoReply.Messages
                Dim ResultMessage As New SignonInfoMessage
                ResultMessage.Severity = CInt(Msg.Severity)
                ResultMessage.ReasonCode = Msg.ReasonCode
                ResultMessage.ReasonText = Msg.ReasonText
                ResultMessage.FileName = Msg.FileName
                ResultMessage.LibraryName = Msg.LibraryName
                ResultMessage.Text = Msg.Text
                ResultMessage.Help = Msg.Help
                Result.Messages.Add(ResultMessage)
            Next
            Return Result
        End Function

        Private Function Get_Signon_InfoX(UserName As String, Password As String, TimeoutMS As Integer) As Signon.Info_Reply
            Dim AttributeReply As Signon.Exchange_Attribute_Reply = Get_Server_AttributesX(TimeoutMS)

            Dim MsgHeader As New Signon.MessageHeader(MessageType.InfoRequest)
            Dim InfoReq As New Signon.Info_Request(MsgHeader, ClientSeed_, AttributeReply.ServerSeed, UserName, Password, AttributeReply.PasswordLevel, AttributeReply.ServerLevel)
            Dim b() As Byte = InfoReq.ToBytes
            stream.Write(b, 0, b.Length)

            Dim o As Object = GetReply(TimeoutMS)
            If o.GetType Is GetType(Signon.Info_Reply) Then
                Return o
            Else
                Throw New Exception("Server replied with an unexpected message type: " & o.GetType.ToString)
            End If

        End Function

        Public Function Change_Password(UserName As String, Password As String, NewPassword As String, TimeoutMS As Integer) As ChangePasswordInfo
            Connect(TimeoutMS)
            Dim ChangePasswordReply As Signon.Change_Password_Reply = Change_PasswordX(UserName, Password, NewPassword, TimeoutMS)
            SayGoodBye()
            Disconnect()
            'XXX more props here?
            Dim Result As New ChangePasswordInfo
            Result.Code = ChangePasswordReply.ResultCode
            Result.Text = ChangePasswordReply.ResultText
            Result.Messages = New List(Of SignonInfoMessage)
            For Each Msg As Signon.Info_Message In ChangePasswordReply.Messages
                Dim InfoMsg As New SignonInfoMessage
                InfoMsg.Severity = Msg.Severity
                InfoMsg.ReasonCode = Msg.ReasonCode
                InfoMsg.ReasonText = Msg.ReasonText
                InfoMsg.FileName = Msg.FileName
                InfoMsg.LibraryName = Msg.LibraryName
                InfoMsg.Text = Msg.Text
                InfoMsg.Help = Msg.Help
                Result.Messages.Add(InfoMsg)
            Next
            Return Result
        End Function

        Private Function Change_PasswordX(UserName As String, Password As String, NewPassword As String, TimeoutMS As Integer) As Signon.Change_Password_Reply
            Dim AttributeReply As Signon.Exchange_Attribute_Reply = Get_Server_AttributesX(TimeoutMS)

            Dim MsgHeader As New Signon.MessageHeader(MessageType.ChangePasswordRequest)
            Dim ChangePasswordRequest As New Signon.Change_Password_Request(MsgHeader, ClientSeed_, AttributeReply.ServerSeed, UserName, Password, NewPassword, AttributeReply.PasswordLevel, AttributeReply.ServerLevel)
            Dim b() As Byte = ChangePasswordRequest.ToBytes
            stream.Write(b, 0, b.Length)

            Dim o As Object = GetReply(TimeoutMS)
            If o.GetType Is GetType(Signon.Change_Password_Reply) Then
                Return o
            Else
                Throw New Exception("Server replied with an unexpected message type: " & o.GetType.ToString)
            End If

        End Function

        Private Function GetReply(TimeoutMS As Integer) As Object
            Try
                Dim b() As Byte
                b = ReadTCPBytes(stream, MessageHeader.HeaderLength, TimeoutMS)
                Dim Header As New MessageHeader(b)
                b = ReadTCPBytes(stream, CInt(Header.MessageLength - MessageHeader.HeaderLength), TimeoutMS)
                Select Case Header.RequestReplyID
                    Case MessageType.ExchangeAttributeReply
                        Return New Exchange_Attribute_Reply(Header, b)
                    Case MessageType.InfoReply
                        Return New Info_Reply(Header, b)
                    Case MessageType.ChangePasswordReply
                        Return New Change_Password_Reply(Header, b)
                    Case Else
                        Throw New Exception("Unimplemented Reply type: " & Header.RequestReplyID.ToString)
                End Select
            Catch ex As Exception
            End Try
            Return Nothing
        End Function

        Private Function ReadTCPBytes(ByRef s As System.IO.Stream, ByVal ExpectedBytes As Integer, ByVal TimeoutMS As Integer) As Byte()
            If Not s.CanRead Then Throw New Exception("Supplied stream is not readable")
            Dim start As Date = Now
            Dim BytesRead As Integer = 0
            Dim b(ExpectedBytes - 1) As Byte
            Do While Now < start.AddMilliseconds(CDbl(TimeoutMS))
                BytesRead += s.Read(b, BytesRead, b.Length - BytesRead)
                If BytesRead >= ExpectedBytes Then Exit Do
                Threading.Thread.CurrentThread.Join(100)
            Loop
            If BytesRead < ExpectedBytes Then Throw New Exception("Insufficient data received from the server")
            Return b
        End Function
    End Class
End Class
