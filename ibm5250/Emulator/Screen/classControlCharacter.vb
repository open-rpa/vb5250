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
        <Serializable()> Friend Class ControlCharacter
            <Serializable()> Public Structure PreFlags
                Dim Non_Stream_Data As Boolean
                Dim Null_All_Non_Bypass_Fields As Boolean
                Dim Null_All_Non_Bypass_Fields_With_MDT_On As Boolean
                Dim Reset_MDT_Flags_In_All_Fields As Boolean
                Dim Reset_MDT_Flags_In_Non_Bypass_Fields As Boolean
                Dim Reset_Pending_AID_And_Lock_Keyboard As Boolean
            End Structure
            <Serializable()> Public Structure PostFlags
                Dim Cursor_Moves_When_Keyboard_Unlocks As Boolean
                Dim Reset_Blinking_Cursor As Boolean
                Dim Set_Blinking_Cursor As Boolean
                Dim Set_Message_Waiting_Indicator_Off As Boolean
                Dim Set_Message_Waiting_Indicator_On As Boolean
                Dim Sound_Alarm As Boolean
                Dim Unlock_Keyboard_And_Reset_Pending_AID As Boolean
            End Structure
            <Serializable()> Public Structure ControlCharacterFlags
                Dim Pre As PreFlags
                Dim Post As PostFlags
            End Structure
            Public Flags As ControlCharacterFlags
            Public Sub New(ByVal Byte0 As Byte, ByVal Byte1 As Byte)
                Me.Flags.Pre.Non_Stream_Data = CC_Pre_Non_Stream_Data(Byte0)
                Me.Flags.Pre.Null_All_Non_Bypass_Fields = CC_Pre_Null_All_Non_Bypass_Fields(Byte0)
                Me.Flags.Pre.Null_All_Non_Bypass_Fields_With_MDT_On = CC_Pre_Null_All_Non_Bypass_Fields_With_MDT_On(Byte0)
                Me.Flags.Pre.Reset_MDT_Flags_In_All_Fields = CC_Pre_Reset_MDT_Flags_In_All_Fields(Byte0)
                Me.Flags.Pre.Reset_MDT_Flags_In_Non_Bypass_Fields = CC_Pre_Reset_MDT_Flags_In_Non_Bypass_Fields(Byte0)
                Me.Flags.Pre.Reset_Pending_AID_And_Lock_Keyboard = CC_Pre_Reset_Pending_AID_And_Lock_Keyboard(Byte0)
                Me.Flags.Post.Cursor_Moves_When_Keyboard_Unlocks = CC_Post_Cursor_Moves_When_Keyboard_Unlocks(Byte1)
                Me.Flags.Post.Reset_Blinking_Cursor = CC_Post_Reset_Blinking_Cursor(Byte1)
                Me.Flags.Post.Set_Blinking_Cursor = CC_Post_Set_Blinking_Cursor(Byte1)
                Me.Flags.Post.Set_Message_Waiting_Indicator_Off = CC_Post_Set_Message_Waiting_Indicator_Off(Byte1)
                Me.Flags.Post.Set_Message_Waiting_Indicator_On = CC_Post_Set_Message_Waiting_Indicator_On(Byte1)
                Me.Flags.Post.Sound_Alarm = CC_Post_Sound_Alarm(Byte1)
                Me.Flags.Post.Unlock_Keyboard_And_Reset_Pending_AID = CC_Post_Unlock_Keyboard_And_Reset_Pending_AID(Byte1)
            End Sub

            '5250 WRITE TO DISPLAY Command Control Characters: SC30-3533-04 table 43 on page 15.6.1-1
            'The following functions decode byte 0, which is not bitmapped in the usual sense.
            Private Function CC_Pre_Reset_Pending_AID_And_Lock_Keyboard(ByVal b As Byte) As Boolean
                b = b And CByte(&HE0) 'bits 0 thru 2 with 0 being most significant
                'Return CBool((b And CByte(&H20)) <> 0)
                Return CBool(b <> 0)
            End Function
            Private Function CC_Pre_Reset_MDT_Flags_In_Non_Bypass_Fields(ByVal b As Byte) As Boolean
                b = b And CByte(&HE0) 'bits 0 thru 2 with 0 being most significant
                Select Case b
                    Case &H40, &HA0, &HC0
                        Return True
                    Case Else
                        Return False
                End Select
            End Function
            Private Function CC_Pre_Reset_MDT_Flags_In_All_Fields(ByVal b As Byte) As Boolean
                b = b And CByte(&HE0) 'bits 0 thru 2 with 0 being most significant
                Select Case b
                    Case &H60, &HE0
                        Return True
                    Case Else
                        Return False
                End Select
            End Function
            Private Function CC_Pre_Null_All_Non_Bypass_Fields_With_MDT_On(ByVal b As Byte) As Boolean
                b = b And CByte(&HE0) 'bits 0 thru 2 with 0 being most significant
                Select Case b
                    Case &H80, &HC0
                        Return True
                    Case Else
                        Return False
                End Select
            End Function
            Private Function CC_Pre_Null_All_Non_Bypass_Fields(ByVal b As Byte) As Boolean
                b = b And CByte(&HE0) 'bits 0 thru 2 with 0 being most significant
                Select Case b
                    Case &HA0, &HE0
                        Return True
                    Case Else
                        Return False
                End Select
            End Function
            Private Function CC_Pre_Non_Stream_Data(ByVal b As Byte) As Boolean
                'If bit 7 is set to on, the data in the WRITE TO DISPLAY command is 	 	 
                'non-stream data and is written to the workstation without data stream 	 	 
                'optimization. If bit 7 is set to off, the data in the WRITE TO DISPLAY 	 	 
                'command and subsequent keystrokes may be optimized to improve performance. 
                '
                Return CBool(b And 1) 'bit 7, which in IBM-land means least significant bit
            End Function

            '5250 WRITE TO DISPLAY Command Control Characters: SC30-3533-04 table 43 on page 15.6.1-1
            'The following functions decode byte 1.
            Private Function CC_Post_Cursor_Moves_When_Keyboard_Unlocks(ByVal b As Byte) As Boolean
                'Cursor moves to default or insert cursor (IC) order
                'position when keyboard unlocks.
                '
                'An exception is when a WTD is received when the keyboard is
                'unlocked and the WTD does not modify the keyboard state; then the
                'cursor is not moved.
                Return Not CBool(b And &H40) 'bit 1
            End Function
            Private Function CC_Post_Reset_Blinking_Cursor(ByVal b As Byte) As Boolean
                'If bits 2 and 3 are both on, the cursor blinks. For the IBM 5292
                'Color Display Station, bits 2 and 3 will be effective only if the
                'workstation operator does not use the blink cursor function.
                Return CBool(b And &H20) 'bit 2
            End Function
            Private Function CC_Post_Set_Blinking_Cursor(ByVal b As Byte) As Boolean
                'If bits 2 and 3 are both on, the cursor blinks. For the IBM 5292
                'Color Display Station, bits 2 and 3 will be effective only if the
                'workstation operator does not use the blink cursor function.
                Return CBool(b And &H10) 'bit 3
            End Function
            Private Function CC_Post_Unlock_Keyboard_And_Reset_Pending_AID(ByVal b As Byte) As Boolean
                Return CBool(b And 8) 'bit 4
                'If the keyboard is already unlocked, this bit is ignored; 	 
                'otherwise, it: 	 	 
                ' 	a. Unlocks the keyboard. 		 
                ' 	b. Turns the keyboard clicker on. 		 
                ' 	c. Turns the Input Inhibited indicator off. 		 
                ' 	d. Moves the cursor to the address given in the last IC order or 		 
                ' 	   defaults to the first position of the first non-bypass input 	 
                ' 	   field if no IC order has been given. If there is no 	 
                ' 	   non-bypass field, it defaults to row 1, column 1.  	 
                ' 	e. Clears all unserviced AID requests. 
                ' 	 
                'The 5494 defers this process until after the operator presses the  	 
                'Error Reset key, if in operator error state.  
            End Function
            Private Function CC_Post_Sound_Alarm(ByVal b As Byte) As Boolean
                Return CBool(b And 4) 'bit 5
            End Function
            Private Function CC_Post_Set_Message_Waiting_Indicator_Off(ByVal b As Byte) As Boolean
                'If bits 6 and 7 are both on, the Message Waiting indicator is set to on.
                Return CBool(b And 2) 'bit 6
            End Function
            Private Function CC_Post_Set_Message_Waiting_Indicator_On(ByVal b As Byte) As Boolean
                'If bits 6 and 7 are both on, the Message Waiting indicator is set to on.
                Return CBool(b And 1) 'bit 7
            End Function
        End Class 'ControlCharacter
    End Class 'EmulatorScreen
End Class 'Emulator

