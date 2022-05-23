Module modiAccess
    Dim logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

    Public Function PreAuthenticate(ByVal Host As String, ByVal UseSSL As Boolean, ByVal UserName As String, ByRef PasswordLevel As Integer, ByRef Password As String, TimeoutMS As Integer) As Boolean
        '
        'PasswordLevel and Password are ByRef and will contain current values when this function returns True.
        '
        logger.Debug("Begin")
        Dim SignonSucceeded As Boolean = False
        Try
            Dim SvrMap As New IBMiClient.Client.SvrMap(Host)
            Dim SignonPort As Integer = 0
            Dim ServiceName As String = "as-signon"
            If UseSSL Then ServiceName = "as-signon-s"
            Try
                SignonPort = SvrMap.GetServicePort(ServiceName, TimeoutMS)
            Catch exx As Exception
                Dim msg As String = "Error getting '" & ServiceName & "' port number from '" & Host & "': " & exx.Message
                logger.Error(msg)
                'MsgBox(msg, MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Error")
            End Try
            If SignonPort < 1 Then
                logger.Warn("GetServicePort returned '" & SignonPort & "'; using default port instead.")
                SignonPort = 8476
                If UseSSL Then SignonPort = 9476
            End If
            logger.Debug("SignonPort is '" & SignonPort.ToString & "'")
            If SignonPort > 0 Then
                Dim Signon As New IBMiClient.Client.Signon(Host, SignonPort, UseSSL)
                Dim ServerInfo As IBMiClient.Client.Signon.ServerInfo = Signon.Get_Server_Attributes(TimeoutMS)

                PasswordLevel = ServerInfo.PasswordLevel 'PasswordLevel > 1 uses SHA, else DES.
                logger.Debug("PasswordLevel is '" & PasswordLevel.ToString & "'")

                Dim ChangePasswordMsgResult As MsgBoxResult = 0
                Dim SignonInfo As IBMiClient.Client.Signon.SignonInfo = Signon.Get_Signon_Info(UserName, Password, TimeoutMS)
                logger.Debug("Get_Signon_Info returned code '" & SignonInfo.Code.ToString & "'")

                If SignonInfo.Code <> 0 Then 'there was an error
                    Dim Reason As String = Nothing
                    For Each Msg As IBMiClient.Client.Signon.SignonInfoMessage In SignonInfo.Messages
                        If Not String.IsNullOrEmpty(Reason) Then Reason += vbCr
                        Reason += Msg.Text
                        If Not String.IsNullOrEmpty(Msg.ReasonText) Then
                            Reason += " (" & Msg.ReasonText & ")"
                        End If
                    Next
                    If String.IsNullOrEmpty(Reason) Then Reason = SignonInfo.Text
                    If String.IsNullOrEmpty(Reason) Then Reason = "Signon failed with error code " & SignonInfo.Code.ToString
                    logger.Debug("Reason is '" & Reason & "'")
                    If Reason.ToLower.Contains("password") And Reason.ToLower.Contains("expired") Then
                        ChangePasswordMsgResult = MsgBox("Your password has expired.  Would you like to change it now?", MsgBoxStyle.YesNo + MsgBoxStyle.Exclamation)
                    Else
                        MsgBox(Reason, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Signon Failed")
                    End If
                Else
                    SignonSucceeded = True
                    If (SignonInfo.ExpirationDate > Date.MinValue) Then 'Password has an expiration date
                        Dim TimeRemaining As TimeSpan = SignonInfo.ExpirationDate - Now
                        If TimeRemaining < TimeSpan.Zero Then 'Should not get here.  If the password has expired Result will be non-zero.
                            ChangePasswordMsgResult = MsgBox("Your password has expired.  Would you like to change it now?", MsgBoxStyle.YesNo + MsgBoxStyle.Exclamation)
                        Else
                            If TimeRemaining.Days < 8 Then
                                Dim Msg As String = "Your password will expire"
                                If TimeRemaining.Days < 1 Then Msg += " today." Else Msg += " in " & TimeRemaining.Days.ToString & IIf(TimeRemaining.Days = 1, " day.", " days.")
                                Msg += "  Would you like to change it now?"
                                ChangePasswordMsgResult = MsgBox(Msg, MsgBoxStyle.YesNo + MsgBoxStyle.Information)
                            End If
                        End If
                    End If
                End If
                If ChangePasswordMsgResult = MsgBoxResult.Yes Then
                    Dim pwResult As Integer = ChangePassword(Signon, UserName, Password, TimeoutMS)
                    If pwResult > 0 Then SignonSucceeded = True
                End If
            End If
        Catch ex As Exception
            logger.Error(ex, ex.Message)
            MsgBox(ex.Message, MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Error")
        End Try
        logger.Debug("Returning '" & SignonSucceeded.ToString & "'")
        Return SignonSucceeded
    End Function

    Public Function ChangePassword(ByVal Host As String, ByVal UseSSL As Boolean, ByVal UserName As String, ByRef Password As String, TimeoutMS As Integer) As Integer
        '
        'Return values:
        '   0 = No password was changed
        '   1 = AS400 password was changed
        '
        Dim SignonSucceeded As Boolean = False
        Try
            Dim SvrMap As New IBMiClient.Client.SvrMap(Host)
            Dim SignonPort As Integer = 0
            Dim ServiceName As String = "as-signon"
            If UseSSL Then ServiceName = "as-signon-s"
            Try
                SignonPort = SvrMap.GetServicePort(ServiceName, TimeoutMS)
            Catch exx As Exception
                Dim msg As String = "Error getting '" & ServiceName & "' port number from '" & Host & "': " & exx.Message
                logger.Error(msg)
                'MsgBox(msg, MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Error")
            End Try
            If SignonPort < 1 Then
                logger.Warn("GetServicePort returned '" & SignonPort & "'; using default port instead.")
                SignonPort = 8476
                If UseSSL Then SignonPort = 9476
            End If
            logger.Debug("SignonPort is '" & SignonPort.ToString & "'")
            If SignonPort > 0 Then
                Dim Signon As New IBMiClient.Client.Signon(Host, SignonPort, UseSSL)
                Return ChangePassword(Signon, UserName, Password, TimeoutMS)
            End If
        Catch ex As Exception
            logger.Error(ex)
        End Try
        Return 0
    End Function

    Private Function ChangePassword(Signon As IBMiClient.Client.Signon, ByVal UserName As String, ByRef Password As String, TimeoutMS As Integer) As Integer
        '
        'Return values:
        '   0 = No password was changed
        '   1 = AS400 password was changed
        '
        Dim RetVal As Integer = 0
        Dim ChangePasswordMsgResult As MsgBoxResult = 0
        Dim pwResult As IBMiClient.Client.Signon.ChangePasswordInfo
        Do
            Dim NewPassword As String
            Dim pcf As New frmPasswordChange(UserName)
            pcf.StartPosition = FormStartPosition.CenterParent
            Dim pcResult As DialogResult = pcf.ShowDialog
            If pcResult = DialogResult.OK Then
                NewPassword = pcf.NewPasswordTextBox.Text.Trim

                pwResult = Signon.Change_Password(UserName, Password, NewPassword, TimeoutMS)
                logger.Debug("Change_Password returned code '" & pwResult.Code.ToString & "'")
                If pwResult.Code = 0 Then
                    MsgBox("iSeries password changed successfully", vbInformation + vbOKOnly, "Password Change")
                    'SignonSucceeded = True
                    RetVal = 1
                    Password = NewPassword
                    Exit Do
                Else
                    Dim s As String = "The password could not be changed for the following reasons:" & vbCr
                    If Not String.IsNullOrWhiteSpace(pwResult.Text) Then s += pwResult.Text
                    For Each m As IBMiClient.Client.Signon.SignonInfoMessage In pwResult.Messages
                        Dim ss As String = Nothing
                        If Not String.IsNullOrWhiteSpace(m.Text) Then ss = m.Text
                        If Not String.IsNullOrWhiteSpace(m.ReasonText) Then ss += " (" & m.ReasonText & ")"
                        If Not String.IsNullOrWhiteSpace(ss) Then s += vbCr & ss
                    Next
                    logger.Debug(s)
                    s += vbCr & vbCr & "Would you like to try again?"
                    ChangePasswordMsgResult = MsgBox(s, MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo, "Password Change")
                End If
            Else
                Exit Do
            End If
        Loop While ChangePasswordMsgResult <> MsgBoxResult.No
        Return RetVal
    End Function

End Module
