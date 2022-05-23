Public Class frmPasswordChange
    Private _UserName As String

    Public Sub New(UserName As String)
        InitializeComponent()

        _UserName = UserName
    End Sub

    Private Sub CancelButton_Click(sender As System.Object, e As System.EventArgs) Handles CancelButton.Click
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub OKButton_Click(sender As System.Object, e As System.EventArgs) Handles OKButton.Click
        Dim NewPassword As String = NewPasswordTextBox.Text.Trim
        If String.IsNullOrWhiteSpace(NewPassword) Then
            MsgBox("Please enter a new password", MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Null Password")
            Try
                NewPasswordTextBox.Focus()
            Catch ex As Exception
            End Try
            Exit Sub
        End If
        If ConfirmNewPasswordTextBox.Text.Trim <> NewPassword Then
            MsgBox("The new passwords don't match!", MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Mismatched Password")
            Try
                NewPasswordTextBox.Focus()
            Catch ex As Exception
            End Try
            Exit Sub
        End If

        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub frmPasswordChange_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
    End Sub

    Private Sub NewPasswordTextBox_Enter(sender As Object, e As System.EventArgs) Handles NewPasswordTextBox.Enter
        NewPasswordTextBox.SelectAll()
    End Sub

    Private Sub ConfirmNewPasswordTextBox_Enter(sender As Object, e As System.EventArgs) Handles ConfirmNewPasswordTextBox.Enter
        ConfirmNewPasswordTextBox.SelectAll()
    End Sub
End Class