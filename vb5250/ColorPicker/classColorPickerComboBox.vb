'Converted to VB and modified from Jonathan Wood's C# code posted here:
'http://www.blackbeltcoder.com/Articles/controls/creating-a-color-picker-with-an-owner-draw-combobox
'
'This code is licensed under the Code Project Open License (CPOL) version 1.02.
'http://www.codeproject.com/info/cpol10.aspx

Public Class ColorPickerComboBox
    Inherits ComboBox

    Public ColorMapKey As String

    'Data for each color in the list
    Public Class ColorInfo
        Public Text As String
        Public Color As Color

        Public Sub New(Text As String, Color As Color)
            Me.Text = Text
            Me.Color = Color
        End Sub
    End Class

    Public Sub New()
        Me.DropDownStyle = ComboBoxStyle.DropDownList
        Me.DrawMode = Windows.Forms.DrawMode.OwnerDrawFixed
        Me.AddKnownColors()
    End Sub

    'Populate control with standard colors
    Public Sub AddKnownColors()
        Items.Clear()
        Dim c As Color

        'Add the non-system colors
        For Each kc As KnownColor In [Enum].GetValues(GetType(KnownColor))
            c = Color.FromKnownColor(kc)
            If Not c.IsSystemColor Then
                Select Case kc
                    Case KnownColor.Transparent
                    Case Else
                        Items.Add(New ColorInfo(kc.ToString, Color.FromKnownColor(kc)))
                End Select
            End If
        Next

        'Add the system colors to the bottom of the list
        For Each kc As KnownColor In [Enum].GetValues(GetType(KnownColor))
            c = Color.FromKnownColor(kc)
            If c.IsSystemColor Then
                Select Case kc
                    Case KnownColor.Transparent
                    Case Else
                        Items.Add(New ColorInfo(kc.ToString, Color.FromKnownColor(kc)))
                End Select
            End If
        Next
        c = Nothing

    End Sub

    'Draw list item
    Protected Sub OnDrawItem(sender As Object, e As DrawItemEventArgs) Handles MyBase.DrawItem

        If (e.Index >= 0) Then

            'Get this color
            Dim color As ColorInfo = Items(e.Index)

            'Fill background
            e.DrawBackground()

            'Draw color box
            Dim rect As New Rectangle
            rect.X = e.Bounds.X + 2
            rect.Y = e.Bounds.Y + 2
            rect.Width = 18
            rect.Height = e.Bounds.Height - 5
            e.Graphics.FillRectangle(New SolidBrush(color.Color), rect)
            e.Graphics.DrawRectangle(SystemPens.WindowText, rect)

            'Write color name
            Dim brush As Brush
            'If ((e.State And DrawItemState.Selected) <> DrawItemState.None) Then
            '    brush = SystemBrushes.HighlightText
            'Else
            brush = SystemBrushes.WindowText
            'End If
            e.Graphics.DrawString(color.Text, Font, brush,
                e.Bounds.X + rect.X + rect.Width + 2,
                e.Bounds.Y + ((e.Bounds.Height - Font.Height) / 2))

            'Draw the focus rectangle if appropriate
            If ((e.State And DrawItemState.NoFocusRect) = DrawItemState.None) Then
                e.DrawFocusRectangle()
            End If
        End If
    End Sub

    '<summary>
    'Gets or sets the currently selected item.
    '</summary>
    Public Property SelectedItem As ColorInfo
        Get
            Return MyBase.SelectedItem
        End Get
        Set(value As ColorInfo)
            MyBase.SelectedItem = value
        End Set
    End Property

    '<summary>
    'Gets the text of the selected item, or sets the selection to
    'the item with the specified text.
    '</summary>
    Public Property SelectedText As String
        Get
            If (SelectedIndex >= 0) Then Return SelectedItem.Text
            Return String.Empty
        End Get
        Set(value As String)
            For i As Integer = 0 To Items.Count - 1
                If Items(i).Text = value Then
                    SelectedIndex = i
                    Exit For
                End If
            Next
        End Set
    End Property

    '<summary>
    'Gets the value of the selected item, or sets the selection to
    'the item with the specified value.
    '</summary>
    Public Property SelectedValue As Color
        Get
            If SelectedIndex >= 0 Then Return SelectedItem.Color
            Return Color.White
        End Get
        Set(value As Color)
            For i As Integer = 0 To Items.Count - 1
                If Items(i).Color = value Then
                    SelectedIndex = i
                    Exit For
                End If
            Next
        End Set
    End Property

End Class
