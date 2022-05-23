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
Imports System.Text
Imports System.Collections.Specialized
Imports System.Security.Cryptography

Namespace SimpleCryptoExceptions
    Public Class SuppliedStringNotEncryptedException
        Inherits System.ApplicationException
        Public Sub New()
            MyBase.New("The supplied string is not encrypted.")
        End Sub
    End Class
End Namespace

Public Class SimpleCrypto
    'This class provides extremely weak security.  It is only intended to hide passwords from casual observers.
    'Since this is open source software, the key below is public.
    Private lbtVector() As Byte = {240, 3, 45, 29, 0, 76, 173, 59}
    Private lscryptoKey As String = "aN3C%htPDJ3pK&lEm!gs"

    Public Function Decrypt(ByVal sQueryString As String) As String
        Dim buffer() As Byte
        Dim loCryptoClass As New TripleDESCryptoServiceProvider
        Dim loCryptoProvider As New MD5CryptoServiceProvider
        If Left(sQueryString, 3) = "{-!" AndAlso Right(sQueryString, 3) = "!-}" Then
            sQueryString = Replace(sQueryString, "{-!", "")
            sQueryString = Replace(sQueryString, "!-}", "")
        Else
            Throw New SimpleCryptoExceptions.SuppliedStringNotEncryptedException
        End If
        Try
            buffer = Convert.FromBase64String(sQueryString)
            loCryptoClass.Key = loCryptoProvider.ComputeHash(ASCIIEncoding.ASCII.GetBytes(lscryptoKey))
            loCryptoClass.IV = lbtVector
            Return Encoding.ASCII.GetString(loCryptoClass.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length()))
        Catch ex As Exception
            Throw
        Finally
            loCryptoClass.Clear()
            loCryptoProvider.Clear()
            loCryptoClass = Nothing
            loCryptoProvider = Nothing
        End Try
    End Function

    Public Function Encrypt(ByVal sInputVal As String) As String
        Dim loCryptoClass As New TripleDESCryptoServiceProvider
        Dim loCryptoProvider As New MD5CryptoServiceProvider
        Dim lbtBuffer() As Byte
        Try
            lbtBuffer = System.Text.Encoding.ASCII.GetBytes(sInputVal)
            loCryptoClass.Key = loCryptoProvider.ComputeHash(ASCIIEncoding.ASCII.GetBytes(lscryptoKey))
            loCryptoClass.IV = lbtVector
            sInputVal = Convert.ToBase64String(loCryptoClass.CreateEncryptor().TransformFinalBlock(lbtBuffer, 0, lbtBuffer.Length()))
            Encrypt = "{-!" & sInputVal & "!-}"
        Catch ex As CryptographicException
            Throw
        Catch ex As FormatException
            Throw
        Catch ex As Exception
            Throw
        Finally
            loCryptoClass.Clear()
            loCryptoProvider.Clear()
            loCryptoClass = Nothing
            loCryptoProvider = Nothing
        End Try
    End Function
End Class
