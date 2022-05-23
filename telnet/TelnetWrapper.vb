'
' Ported from Seva Petrov's .Net Telnet, which was ported from Matthias L. Jugel's and Marcus Meissner's JTA.
'
' Copyright 2012, 2013 Alec Skelly
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
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Security.Cryptography.X509Certificates
Imports System.Security
Imports System.Net.Security
Imports System.Windows.Forms

Imports Telnet.Net.Graphite.Telnet

Namespace De.Mud.Telnet
    ''' <summary>
    ''' TelnetWrapper is a sample class demonstrating the use of the 
    ''' telnet protocol handler.
    ''' </summary>
    Public Class TelnetWrapper
        Inherits TelnetProtocolHandler

#Region "Globals and properties"
        Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

        ' ManualResetEvent instances signal completion.
        Private connectDone As New ManualResetEvent(False)
        Private CertActionDone As New ManualResetEvent(False)
        Private sendDone As New ManualResetEvent(False)

        Public Event Disconnected As DisconnectedEventHandler
        Public Event DataAvailable As DataAvailableEventHandler
        Public Event ConnectionAttemptCompleted As ConnectionAttemptCompletedEventHandler

        Private tcp_client As TcpClient
        Private stream As Stream

        Protected m_hostname As String
        Protected m_port As Integer
        Protected m_UseSSL As Boolean

        ''' <summary>
        ''' Secure the connection with SSL?
        ''' </summary>
        Public Property UseSSL As Boolean
            Get
                Return m_UseSSL
            End Get
            Set(value As Boolean)
                If Me.Connected Then
                    Throw New Exception("Cannot reconfigure SSL encryption on a connected socket")
                Else
                    m_UseSSL = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' True if SSL is authenticated and encrypted, otherwise False.  ReadOnly.
        ''' </summary>
        Public ReadOnly Property Secured As Boolean
            Get
                If stream IsNot Nothing Then
                    If stream.GetType Is GetType(SslStream) Then
                        Dim SSLStream As System.Net.Security.SslStream = DirectCast(stream, SslStream)
                        Return (SSLStream.IsAuthenticated And SSLStream.IsEncrypted)
                    End If
                End If
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets the remote certificate from the SSL stream.
        ''' </summary>
        Public ReadOnly Property Certificate As X509Certificate2
            Get
                If stream IsNot Nothing Then
                    If stream.GetType Is GetType(SslStream) Then
                        Try
                            Dim SSLStream As System.Net.Security.SslStream = DirectCast(stream, SslStream)
                            Return New X509Certificate2(SSLStream.RemoteCertificate)
                        Catch ex As Exception
                            Logger.Error(ex)
                        End Try
                    End If
                End If
                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets the name of the protocol in use by the SSL stream.
        ''' </summary>
        Public ReadOnly Property SecurityProtocolInfo As String
            Get
                If stream IsNot Nothing Then
                    If stream.GetType Is GetType(SslStream) Then
                        Try
                            Dim SSLStream As System.Net.Security.SslStream = DirectCast(stream, SslStream)
                            Return "Protocol: " & SSLStream.SslProtocol.ToString _
                                & vbCr & "Cipher: " & SSLStream.CipherAlgorithm.ToString & "(" & SSLStream.CipherStrength.ToString & ")" _
                                & vbCr & "KeyExchange: " & SSLStream.KeyExchangeAlgorithm.ToString & "(" & SSLStream.KeyExchangeStrength.ToString & ")"
                        Catch ex As Exception
                            Logger.Error(ex)
                        End Try
                    End If
                End If
                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' Sets the name of the host to connect to.
        ''' </summary>
        Public WriteOnly Property Hostname() As String
            Set(ByVal value As String)
                m_hostname = value
            End Set
        End Property

        ''' <summary>
        ''' Sets the port on the remote host.
        ''' </summary>
        Public WriteOnly Property Port() As Integer
            Set(ByVal value As Integer)
                If value > 0 Then
                    m_port = value
                Else
                    Throw (New ArgumentException("Port number must be greater than 0.", "Port"))
                End If
            End Set
        End Property

        ''' <summary>
        ''' Sets the terminal width.
        ''' </summary>
        Public WriteOnly Property TerminalWidth() As Integer
            Set(ByVal value As Integer)
                windowSize.Width = value
            End Set
        End Property

        ''' <summary>
        ''' Sets the terminal height.
        ''' </summary>
        Public WriteOnly Property TerminalHeight() As Integer
            Set(ByVal value As Integer)
                windowSize.Height = value
            End Set
        End Property

        ''' <summary>
        ''' Sets the terminal type.
        ''' </summary>
        Public WriteOnly Property Terminal_Type() As String
            Set(ByVal value As String)
                terminalType = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a value indicating whether a connection to the remote
        ''' resource exists.
        ''' </summary>
        Public ReadOnly Property Connected() As Boolean
            Get
                Return TCPClient_Connected(tcp_client)
            End Get
        End Property

#End Region

#Region "Public interface"

        ''' <summary>
        ''' Connects to the remote host  and opens the connection.
        ''' </summary>
        Public Sub Connect()
            Connect(m_hostname, m_port, m_UseSSL)
        End Sub

        ''' <summary>
        ''' Connects to the specified remote host on the specified port
        ''' and opens the connection.
        ''' </summary>
        ''' <param name="host">Hostname of the Telnet server.</param>
        ''' <param name="port">The Telnet port on the remote host.</param>
        Public Sub Connect(ByVal host As String, ByVal port As Integer)
            Connect(host, port, m_UseSSL)
        End Sub

        ''' <summary>
        ''' Connects to the specified remote host on the specified port
        ''' and opens the connection.
        ''' </summary>
        ''' <param name="host">Hostname of the Telnet server.</param>
        ''' <param name="port">The Telnet port on the remote host.</param>
        ''' <param name="UseSSL">Encrypt the connection with SSL?</param>
        Public Sub Connect(ByVal host As String, ByVal port As Integer, UseSSL As Boolean)
            Logger.Trace("")
            m_hostname = host
            m_port = port
            m_UseSSL = UseSSL
            Try
                ' Establish the remote endpoint for the socket.
                Dim ipAddress As IPAddress = Nothing
                ipAddress.TryParse(host, ipAddress)
                If ipAddress Is Nothing Then
                    Dim ipHostInfo As IPHostEntry = Dns.GetHostEntry(host) 'Dns.Resolve(host)
                    If ipHostInfo.AddressList.Length > 0 Then
                        ipAddress = ipHostInfo.AddressList(0)
                    End If
                End If
                If ipAddress Is Nothing Then
                    Throw New ApplicationException("Unable to resolve host address")
                End If

                '  Create a TCP/IP  socket.
                tcp_client = New TcpClient(AddressFamily.InterNetwork)

                ' Connect to the remote endpoint.
                connectDone.Reset()
                CertActionDone.Reset()
                Logger.Debug("Calling BeginConnect...")
                Dim Result As System.IAsyncResult = tcp_client.BeginConnect(ipAddress, port, New AsyncCallback(AddressOf ConnectCallback), tcp_client)
                Dim Success As Boolean = Result.AsyncWaitHandle.WaitOne(15000) 'XXX 15 second timeout for BeginConnect.
                Logger.Debug("BeginConnect finished")
                'At this point we have not completed the connection, but it has either been accepted by the remote host or has timed out.
                 If Success Then
                    If UseSSL Then CertActionDone.WaitOne() 'Wait for SSL negotiation to complete.  We may be waiting for the user to accept the remote certificate, so don't time out.
                    connectDone.WaitOne() 'Wait for ConnectCallback to complete.  This is set in the Finally block of ConnectCallback.
                Else
                    Throw New Exception("Timeout connecting to host") 'Timed out during BeginConnect
                End If
            Catch ex As Exception
                Logger.Error(ex.Message, ex)
                Disconnect()
                Throw
            Finally
                Reset()
                Logger.Debug("Firing ConnectionAttemptCompleted event")
                RaiseEvent ConnectionAttemptCompleted(Me, New EventArgs)
            End Try
        End Sub

        ''' <summary>
        ''' Sends a command to the remote host. A newline is appended.
        ''' </summary>
        ''' <param name="cmd">the command</param>
        ''' <returns>output of the command</returns>
        Public Function Send(ByVal cmd As String) As String
            Logger.Trace("")
            Try
                Dim arr As Byte() = Encoding.ASCII.GetBytes(cmd)
                Transpose(arr)
                Return Nothing
            Catch ex As Exception
                Logger.Error(ex.Message, ex)
                Disconnect()
                Throw (New ApplicationException("Error writing to socket.", ex))
            End Try
        End Function
        Public Sub Send(ByVal bytes() As Byte)
            Logger.Trace("")
            Try
                Transpose(bytes)
            Catch ex As Exception
                Logger.Error(ex.Message, ex)
                Disconnect()
                Throw (New ApplicationException("Error writing to socket.", ex))
            End Try
        End Sub

        ''' <summary>
        ''' Starts receiving data.
        ''' </summary>
        Public Sub Receive()
             Receive(tcp_client)
        End Sub

        ''' <summary>
        ''' Disconnects the socket and closes the connection.
        ''' </summary>
        Public Sub Disconnect()
            Disconnect(Nothing)
        End Sub

        Public Sub Disconnect(ByVal Reason As String)
            Logger.Trace("")
            Static Disconnecting As Boolean
            If Not Disconnecting Then
                Logger.Debug("Disconnecting ==> True")
                Disconnecting = True
                'Try
                '    tcp_client.Client.Shutdown(SocketShutdown.Both)
                'Catch ex As Exception
                '    Logger.Error(ex.Message, ex)
                'End Try
                Try
                    tcp_client.Close()
                Catch ex As ObjectDisposedException
                    'Do nothing; this happens after we've already disconnected.
                Catch ex As Exception
                    Logger.Error(ex.Message, ex)
                End Try

                'Setting stream to Nothing here enables our "Secured" property to return False on a closed tcpclient.
                stream = Nothing

                Try
                    RaiseEvent Disconnected(Me, New DisconnectEventArgs(Reason))
                Catch ex As Exception
                    Logger.Error(ex.Message, ex)
                End Try
                Logger.Debug("Disconnecting ==> False")
                Disconnecting = False
            Else
                Logger.Debug("Aborting because Disconnecting = True")
            End If
        End Sub
