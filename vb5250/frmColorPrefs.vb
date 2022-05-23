'
' Copyright 2013 Alec Skelly
'
' This file is part of VB5250, a telnet client implementing IBM's 5250 protocol.
'
' VB5250 is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' VB5250 is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with VB5250. If not, see <http://www.gnu.org/licenses/>.
' 
Friend Class frmColorPrefs
    Private _EmulatorView As frmEmulatorView
    Private _EmulatorIndex As Integer
    Private _ColorMap As Dictionary(Of String, KnownColor)
    Private _OriginalColorMap As Dictionary(Of String, KnownColor)
    Private _SuppressScreenUpdates As Boolean

    Private Sub frmColorPrefs_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        'XXX Why is it taking several seconds before this event fires?

        Me.ColorPickerComboBoxBackground.ColorMapKey = "Background"
        Me.ColorPickerComboBoxFieldBackground.ColorMapKey = "FieldBackground"
        Me.ColorPickerComboBoxGreen.ColorMapKey = "Green"
        Me.ColorPickerComboBoxWhite.ColorMapKey = "White"
        Me.ColorPickerComboBoxRed.ColorMapKey = "Red"
        Me.ColorPickerComboBoxTurquoise.ColorMapKey = "Turquoise"
        Me.ColorPickerComboBoxYellow.ColorMapKey = "Yellow"
        Me.ColorPickerComboBoxPink.ColorMapKey = "Pink"
        Me.ColorPickerComboBoxBlue.ColorMapKey = "Blue"

        If _ColorMap IsNot Nothing Then
            For Each ctl As Control In Me.GroupBoxColors.Controls
                If ctl.GetType() Is GetType(ColorPickerComboBox) Then
                    Dim cp As ColorPickerComboBox = DirectCast(ctl, ColorPickerComboBox)
                    cp.SelectedValue = Color.FromKnownColor(_ColorMap(cp.ColorMapKey))
                    AddHandler cp.SelectedValueChanged, AddressOf Me.ColorPickerComboBox_SelectedValueChanged
                End If
            Next
        End If

        With Me.ComboBoxTheme
            .Items.Add("Classic")
            .Items.Add("Windows")
            .Items.Add("Forest")
            .Items.Add("Salmon")
            .Items.Add("Steven Tyler")
            .Items.Add("Jumanji")
            .Items.Add("Cinnamon Raisin")
        End With

    End Sub

    Public Sub New(ByVal EmulatorIndex As Integer, ByRef ColorMap As Dictionary(Of String, KnownColor), Title As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If EmulatorIndex > -1 Then
            Me._EmulatorIndex = EmulatorIndex

            Try
                If EmulatorView IsNot Nothing AndAlso EmulatorView.ContainsKey(Me._EmulatorIndex) Then Me._EmulatorView = EmulatorView(Me._EmulatorIndex).Form
            Catch ex As Exception
                MsgBox(ex.Message) 'XXX
            End Try

            Me._ColorMap = ColorMap
            Me.CopyColorMap(Me._ColorMap, Me._OriginalColorMap) 'back up current values
        End If
        Me.Text = Title
    End Sub

    Private Sub ButtonOK_Click(sender As System.Object, e As System.EventArgs) Handles ButtonOK.Click
        If _ColorMap IsNot Nothing Then
            _ColorMap("Background") = Me.ColorPickerComboBoxBackground.SelectedValue.ToKnownColor
            _ColorMap("FieldBackground") = Me.ColorPickerComboBoxFieldBackground.SelectedValue.ToKnownColor
            _ColorMap("Green") = Me.ColorPickerComboBoxGreen.SelectedValue.ToKnownColor
            _ColorMap("White") = Me.ColorPickerComboBoxWhite.SelectedValue.ToKnownColor
            _ColorMap("Red") = Me.ColorPickerComboBoxRed.SelectedValue.ToKnownColor
            _ColorMap("Turquoise") = Me.ColorPickerComboBoxTurquoise.SelectedValue.ToKnownColor
            _ColorMap("Yellow") = Me.ColorPickerComboBoxYellow.SelectedValue.ToKnownColor
            _ColorMap("Pink") = Me.ColorPickerComboBoxPink.SelectedValue.ToKnownColor
            _ColorMap("Blue") = Me.ColorPickerComboBoxBlue.SelectedValue.ToKnownColor
        End If
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub ComboBoxTheme_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles ComboBoxTheme.SelectedIndexChanged
        If ComboBoxTheme.SelectedItem IsNot Nothing Then
            Me._SuppressScreenUpdates = True
            Select Case ComboBoxTheme.SelectedItem
                Case "Classic"
                    Me.ColorPickerComboBoxBackground.SelectedText = "Black"
                    Me.ColorPickerComboBoxFieldBackground.SelectedText = "Black"
                    Me.ColorPickerComboBoxGreen.SelectedText = "LimeGreen"
                    Me.ColorPickerComboBoxWhite.SelectedText = "White"
                    Me.ColorPickerComboBoxRed.SelectedText = "Red"
                    Me.ColorPickerComboBoxTurquoise.SelectedText = "Cyan"
                    Me.ColorPickerComboBoxYellow.SelectedText = "Yellow"
                    Me.ColorPickerComboBoxPink.SelectedText = "Fuchsia"
                    Me.ColorPickerComboBoxBlue.SelectedText = "CornflowerBlue"
                Case "Windows"
                    Me.ColorPickerComboBoxBackground.SelectedText = "Control"
                    Me.ColorPickerComboBoxFieldBackground.SelectedText = "Window"
                    Me.ColorPickerComboBoxGreen.SelectedText = "ControlText"
                    Me.ColorPickerComboBoxWhite.SelectedText = "Highlight"
                    Me.ColorPickerComboBoxRed.SelectedText = "Red"
                    Me.ColorPickerComboBoxTurquoise.SelectedText = "Cyan"
                    Me.ColorPickerComboBoxYellow.SelectedText = "Goldenrod"
                    Me.ColorPickerComboBoxPink.SelectedText = "Fuchsia"
                    Me.ColorPickerComboBoxBlue.SelectedText = "GrayText"
                Case "Forest"
                    Me.ColorPickerComboBoxBackground.SelectedText = "DarkOliveGreen"
                    Me.ColorPickerComboBoxFieldBackground.SelectedText = "Cornsilk"
                    Me.ColorPickerComboBoxGreen.SelectedText = "Black"
                    Me.ColorPickerComboBoxWhite.SelectedText = "YellowGreen"
                    Me.ColorPickerComboBoxRed.SelectedText = "Maroon"
                    Me.ColorPickerComboBoxTurquoise.SelectedText = "Cyan"
                    Me.ColorPickerComboBoxYellow.SelectedText = "Goldenrod"
                    Me.ColorPickerComboBoxPink.SelectedText = "MediumOrchid"
                    Me.ColorPickerComboBoxBlue.SelectedText = "SkyBlue"
                Case "Salmon"
                    Me.ColorPickerComboBoxBackground.SelectedText = "DarkSalmon"
                    Me.ColorPickerComboBoxFieldBackground.SelectedText = "SeaShell"
                    Me.ColorPickerComboBoxGreen.SelectedText = "DarkGreen"
                    Me.ColorPickerComboBoxWhite.SelectedText = "Firebrick"
                    Me.ColorPickerComboBoxRed.SelectedText = "Maroon"
                    Me.ColorPickerComboBoxTurquoise.SelectedText = "Cyan"
                    Me.ColorPickerComboBoxYellow.SelectedText = "PaleGoldenrod"
                    Me.ColorPickerComboBoxPink.SelectedText = "MediumVioletRed"
                    Me.ColorPickerComboBoxBlue.SelectedText = "DarkCyan"
                Case "Steven Tyler"
                    Me.ColorPickerComboBoxBackground.SelectedText = "MediumVioletRed"
                    Me.ColorPickerComboBoxFieldBackground.SelectedText = "PaleVioletRed"
                    Me.ColorPickerComboBoxGreen.SelectedText = "PeachPuff"
                    Me.ColorPickerComboBoxWhite.SelectedText = "LavenderBlush"
                    Me.ColorPickerComboBoxRed.SelectedText = "Tomato"
                    Me.ColorPickerComboBoxTurquoise.SelectedText = "DarkTurquoise"
                    Me.ColorPickerComboBoxYellow.SelectedText = "Yellow"
                    Me.ColorPickerComboBoxPink.SelectedText = "HotPink"
                    Me.ColorPickerComboBoxBlue.SelectedText = "DeepSkyBlue"
                Case "Jumanji"
                    Me.ColorPickerComboBoxBackground.SelectedText = "Sienna"
                    Me.ColorPickerComboBoxFieldBackground.SelectedText = "DarkOliveGreen"
                    Me.ColorPickerComboBoxGreen.SelectedText = "Khaki"
                    Me.ColorPickerComboBoxWhite.SelectedText = "Ivory"
                    Me.ColorPickerComboBoxRed.SelectedText = "Maroon"
                    Me.ColorPickerComboBoxTurquoise.SelectedText = "YellowGreen"
                    Me.ColorPickerComboBoxYellow.SelectedText = "Gold"
                    Me.ColorPickerComboBoxPink.SelectedText = "Orchid"
                    Me.ColorPickerComboBoxBlue.SelectedText = "DarkSlateGray"
                Case "Cinnamon Raisin"
                    Me.ColorPickerComboBoxBackground.SelectedText = "AntiqueWhite"
                    Me.ColorPickerComboBoxFieldBackground.SelectedText = "White"
                    Me.ColorPickerComboBoxGreen.SelectedText = "DarkRed"
                    Me.ColorPickerComboBoxWhite.SelectedText = "Goldenrod"
                    Me.ColorPickerComboBoxRed.SelectedText = "Navy"
                    Me.ColorPickerComboBoxTurquoise.SelectedText = "Thistle"
                    Me.ColorPickerComboBoxYellow.SelectedText = "Chocolate"
                    Me.ColorPickerComboBoxPink.SelectedText = "HotPink"
                    Me.ColorPickerComboBoxBlue.SelectedText = "DarkSlateBlue"
            End Select
            Me._SuppressScreenUpdates = False
            If _EmulatorView IsNot Nothing Then _EmulatorView.Redraw()
        End If
    End Sub

    Private Sub ButtonCancel_Click(sender As System.Object, e As System.EventArgs) Handles ButtonCancel.Click
        Me.CopyColorMap(Me._OriginalColorMap, Me._ColorMap) 'restore original settings
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        If _EmulatorView IsNot Nothing Then Me._EmulatorView.Redraw()
        Me.Close()
    End Sub

    Private Sub CopyColorMap(ByRef Source As Dictionary(Of String, KnownColor), ByRef Destination As Dictionary(Of String, KnownColor))
        If Destination Is Nothing Then Destination = New Dictionary(Of String, KnownColor)
        Destination.Clear()
        For Each kvp As KeyValuePair(Of String, KnownColor) In Source
            Destination.Add(kvp.Key, kvp.Value)
        Next
    End Sub

    Private Sub ColorPickerComboBox_SelectedValueChanged(sender As Object, e As System.EventArgs)
        Dim cp As ColorPickerComboBox = DirectCast(sender, ColorPickerComboBox)
        _ColorMap(cp.ColorMapKey) = cp.SelectedValue.ToKnownColor
        If _EmulatorView IsNot Nothing Then
            If Not Me._SuppressScreenUpdates Then Me._EmulatorView.Redraw()
        End If
    End Sub

End Class