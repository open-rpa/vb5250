<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormSSLCertErrors
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormSSLCertErrors))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.CertErrorsListBox = New System.Windows.Forms.ListBox()
        Me.RejectButton = New System.Windows.Forms.Button()
        Me.AcceptButton = New System.Windows.Forms.Button()
        Me.AcceptAlwaysButton = New System.Windows.Forms.Button()
        Me.CertPropsListView = New System.Windows.Forms.ListView()
        Me.PropertyColumnHeader = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ValueColumnHeader = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(13, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(235, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "The host certificate contains the following errors:"
        '
        'CertErrorsListBox
        '
        Me.CertErrorsListBox.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CertErrorsListBox.FormattingEnabled = True
        Me.CertErrorsListBox.Location = New System.Drawing.Point(12, 30)
        Me.CertErrorsListBox.Name = "CertErrorsListBox"
        Me.CertErrorsListBox.Size = New System.Drawing.Size(491, 69)
        Me.CertErrorsListBox.TabIndex = 1
        '
        'RejectButton
        '
        Me.RejectButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.RejectButton.Location = New System.Drawing.Point(204, 304)
        Me.RejectButton.Name = "RejectButton"
        Me.RejectButton.Size = New System.Drawing.Size(96, 23)
        Me.RejectButton.TabIndex = 2
        Me.RejectButton.Text = "Reject"
        Me.RejectButton.UseVisualStyleBackColor = True
        '
        'AcceptButton
        '
        Me.AcceptButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.AcceptButton.Location = New System.Drawing.Point(306, 304)
        Me.AcceptButton.Name = "AcceptButton"
        Me.AcceptButton.Size = New System.Drawing.Size(96, 23)
        Me.AcceptButton.TabIndex = 3
        Me.AcceptButton.Text = "Accept"
        Me.AcceptButton.UseVisualStyleBackColor = True
        '
        'AcceptAlwaysButton
        '
        Me.AcceptAlwaysButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.AcceptAlwaysButton.Location = New System.Drawing.Point(408, 304)
        Me.AcceptAlwaysButton.Name = "AcceptAlwaysButton"
        Me.AcceptAlwaysButton.Size = New System.Drawing.Size(96, 23)
        Me.AcceptAlwaysButton.TabIndex = 4
        Me.AcceptAlwaysButton.Text = "Accept Always"
        Me.AcceptAlwaysButton.UseVisualStyleBackColor = True
        '
        'CertPropsListView
        '
        Me.CertPropsListView.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CertPropsListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.PropertyColumnHeader, Me.ValueColumnHeader})
        Me.CertPropsListView.FullRowSelect = True
        Me.CertPropsListView.GridLines = True
        Me.CertPropsListView.Location = New System.Drawing.Point(12, 106)
        Me.CertPropsListView.Name = "CertPropsListView"
        Me.CertPropsListView.Size = New System.Drawing.Size(491, 192)
        Me.CertPropsListView.TabIndex = 2
        Me.CertPropsListView.UseCompatibleStateImageBehavior = False
        Me.CertPropsListView.View = System.Windows.Forms.View.Details
        '
        'PropertyColumnHeader
        '
        Me.PropertyColumnHeader.Text = "Property"
        '
        'ValueColumnHeader
        '
        Me.ValueColumnHeader.Text = "Value"
        '
        'FormSSLCertErrors
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(515, 332)
        Me.Controls.Add(Me.CertPropsListView)
        Me.Controls.Add(Me.AcceptAlwaysButton)
        Me.Controls.Add(Me.AcceptButton)
        Me.Controls.Add(Me.RejectButton)
        Me.Controls.Add(Me.CertErrorsListBox)
        Me.Controls.Add(Me.Label1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(337, 265)
        Me.Name = "FormSSLCertErrors"
        Me.Text = "Certificate Errors"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents CertErrorsListBox As System.Windows.Forms.ListBox
    Friend WithEvents RejectButton As System.Windows.Forms.Button
    Friend WithEvents AcceptButton As System.Windows.Forms.Button
    Friend WithEvents AcceptAlwaysButton As System.Windows.Forms.Button
    Friend WithEvents CertPropsListView As System.Windows.Forms.ListView
    Friend WithEvents PropertyColumnHeader As System.Windows.Forms.ColumnHeader
    Friend WithEvents ValueColumnHeader As System.Windows.Forms.ColumnHeader
End Class
