Public Class frmFontPrefs
    Private _EmulatorIndex As Integer
    Private _EmulatorView As frmEmulatorView
    Private _SuppressScreenUpdates As Boolean
    Private _OriginalFontFamily As FontFamily
    Private _FontFamilyNames As New List(Of String)
    Private _OriginalEmulatorSize As Size

    Public Sub New(ByVal EmulatorIndex As Integer, ByRef FontFamily As FontFamily, ByRef FontFamilyNames As List(Of String), Title As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If EmulatorIndex > -1 Then
            Me._EmulatorIndex = EmulatorIndex

            Try
                If EmulatorView IsNot Nothing AndAlso EmulatorView.ContainsKey(Me._EmulatorIndex) Then Me._EmulatorView = EmulatorView(Me._EmulatorIndex).Form
                Me._OriginalFontFamily = New System.Drawing.FontFamily(FontFamily.Name) 'back up current value
                Me._FontFamilyNames = FontFamilyNames
                Me._OriginalEmulatorSize = Me._EmulatorView.Size
            Catch ex As Exception
                MsgBox(ex.Message) 'XXX
            End Try

        End If
        Me.MinimumSize = Me.Size
        Me.Text = Title
    End Sub

    Private Sub ComboBoxFontFamilyName_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxFontFamilyName.SelectedIndexChanged
        EmulatorView(Me._EmulatorIndex).Settings.FontFamily = New FontFamily(ComboBoxFontFamilyName.Text)
        If Not Me._SuppressScreenUpdates Then
            If _EmulatorView IsNot Nothing Then
                If Me._EmulatorView.WindowState = FormWindowState.Maximized Then
                    Me._EmulatorView.Redraw()
                Else
                    Me._EmulatorView.Width -= 1
                    Me._EmulatorView.Size = Me._OriginalEmulatorSize
                End If
            End If
        End If
    End Sub

    Private Sub ButtonCancel_Click(sender As Object, e As EventArgs) Handles ButtonCancel.Click
        EmulatorView(Me._EmulatorIndex).Settings.FontFamily = Me._OriginalFontFamily 'restore original setting
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        If _EmulatorView IsNot Nothing Then Me._EmulatorView.Redraw()
        Me.Close()
    End Sub

    Private Sub formFontPrefs_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me._SuppressScreenUpdates = True
        Me.ComboBoxFontFamilyName.Items.Clear()
        For Each s As String In Me._FontFamilyNames
            Me.ComboBoxFontFamilyName.Items.Add(s)
            If EmulatorView(Me._EmulatorIndex).Settings.FontFamily.Name = s Then Me.ComboBoxFontFamilyName.SelectedItem = s
        Next
        Me._SuppressScreenUpdates = False
    End Sub

    Private Sub ButtonOK_Click(sender As Object, e As EventArgs) Handles ButtonOK.Click
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub
End Class