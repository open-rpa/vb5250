<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPasswordChange
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.NewPasswordTextBox = New System.Windows.Forms.TextBox()
        Me.ConfirmNewPasswordTextBox = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.OKButton = New System.Windows.Forms.Button()
        Me.CancelButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'NewPasswordTextBox
        '
        Me.NewPasswordTextBox.Location = New System.Drawing.Point(141, 12)
        Me.NewPasswordTextBox.Name = "NewPasswordTextBox"
        Me.NewPasswordTextBox.Size = New System.Drawing.Size(171, 20)
        Me.NewPasswordTextBox.TabIndex = 0
        Me.NewPasswordTextBox.UseSystemPasswordChar = True
        '
        'ConfirmNewPasswordTextBox
        '
        Me.ConfirmNewPasswordTextBox.Location = New System.Drawing.Point(141, 39)
        Me.ConfirmNewPasswordTextBox.Name = "ConfirmNewPasswordTextBox"
        Me.ConfirmNewPasswordTextBox.Size = New System.Drawing.Size(171, 20)
        Me.ConfirmNewPasswordTextBox.TabIndex = 1
        Me.ConfirmNewPasswordTextBox.UseSystemPasswordChar = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(16, 15)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(78, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "New Password"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(16, 42)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(116, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Confirm New Password"
        '
        'OKButton
        '
        Me.OKButton.Location = New System.Drawing.Point(237, 85)
        Me.OKButton.Name = "OKButton"
        Me.OKButton.Size = New System.Drawing.Size(75, 23)
        Me.OKButton.TabIndex = 3
        Me.OKButton.Text = "OK"
        Me.OKButton.UseVisualStyleBackColor = True
        '
        'CancelButton
        '
        Me.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.CancelButton.Location = New System.Drawing.Point(153, 85)
        Me.CancelButton.Name = "CancelButton"
        Me.CancelButton.Size = New System.Drawing.Size(75, 23)
        Me.CancelButton.TabIndex = 5
        Me.CancelButton.Text = "Cancel"
        Me.CancelButton.UseVisualStyleBackColor = True
        '
        'frmPasswordChange
        '
        Me.AcceptButton = Me.OKButton
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(324, 119)
        Me.ControlBox = False
        Me.Controls.Add(Me.CancelButton)
        Me.Controls.Add(Me.OKButton)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.ConfirmNewPasswordTextBox)
        Me.Controls.Add(Me.NewPasswordTextBox)
        Me.Name = "frmPasswordChange"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.Text = "Change Password"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents NewPasswordTextBox As System.Windows.Forms.TextBox
    Friend WithEvents ConfirmNewPasswordTextBox As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents OKButton As System.Windows.Forms.Button
    Friend WithEvents CancelButton As System.Windows.Forms.Button
End Class
