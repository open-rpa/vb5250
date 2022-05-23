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

Namespace Net.Graphite.Telnet
	''' <summary>
	''' Encapsulates arguments of the event fired when data becomes
	''' available on the telnet socket.
	''' </summary>
	Public Class DataAvailableEventArgs
		Inherits EventArgs
        'Private m_data As String
        Private m_data() As Byte

		''' <summary>
		''' Creates a new instance of the DataAvailableEventArgs class.
		''' </summary>
		''' <param name="output">Output from the session.</param>
        Public Sub New(ByVal output() As Byte)
            m_data = output
        End Sub
        'Public Sub New(output As String)
        '	m_data = output
        'End Sub

		''' <summary>
		''' Gets the data from the telnet session.
		''' </summary>
        '''Public ReadOnly Property Data As String
        Public ReadOnly Property Data() As Byte()
            Get
                Return m_data
            End Get
        End Property
    End Class

    Public Class DisconnectEventArgs
        Inherits System.EventArgs
        Public Reason As String
        Public Sub New(ByVal Reason As String)
            Me.Reason = Reason
        End Sub
    End Class
End Namespace
