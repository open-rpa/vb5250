Public Class FormSSLCertErrors
    Friend Enum CertAction As Byte
        Reject = 0
        Accept = 1
        AcceptAndStore = 2
    End Enum
    Friend Action As CertAction = CertAction.Reject

    Private Sub RejectButton_Click(sender As Object, e As EventArgs) Handles RejectButton.Click
        Me.Action = CertAction.Reject
        Me.Close()
    End Sub

    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles AcceptButton.Click
        Me.Action = CertAction.Accept
        Me.Close()
    End Sub

    Private Sub AcceptAlwaysButton_Click(sender As Object, e As EventArgs) Handles AcceptAlwaysButton.Click
        Me.Action = CertAction.AcceptAndStore
        Me.Close()
    End Sub

    Private Sub FormSSLCertErrors_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        CertPropsListView.AutoResizeColumns(Windows.Forms.ColumnHeaderAutoResizeStyle.ColumnContent)
    End Sub
End Class