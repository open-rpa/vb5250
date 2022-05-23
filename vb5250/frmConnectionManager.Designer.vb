<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmConnectionManager
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmConnectionManager))
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.ConnectionsTreeView = New System.Windows.Forms.TreeView()
        Me.TreeNodeImageList = New System.Windows.Forms.ImageList(Me.components)
        Me.ConnectionSettingsPanel = New System.Windows.Forms.Panel()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.PreAuthenticateCheckBox = New System.Windows.Forms.CheckBox()
        Me.PasswordTextBox = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.UserNameTextBox = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.GetServerInfoCheckBox = New System.Windows.Forms.CheckBox()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.EncryptionMethodComboBox = New System.Windows.Forms.ComboBox()
        Me.ComboBoxFontFamilyName = New System.Windows.Forms.ComboBox()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.TextBoxKBDTYPE = New System.Windows.Forms.TextBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.TextBoxCHARSET = New System.Windows.Forms.TextBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.TextBoxCODEPAGE = New System.Windows.Forms.TextBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.ComboBoxHostLocale = New System.Windows.Forms.ComboBox()
        Me.SSLCheckBox = New System.Windows.Forms.CheckBox()
        Me.ConnectButton = New System.Windows.Forms.Button()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.ColorMapButton = New System.Windows.Forms.Button()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.BypassLoginCheckBox = New System.Windows.Forms.CheckBox()
        Me.ScreenSizeComboBox = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.StationNameTextBox = New System.Windows.Forms.TextBox()
        Me.PortTextBox = New System.Windows.Forms.TextBox()
        Me.HostNameTextBox = New System.Windows.Forms.TextBox()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.NewFolderToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.NewConnectionToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.DeleteNodeToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.NewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NewFolderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NewConnectionToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ConnectToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.Label19 = New System.Windows.Forms.Label()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.ConnectionSettingsPanel.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 52)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.ConnectionsTreeView)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.ConnectionSettingsPanel)
        Me.SplitContainer1.Size = New System.Drawing.Size(595, 411)
        Me.SplitContainer1.SplitterDistance = 197
        Me.SplitContainer1.TabIndex = 0
        '
        'ConnectionsTreeView
        '
        Me.ConnectionsTreeView.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ConnectionsTreeView.ImageKey = "ClosedFolder"
        Me.ConnectionsTreeView.ImageList = Me.TreeNodeImageList
        Me.ConnectionsTreeView.Location = New System.Drawing.Point(0, 0)
        Me.ConnectionsTreeView.Name = "ConnectionsTreeView"
        Me.ConnectionsTreeView.SelectedImageKey = "ClosedFolder"
        Me.ConnectionsTreeView.Size = New System.Drawing.Size(197, 411)
        Me.ConnectionsTreeView.TabIndex = 0
        '
        'TreeNodeImageList
        '
        Me.TreeNodeImageList.ImageStream = CType(resources.GetObject("TreeNodeImageList.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.TreeNodeImageList.TransparentColor = System.Drawing.Color.Transparent
        Me.TreeNodeImageList.Images.SetKeyName(0, "ClosedFolder")
        Me.TreeNodeImageList.Images.SetKeyName(1, "OpenFolder")
        Me.TreeNodeImageList.Images.SetKeyName(2, "Connection")
        '
        'ConnectionSettingsPanel
        '
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label19)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label18)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label17)
        Me.ConnectionSettingsPanel.Controls.Add(Me.PreAuthenticateCheckBox)
        Me.ConnectionSettingsPanel.Controls.Add(Me.PasswordTextBox)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label5)
        Me.ConnectionSettingsPanel.Controls.Add(Me.UserNameTextBox)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label9)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label4)
        Me.ConnectionSettingsPanel.Controls.Add(Me.GetServerInfoCheckBox)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label16)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label15)
        Me.ConnectionSettingsPanel.Controls.Add(Me.EncryptionMethodComboBox)
        Me.ConnectionSettingsPanel.Controls.Add(Me.ComboBoxFontFamilyName)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label14)
        Me.ConnectionSettingsPanel.Controls.Add(Me.TextBoxKBDTYPE)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label13)
        Me.ConnectionSettingsPanel.Controls.Add(Me.TextBoxCHARSET)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label12)
        Me.ConnectionSettingsPanel.Controls.Add(Me.TextBoxCODEPAGE)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label11)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label10)
        Me.ConnectionSettingsPanel.Controls.Add(Me.ComboBoxHostLocale)
        Me.ConnectionSettingsPanel.Controls.Add(Me.SSLCheckBox)
        Me.ConnectionSettingsPanel.Controls.Add(Me.ConnectButton)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label8)
        Me.ConnectionSettingsPanel.Controls.Add(Me.ColorMapButton)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label6)
        Me.ConnectionSettingsPanel.Controls.Add(Me.BypassLoginCheckBox)
        Me.ConnectionSettingsPanel.Controls.Add(Me.ScreenSizeComboBox)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label7)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label3)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label2)
        Me.ConnectionSettingsPanel.Controls.Add(Me.Label1)
        Me.ConnectionSettingsPanel.Controls.Add(Me.StationNameTextBox)
        Me.ConnectionSettingsPanel.Controls.Add(Me.PortTextBox)
        Me.ConnectionSettingsPanel.Controls.Add(Me.HostNameTextBox)
        Me.ConnectionSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ConnectionSettingsPanel.Location = New System.Drawing.Point(0, 0)
        Me.ConnectionSettingsPanel.Name = "ConnectionSettingsPanel"
        Me.ConnectionSettingsPanel.Size = New System.Drawing.Size(394, 411)
        Me.ConnectionSettingsPanel.TabIndex = 0
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(15, 269)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(85, 13)
        Me.Label17.TabIndex = 36
        Me.Label17.Text = "Pre-authenticate"
        '
        'PreAuthenticateCheckBox
        '
        Me.PreAuthenticateCheckBox.AutoSize = True
        Me.PreAuthenticateCheckBox.Location = New System.Drawing.Point(129, 269)
        Me.PreAuthenticateCheckBox.Name = "PreAuthenticateCheckBox"
        Me.PreAuthenticateCheckBox.Size = New System.Drawing.Size(15, 14)
        Me.PreAuthenticateCheckBox.TabIndex = 35
        Me.PreAuthenticateCheckBox.UseVisualStyleBackColor = True
        '
        'PasswordTextBox
        '
        Me.PasswordTextBox.Location = New System.Drawing.Point(129, 243)
        Me.PasswordTextBox.Name = "PasswordTextBox"
        Me.PasswordTextBox.Size = New System.Drawing.Size(100, 20)
        Me.PasswordTextBox.TabIndex = 4
        Me.PasswordTextBox.UseSystemPasswordChar = True
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(15, 246)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(53, 13)
        Me.Label5.TabIndex = 11
        Me.Label5.Text = "Password"
        '
        'UserNameTextBox
        '
        Me.UserNameTextBox.Location = New System.Drawing.Point(129, 217)
        Me.UserNameTextBox.Name = "UserNameTextBox"
        Me.UserNameTextBox.Size = New System.Drawing.Size(100, 20)
        Me.UserNameTextBox.TabIndex = 3
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(15, 193)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(74, 13)
        Me.Label9.TabIndex = 19
        Me.Label9.Text = "Login Security"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(15, 220)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(60, 13)
        Me.Label4.TabIndex = 10
        Me.Label4.Text = "User Name"
        '
        'GetServerInfoCheckBox
        '
        Me.GetServerInfoCheckBox.AutoSize = True
        Me.GetServerInfoCheckBox.Location = New System.Drawing.Point(235, 193)
        Me.GetServerInfoCheckBox.Name = "GetServerInfoCheckBox"
        Me.GetServerInfoCheckBox.Size = New System.Drawing.Size(15, 14)
        Me.GetServerInfoCheckBox.TabIndex = 34
        Me.GetServerInfoCheckBox.UseVisualStyleBackColor = True
        Me.GetServerInfoCheckBox.Visible = False
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(252, 193)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(59, 13)
        Me.Label16.TabIndex = 33
        Me.Label16.Text = "Ask Server"
        Me.Label16.Visible = False
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(15, 166)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(28, 13)
        Me.Label15.TabIndex = 32
        Me.Label15.Text = "Font"
        '
        'EncryptionMethodComboBox
        '
        Me.EncryptionMethodComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.EncryptionMethodComboBox.FormattingEnabled = True
        Me.EncryptionMethodComboBox.Location = New System.Drawing.Point(129, 190)
        Me.EncryptionMethodComboBox.Name = "EncryptionMethodComboBox"
        Me.EncryptionMethodComboBox.Size = New System.Drawing.Size(100, 21)
        Me.EncryptionMethodComboBox.TabIndex = 18
        '
        'ComboBoxFontFamilyName
        '
        Me.ComboBoxFontFamilyName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxFontFamilyName.FormattingEnabled = True
        Me.ComboBoxFontFamilyName.Location = New System.Drawing.Point(129, 163)
        Me.ComboBoxFontFamilyName.Name = "ComboBoxFontFamilyName"
        Me.ComboBoxFontFamilyName.Size = New System.Drawing.Size(245, 21)
        Me.ComboBoxFontFamilyName.TabIndex = 31
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Enabled = False
        Me.Label14.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label14.Location = New System.Drawing.Point(313, 147)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(46, 12)
        Me.Label14.TabIndex = 30
        Me.Label14.Text = "KBDTYPE"
        '
        'TextBoxKBDTYPE
        '
        Me.TextBoxKBDTYPE.Enabled = False
        Me.TextBoxKBDTYPE.Location = New System.Drawing.Point(297, 124)
        Me.TextBoxKBDTYPE.Name = "TextBoxKBDTYPE"
        Me.TextBoxKBDTYPE.Size = New System.Drawing.Size(78, 20)
        Me.TextBoxKBDTYPE.TabIndex = 29
        Me.TextBoxKBDTYPE.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Enabled = False
        Me.Label13.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label13.Location = New System.Drawing.Point(227, 147)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(50, 12)
        Me.Label13.TabIndex = 28
        Me.Label13.Text = "CHARSET"
        '
        'TextBoxCHARSET
        '
        Me.TextBoxCHARSET.Enabled = False
        Me.TextBoxCHARSET.Location = New System.Drawing.Point(213, 124)
        Me.TextBoxCHARSET.Name = "TextBoxCHARSET"
        Me.TextBoxCHARSET.Size = New System.Drawing.Size(78, 20)
        Me.TextBoxCHARSET.TabIndex = 27
        Me.TextBoxCHARSET.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Enabled = False
        Me.Label12.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label12.Location = New System.Drawing.Point(138, 147)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(58, 12)
        Me.Label12.TabIndex = 26
        Me.Label12.Text = "CODEPAGE"
        '
        'TextBoxCODEPAGE
        '
        Me.TextBoxCODEPAGE.Enabled = False
        Me.TextBoxCODEPAGE.Location = New System.Drawing.Point(129, 124)
        Me.TextBoxCODEPAGE.Name = "TextBoxCODEPAGE"
        Me.TextBoxCODEPAGE.Size = New System.Drawing.Size(78, 20)
        Me.TextBoxCODEPAGE.TabIndex = 25
        Me.TextBoxCODEPAGE.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(15, 100)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(64, 13)
        Me.Label11.TabIndex = 24
        Me.Label11.Text = "Host Locale"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(212, 49)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(27, 13)
        Me.Label10.TabIndex = 22
        Me.Label10.Text = "SSL"
        '
        'ComboBoxHostLocale
        '
        Me.ComboBoxHostLocale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxHostLocale.FormattingEnabled = True
        Me.ComboBoxHostLocale.Location = New System.Drawing.Point(129, 97)
        Me.ComboBoxHostLocale.Name = "ComboBoxHostLocale"
        Me.ComboBoxHostLocale.Size = New System.Drawing.Size(247, 21)
        Me.ComboBoxHostLocale.TabIndex = 23
        '
        'SSLCheckBox
        '
        Me.SSLCheckBox.AutoSize = True
        Me.SSLCheckBox.Location = New System.Drawing.Point(197, 48)
        Me.SSLCheckBox.Name = "SSLCheckBox"
        Me.SSLCheckBox.Size = New System.Drawing.Size(15, 14)
        Me.SSLCheckBox.TabIndex = 21
        Me.SSLCheckBox.UseVisualStyleBackColor = True
        '
        'ConnectButton
        '
        Me.ConnectButton.Location = New System.Drawing.Point(300, 374)
        Me.ConnectButton.Name = "ConnectButton"
        Me.ConnectButton.Size = New System.Drawing.Size(75, 23)
        Me.ConnectButton.TabIndex = 20
        Me.ConnectButton.Text = "Connect"
        Me.ConnectButton.UseVisualStyleBackColor = True
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(15, 341)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(75, 13)
        Me.Label8.TabIndex = 17
        Me.Label8.Text = "Color Mapping"
        '
        'ColorMapButton
        '
        Me.ColorMapButton.Location = New System.Drawing.Point(129, 336)
        Me.ColorMapButton.Name = "ColorMapButton"
        Me.ColorMapButton.Size = New System.Drawing.Size(26, 23)
        Me.ColorMapButton.TabIndex = 16
        Me.ColorMapButton.Text = "..."
        Me.ColorMapButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ColorMapButton.UseVisualStyleBackColor = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(15, 312)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(64, 13)
        Me.Label6.TabIndex = 12
        Me.Label6.Text = "Screen Size"
        '
        'BypassLoginCheckBox
        '
        Me.BypassLoginCheckBox.AutoSize = True
        Me.BypassLoginCheckBox.Location = New System.Drawing.Point(129, 289)
        Me.BypassLoginCheckBox.Name = "BypassLoginCheckBox"
        Me.BypassLoginCheckBox.Size = New System.Drawing.Size(15, 14)
        Me.BypassLoginCheckBox.TabIndex = 15
        Me.BypassLoginCheckBox.UseVisualStyleBackColor = True
        '
        'ScreenSizeComboBox
        '
        Me.ScreenSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ScreenSizeComboBox.FormattingEnabled = True
        Me.ScreenSizeComboBox.Items.AddRange(New Object() {"24x80", "27x132"})
        Me.ScreenSizeComboBox.Location = New System.Drawing.Point(129, 309)
        Me.ScreenSizeComboBox.Name = "ScreenSizeComboBox"
        Me.ScreenSizeComboBox.Size = New System.Drawing.Size(100, 21)
        Me.ScreenSizeComboBox.TabIndex = 14
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(15, 290)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(111, 13)
        Me.Label7.TabIndex = 13
        Me.Label7.Text = "Bypass Logon Screen"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(15, 74)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(71, 13)
        Me.Label3.TabIndex = 9
        Me.Label3.Text = "Station Name"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(15, 48)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(26, 13)
        Me.Label2.TabIndex = 8
        Me.Label2.Text = "Port"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(15, 22)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(108, 13)
        Me.Label1.TabIndex = 7
        Me.Label1.Text = "Hostname or Address"
        '
        'StationNameTextBox
        '
        Me.StationNameTextBox.Location = New System.Drawing.Point(129, 71)
        Me.StationNameTextBox.Name = "StationNameTextBox"
        Me.StationNameTextBox.Size = New System.Drawing.Size(100, 20)
        Me.StationNameTextBox.TabIndex = 2
        '
        'PortTextBox
        '
        Me.PortTextBox.Location = New System.Drawing.Point(129, 45)
        Me.PortTextBox.Name = "PortTextBox"
        Me.PortTextBox.Size = New System.Drawing.Size(62, 20)
        Me.PortTextBox.TabIndex = 1
        '
        'HostNameTextBox
        '
        Me.HostNameTextBox.Location = New System.Drawing.Point(129, 19)
        Me.HostNameTextBox.Name = "HostNameTextBox"
        Me.HostNameTextBox.Size = New System.Drawing.Size(247, 20)
        Me.HostNameTextBox.TabIndex = 0
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.HelpToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(595, 24)
        Me.MenuStrip1.TabIndex = 1
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExitToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "&File"
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(92, 22)
        Me.ExitToolStripMenuItem.Text = "&Exit"
        '
        'HelpToolStripMenuItem
        '
        Me.HelpToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AboutToolStripMenuItem})
        Me.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        Me.HelpToolStripMenuItem.Size = New System.Drawing.Size(44, 20)
        Me.HelpToolStripMenuItem.Text = "&Help"
        '
        'AboutToolStripMenuItem
        '
        Me.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        Me.AboutToolStripMenuItem.Size = New System.Drawing.Size(107, 22)
        Me.AboutToolStripMenuItem.Text = "&About"
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NewFolderToolStripButton, Me.NewConnectionToolStripButton, Me.DeleteNodeToolStripButton})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 24)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(595, 25)
        Me.ToolStrip1.TabIndex = 2
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'NewFolderToolStripButton
        '
        Me.NewFolderToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.NewFolderToolStripButton.Image = CType(resources.GetObject("NewFolderToolStripButton.Image"), System.Drawing.Image)
        Me.NewFolderToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.NewFolderToolStripButton.Name = "NewFolderToolStripButton"
        Me.NewFolderToolStripButton.Size = New System.Drawing.Size(23, 22)
        Me.NewFolderToolStripButton.Text = "New Folder"
        '
        'NewConnectionToolStripButton
        '
        Me.NewConnectionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.NewConnectionToolStripButton.Image = CType(resources.GetObject("NewConnectionToolStripButton.Image"), System.Drawing.Image)
        Me.NewConnectionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.NewConnectionToolStripButton.Name = "NewConnectionToolStripButton"
        Me.NewConnectionToolStripButton.Size = New System.Drawing.Size(23, 22)
        Me.NewConnectionToolStripButton.Text = "New Connection"
        '
        'DeleteNodeToolStripButton
        '
        Me.DeleteNodeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.DeleteNodeToolStripButton.Image = CType(resources.GetObject("DeleteNodeToolStripButton.Image"), System.Drawing.Image)
        Me.DeleteNodeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.DeleteNodeToolStripButton.Name = "DeleteNodeToolStripButton"
        Me.DeleteNodeToolStripButton.Size = New System.Drawing.Size(23, 22)
        Me.DeleteNodeToolStripButton.Text = "Delete Selected Item"
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NewToolStripMenuItem, Me.ConnectToolStripMenuItem})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(120, 48)
        '
        'NewToolStripMenuItem
        '
        Me.NewToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NewFolderToolStripMenuItem, Me.NewConnectionToolStripMenuItem})
        Me.NewToolStripMenuItem.Name = "NewToolStripMenuItem"
        Me.NewToolStripMenuItem.Size = New System.Drawing.Size(119, 22)
        Me.NewToolStripMenuItem.Text = "New"
        '
        'NewFolderToolStripMenuItem
        '
        Me.NewFolderToolStripMenuItem.Image = CType(resources.GetObject("NewFolderToolStripMenuItem.Image"), System.Drawing.Image)
        Me.NewFolderToolStripMenuItem.Name = "NewFolderToolStripMenuItem"
        Me.NewFolderToolStripMenuItem.Size = New System.Drawing.Size(136, 22)
        Me.NewFolderToolStripMenuItem.Text = "Folder"
        '
        'NewConnectionToolStripMenuItem
        '
        Me.NewConnectionToolStripMenuItem.Image = CType(resources.GetObject("NewConnectionToolStripMenuItem.Image"), System.Drawing.Image)
        Me.NewConnectionToolStripMenuItem.Name = "NewConnectionToolStripMenuItem"
        Me.NewConnectionToolStripMenuItem.Size = New System.Drawing.Size(136, 22)
        Me.NewConnectionToolStripMenuItem.Text = "Connection"
        '
        'ConnectToolStripMenuItem
        '
        Me.ConnectToolStripMenuItem.Name = "ConnectToolStripMenuItem"
        Me.ConnectToolStripMenuItem.Size = New System.Drawing.Size(119, 22)
        Me.ConnectToolStripMenuItem.Text = "Connect"
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(147, 269)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(242, 13)
        Me.Label18.TabIndex = 37
        Me.Label18.Text = "(Logon feedback, change expired password, etc.)"
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(147, 289)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(164, 13)
        Me.Label19.TabIndex = 38
        Me.Label19.Text = "(Prevents easy password sniffing)"
        '
        'frmConnectionManager
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(595, 464)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "frmConnectionManager"
        Me.Text = "VB5250 Connection Manager"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ConnectionSettingsPanel.ResumeLayout(False)
        Me.ConnectionSettingsPanel.PerformLayout()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents ConnectionsTreeView As System.Windows.Forms.TreeView
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents NewFolderToolStripButton As System.Windows.Forms.ToolStripButton
    Friend WithEvents NewConnectionToolStripButton As System.Windows.Forms.ToolStripButton
    Friend WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents NewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NewFolderToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NewConnectionToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ConnectToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TreeNodeImageList As System.Windows.Forms.ImageList
    Friend WithEvents DeleteNodeToolStripButton As System.Windows.Forms.ToolStripButton
    Friend WithEvents ConnectionSettingsPanel As System.Windows.Forms.Panel
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents PasswordTextBox As System.Windows.Forms.TextBox
    Friend WithEvents UserNameTextBox As System.Windows.Forms.TextBox
    Friend WithEvents StationNameTextBox As System.Windows.Forms.TextBox
    Friend WithEvents PortTextBox As System.Windows.Forms.TextBox
    Friend WithEvents HostNameTextBox As System.Windows.Forms.TextBox
    Friend WithEvents BypassLoginCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents ScreenSizeComboBox As System.Windows.Forms.ComboBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents ColorMapButton As System.Windows.Forms.Button
    Friend WithEvents EncryptionMethodComboBox As System.Windows.Forms.ComboBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents ConnectButton As System.Windows.Forms.Button
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents SSLCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents ComboBoxHostLocale As System.Windows.Forms.ComboBox
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents TextBoxKBDTYPE As System.Windows.Forms.TextBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents TextBoxCHARSET As System.Windows.Forms.TextBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents TextBoxCODEPAGE As System.Windows.Forms.TextBox
    Friend WithEvents ComboBoxFontFamilyName As System.Windows.Forms.ComboBox
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents GetServerInfoCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents PreAuthenticateCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents Label18 As System.Windows.Forms.Label
End Class
