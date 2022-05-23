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

Partial Public Class Emulator
    Public Class StartupResponse
        Private _Code As String
        Private _Kerberos As Boolean
        Private Codes As Dictionary(Of String, String)
        Public ReadOnly Property Code As String
            Get
                Return _Code
            End Get
        End Property
        Public ReadOnly Property Description As String
            Get
                Return Codes(_Code)
            End Get
        End Property
        Public ReadOnly Property Success As Boolean
            Get
                Select Case _Code
                    Case "I901", "I902", "I906"
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property
        Public Sub New(ByVal ResponseCode As String, ByVal UsingKerberos As Boolean)
            _Code = ResponseCode
            _Kerberos = UsingKerberos
            Codes = New Dictionary(Of String, String)
            'Success codes
            Codes.Add("I901", "Virtual device has less function than source device.")
            Codes.Add("I902", "Session successfully started.")
            Codes.Add("I906", "Automatic sign-on requested, but not allowed.")
            'Failure codes
            Codes.Add("2702", "Device description not found.")
            Codes.Add("2703", "Controller description not found.")
            Codes.Add("2777", "Damaged device description.")
            Codes.Add("8901", "Device not varied on.")
            Codes.Add("8902", "Device not available.")
            Codes.Add("8903", "Device not valid for session.")
            Codes.Add("8906", "Session initiation failed.")
            Codes.Add("8907", "Session failure.")
            Codes.Add("8910", "Controller not valid for session.")
            Codes.Add("8916", "No matching device found.")
            Codes.Add("8917", "Not authorized to object.")
            Codes.Add("8918", "Job canceled.")
            Codes.Add("8920", "Object partially damaged.")
            Codes.Add("8921", "Communications error.")
            Codes.Add("8922", "Negative response received.")
            Codes.Add("8923", "Start-up record built incorrectly.")
            Codes.Add("8925", "Creation of device failed.")
            Codes.Add("8928", "Change of device failed.")
            Codes.Add("8929", "Vary on or vary off failed.")
            Codes.Add("8930", "Message queue does not exist.")
            Codes.Add("8934", "Start-up for S/36 WSF received.")
            Codes.Add("8935", "Session rejected.")
            Codes.Add("8936", "Security failure on session attempt.")
            Codes.Add("8937", "Automatic sign-on rejected.")
            Codes.Add("8940", "Automatic configuration failed or not allowed.")
            Codes.Add("I904", "Source system at incompatible release.")
            If _Kerberos Then
                Codes.Add("0001", "User profile is disabled.")
                Codes.Add("0002", "Kerberos principal maps to a system user profile.")
                Codes.Add("0003", "Enterprise Identity Map (EIM) configuration error.")
                Codes.Add("0004", "EIM does not map Kerberos principal to user profile.")
                Codes.Add("0005", "EIM maps Kerberos principal to multiple user profiles.")
                Codes.Add("0006", "EIM maps Kerberos principal to user profile not found on system.")
                Codes.Add("1000", "None of the requested mechanisms are supported by the local system.")
                Codes.Add("2000", "The input name is not formatted properly or is not valid.")
                Codes.Add("6000", "The received input token contains an incorrect signature.")
                Codes.Add("7000", "No credentials available or credentials valid for context init only.")
                Codes.Add("9000", "Consistency checks performed on the input token failed.")
                Codes.Add("A000", "Consistency checks on the cred structure failed.")
                Codes.Add("B000", "Credentials are no longer valid.")
                Codes.Add("D000", "The runtime failed for reasons that are not defined at the GSS level.")
            Else
                Codes.Add("0001", "System error.")
                Codes.Add("0002", "Userid unknown.")
                Codes.Add("0003", "Userid disabled.")
                Codes.Add("0004", "Invalid password/passphrase/token.")
                Codes.Add("0005", "Password/passphrase/token is expired.")
                Codes.Add("0006", "Pre-V2R2 password.")
                Codes.Add("0008", "Next invalid password/passphrase/token will revoke userid.")
            End If
            If Not Codes.ContainsKey(ResponseCode) Then Codes.Add(ResponseCode, ResponseCode)
        End Sub
    End Class
End Class
