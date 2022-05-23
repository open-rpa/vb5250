<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmEmulatorView
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEmulatorView))
        Me.StatusStripStatus = New System.Windows.Forms.StatusStrip()
        Me.ToolStripStatusLabelConnectionState = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabelSSL = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabelMsg = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabelInsert = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabelError = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabelStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabelCoordinates = New System.Windows.Forms.ToolStripStatusLabel()
        Me.PrintDocument1 = New System.Drawing.Printing.PrintDocument()
        Me.ImageListConnectionStatus = New System.Windows.Forms.ImageList(Me.components)
        Me.ToolStripMain = New System.Windows.Forms.ToolStrip()
        Me.ToolStripButtonConnect = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripComboBoxPrintOrientation = New System.Windows.Forms.ToolStripComboBox()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButtonPrintPreview = New System.Windows.Forms.ToolStripButton()
        Me.MenuStripMain = New System.Windows.Forms.MenuStrip()
        Me.ToolStripMenuItemFile = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemPrint = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemConnection = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemConnect = New System.Windows.Forms.ToolStripMenuItem()
        Me.ChangePasswordToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemPreferences = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemColors = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemFont = New System.Windows.Forms.ToolStripMenuItem()
        Me.UseAlternateCaretToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ImageListFieldContextMenu = New System.Windows.Forms.ImageList(Me.components)
        Me.PanelGreenScreen = New VB5250.DoubleBufferedPanel()
        Me.StatusStripStatus.SuspendLayout()
        Me.ToolStripMain.SuspendLayout()
        Me.MenuStripMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'StatusStripStatus
        '
        Me.StatusStripStatus.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripStatusLabelConnectionState, Me.ToolStripStatusLabelSSL, Me.ToolStripStatusLabelMsg, Me.ToolStripStatusLabelInsert, Me.ToolStripStatusLabelError, Me.ToolStripStatusLabelStatus, Me.ToolStripStatusLabelCoordinates})
        Me.StatusStripStatus.Location = New System.Drawing.Point(0, 594)
        Me.StatusStripStatus.Name = "StatusStripStatus"
        Me.StatusStripStatus.ShowItemToolTips = True
        Me.StatusStripStatus.Size = New System.Drawing.Size(989, 24)
        Me.StatusStripStatus.SizingGrip = False
        Me.StatusStripStatus.TabIndex = 0
        '
        'ToolStripStatusLabelConnectionState
        '
        Me.ToolStripStatusLabelConnectionState.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.ToolStripStatusLabelConnectionState.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner
        Me.ToolStripStatusLabelConnectionState.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripStatusLabelConnectionState.Name = "ToolStripStatusLabelConnectionState"
        Me.ToolStripStatusLabelConnectionState.Size = New System.Drawing.Size(4, 19)
        Me.ToolStripStatusLabelConnectionState.Text = "ConnectionState"
        '
        'ToolStripStatusLabelSSL
        '
        Me.ToolStripStatusLabelSSL.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.ToolStripStatusLabelSSL.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner
        Me.ToolStripStatusLabelSSL.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripStatusLabelSSL.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ToolStripStatusLabelSSL.Name = "ToolStripStatusLabelSSL"
        Me.ToolStripStatusLabelSSL.Size = New System.Drawing.Size(4, 19)
        Me.ToolStripStatusLabelSSL.Text = "SSL"
        '
        'ToolStripStatusLabelMsg
        '
        Me.ToolStripStatusLabelMsg.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.ToolStripStatusLabelMsg.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner
        Me.ToolStripStatusLabelMsg.Enabled = False
        Me.ToolStripStatusLabelMsg.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.ToolStripStatusLabelMsg.Name = "ToolStripStatusLabelMsg"
        Me.ToolStripStatusLabelMsg.Size = New System.Drawing.Size(38, 19)
        Me.ToolStripStatusLabelMsg.Text = "MSG"
        Me.ToolStripStatusLabelMsg.ToolTipText = "Message Waiting"
        '
        'ToolStripStatusLabelInsert
        '
        Me.ToolStripStatusLabelInsert.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.ToolStripStatusLabelInsert.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner
        Me.ToolStripStatusLabelInsert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.ToolStripStatusLabelInsert.Enabled = False
        Me.ToolStripStatusLabelInsert.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.ToolStripStatusLabelInsert.Name = "ToolStripStatusLabelInsert"
        Me.ToolStripStatusLabelInsert.Size = New System.Drawing.Size(31, 19)
        Me.ToolStripStatusLabelInsert.Text = "INS"
        Me.ToolStripStatusLabelInsert.ToolTipText = "Insert/Overwrite"
        '
        'ToolStripStatusLabelError
        '
        Me.ToolStripStatusLabelError.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.ToolStripStatusLabelError.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner
        Me.ToolStripStatusLabelError.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.ToolStripStatusLabelError.Enabled = False
        Me.ToolStripStatusLabelError.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.ToolStripStatusLabelError.Name = "ToolStripStatusLabelError"
        Me.ToolStripStatusLabelError.Size = New System.Drawing.Size(33, 19)
        Me.ToolStripStatusLabelError.Text = "ERR"
        '
        'ToolStripStatusLabelStatus
        '
        Me.ToolStripStatusLabelStatus.BackColor = System.Drawing.SystemColors.Control
        Me.ToolStripStatusLabelStatus.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.ToolStripStatusLabelStatus.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner
        Me.ToolStripStatusLabelStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.ToolStripStatusLabelStatus.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ToolStripStatusLabelStatus.Name = "ToolStripStatusLabelStatus"
        Me.ToolStripStatusLabelStatus.Size = New System.Drawing.Size(793, 19)
        Me.ToolStripStatusLabelStatus.Spring = True
        Me.ToolStripStatusLabelStatus.Text = "Status"
        Me.ToolStripStatusLabelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ToolStripStatusLabelCoordinates
        '
        Me.ToolStripStatusLabelCoordinates.AutoSize = False
        Me.ToolStripStatusLabelCoordinates.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.ToolStripStatusLabelCoordinates.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner
        Me.ToolStripStatusLabelCoordinates.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.ToolStripStatusLabelCoordinates.Name = "ToolStripStatusLabelCoordinates"
        Me.ToolStripStatusLabelCoordinates.Size = New System.Drawing.Size(40, 19)
        Me.ToolStripStatusLabelCoordinates.Text = "0,0"
        Me.ToolStripStatusLabelCoordinates.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.ToolStripStatusLabelCoordinates.ToolTipText = "Cursor Position"
        '
        'PrintDocument1
        '
        '
        'ImageListConnectionStatus
        '
        Me.ImageListConnectionStatus.ImageStream = CType(resources.GetObject("ImageListConnectionStatus.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageListConnectionStatus.TransparentColor = System.Drawing.Color.DeepPink
        Me.ImageListConnectionStatus.Images.SetKeyName(0, "Disconnected")
        Me.ImageListConnectionStatus.Images.SetKeyName(1, "Connected")
        Me.ImageListConnectionStatus.Images.SetKeyName(2, "Secured")
        Me.ImageListConnectionStatus.Images.SetKeyName(3, "Unsecured")
        '
        'ToolStripMain
        '
        Me.ToolStripMain.AllowMerge = False
        Me.ToolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStripMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButtonConnect, Me.ToolStripComboBoxPrintOrientation, Me.ToolStripButton1, Me.ToolStripButtonPrintPreview})
        Me.ToolStripMain.Location = New System.Drawing.Point(0, 24)
        Me.ToolStripMain.Name = "ToolStripMain"
        Me.ToolStripMain.Size = New System.Drawing.Size(989, 25)
        Me.ToolStripMain.TabIndex = 2
        Me.ToolStripMain.Text = "ToolStripMain"
        '
        'ToolStripButtonConnect
        '
        Me.ToolStripButtonConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonConnect.Image = CType(resources.GetObject("ToolStripButtonConnect.Image"), System.Drawing.Image)
        Me.ToolStripButtonConnect.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonConnect.Name = "ToolStripButtonConnect"
        Me.ToolStripButtonConnect.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonConnect.Text = "Connect/Disconnect"
        '
        'ToolStripComboBoxPrintOrientation
        '
        Me.ToolStripComboBoxPrintOrientation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ToolStripComboBoxPrintOrientation.DropDownWidth = 80
        Me.ToolStripComboBoxPrintOrientation.Name = "ToolStripComboBoxPrintOrientation"
        Me.ToolStripComboBoxPrintOrientation.Size = New System.Drawing.Size(80, 25)
        '
        'ToolStripButton1
        '
        Me.ToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton1.Image = CType(resources.GetObject("ToolStripButton1.Image"), System.Drawing.Image)
        Me.ToolStripButton1.ImageTransparentColor = System.Drawing.Color.Black
        Me.ToolStripButton1.Name = "ToolStripButton1"
        Me.ToolStripButton1.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton1.Text = "Print"
        '
        'ToolStripButtonPrintPreview
        '
        Me.ToolStripButtonPrintPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonPrintPreview.Image = CType(resources.GetObject("ToolStripButtonPrintPreview.Image"), System.Drawing.Image)
        Me.ToolStripButtonPrintPreview.ImageTransparentColor = System.Drawing.Color.Black
        Me.ToolStripButtonPrintPreview.Name = "ToolStripButtonPrintPreview"
        Me.ToolStripButtonPrintPreview.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonPrintPreview.Text = "Print Preview"
        '
        'MenuStripMain
        '
        Me.MenuStripMain.AllowMerge = False
        Me.MenuStripMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItemFile, Me.ToolStripMenuItemConnection, Me.ToolStripMenuItemPreferences})
        Me.MenuStripMain.Location = New System.Drawing.Point(0, 0)
        Me.MenuStripMain.Name = "MenuStripMain"
        Me.MenuStripMain.Size = New System.Drawing.Size(989, 24)
        Me.MenuStripMain.TabIndex = 3
        Me.MenuStripMain.Text = "Menu"
        '
        'ToolStripMenuItemFile
        '
        Me.ToolStripMenuItemFile.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItemPrint})
        Me.ToolStripMenuItemFile.Name = "ToolStripMenuItemFile"
        Me.ToolStripMenuItemFile.Size = New System.Drawing.Size(37, 20)
        Me.ToolStripMenuItemFile.Text = "File"
        '
        'ToolStripMenuItemPrint
        '
        Me.ToolStripMenuItemPrint.Name = "ToolStripMenuItemPrint"
        Me.ToolStripMenuItemPrint.Size = New System.Drawing.Size(108, 22)
        Me.ToolStripMenuItemPrint.Text = "Print..."
        '
        'ToolStripMenuItemConnection
        '
        Me.ToolStripMenuItemConnection.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItemConnect, Me.ChangePasswordToolStripMenuItem})
        Me.ToolStripMenuItemConnection.Name = "ToolStripMenuItemConnection"
        Me.ToolStripMenuItemConnection.Size = New System.Drawing.Size(81, 20)
        Me.ToolStripMenuItemConnection.Text = "Connection"
        '
        'ToolStripMenuItemConnect
        '
        Me.ToolStripMenuItemConnect.Name = "ToolStripMenuItemConnect"
        Me.ToolStripMenuItemConnect.Size = New System.Drawing.Size(168, 22)
        Me.ToolStripMenuItemConnect.Text = "Connect"
        '
        'ChangePasswordToolStripMenuItem
        '
        Me.ChangePasswordToolStripMenuItem.Name = "ChangePasswordToolStripMenuItem"
        Me.ChangePasswordToolStripMenuItem.Size = New System.Drawing.Size(168, 22)
        Me.ChangePasswordToolStripMenuItem.Text = "Change Password"
        '
        'ToolStripMenuItemPreferences
        '
        Me.ToolStripMenuItemPreferences.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItemColors, Me.ToolStripMenuItemFont, Me.UseAlternateCaretToolStripMenuItem})
        Me.ToolStripMenuItemPreferences.Name = "ToolStripMenuItemPreferences"
        Me.ToolStripMenuItemPreferences.Size = New System.Drawing.Size(80, 20)
        Me.ToolStripMenuItemPreferences.Text = "Preferences"
        '
        'ToolStripMenuItemColors
        '
        Me.ToolStripMenuItemColors.Name = "ToolStripMenuItemColors"
        Me.ToolStripMenuItemColors.Size = New System.Drawing.Size(175, 22)
        Me.ToolStripMenuItemColors.Text = "Colors..."
        '
        'ToolStripMenuItemFont
        '
        Me.ToolStripMenuItemFont.Name = "ToolStripMenuItemFont"
        Me.ToolStripMenuItemFont.Size = New System.Drawing.Size(175, 22)
        Me.ToolStripMenuItemFont.Text = "Font..."
        '
        'UseAlternateCaretToolStripMenuItem
        '
        Me.UseAlternateCaretToolStripMenuItem.CheckOnClick = True
        Me.UseAlternateCaretToolStripMenuItem.Name = "UseAlternateCaretToolStripMenuItem"
        Me.UseAlternateCaretToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.UseAlternateCaretToolStripMenuItem.Text = "Use Alternate Caret"
        '
        'ImageListFieldContextMenu
        '
        Me.ImageListFieldContextMenu.ImageStream = CType(resources.GetObject("ImageListFieldContextMenu.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageListFieldContextMenu.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageListFieldContextMenu.Images.SetKeyName(0, "CopyHS.png")
        Me.ImageListFieldContextMenu.Images.SetKeyName(1, "PasteHS.png")
        '
        'PanelGreenScreen
        '
        Me.PanelGreenScreen.Location = New System.Drawing.Point(0, 52)
        Me.PanelGreenScreen.Name = "PanelGreenScreen"
        Me.PanelGreenScreen.Size = New System.Drawing.Size(989, 542)
        Me.PanelGreenScreen.TabIndex = 1
        '
        'frmEmulatorView
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(989, 618)
        Me.Controls.Add(Me.ToolStripMain)
        Me.Controls.Add(Me.PanelGreenScreen)
        Me.Controls.Add(Me.StatusStripStatus)
        Me.Controls.Add(Me.MenuStripMain)
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStripMain
        Me.Name = "frmEmulatorView"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        Me.Text = "frmEmulatorView"
        Me.StatusStripStatus.ResumeLayout(False)
        Me.StatusStripStatus.PerformLayout()
        Me.ToolStripMain.ResumeLayout(False)
        Me.ToolStripMain.PerformLayout()
        Me.MenuStripMain.ResumeLayout(False)
        Me.MenuStripMain.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents StatusStripStatus As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripStatusLabelStatus As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabelCoordinates As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents PanelGreenScreen As DoubleBufferedPanel
    Friend WithEvents ToolStripStatusLabelError As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabelInsert As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabelMsg As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents PrintDocument1 As System.Drawing.Printing.PrintDocument
    Friend WithEvents ToolStripStatusLabelConnectionState As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ImageListConnectionStatus As System.Windows.Forms.ImageList
    Friend WithEvents ToolStripMain As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripButtonConnect As System.Windows.Forms.ToolStripButton
    Friend WithEvents MenuStripMain As System.Windows.Forms.MenuStrip
    Friend WithEvents ToolStripMenuItemConnection As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItemConnect As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItemPreferences As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItemColors As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButtonPrintPreview As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripMenuItemFile As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItemPrint As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ImageListFieldContextMenu As System.Windows.Forms.ImageList
    Friend WithEvents ToolStripStatusLabelSSL As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ToolStripMenuItemFont As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripComboBoxPrintOrientation As System.Windows.Forms.ToolStripComboBox
    Friend WithEvents ChangePasswordToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UseAlternateCaretToolStripMenuItem As ToolStripMenuItem
End Class
