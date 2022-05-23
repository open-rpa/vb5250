'
' Ported from Seva Petrov's .Net Telnet.
'
' Copyright 2012, 2013 Alec Skelly
' (c) Seva Petrov 2002. All Rights Reserved.
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
Imports System.Net.Sockets

Namespace Net.Graphite.Telnet
	''' <summary>
	''' State object for receiving data from remote device.
	''' </summary>
	Public Class State
		''' <summary>
		''' Size of receive buffer.
		''' </summary>
		Public Const BufferSize As Integer = 256
		''' <summary>
		''' Client socket.
		''' </summary>
        Public WorkClient As TcpClient = Nothing
        ''' <summary>
        ''' Receive buffer.
        ''' </summary>
		Public Buffer As Byte() = New Byte(BufferSize - 1) {}
	End Class
End Namespace
