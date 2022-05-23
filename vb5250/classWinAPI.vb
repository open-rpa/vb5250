'
' Copyright 2013 Alec Skelly
'
' This file is part of VB5250, a telnet client implementing IBM's 5250 protocol.
'
' VB5250 is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' VB5250 is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with VB5250. If not, see <http://www.gnu.org/licenses/>.
' 
Imports System.Runtime.InteropServices

Public Class WinAPI
    Public Const WS_EX_COMPOSITED As Integer = &H2000000
    Public Const EM_SETPASSWORDCHAR As Integer = &HCC
    Public Const VK_INSERT As Integer = &H2D
    Public Const WM_KEYDOWN As Integer = &H100
    Public Const WM_KEYUP As Integer = &H101

    Public Const WM_PAINT As Integer = &HF
    Public Const WM_KILLFOCUS As Integer = &H8

    Public Declare Auto Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hwnd As Int32, ByVal wMsg As Int32, ByVal wParam As Int32, ByVal lParam As String) As Int32
    Public Declare Auto Function CreateCaret Lib "user32" (ByVal hWnd As IntPtr, ByVal hBitmap As IntPtr, ByVal nWidth As Integer, ByVal nHeight As Integer) As Boolean
    Public Declare Auto Function ShowCaret Lib "user32" (ByVal hWnd As IntPtr) As Boolean
    Public Declare Auto Function DestroyCaret Lib "user32" () As Boolean

End Class

