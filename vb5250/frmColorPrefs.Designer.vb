<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmColorPrefs
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.ButtonOK = New System.Windows.Forms.Button()
        Me.ButtonCancel = New System.Windows.Forms.Button()
        Me.ComboBoxTheme = New System.Windows.Forms.ComboBox()
        Me.GroupBoxColors = New System.Windows.Forms.GroupBox()
        Me.ColorPickerComboBoxBackground = New VB5250.ColorPickerComboBox()
        Me.ColorPickerComboBoxRed = New VB5250.ColorPickerComboBox()
        Me.ColorPickerComboBoxTurquoise = New VB5250.ColorPickerComboBox()
        Me.ColorPickerComboBoxYellow = New VB5250.ColorPickerComboBox()
        Me.ColorPickerComboBoxPink = New VB5250.ColorPickerComboBox()
        Me.ColorPickerComboBoxBlue = New VB5250.ColorPickerComboBox()
        Me.ColorPickerComboBoxFieldBackground = New VB5250.ColorPickerComboBox()
        Me.ColorPickerComboBoxGreen = New VB5250.ColorPickerComboBox()
        Me.ColorPickerComboBoxWhite = New VB5250.ColorPickerComboBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.GroupBoxColors.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(15, 22)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(65, 13)
        Me.Label1.TabIndex = 12
        Me.Label1.Text = "Background"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(15, 49)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(90, 13)
        Me.Label2.TabIndex = 13
        Me.Label2.Text = "Field Background"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(15, 76)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(36, 13)
        Me.Label3.TabIndex = 14
        Me.Label3.Text = "Green"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(15, 103)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(35, 13)
        Me.Label4.TabIndex = 15
        Me.Label4.Text = "White"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(15, 130)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(27, 13)
        Me.Label5.TabIndex = 16
        Me.Label5.Text = "Red"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(15, 157)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(54, 13)
        Me.Label6.TabIndex = 17
        Me.Label6.Text = "Turquoise"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(15, 184)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(38, 13)
        Me.Label7.TabIndex = 18
        Me.Label7.Text = "Yellow"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(15, 211)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(28, 13)
        Me.Label8.TabIndex = 19
        Me.Label8.Text = "Pink"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(15, 238)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(28, 13)
        Me.Label9.TabIndex = 20
        Me.Label9.Text = "Blue"
        '
        'ButtonOK
        '
        Me.ButtonOK.Location = New System.Drawing.Point(229, 319)
        Me.ButtonOK.Name = "ButtonOK"
        Me.ButtonOK.Size = New System.Drawing.Size(75, 23)
        Me.ButtonOK.TabIndex = 11
        Me.ButtonOK.Text = "OK"
        Me.ButtonOK.UseVisualStyleBackColor = True
        '
        'ButtonCancel
        '
        Me.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ButtonCancel.Location = New System.Drawing.Point(148, 319)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(75, 23)
        Me.ButtonCancel.TabIndex = 10
        Me.ButtonCancel.Text = "Cancel"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'ComboBoxTheme
        '
        Me.ComboBoxTheme.FormattingEnabled = True
        Me.ComboBoxTheme.Location = New System.Drawing.Point(126, 12)
        Me.ComboBoxTheme.Name = "ComboBoxTheme"
        Me.ComboBoxTheme.Size = New System.Drawing.Size(178, 21)
        Me.ComboBoxTheme.TabIndex = 0
        '
        'GroupBoxColors
        '
        Me.GroupBoxColors.Controls.Add(Me.ColorPickerComboBoxBackground)
        Me.GroupBoxColors.Controls.Add(Me.ColorPickerComboBoxRed)
        Me.GroupBoxColors.Controls.Add(Me.ColorPickerComboBoxTurquoise)
        Me.GroupBoxColors.Controls.Add(Me.ColorPickerComboBoxYellow)
        Me.GroupBoxColors.Controls.Add(Me.Label9)
        Me.GroupBoxColors.Controls.Add(Me.ColorPickerComboBoxPink)
        Me.GroupBoxColors.Controls.Add(Me.Label8)
        Me.GroupBoxColors.Controls.Add(Me.ColorPickerComboBoxBlue)
        Me.GroupBoxColors.Controls.Add(Me.Label7)
        Me.GroupBoxColors.Controls.Add(Me.ColorPickerComboBoxFieldBackground)
        Me.GroupBoxColors.Controls.Add(Me.Label6)
        Me.GroupBoxColors.Controls.Add(Me.ColorPickerComboBoxGreen)
        Me.GroupBoxColors.Controls.Add(Me.Label5)
        Me.GroupBoxColors.Controls.Add(Me.ColorPickerComboBoxWhite)
        Me.GroupBoxColors.Controls.Add(Me.Label4)
        Me.GroupBoxColors.Controls.Add(Me.Label1)
        Me.GroupBoxColors.Controls.Add(Me.Label3)
        Me.GroupBoxColors.Controls.Add(Me.Label2)
        Me.GroupBoxColors.Location = New System.Drawing.Point(12, 39)
        Me.GroupBoxColors.Name = "GroupBoxColors"
        Me.GroupBoxColors.Size = New System.Drawing.Size(310, 274)
        Me.GroupBoxColors.TabIndex = 24
        Me.GroupBoxColors.TabStop = False
        '
        'ColorPickerComboBoxBackground
        '
        Me.ColorPickerComboBoxBackground.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.ColorPickerComboBoxBackground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ColorPickerComboBoxBackground.FormattingEnabled = True
        Me.ColorPickerComboBoxBackground.Location = New System.Drawing.Point(114, 19)
        Me.ColorPickerComboBoxBackground.Name = "ColorPickerComboBoxBackground"
        Me.ColorPickerComboBoxBackground.SelectedValue = System.Drawing.Color.White
        Me.ColorPickerComboBoxBackground.Size = New System.Drawing.Size(178, 21)
        Me.ColorPickerComboBoxBackground.TabIndex = 1
        '
        'ColorPickerComboBoxRed
        '
        Me.ColorPickerComboBoxRed.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.ColorPickerComboBoxRed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ColorPickerComboBoxRed.FormattingEnabled = True
        Me.ColorPickerComboBoxRed.Location = New System.Drawing.Point(114, 127)
        Me.ColorPickerComboBoxRed.Name = "ColorPickerComboBoxRed"
        Me.ColorPickerComboBoxRed.SelectedValue = System.Drawing.Color.White
        Me.ColorPickerComboBoxRed.Size = New System.Drawing.Size(178, 21)
        Me.ColorPickerComboBoxRed.TabIndex = 5
        '
        'ColorPickerComboBoxTurquoise
        '
        Me.ColorPickerComboBoxTurquoise.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.ColorPickerComboBoxTurquoise.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ColorPickerComboBoxTurquoise.FormattingEnabled = True
        Me.ColorPickerComboBoxTurquoise.Location = New System.Drawing.Point(114, 154)
        Me.ColorPickerComboBoxTurquoise.Name = "ColorPickerComboBoxTurquoise"
        Me.ColorPickerComboBoxTurquoise.SelectedValue = System.Drawing.Color.White
        Me.ColorPickerComboBoxTurquoise.Size = New System.Drawing.Size(178, 21)
        Me.ColorPickerComboBoxTurquoise.TabIndex = 6
        '
        'ColorPickerComboBoxYellow
        '
        Me.ColorPickerComboBoxYellow.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.ColorPickerComboBoxYellow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ColorPickerComboBoxYellow.FormattingEnabled = True
        Me.ColorPickerComboBoxYellow.Location = New System.Drawing.Point(114, 181)
        Me.ColorPickerComboBoxYellow.Name = "ColorPickerComboBoxYellow"
        Me.ColorPickerComboBoxYellow.SelectedValue = System.Drawing.Color.White
        Me.ColorPickerComboBoxYellow.Size = New System.Drawing.Size(178, 21)
        Me.ColorPickerComboBoxYellow.TabIndex = 7
        '
        'ColorPickerComboBoxPink
        '
        Me.ColorPickerComboBoxPink.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.ColorPickerComboBoxPink.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ColorPickerComboBoxPink.FormattingEnabled = True
        Me.ColorPickerComboBoxPink.Location = New System.Drawing.Point(114, 208)
        Me.ColorPickerComboBoxPink.Name = "ColorPickerComboBoxPink"
        Me.ColorPickerComboBoxPink.SelectedValue = System.Drawing.Color.White
        Me.ColorPickerComboBoxPink.Size = New System.Drawing.Size(178, 21)
        Me.ColorPickerComboBoxPink.TabIndex = 8
        '
        'ColorPickerComboBoxBlue
        '
        Me.ColorPickerComboBoxBlue.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.ColorPickerComboBoxBlue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ColorPickerComboBoxBlue.FormattingEnabled = True
        Me.ColorPickerComboBoxBlue.Location = New System.Drawing.Point(114, 235)
        Me.ColorPickerComboBoxBlue.Name = "ColorPickerComboBoxBlue"
        Me.ColorPickerComboBoxBlue.SelectedValue = System.Drawing.Color.White
        Me.ColorPickerComboBoxBlue.Size = New System.Drawing.Size(178, 21)
        Me.ColorPickerComboBoxBlue.TabIndex = 9
        '
        'ColorPickerComboBoxFieldBackground
        '
        Me.ColorPickerComboBoxFieldBackground.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.ColorPickerComboBoxFieldBackground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ColorPickerComboBoxFieldBackground.FormattingEnabled = True
        Me.ColorPickerComboBoxFieldBackground.Location = New System.Drawing.Point(114, 46)
        Me.ColorPickerComboBoxFieldBackground.Name = "ColorPickerComboBoxFieldBackground"
        Me.ColorPickerComboBoxFieldBackground.SelectedValue = System.Drawing.Color.White
        Me.ColorPickerComboBoxFieldBackground.Size = New System.Drawing.Size(178, 21)
        Me.ColorPickerComboBoxFieldBackground.TabIndex = 2
        '
        'ColorPickerComboBoxGreen
        '
        Me.ColorPickerComboBoxGreen.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.ColorPickerComboBoxGreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ColorPickerComboBoxGreen.FormattingEnabled = True
        Me.ColorPickerComboBoxGreen.Location = New System.Drawing.Point(114, 73)
        Me.ColorPickerComboBoxGreen.Name = "ColorPickerComboBoxGreen"
        Me.ColorPickerComboBoxGreen.SelectedValue = System.Drawing.Color.White
        Me.ColorPickerComboBoxGreen.Size = New System.Drawing.Size(178, 21)
        Me.ColorPickerComboBoxGreen.TabIndex = 3
        '
        'ColorPickerComboBoxWhite
        '
        Me.ColorPickerComboBoxWhite.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.ColorPickerComboBoxWhite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ColorPickerComboBoxWhite.FormattingEnabled = True
        Me.ColorPickerComboBoxWhite.Location = New System.Drawing.Point(114, 100)
        Me.ColorPickerComboBoxWhite.Name = "ColorPickerComboBoxWhite"
        Me.ColorPickerComboBoxWhite.SelectedValue = System.Drawing.Color.White
        Me.ColorPickerComboBoxWhite.Size = New System.Drawing.Size(178, 21)
        Me.ColorPickerComboBoxWhite.TabIndex = 4
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(27, 15)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(69, 13)
        Me.Label10.TabIndex = 25
        Me.Label10.Text = "Apply Theme"
        '
        'frmColorPrefs
        '
        Me.AcceptButton = Me.ButtonOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ButtonCancel
        Me.ClientSize = New System.Drawing.Size(333, 351)
        Me.ControlBox = False
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.GroupBoxColors)
        Me.Controls.Add(Me.ComboBoxTheme)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Controls.Add(Me.ButtonOK)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Name = "frmColorPrefs"
        Me.Text = "Color Preferences"
        Me.GroupBoxColors.ResumeLayout(False)
        Me.GroupBoxColors.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ColorPickerComboBoxRed As ColorPickerComboBox
    Friend WithEvents ColorPickerComboBoxTurquoise As ColorPickerComboBox
    Friend WithEvents ColorPickerComboBoxYellow As ColorPickerComboBox
    Friend WithEvents ColorPickerComboBoxPink As ColorPickerComboBox
    Friend WithEvents ColorPickerComboBoxBlue As ColorPickerComboBox
    Friend WithEvents ColorPickerComboBoxBackground As ColorPickerComboBox
    Friend WithEvents ColorPickerComboBoxFieldBackground As ColorPickerComboBox
    Friend WithEvents ColorPickerComboBoxGreen As ColorPickerComboBox
    Friend WithEvents ColorPickerComboBoxWhite As ColorPickerComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents ButtonOK As System.Windows.Forms.Button
    Friend WithEvents ButtonCancel As System.Windows.Forms.Button
    Friend WithEvents ComboBoxTheme As System.Windows.Forms.ComboBox
    Friend WithEvents GroupBoxColors As System.Windows.Forms.GroupBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
End Class
