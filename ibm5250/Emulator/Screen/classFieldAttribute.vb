'
' Copyright 2012, 2013 Alec Skelly
'
' This file is part of IBM5250, a VB.Net implementation of IBM's 5250 protocol.
'
' IBM5250 is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' IBM5250 is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with IBM5250. If not, see <http://www.gnu.org/licenses/>.
' 

Partial Class Emulator
    Partial Class EmulatorScreen
        <Serializable()> Public Class FieldAttribute
            Public Enum ColorAttribute As Byte
                [Default] = &H0
                Green = &H20
                GreenReverse = &H21
                White = &H22
                WhiteReverse = &H23
                GreenUnderscore = &H24
                GreenUnderscoreReverse = &H25
                WhiteUnderscore = &H26
                NonDisplay27 = &H27
                Red = &H28
                RedReverse = &H29
                RedBlink = &H2A
                RedBlinkReverse = &H2B
                RedUnderscore = &H2C
                RedUnderscoreReverse = &H2D
                RedUnderscoreBlink = &H2E
                NonDisplay2F = &H2F
                TurquoiseColumn = &H30
                TurquoiseColumnReverse = &H31
                YellowColumn = &H32
                YellowColumnReverse = &H33
                TurquoiseUnderscore = &H34
                TurquoiseUnderscoreReverse = &H35
                YellowUnderscore = &H36
                NonDisplay37 = &H37
                Pink = &H38
                PinkReverse = &H39
                Blue = &H3A
                BlueReverse = &H3B
                PinkUnderscore = &H3C
                PinkUnderscoreReverse = &H3D
                BlueUnderscore = &H3E
                NonDisplay3f = &H3F
            End Enum
            Public Attribute As ColorAttribute

            'Public DefaultForeColor As KnownColor
            'Public ForeColor As KnownColor
            'Public BackColor As KnownColor
            'Private OriginalBackColor As KnownColor
            Public DefaultForeColor As String
            Public ForeColor As String
            Public BackColor As String
            Public OriginalBackColor As String

            Public Underscore As Boolean
            Public ColumnLines As Boolean
            Public NonDisplay As Boolean
            'XXX need more stuff here to fully describe attribute

            Public Sub New()
                Me.New(ColorAttribute.Default, "Green", "Background")
            End Sub
            Public Sub New(ByVal b As Byte)
                Me.New(b, "Green", "Background")
            End Sub
            Public Sub New(ByVal b As Byte, ByVal DefaultForeColor As String, ByVal BackColor As String)
                Me.Attribute = CType(b, ColorAttribute)
                Me.DefaultForeColor = DefaultForeColor
                Me.ForeColor = Me.DefaultForeColor
                Me.OriginalBackColor = BackColor
                Me.BackColor = Me.OriginalBackColor
                Select Case Me.Attribute
                    Case ColorAttribute.Green, ColorAttribute.GreenReverse, ColorAttribute.GreenUnderscore, ColorAttribute.GreenUnderscoreReverse
                        Me.ForeColor = "Green"
                    Case ColorAttribute.White, ColorAttribute.WhiteReverse, ColorAttribute.WhiteUnderscore
                        Me.ForeColor = "White"
                    Case ColorAttribute.Blue, ColorAttribute.BlueReverse, ColorAttribute.BlueUnderscore
                        Me.ForeColor = "Blue"
                    Case ColorAttribute.Pink, ColorAttribute.PinkReverse, ColorAttribute.PinkUnderscore, ColorAttribute.PinkUnderscoreReverse
                        Me.ForeColor = "Pink"
                    Case ColorAttribute.Red, ColorAttribute.RedReverse, ColorAttribute.RedUnderscore, ColorAttribute.RedUnderscoreReverse, ColorAttribute.RedBlink, ColorAttribute.RedBlinkReverse
                        Me.ForeColor = "Red"
                    Case ColorAttribute.TurquoiseUnderscore, ColorAttribute.TurquoiseUnderscoreReverse, ColorAttribute.TurquoiseColumn, ColorAttribute.TurquoiseColumnReverse
                        Me.ForeColor = "Turquoise"
                    Case ColorAttribute.YellowUnderscore, ColorAttribute.YellowColumn, ColorAttribute.YellowColumnReverse
                        Me.ForeColor = "Yellow"
                    Case ColorAttribute.Default
                        Me.ForeColor = Me.DefaultForeColor
                    Case Else
                        Me.ForeColor = "Red"
                End Select
                If Me.Attribute.ToString.Contains("Blink") Then
                    'XXX

                End If
                If Me.Attribute.ToString.Contains("Column") Then
                    'XXX

                End If
                If Me.Attribute.ToString.Contains("NonDisplay") Then
                    Me.NonDisplay = True
                    Me.ForeColor = Me.BackColor
                End If
                If Me.Attribute.ToString.Contains("Reverse") Then
                    Me.BackColor = Me.ForeColor
                    Me.ForeColor = Me.OriginalBackColor
                End If
                Me.Underscore = Me.Attribute.ToString.Contains("Underscore")
                Me.ColumnLines = Me.Attribute.ToString.Contains("Column")
            End Sub
        End Class 'FieldAttribute
    End Class 'EmulatorScreen
End Class 'Emulator