#End Region

#Region "IO methods"

        ''' <summary>
        ''' Writes data to the socket.
        ''' </summary>
        ''' <param name="b">the buffer to be written</param>
        Protected Overrides Sub Write(ByVal b As Byte())
            Logger.Trace("")
            Send(tcp_client, b)
            sendDone.WaitOne()
        End Sub

        ''' <summary>
        ''' Callback for the connect operation.
        ''' </summary>
        ''' <param name="ar">Stores state information for this asynchronous 
        ''' operation as well as any user-defined data.</param>
        Private Sub ConnectCallback(ByVal ar As IAsyncResult)
            Logger.Trace("")
            Try
                ' Retrieve the socket from the state object.
                'tcp_client = DirectCast(ar.AsyncState, TcpClient)

                ' Complete the connection.
                tcp_client.EndConnect(ar)

                If m_UseSSL Then
                    Dim ssl_stream As New SslStream(tcp_client.GetStream, True, New RemoteCertificateValidationCallback(AddressOf ServerCertificateValidationCallback))
                    ssl_stream.AuthenticateAsClient(m_hostname, Nothing, Authentication.SslProtocols.None, False)
                    stream = ssl_stream
                Else
                    stream = tcp_client.GetStream
                End If
                connectDone.Set() 'Signal that the connection attempt has completed.
            Catch ex As Exception
                Logger.Error(ex.Message, ex)
                CertActionDone.Set() 'Signal that SSL authentication has completed so we don't wait forever.
                connectDone.Set() 'Signal that the connection attempt has completed. Don't move this to a Finally block or Disconnect() will hang waiting for the signal.
                Disconnect()
            End Try
        End Sub

        Private Function ServerCertificateValidationCallback(ByVal sender As Object, ByVal certificate As X509Certificate, ByVal chain As X509Chain, ByVal sslPolicyErrors As SslPolicyErrors) As Boolean
            Logger.Trace("")
            Try
                If sslPolicyErrors = sslPolicyErrors.None Then
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
                    If sslPolicyErrors And sslPolicyErrors.RemoteCertificateNotAvailable Then Reasons.Add("Remote certificate not available")
                    If sslPolicyErrors And sslPolicyErrors.RemoteCertificateNameMismatch Then Reasons.Add("Remote certificate name mismatch")
                    If sslPolicyErrors And sslPolicyErrors.RemoteCertificateChainErrors Then Reasons.Add("Remote certificate chain errors")
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
                Logger.Error(ex.Message, ex)
                Return False
            Finally
                CertActionDone.Set()
            End Try
        End Function

        ''' <summary>
        ''' Begins receiving for the data coming from the socket.
        ''' </summary>
        ''' <param name="client">The socket to get data from.</param>
        Private Sub Receive(ByVal client As TcpClient)
            Logger.Trace("")
            If client Is Nothing Then Logger.Debug("client is Nothing!")
            If stream Is Nothing Then Logger.Debug("stream is Nothing!")
            If client IsNot Nothing Then
                If stream IsNot Nothing Then
                    Try
                        ' Create the state object.
                        Dim state__1 As New State
                        state__1.WorkClient = client

                        ' Begin receiving the data from the remote device.
                        stream.BeginRead(state__1.Buffer, 0, State.BufferSize, New AsyncCallback(AddressOf ReceiveCallback), state__1)
                    Catch ex As Exception
                        Logger.Error(ex.Message, ex)
                        Disconnect()
                        Throw (New ApplicationException("Error on read from socket.", ex))
                    End Try
                End If
            End If
        End Sub

        ''' <summary>
        ''' Callback for the receive operation.
        ''' </summary>
        ''' <param name="ar">Stores state information for this asynchronous 
        ''' operation as well as any user-defined data.</param>
        Private Sub ReceiveCallback(ByVal ar As IAsyncResult)
            Logger.Trace("")
            Try
                ' Retrieve the state object and the client socket 
                ' from the async state object.
                Dim state__1 As State = DirectCast(ar.AsyncState, State)
                'Dim client As TcpClient = state__1.WorkClient

                ' Read data from the remote device.
                Dim bytesRead As Integer = stream.EndRead(ar)

                If bytesRead > 0 Then
                    InputFeed(state__1.Buffer, bytesRead)
                    Dim NumDataBytes As Integer = Negotiate(state__1.Buffer)

                    ' Notify the caller that we have data.
                    'RaiseEvent DataAvailable(Me, New DataAvailableEventArgs(Encoding.ASCII.GetString(state__1.Buffer, 0, bytesRead)))
                    'If NumDataBytes > 0 Then
                    '    Dim DataBytes(NumDataBytes - 1) As Byte
                    '    Array.Copy(state__1.Buffer, 0, DataBytes, 0, DataBytes.Length)
                    '    RaiseEvent DataAvailable(Me, New DataAvailableEventArgs(DataBytes))
                    'End If

                    ' Get the rest of the data.
                    stream.BeginRead(state__1.Buffer, 0, State.BufferSize, New AsyncCallback(AddressOf ReceiveCallback), state__1)
                Else
                    ' Raise an event here signalling completion
                    'Disconnect()

                End If
            Catch ex As ObjectDisposedException
                'Do nothing; this happens after we've already disconnected.
            Catch ex As Exception
                If ex.InnerException IsNot Nothing AndAlso ex.InnerException.GetType Is GetType(ObjectDisposedException) Then
                    'Do nothing; this happens after we've already disconnected.
                Else
                    Logger.Error(ex.Message, ex)
                    Disconnect("Error reading from socket or processing received data: " & ex.Message)
                End If
            End Try
        End Sub

        ''' <summary>
        ''' Writes data to the socket.
        ''' </summary>
        ''' <param name="client">The socket to write to.</param>
        ''' <param name="byteData">The data to write.</param>
        Private Sub Send(ByVal client As TcpClient, ByVal byteData As Byte())
            Logger.Trace("")
            ' Begin sending the data to the remote device.
            sendDone.Reset()
            stream.BeginWrite(byteData, 0, byteData.Length, New AsyncCallback(AddressOf SendCallback), client)
        End Sub

        ''' <summary>
        ''' Callback for the send operation.
        ''' </summary>
        ''' <param name="ar">Stores state information for this asynchronous 
        ''' operation as well as any user-defined data.</param>
        Private Sub SendCallback(ByVal ar As IAsyncResult)
            Logger.Trace("")
            ' Retrieve the socket from the state object.
            'Dim client As TcpClient = DirectCast(ar.AsyncState, TcpClient)

            ' Complete sending the data to the remote device.
            Try
                stream.EndWrite(ar)
            Catch ex As Exception
                Logger.Error(ex.Message, ex)
            End Try

            ' Signal that all bytes have been sent.
            sendDone.[Set]()
        End Sub

