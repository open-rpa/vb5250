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
Friend Class ConnectionSettings
    Public UserName As String
    Public Password As String
    Public SavePassword As Boolean 'Save an encrypted version of the password in the INI file?
    Public StationName As String
    Public HostAddress As String
    Public HostPort As Integer
    Public UseSSL As Boolean
    'Public GetServerInfo As Boolean 'Use the IBM host access LIPI servers to learn about the server's capabilities.
    'Public GetUserInfo As Boolean   'Use the IBM host access LIPI servers to pre-authenticate and change password.
    Public PreAuthenticate As Boolean 'Use the IBM host access LIPI servers to detect server capabilities, pre-authenticate, and change password.
    Public BypassLogin As Boolean 'send credentials during telnet handshake; bypass login screen.
    Public AuthEncryptionMethod As Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType 'Crypto method for BypassLogin.
    Public Allow_ScreenSize_27x132 As Boolean
    Public ColorMap As Dictionary(Of String, KnownColor)
    Public AlternateCaretEnabled As Boolean
    Public LocaleName As String
    Public FontFamily As FontFamily
    Public TimeoutMS As Integer

    Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

    Public Sub New()
        'Create default color map
        ColorMap = New Dictionary(Of String, KnownColor)
        ColorMap.Add("Background", KnownColor.Black)
        ColorMap.Add("FieldBackground", KnownColor.Black)
        ColorMap.Add("Green", KnownColor.LimeGreen)
        ColorMap.Add("White", KnownColor.White)
        ColorMap.Add("Blue", KnownColor.CornflowerBlue)
        ColorMap.Add("Pink", KnownColor.Fuchsia)
        ColorMap.Add("Red", KnownColor.Red)
        ColorMap.Add("Turquoise", KnownColor.Cyan)
        ColorMap.Add("Yellow", KnownColor.Yellow)
        ColorMap.Add("Black", KnownColor.Black)
        AlternateCaretEnabled = False               '

        Try
            Dim f As New Font("Consolas", 10, FontStyle.Regular) 'Installed by default on Windows 7
            If f.FontFamily.Name = "Consolas" Then
                FontFamily = f.FontFamily
            Else
                f = New Font("Lucida Console", 10, FontStyle.Regular) 'Installed by default on Windows XP
                If f.FontFamily.Name = "Lucida Console" Then
                    FontFamily = f.FontFamily
                Else
                    FontFamily = System.Drawing.FontFamily.GenericMonospace
                End If
            End If
        Catch ex As Exception
            Logger.Error("Error setting default font family: " & ex.Message, ex)
        End Try

        Allow_ScreenSize_27x132 = True
        BypassLogin = False
        'GetServerInfo = True
        'GetUserInfo = True
        PreAuthenticate = True
        AuthEncryptionMethod = Telnet.De.Mud.Telnet.TelnetProtocolHandler.IBMAuthEncryptionType.None
        HostPort = 23
        UseSSL = False
        StationName = My.Computer.Name
        UserName = My.Computer.FileSystem.GetName(My.User.Name)
        LocaleName = "Default"
        TimeoutMS = 5000
    End Sub

    Public Shared Function EnumFixedPitchFonts() As List(Of String)
        Dim retval As New List(Of String)()
        Using gr As Graphics = Application.OpenForms(0).CreateGraphics()
            For Each fam As FontFamily In FontFamily.Families
                If fam.IsStyleAvailable(FontStyle.Regular) Then
                    Using fnt As New Font(fam, 10, FontStyle.Regular)
                        If gr.MeasureString("iii", fnt).Width = gr.MeasureString("WWW", fnt).Width Then
                            retval.Add(fam.Name)
                        End If
                    End Using
                End If
            Next
            Return retval
        End Using
    End Function
End Class
