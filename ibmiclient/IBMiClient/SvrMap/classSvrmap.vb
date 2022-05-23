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
    Public Class SvrMap
        Private tc As System.Net.Sockets.TcpClient
        Public Const as_svrmap_port As Integer = 449
        Private Host_ As String
        Private ConnectException As Exception
        Private ConnectDone As New System.Threading.ManualResetEvent(False)

        Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

        Public Sub New(Host As String)
            Logger.Trace("Host='" & Host & "'")
            If String.IsNullOrWhiteSpace(Host) Then Throw New ArgumentException("Host cannot be null")
            Host_ = Host
        End Sub

        Private Function Connect(TimeoutMS As Integer) As Boolean
            If String.IsNullOrWhiteSpace(Host_) Then Throw New Exception("The Host parameter has not been set")
            Return Connect(Host_, TimeoutMS)
        End Function
        Private Function Connect(Host As String, TimeoutMS As Integer) As Boolean
            Logger.Trace("Host='" & Host & "'")
            If String.IsNullOrWhiteSpace(Host) Then Throw New ArgumentException("Host cannot be null")
            Try
                ConnectException = Nothing
                tc = New System.Net.Sockets.TcpClient
                ConnectDone.Reset()
                Dim Result As IAsyncResult = tc.BeginConnect(Host, as_svrmap_port, New AsyncCallback(AddressOf ConnectCallback), Nothing)
                Dim Success As Object = Result.AsyncWaitHandle.WaitOne(TimeoutMS)
                If Success Then
                    ConnectDone.WaitOne() 'Wait for ConnectCallback to complete.
                    If ConnectException IsNot Nothing Then Throw ConnectException
                    Logger.Debug("Success")
                    Return True
                Else
                    Throw New Exception("Failed to connect to host '" & Host & "' on port " & as_svrmap_port.ToString)
                End If
            Catch ex As Exception
                Logger.Error(ex)
            End Try
            Return False
        End Function

        Private Sub ConnectCallback(ByVal ar As IAsyncResult)
            Logger.Trace("")
            Try
                tc.EndConnect(ar) 'Complete the connection.
                ConnectDone.Set() 'Signal that the connection attempt has completed.
            Catch ex As Exception
                Logger.Error(ex)
                ConnectException = ex
                ConnectDone.Set() 'Signal that the connection attempt has completed. Don't move this to a Finally block or Disconnect() will hang waiting for the signal.
                Disconnect()
            End Try
        End Sub

        Private Sub Disconnect()
            Logger.Trace("")
            Try
                If tc IsNot Nothing Then tc.Close()
            Catch ex As Exception
                Logger.Error(ex)
            End Try
        End Sub

        Public Function GetServicePort(ServiceName As String, TimeoutMS As Integer) As Integer
            Return ConnectAndGetServicePort(Host_, ServiceName, TimeoutMS)
        End Function

        Private Function ConnectAndGetServicePort(Host As String, ServiceName As String, TimeoutMS As Integer) As Integer
            Logger.Trace("Host='" & Host & "', ServiceName='" & ServiceName & "', TimeoutMS=" & TimeoutMS.ToString)
            Try
                Disconnect()
                If Connect(Host, TimeoutMS) Then Return GetServicePortX(ServiceName, TimeoutMS)
            Catch ex As Exception
                Logger.Error(ex)
            Finally
                Disconnect()
            End Try
            Return 0
        End Function

        Private Function GetServicePortX(ServiceName As String, TimeoutMS As Integer) As Integer
            Logger.Trace("ServiceName='" & ServiceName & "', TimeoutMS=" & TimeoutMS.ToString)
            If String.IsNullOrWhiteSpace(ServiceName) Then Throw New ArgumentException("ServiceName cannot be null")
            Try
                Dim s As System.Net.Sockets.NetworkStream = tc.GetStream
                Dim b() As Byte
                b = System.Text.Encoding.UTF8.GetBytes(ServiceName)
                ReDim Preserve b(ServiceName.Length) 'one extra byte for terminating zero.
                s.Write(b, 0, b.Length)
                Dim ExpectedBytes As Integer = 5
                Dim start As Date = Now
                Dim BytesRead As Integer = 0
                ReDim b(ExpectedBytes - 1)
                Do While Now < start.AddMilliseconds(CDbl(TimeoutMS))
                    BytesRead += s.Read(b, BytesRead, b.Length - BytesRead)
                    If BytesRead >= ExpectedBytes Then Exit Do
                    Threading.Thread.CurrentThread.Join(100)
                Loop
                If BytesRead >= ExpectedBytes Then
                    Dim ms As New System.IO.MemoryStream(b)
                    Dim UnknownByte As Byte = ms.ReadByte() 'result code?  =43.
                    Dim ServerPort As UInt32 = ReadUInt32(ms)
                    Logger.Debug("Success: ServicePort=" & ServerPort.ToString)
                    Return ServerPort
                End If
            Catch ex As Exception
                Logger.Error(ex)
            End Try
            Return 0
        End Function
    End Class
End Class