#End Region

        Private Function TCPClient_Connected(Client As TcpClient) As Boolean
            Try
                Logger.Trace("Started")
                If Client Is Nothing Then Return False

                'Return Client.Connected 'this is unreliable
                Dim ipProperties As System.Net.NetworkInformation.IPGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties()
                Dim tcpConnections As System.Net.NetworkInformation.TcpConnectionInformation() = ipProperties.GetActiveTcpConnections().Where(Function(x) x.LocalEndPoint.Equals(Client.Client.LocalEndPoint) AndAlso x.RemoteEndPoint.Equals(Client.Client.RemoteEndPoint)).ToArray()
                If tcpConnections IsNot Nothing AndAlso tcpConnections.Length > 0 Then
                    Return (tcpConnections.First().State = System.Net.NetworkInformation.TcpState.Established)
                Else
                    Return False
                End If
            Catch ex As Exception
                'Logger.Error(ex.Message, ex)
                Return False
            Finally
                Logger.Trace("Finished")
            End Try
        End Function

        Protected Overrides Sub SetLocalEcho(ByVal echo As Boolean)
        End Sub

        Protected Overrides Sub NotifyEndOfRecord(ByVal buf() As Byte)

            If buf.Length > 0 Then
                RaiseEvent DataAvailable(Me, New DataAvailableEventArgs(buf))
            End If

        End Sub

