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
    <Serializable()> Public Class EmulatorKeyboard
        Public Event StateChanged(ByVal PriorState As Keyboard_State, ByVal CurrentState As Keyboard_State)
        Public Event InsertChanged(ByVal NewValue As Boolean)

        Public Enum Keyboard_State As Byte
            Normal_Unlocked = 0
            Normal_Locked = 1
            Pre_Help_Error = 2
            Post_Help_Error = 3
            Hardware_Error = 4
            Power_On = 5
            SS_Message = 6
            System_Request = 7
        End Enum

        Private _Locked As Boolean = False
        Private _State As Keyboard_State = Keyboard_State.Normal_Unlocked
        Private _Insert As Boolean = False

        Public ReadOnly Property Locked As Boolean
            Get
                Return _Locked
            End Get
        End Property

        Public Property Insert As Boolean
            Set(value As Boolean)
                If value <> _Insert Then
                    _Insert = value
                    RaiseEvent InsertChanged(_Insert)
                End If
            End Set
            Get
                Return _Insert
            End Get
        End Property

        Public Property State As Keyboard_State
            Set(value As Keyboard_State)
                SetState(value)
            End Set
            Get
                Return _State
            End Get
        End Property

        Private Sub SetState(NewState As Keyboard_State)
            'XXX is it useful for this to be static?
            Static PriorState As Keyboard_State
            'PriorState contains the state prior to the previous call to SetState()

            PriorState = _State
            'PriorState now contains the state prior to this call to SetState()

            'Me.Screen.ErrorText = Me.Screen.PriorErrorText

            Select Case NewState
                Case Keyboard_State.Hardware_Error
                    _Locked = True
                Case Keyboard_State.Normal_Locked
                    _Locked = True
                Case Keyboard_State.Normal_Unlocked
                    _Locked = False
                Case Keyboard_State.Power_On
                    _Locked = True 'This isn't correct for this state, but it's best for our purposes.
                Case Keyboard_State.Pre_Help_Error
                    _Locked = True
                Case Keyboard_State.Post_Help_Error
                    _Locked = True
                Case Keyboard_State.SS_Message
                    _Locked = True
                Case Keyboard_State.System_Request
                    _Locked = False
            End Select

            RaiseEvent StateChanged(PriorState, State)

        End Sub
    End Class
End Class 'Emulator

