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
Module modGlobals
    Public Function ToBool(ByVal obj As Object) As Boolean
        ToBool = False
        If obj IsNot Nothing Then
            If obj.GetType Is GetType(String) Then
                Select Case obj.ToString.ToUpper
                    Case "Y", "YES", "TRUE"
                        ToBool = True
                        Exit Function
                End Select
            ElseIf IsNumeric(obj) Then
                Try
                    If CInt(obj) <> 0 Then ToBool = True
                Catch ex As Exception
                End Try
            End If
        End If
    End Function
End Module