#Region "Cleanup"

        Public Sub Close()
            Logger.Trace("")
            Dispose()
        End Sub

        Public Sub Dispose()
            Logger.Trace("")
            GC.SuppressFinalize(Me)
            Dispose(True)
        End Sub

        Protected Sub Dispose(ByVal disposing As Boolean)
            Logger.Trace("")
            If disposing Then
                Disconnect()
            End If
        End Sub

        Protected Overrides Sub Finalize()
            Logger.Trace("")
            Try
                Dispose(False)
            Finally
                MyBase.Finalize()
            End Try
        End Sub

#End Region
    End Class

#Region "Event handlers"

    ''' <summary>
    ''' A delegate type for hooking up disconnect notifications.
    ''' </summary>
    Public Delegate Sub DisconnectedEventHandler(ByVal sender As Object, ByVal e As disconnecteventargs)

    ''' <summary>
    ''' A delegate type for hooking up data available notifications.
    ''' </summary>
    Public Delegate Sub DataAvailableEventHandler(ByVal sender As Object, ByVal e As DataAvailableEventArgs)

    ''' <summary>
    ''' A delegate type for hooking up connection completion notifications.
    ''' </summary>
    Public Delegate Sub ConnectionAttemptCompletedEventHandler(ByVal sender As Object, ByVal e As EventArgs)
#End Region
End Namespace
