Public Class frmLogon
    Private Sub OKButton_Click(sender As Object, e As EventArgs) Handles OKButton.Click
        If String.IsNullOrWhiteSpace(UserNameTextBox.Text) Or String.IsNullOrWhiteSpace(PasswordTextBox.Text) Then
            MsgBox("Please supply both User Name and Password", MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Missing Information")
        Else
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End If
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub frmLogon_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        If String.IsNullOrWhiteSpace(UserNameTextBox.Text) Then UserNameTextBox.Focus() Else PasswordTextBox.Focus()
    End Sub
End Class