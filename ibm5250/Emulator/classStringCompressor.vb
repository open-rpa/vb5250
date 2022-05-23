'
' Copyright 2012, 2013 Alec Skelly
'
' This file is part of IBM5250, a VB.Net implementation of IBM's 5250 protocol.
'
' IBM5250 is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' IBM5250 is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with IBM5250. If not, see <http://www.gnu.org/licenses/>.
' 

Imports System
Imports System.IO
Imports System.IO.Compression
Imports System.Text
Public Class StringCompressor
    Public Shared Function Compress(ByVal s As String) As String
        Dim bytes() As Byte = Encoding.Unicode.GetBytes(s)
        Using msi As New MemoryStream(bytes)
            Using mso As New MemoryStream
                Using gs As New GZipStream(mso, CompressionMode.Compress)
                    msi.CopyTo(gs)
                End Using
                Return Convert.ToBase64String(mso.ToArray())
            End Using
        End Using
    End Function

    Public Shared Function Decompress(ByVal s As String) As String
        Dim bytes() As Byte = Convert.FromBase64String(s)
        Using msi As New MemoryStream(bytes)
            Using mso As New MemoryStream()
                Using gs As New GZipStream(msi, CompressionMode.Decompress)
                    gs.CopyTo(mso)
                End Using
                Return Encoding.Unicode.GetString(mso.ToArray())
            End Using
        End Using
    End Function

End Class
