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

<Serializable()> Partial Public Class Emulator
    Public Const ESC As Byte = &H4
    Public WithEvents Screen As New EmulatorScreen(24, 80, Me)
    Public WithEvents Keyboard As New EmulatorKeyboard
    Public Invited As Boolean
    Private ScreenSize27x132_Allowed As Boolean
    Public TerminalType As String       'The first part of the workstation model number, '3477' in 'IBM-3477-FC'
    Public TerminalModel As String      'The second part of the workstation model number, 'FC' in 'IBM-3477-FC'
    Private SavedScreen As EmulatorScreen = Nothing
    Private SavedKeyboard As EmulatorKeyboard = Nothing

    Public Event ScreenCleared()
    Public Event StringsChanged()
    Public Event FieldsChanged()
    Public Event FieldAttributeChanged(ByVal FieldIndex As Integer)
    Public Event PopupAdded(ByVal key As String)
    Public Event PopupRemoved(ByVal key As String)
    Public Event CursorPositionSet(ByVal Row As Byte, ByVal Column As Byte)
    Public Event KeyboardStateChanged(ByVal PriorState As EmulatorKeyboard.Keyboard_State, ByVal CurrentState As EmulatorKeyboard.Keyboard_State)
    Public Event InsertChanged(ByVal NewState As Boolean)
    Public Event MessageWaitingChanged(ByVal NewState As Boolean)
    Public Event DataStreamError(ByVal NegativeResponse As NegativeResponse)
    Public Event ErrorTextChanged()
    Public Event DataReady(ByVal Bytes() As Byte, ByVal OpCode As TN5250.OpCodes)
    Public Event StartupResponseReceived(ByVal ResponseCode As String)

    Private Logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger()

    Public Locale As Localization.Locale
    Public LocaleInfo As Localization

    Public Enum Mode As Byte
        DP = 0      'Data Processing
        WP = 1      'Word Processing
        PTDS = 2    'Pass-through Data Stream Processing (printers, etc.)
    End Enum

    Public Enum AID As Byte
        Clear = &HBD
        Enter = &HF1    'Record Advance
        Help = &HF3
        RollDown = &HF4 'Page Up!
        RollUp = &HF5   'Page Down!
        RollLeft = &HD9
        RollRight = &HDA
        Print = &HF6
        RecordBackspace = &HF8 'Home key pressed when the cursor is already at the home position.
        AutoEnter_SLP = &H3F
        AutoEnter_ForwardEdgeTrigger = &H50
        PA1 = &H6C
        PA2 = &H6E
        PA3 = &H6B
        PF1 = &H31
        PF2 = &H32
        PF3 = &H33
        PF4 = &H34
        PF5 = &H35
        PF6 = &H36
        PF7 = &H37
        PF8 = &H38
        PF9 = &H39
        PF10 = &H3A
        PF11 = &H3B
        PF12 = &H3C
        PF13 = &HB1
        PF14 = &HB2
        PF15 = &HB3
        PF16 = &HB4
        PF17 = &HB5
        PF18 = &HB6
        PF19 = &HB7
        PF20 = &HB8
        PF21 = &HB9
        PF22 = &HBA
        PF23 = &HBB
        PF24 = &HBC
        Custom70 = &H70
        Custom71 = &H71
        Custom72 = &H72
        Custom73 = &H73
        Custom74 = &H74
        Custom75 = &H75
        Custom76 = &H76
        Custom77 = &H77
        Custom78 = &H78
        Custom79 = &H79
        Custom7A = &H7A
        Custom7B = &H7B
        Custom7C = &H7C
        Custom7D = &H7D
        Custom7E = &H7E
        Custom7F = &H7F
    End Enum

    '5250 Workstation Data Stream Commands: SC30-3533-04 table 42 on page 15.2-1
    Public Enum Command As Byte
        CLEAR_UNIT = &H40   'DP, WP                                                       
        CLEAR_UNIT_ALTERNATE = &H20 'DP, WP
        CLEAR_FORMAT_TABLE = &H50   'DP, WP
        WRITE_TO_DISPLAY = &H11 'DP
        WRITE_ERROR_CODE = &H21  'DP, WP
        WRITE_ERROR_CODE_TO_WINDOW = &H22   'DP
        READ_INPUT_FIELDS = &H42    'DP
        READ_MDT_FIELDS = &H52  'DP
        READ_MDT_ALTERNATE = &H82   'DP
        READ_SCREEN = &H62  'DP, WP
        READ_SCREEN_WITH_EXTENDED_ATTRIBUTES = &H64 'DP, WP
        READ_SCREEN_TO_PRINT = &H66 'DP
        READ_SCREEN_TO_PRINT_WITH_EXTENDED_ATTRIBUTES = &H68    'DP
        READ_SCREEN_TO_PRINT_WITH_EXTENDED_ATTRIBUTES_AND_GRIDLINES = &H6C  'DP
        READ_IMMEDIATE = &H72   'DP
        READ_MODIFIED_IMMEDIATE_ALTERNATE = &H83    'DP
        SAVE_SCREEN = &H2   'DP, WP
        SAVE_PARTIAL_SCREEN = &H3   'DP
        RESTORE_SCREEN = &H12   'DP, WP
        RESTORE_PARTIAL_SCREEN = &H13   'DP
        ROLL = &H23 'DP
        UNDOCUMENTED_1 = &H26 'XXX This sometimes appears following WRITE_ERROR_CODE.
        WRITE_STRUCTURED_FIELD = &HF3   'DP, WP (But different forms)
        WRITE_SINGLE_STRUCTURED_FIELD = &HF4    'DP
        COPY_TO_PRINTER = &H16  'DP, WP
    End Enum

    Public Enum NegativeResponse As UInt32
        '...
        Command_Not_Valid = &H10030101UI
        Clear_Unit_Alternate_Command_Not_Valid = &H10030105UI
        Enter_Text_Mode_Not_Valid = &H10030123UI
        Format_Table_Resequencing_Error = &H10050103UI
        Structured_Field_Length_Not_Valid = &H10050110UI
        Structured_Field_Type_Not_Valid = &H10050111UI
        Structured_Field_Parameter_Not_Valid = &H10050112UI
        Structured_Field_Minor_Structure_Length_Not_Valid = &H10050113UI
        Structured_Field_Minor_Structure_Parameter_Not_Valid = &H10050114UI
        Command_Not_Valid_In_WP_Mode = &H1005011BUI
        Command_Not_Valid_In_DP_Mode = &H1005011CUI
        Command_Not_Valid_With_Unlocked_Keyboard = &H1005011DUI
        Premature_Data_Stream_Termination = &H10050121UI
        Write_To_Display_Row_Col_Address_Not_Valid = &H10050122UI
        Repeat_To_Address_Less_Than_Current_Address = &H10050123UI
        Start_Of_Field_Length_Not_Valid = &H10050125UI
        Start_Of_Field_Address_Not_Valid = &H10050126UI
        Restore_Data_Not_Valid = &H10050127UI
        Field_Extends_Past_End_Of_Display = &H10050128UI
        Format_Table_Overflow = &H10050129UI
        Write_Past_End_Of_Display = &H1005012AUI
        Start_Of_Header_Length_Not_Valid = &H1005012BUI
        Roll_Parameter_Not_Valid = &H1005012CUI
        Extended_Attribute_Type_Not_Valid = &H1005012DUI
        RAM_Load_Parameter_Not_Valid = &H1005012EUI
        Extended_Attribute_Not_Valid = &H1005012FUI
        Start_Of_Field_Attribute_Not_Valid = &H10050130UI
        Escape_Code_Expected_But_Not_Found = &H10050131UI
        Write_Error_Code_To_Window_Row_Col_Address_Not_Valid = &H10050132UI
        Write_Error_Code_To_Window_Not_Valid_With_Current_Error_Line = &H10050133UI
        Save_Partial_Screen_Followed_By_Read_Or_Save = &H10050134UI
        Continued_Entry_Field_Segment_Not_Valid = &H10050135UI
        Word_Wrap_Not_Allowed = &H10050136UI
        Scroll_Bar_Write_Beyond_Last_Column = &H10050138UI
        Scroll_Bar_Position_Not_Valid = &H10050139UI
        Selection_Field_Choice_Required = &H1005013AUI 'At least one selection field choice must be allowed to accept the cursor
        Selection_Field_Column_Address_Not_Valid = &H1005013BUI 'An attempt was made to write a selection field choice before column 1 or beyond the last column
        Selection_Field_Too_Many_Choices = &H1005013CUI
        Selection_Field_Too_Many_Defaults = &H1005013DUI
        Too_Many_Windows = &H1005013EUI
        Write_Data_To_Non_Entry_Field = &H10050140UI
        Write_Data_Length_Not_Valid = &H10050141UI
        Write_FF_To_Screen = &H10050142UI
        Fax_And_Image_Feature_Not_Supported = &H10050148UI
        '...
        Data_Stream_Too_Long = &H1005014FUI 'Data stream longer than 16,368 bytes
        '...
    End Enum

    Public Enum SystemReferenceCode As UInt16 'SRC.  This is a subset of available SRCs.
        'You pressed Help. However, either no SRC was displayed or the application program does not support Help.
        Help_key_not_allowed = 0
        'A keyboard overrun occurred because the 5494 could not keep up with the rate at which you were entering information. The last character entered was not recognized. If type ahead has been enabled, your buffer is full.
        Keyboard_overrun = 1
        'The 5494 received an incorrect scan code from the workstation's keyboard. Either the scan code is incorrect for the keyboard at the workstation, or an error occurred in translating the keystroke.
        Incorrect_scan_code = 2
        'You pressed a Command key sequence, a PF key that is not supported or not valid for the current field, or an Alt key sequence that is not valid.
        Command_key_not_valid = 3
        'You tried to enter data from the keyboard into a field where only an entry from a magnetic stripe reader or a light pen is allowed.
        Data_not_allowed_in_this_field = 4
        'You have tried to enter data. However, the cursor is not in an input field on the display. Data cannot be entered in a protected area of the display. 
        Cursor_in_protected_area_of_display = 5
        'You pressed a key that is not valid after pressing the Sys Req Key and before pressing Enter/Rec Adv or Error Reset. 
        Key_following_SysReq_key_is_not_valid = 6
        'There is at least one field on the screen into which you must enter data before the screen can be changed or processed. (The cursor goes to the first character position of the first mandatory entry field.)
        Mandatory_entry_field_You_must_enter_data = 7 'Mandatory entry field; you must enter data. 
        'You tried to enter non-alphabetic characters into a mandatory alphabetic field. Valid characters are A through Z, space, comma (,), period (.), hyphen (-), apostrophe ('), and Dup. Dup can be used to duplicate these characters in the field. 
        This_field_must_have_alphabetic_characters = 8
        'You tried to enter nonnumeric characters into a numeric-only field. Valid characters are 0 through 9, space, comma (,), period (.), plus (+), minus (-), and Dup. Dup can be used to duplicate these characters in the field. 
        This_field_must_have_numeric_characters = 9
        'The key pressed is not valid for a signed numeric field. Valid entries are 0 through 9 and Dup.
        Only_characters_0_through_9_allowed = &H10
        'You tried to enter data into the last position of a signed numeric field.
        Data_not_allowed_in_sign_position = &H11
        'There is no room to insert data into this field. Either there is no more room in the field or the cursor is in the last position of the field.
        Insert_mode_No_room_to_insert_data = &H12 'Insert mode; no room to insert data.
        'You tried to exit a field while the workstation was still in insert mode.
        Insert_mode_Only_data_keys_permitted = &H13 'Insert mode; only data keys permitted. 
        'You pressed a function key that would move the cursor out of this field. However, the requirements of this mandatory-fill field were not met. A mandatory-fill field must be completely filled or left blank.
        Must_fill_field_to_exit = &H14
        'Modulo 10 or 11 check digit error. You entered data into a self-check field, and the number you entered and the check digit do not compare.
        Modulo_10_or_11_check_digit_error = &H15
        'You pressed Field- when the cursor was not in a numeric-only, digits-only, or signed numeric field.
        Field_minus_not_valid_in_this_field = &H16
        'You pressed Field-, Field+, or Field Exit. However, the requirements for this mandatory-fill field were not met. A mandatory-fill field must be completely filled unless you exit it from the first position of the field. 
        Mandatory_fill_field_Keypress_not_valid = &H17 'Mandatory-fill field; key pressed is not valid.
        'The cursor is in a right-adjust or field-exit-required field, and you pressed a data key.
        Key_used_to_exit_this_field_not_valid = &H18
        'You pressed Dup or Field Mark. However, the key is not permitted in this field.
        Dup_or_Field_Mark_not_permitted = &H19 'Dup or Field Mark not permitted in this field.
        'You pressed a function key that is not permitted in this field. Press Field-, Field+, or Field Exit to exit this field before pressing one of the following function keys:
        '       Test Req
        '       Clear
        '       Enter/Rec Adv
        '       Print
        '       Help
        '       Roll
        '       Home (when the cursor is in the home position)
        '       PF/Cmd1-24
        '       Sys Req
        '       Rec Backspace. 
        Function_key_not_valid_for_right_adjust_field = &H20
        'The cursor is positioned in a mandatory entry field. You must enter data into a mandatory entry field before you can exit the field by pressing Field-, Field+, or Field Exit. You can exit from any position if no data is entered. 
        Must_enter_data_in_mandatory_entry_field = &H21
        'An AS/400 system error occurred. The status of the current field is not known. This error can occur during an insert or delete operation.
        AS400_System_error = &H22
        'The workstation is in hexadecimal mode but the first key pressed was not a character 4 through 9 or A through F, or the second key pressed was not a character 0 through 9 or A through F.
        'This error also occurs when a hexadecimal code is used in a numeric-only, signed numeric, digits-only, alphabetic-only, or I/O field. 
        Hexadecimal_mode_Entry_not_valid = &H23 'Hexadecimal mode; entry not valid.
        'You pressed a key that is not valid. Only characters 0 through 9 and Dup (if specified in the field format word) are allowed in this field. 
        Decimal_field_Entry_not_valid = &H24
        'You pressed Field- to exit a numeric-only field but the last position of the field was not a character 0 through 9. 
        Field_minus_entry_not_allowed = &H26
        'You pressed a key that is either blank or not defined for your workstation. 
        Cannot_use_undefined_key = &H27
        'The second key pressed during a diacritic key function did not produce a valid diacritic character. 
        Diacritic_character_not_valid = &H29
        'The data received from the magnetic stripe reader (MSR) card was longer than the maximum allowed. 
        Data_buffer_overflow = &H31
        'The data from the magnetic stripe reader (MSR) card was not received correctly.
        MSR_rx_error = &H32
        'The data received from the magnetic stripe reader (MSR) card was secured data (for example, an operator ID card), and this field was not specified for secured data. 
        MSR_data_not_authorized = &H33
        'The data received from the MSR card will not fit into the active input field.
        MSR_data_exceeds_length_of_field = &H34
        'The card to be read was incorrectly inserted into the magnetic stripe reader (MSR), was incorrectly made, or is damaged.
        MSR_error = &H35
        'You pressed Cursor Select while in field-exit-required state.
        Cursor_select_not_allowed_when_field_exit_required = &H36 'Cursor select not allowed in field exit required state. 
        'You pressed Cursor Select in a non-selectable field. 
        Cursor_select_in_non_selectable_field = &H37
        'You attempted to use the selector light pen or magnetic stripe reader (MSR) while using text processing. These functions are not valid for text processing.
        Light_pen_and_MSR_not_allowed = &H38
    End Enum

    Public Shared Function IsAttribute(ByVal b As Byte) As Boolean
        Return (b And &HE0) = &H20
    End Function

    Public Function EBCDIC_To_UTF8(ByVal b() As Byte) As Byte()
        Return System.Text.Encoding.Convert(Me.Locale.Encoding, System.Text.Encoding.UTF8, b)
    End Function
    Public Function UTF8_To_EBCDIC(ByVal b() As Byte) As Byte()
        Return System.Text.Encoding.Convert(System.Text.Encoding.UTF8, Me.Locale.Encoding, b)
    End Function

    Private Function Is_WTD_Data_Byte(ByVal b As Byte) As Boolean
        'See SC30-3533-04 section 15.6.2, "Data Characters"
        '
        'This method is abandoned because we saw some bytes that were not Orders but were also not explicitly Data.
        'Select Case b
        '    Case 0, &H1C, &H1E, &HE, &HF, &H20 To &HFE
        '        Return True
        '    Case Else
        '        Return False
        'End Select
        '
        '
        'This method considers anything that's not an Order to be Data.
        Return Not ((b = &HFF) Or [Enum].IsDefined(GetType(EmulatorScreen.WTD_Order), b))
    End Function

    'Private Sub IncrementCursorPosition(ByVal n As Integer)
    '    Dim Offs As Integer = ((Screen.Row - 1) * Screen.Columns) + Screen.Column - 1
    '    Dim MaxOffs As Integer = (Screen.Rows * Screen.Columns) - 1
    '    Offs += n
    '    If Offs > MaxOffs Then Offs -= MaxOffs 'wrap back to top of screen
    '    Screen.Row = (Offs \ Screen.Rows) + 1
    '    Screen.Column = (Offs Mod Screen.Columns) + 1
    'End Sub

    Private Sub Handle_Text(ByRef text As String, ByVal Attribute As Emulator.EmulatorScreen.FieldAttribute)
        If text IsNot Nothing Then
            Logger.Trace(vbTab & "Text: '" & text & "'")
            text = Nothing
        End If
    End Sub

    Private Sub OnKeyboardStateChanged(ByVal PriorState As EmulatorKeyboard.Keyboard_State, ByVal CurrentState As EmulatorKeyboard.Keyboard_State)
        RaiseEvent KeyboardStateChanged(PriorState, CurrentState)
    End Sub

    Private Sub OnInsertChanged(ByVal NewValue As Boolean) Handles Keyboard.InsertChanged
        RaiseEvent InsertChanged(NewValue)
    End Sub

    Private Sub OnScreenCleared() Handles Screen.Cleared
        RaiseEvent ScreenCleared()
    End Sub

    Private Sub OnStringsChanged() Handles Screen.StringsChanged
        RaiseEvent StringsChanged()
    End Sub

    Private Sub OnFieldsChanged() Handles Screen.FieldsChanged
        RaiseEvent FieldsChanged()
    End Sub

    Private Sub OnFieldAttributeUpdated(ByVal FieldIndex As Integer) Handles Screen.FieldAttributeChanged
        RaiseEvent FieldAttributeChanged(FieldIndex)
    End Sub

    Public Sub OnPopupAdded(ByVal key As String) Handles Screen.PopupAdded
        RaiseEvent PopupAdded(key)
    End Sub
    Public Sub OnPopupRemoved(ByVal key As String) Handles Screen.PopupRemoved
        RaiseEvent PopupRemoved(key)
    End Sub
    Public Function ReadDataBuffer(ByVal buf() As Byte, ByVal start As Integer, ByVal Length As Integer) As Boolean 'XXX change this to a sub and throw exceptions instead of true/false
        If Length > 0 Then
            Try
                Screen.Complete = False
                Dim MaxOffs As Integer = start + Length - 1
                Dim StringsChanged As Boolean = False
                Dim FieldsChanged As Boolean = False
                Dim Got_InsertCursor As Boolean = False
                Do
                    If buf(start) = Emulator.ESC Then
                        start += 1
                        Dim Cmd As Command = CType(buf(start), Command)
                        start += 1
                        Logger.Trace(Cmd.ToString & vbLf)
                        Select Case Cmd

                            'XXX lots more commands to deal with here!

                            Case Command.CLEAR_UNIT
                                'Logger.Trace(Cmd.ToString & vbLf)
                                'XXX

                                'Locks the workstation keyboard
                                'Turns the keyboard clicker off
                                'Turns the input inhibited indicator on
                                'Clears the error state (or system request state)
                                'Clears the Insert mode (with its associated indicators)
                                'Clears the Diacritic mode (with its associated indicators), unless type-ahead mode is active
                                'Clears the Command mode, unless type-ahead mode is active
                                'Resets any keystroke processing state
                                'Clears the format table (with its effects on the keyboard shift state)
                                'Clears the modified data tag (MDT) bit
                                'Resets read sequencing
                                'Sets up all PF or Command keys to return data
                                'Clears the workstation screen by writing nulls to the regeneration buffer and nulling any extended attributes
                                'Screen size is set (reset) to 24 rows by 80 columns
                                'Writes an attribute byte to row 1, column 2 of the display with the following characteristics:
                                '- Nonblink
                                '- Nonreverse video
                                '- Normal intensity.
                                'Places the cursor (blinking) at row 1, column 1 and sets this address as the insert cursor address
                                'Clears any AID request not already serviced
                                'Clears any pending READ INPUT or READ MDT commands.
                                'For older displays without a separate message line, line 24 is set as the message line. For displays with a separate message line, the
                                'message line is set as the default.
                                'If the workstation is in SS message state, the 5494 rejects the CLEAR UNIT command and returns a contention error negative response.

                                If Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.SS_Message Then
                                    RaiseEvent DataStreamError(NegativeResponse.Command_Not_Valid) 'XXX unabled to find a negative response called "contention error".
                                    Return False
                                Else
                                    Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.Normal_Locked
                                    If Me.Screen.Rows = 24 And Me.Screen.Columns = 80 Then
                                        Me.Screen.Clear()
                                    Else
                                        Me.Screen = New EmulatorScreen(24, 80, Me)
                                        Me.Screen.Clear() 'fire the ScreenCleared event so the GUI can resize
                                    End If
                                End If

                            Case Command.CLEAR_UNIT_ALTERNATE
                                If Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.SS_Message Then
                                    RaiseEvent DataStreamError(NegativeResponse.Command_Not_Valid) 'XXX unabled to find a negative response called "contention error".
                                    Return False
                                Else
                                    Dim Param As Byte = buf(start)
                                    Logger.Trace(vbTab & "Param: &H" & Hex(Param))
                                    start += 1
                                    Select Case Param
                                        Case 0
                                            If Me.ScreenSize27x132_Allowed Then
                                                Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.Normal_Locked
                                                Me.Screen = New EmulatorScreen(27, 132, Me)
                                                Me.Screen.Clear() 'fire the ScreenCleared event so the GUI can resize
                                            Else
                                                RaiseEvent DataStreamError(NegativeResponse.Command_Not_Valid)
                                                Return False
                                            End If
                                        Case &H80
                                            Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.Normal_Locked
                                            Me.Screen.Clear()
                                        Case Else
                                            RaiseEvent DataStreamError(NegativeResponse.Clear_Unit_Alternate_Command_Not_Valid)
                                            Return False
                                    End Select
                                End If

                            Case Command.CLEAR_FORMAT_TABLE
                                'XXX This command has never been tested
                                If (Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.SS_Message) Or
                                    (Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.System_Request) Then
                                    RaiseEvent DataStreamError(NegativeResponse.Command_Not_Valid) 'XXX unabled to find a negative response called "contention error".
                                    Return False
                                Else
                                    Me.Screen.ClearFields(True)
                                    FieldsChanged = True
                                    StringsChanged = True
                                End If

                            Case Command.WRITE_TO_DISPLAY
                                If (Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.SS_Message) Or
                                    (Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.System_Request) Then
                                    RaiseEvent DataStreamError(NegativeResponse.Command_Not_Valid)
                                    Return False
                                Else

                                    Dim cc As New EmulatorScreen.ControlCharacter(buf(start), buf(start + 1))
                                    start += 2
                                    Logger.Trace(vbTab & "---Pre flags---")
                                    Logger.Trace(vbTab & "Non-Stream Data: " & cc.Flags.Pre.Non_Stream_Data.ToString)
                                    Logger.Trace(vbTab & "Null all non-bypass fields: " & cc.Flags.Pre.Null_All_Non_Bypass_Fields.ToString)
                                    Logger.Trace(vbTab & "Null all non-bypass fields with MDT on: " & cc.Flags.Pre.Null_All_Non_Bypass_Fields_With_MDT_On.ToString)
                                    Logger.Trace(vbTab & "Reset MDT flags in all fields: " & cc.Flags.Pre.Reset_MDT_Flags_In_All_Fields.ToString)
                                    Logger.Trace(vbTab & "Reset MDT flags in non-bypass fields: " & cc.Flags.Pre.Reset_MDT_Flags_In_Non_Bypass_Fields.ToString)
                                    Logger.Trace(vbTab & "Reset pending AID and lock keyboard: " & cc.Flags.Pre.Reset_Pending_AID_And_Lock_Keyboard.ToString)
                                    Logger.Trace(vbTab & "---Post flags---")
                                    Logger.Trace(vbTab & "Cursor moves when keyboard unlocks: " & cc.Flags.Post.Cursor_Moves_When_Keyboard_Unlocks.ToString)
                                    Logger.Trace(vbTab & "Reset blinking cursor: " & cc.Flags.Post.Reset_Blinking_Cursor.ToString)
                                    Logger.Trace(vbTab & "Set blinking cursor: " & cc.Flags.Post.Set_Blinking_Cursor.ToString)
                                    Logger.Trace(vbTab & "Set message waiting indicator off: " & cc.Flags.Post.Set_Message_Waiting_Indicator_Off.ToString)
                                    Logger.Trace(vbTab & "Set message waiting indicator on: " & cc.Flags.Post.Set_Message_Waiting_Indicator_On.ToString)
                                    Logger.Trace(vbTab & "Sound alarm: " & cc.Flags.Post.Sound_Alarm.ToString)
                                    Logger.Trace(vbTab & "Reset pending AID and unlock keyboard: " & cc.Flags.Post.Unlock_Keyboard_And_Reset_Pending_AID.ToString)

                                    If cc.Flags.Pre.Reset_Pending_AID_And_Lock_Keyboard Then
                                        Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.Normal_Locked
                                    End If

                                    Dim text As String = Nothing 'accumulates regular characters to make a string
                                    'Dim TextStream As New System.IO.MemoryStream

                                    Dim CurrentAttribute As New Emulator.EmulatorScreen.FieldAttribute(Emulator.EmulatorScreen.FieldAttribute.ColorAttribute.Green, Me.Screen.DefaultForeColor, Screen.BackColor)
                                    Do While (start <= MaxOffs) AndAlso (buf(start) <> Emulator.ESC)
                                        Dim b As Byte = buf(start)
                                        start += 1
                                        'Dim ThisByteWasText As Boolean = False
                                        'Select Case b
                                        'Case 0, &H1C, &H1E, &HE, &HF, &H20 To &HFE 'Data characters
                                        If Is_WTD_Data_Byte(b) Then

                                            StringsChanged = True

                                            If b = 0 Then 'Null
                                                'If this Null byte just overwrote the starting address of a field, remove the field.
                                                If Me.Screen.RemoveField(Me.Screen.Row, Me.Screen.Column, True, False) Then
                                                    FieldsChanged = True
                                                End If
                                            End If

                                            Me.Screen.WriteTextBuffer(b)

                                            If IsAttribute(b) Then
                                                Handle_Text(text, CurrentAttribute)
                                                'Dim attr As IBM5250.ColorFieldAttribute = CType(b, IBM5250.ColorFieldAttribute)
                                                'CurrentAttribute = CType(b, IBM5250.FieldAttribute.ColorAttribute)
                                                CurrentAttribute = New Emulator.EmulatorScreen.FieldAttribute(b, Me.Screen.DefaultForeColor, Screen.BackColor)
                                                Logger.Trace(vbTab & "----------")
                                                Logger.Trace(vbTab & "Attribute: " & CurrentAttribute.Attribute.ToString)
                                                Logger.Trace(vbTab & "----------")
                                                text += " " 'attributes are displayed as spaces

                                                'attribute may apply to a field, so we need to update field attributes here.
                                                Dim findex As Integer = Screen.FieldIndexOfAddress(Screen.Row, Screen.Column)
                                                If findex > -1 Then Screen.UpdateFieldAttribute(findex, CurrentAttribute, False)

                                            Else
                                                Dim e As Byte = EBCDIC_To_UTF8(New Byte() {b})(0)
                                                'Logger.Trace(vbTab & "Data byte: &H" & Hex(e) & vbTab & "[" & Chr(e) & "]")
                                                'Logger.Trace(vbTab & "Data byte: [" & see.see_ebc(b) & "]")
                                                text += Chr(e)
                                                'ThisByteWasText = True
                                            End If
                                        Else
                                            Handle_Text(text, CurrentAttribute)

                                            If [Enum].IsDefined(GetType(EmulatorScreen.WTD_Order), b) Then
                                                'Case 1 To 3, &H10 To &H15, &H1D 'Orders
                                                Dim Order As EmulatorScreen.WTD_Order = CType(b, EmulatorScreen.WTD_Order)
                                                Logger.Trace(vbTab & "----------")
                                                Logger.Trace(vbTab & "Order: " & Order.ToString)
                                                Logger.Trace(vbTab & "----------")
                                                Select Case Order
                                                    Case EmulatorScreen.WTD_Order.Set_Buffer_Address
                                                        'The set buffer address (SBA) order specifies the address at which data transfer and input field definition will begin. Any location within the
                                                        'boundaries of the workstation presentation screen is valid.
                                                        Dim Row, Col As Byte
                                                        Row = buf(start)
                                                        Col = buf(start + 1)
                                                        start += 2
                                                        Logger.Trace(vbTab & vbTab & "Address: (" & Row.ToString & ", " & Col.ToString & ")")

                                                        If (Row > 0 And Row <= Me.Screen.Rows) And (Col > 0 And Col <= Me.Screen.Columns) Then
                                                            Screen.Row = Row
                                                            Screen.Column = Col
                                                        Else
                                                            'Throw New ApplicationException("The supplied buffer address was outside the screen area")
                                                            RaiseEvent DataStreamError(NegativeResponse.Write_To_Display_Row_Col_Address_Not_Valid)
                                                            Return False
                                                        End If

                                                    Case EmulatorScreen.WTD_Order.Insert_Cursor
                                                        Dim Row, Col As Byte
                                                        Row = buf(start)
                                                        Col = buf(start + 1)
                                                        start += 2
                                                        Logger.Trace(vbTab & vbTab & "Address: (" & Row.ToString & ", " & Col.ToString & ")")

                                                        If (Row > 0 And Row <= Me.Screen.Rows) And (Col > 0 And Col <= Me.Screen.Columns) Then
                                                            Screen.HomeCoordinates.Row = Row
                                                            Screen.HomeCoordinates.Column = Col
                                                            Got_InsertCursor = True
                                                        Else
                                                            'Throw New ApplicationException("The supplied buffer address was outside the screen area")
                                                            RaiseEvent DataStreamError(NegativeResponse.Write_To_Display_Row_Col_Address_Not_Valid)
                                                            Return False
                                                        End If

                                                    Case EmulatorScreen.WTD_Order.Move_Cursor
                                                        Dim Row, Col As Byte
                                                        Row = buf(start)
                                                        Col = buf(start + 1)
                                                        start += 2
                                                        Logger.Trace(vbTab & vbTab & "Address: (" & Row.ToString & ", " & Col.ToString & ")")

                                                        If (Row > 0 And Row <= Me.Screen.Rows) And (Col > 0 And Col <= Me.Screen.Columns) Then
                                                            'XXX It's unclear whether we're supposed to move now or at the end of the WTD.
                                                            '    Moving now means we effect any remaining orders/data in the buffer.
                                                            '    We should probably use separate accounting for buffer address and cursor address 
                                                            '    to prevent conflict.
                                                            Me.Screen.Row = Row
                                                            Me.Screen.Column = Col
                                                        Else
                                                            'Throw New ApplicationException("The supplied buffer address was outside the screen area")
                                                            RaiseEvent DataStreamError(NegativeResponse.Write_To_Display_Row_Col_Address_Not_Valid)
                                                            Return False
                                                        End If

                                                    Case EmulatorScreen.WTD_Order.Repeat_To_Address
                                                        'The repeat to address (RA) order results in the repetition of a selected character from the current workstation screen address up to and including
                                                        'the screen row and column addresses given in the order.
                                                        'The current display address is set to the location specified in the RA, plus one.
                                                        Dim Row, Col, c As Byte
                                                        Row = buf(start)
                                                        Col = buf(start + 1)
                                                        c = buf(start + 2)
                                                        start += 3
                                                        Logger.Trace(vbTab & vbTab & "Address: (" & Row.ToString & ", " & Col.ToString & ")")
                                                        Dim e As Byte = EBCDIC_To_UTF8(New Byte() {c})(0)
                                                        Logger.Trace(vbTab & vbTab & "Character: &H" & Hex(e) & vbTab & "[" & Chr(e) & "]")

                                                        If (Row > 0 And Row <= Me.Screen.Rows) And (Col > 0 And Col <= Me.Screen.Columns) Then
                                                            If Screen.GetTextBufferAddress(Row, Col) >= Screen.GetTextBufferAddress() Then
                                                                Me.Screen.RepeatToAddress(Row, Col, c, True)
                                                                FieldsChanged = True 'XXX should get result from Screen.RepeatToAddress instead
                                                                StringsChanged = True
                                                            Else
                                                                RaiseEvent DataStreamError(NegativeResponse.Repeat_To_Address_Less_Than_Current_Address)
                                                                Return False
                                                            End If
                                                        Else
                                                            RaiseEvent DataStreamError(NegativeResponse.Write_To_Display_Row_Col_Address_Not_Valid)
                                                            Return False
                                                        End If

                                                    Case EmulatorScreen.WTD_Order.Erase_To_Address
                                                        Dim Row, Col, ListLen As Byte
                                                        Row = buf(start)
                                                        Col = buf(start + 1)
                                                        ListLen = buf(start + 2)
                                                        start += 3
                                                        Logger.Trace(vbTab & vbTab & "Address: (" & Row.ToString & ", " & Col.ToString & ")")
                                                        If (ListLen < 2) Or (ListLen > 5) Then
                                                            RaiseEvent DataStreamError(NegativeResponse.Write_Data_Length_Not_Valid)
                                                            Return False
                                                        End If
                                                        Dim AttributeTypes(ListLen - 2) As EmulatorScreen.Erase_To_Address_Attribute_Types
                                                        For i As Integer = 0 To AttributeTypes.Length - 1
                                                            AttributeTypes(i) = buf(start)
                                                            Logger.Trace(vbTab & vbTab & "Attribute Type: " & AttributeTypes(i).ToString)
                                                            start += 1
                                                            If (AttributeTypes(i) > EmulatorScreen.Erase_To_Address_Attribute_Types.Extended_Ideographic_Attributes) And
                                                                (AttributeTypes(i) <> EmulatorScreen.Erase_To_Address_Attribute_Types.All) Then
                                                                'XXX since we don't currently support extended attributes only .All should be allowed
                                                                RaiseEvent DataStreamError(NegativeResponse.Extended_Attribute_Type_Not_Valid)
                                                                Return False
                                                            End If
                                                        Next
                                                        If (Row > 0 And Row <= Me.Screen.Rows) And (Col > 0 And Col <= Me.Screen.Columns) Then
                                                            If Screen.GetTextBufferAddress(Row, Col) >= Screen.GetTextBufferAddress() Then
                                                                Me.Screen.EraseToAddress(Row, Col, AttributeTypes, True)
                                                                FieldsChanged = True 'XXX should get result from Screen.EraseToAddress instead
                                                                StringsChanged = True
                                                            Else
                                                                'There doesn't seem to be a negative response specific to this command, so...
                                                                RaiseEvent DataStreamError(NegativeResponse.Repeat_To_Address_Less_Than_Current_Address)
                                                                Return False
                                                            End If
                                                        Else
                                                            RaiseEvent DataStreamError(NegativeResponse.Write_To_Display_Row_Col_Address_Not_Valid)
                                                            Return False
                                                        End If

                                                    Case EmulatorScreen.WTD_Order.Start_Of_Header
                                                        '﻿The start of header (SOH) order specifies header information for the format table. When the 5494 receives this order, it first clears the format 	 
                                                        'table and then inserts the contents of the SOH order.

                                                        Dim hdr As New EmulatorScreen.StartOfHeader_Header(buf, start)
                                                        If (hdr.Length < 1) Or (hdr.Length > 7) Then
                                                            RaiseEvent DataStreamError(NegativeResponse.Start_Of_Header_Length_Not_Valid)
                                                            Return False
                                                        End If

                                                        start += hdr.Length + 1 'The length byte itself is not included in the length value, so add 1
                                                        Logger.Trace(vbTab & vbTab & "Length: " & hdr.Length.ToString)
                                                        Logger.Trace(vbTab & vbTab & "Flag: &H" & Hex(hdr.Flag))
                                                        Logger.Trace(vbTab & vbTab & "Starting Field for Reads: " & hdr.Starting_Field_For_Reads.ToString)
                                                        Logger.Trace(vbTab & vbTab & "Error row: " & hdr.Error_Row.ToString)
                                                        'Logger.Trace(vbTab & vbTab & "PFxx data inhibit bits: &H" & Hex(hdr.PF_Data_Inhibit))
                                                        Logger.Trace(vbTab & vbTab & "Inhibited PFxx keys: " & String.Join(",", hdr.Inhibited_AID_Codes))

                                                        Me.Screen.Header = hdr

                                                    Case EmulatorScreen.WTD_Order.Transparent_Data
                                                        'XXX this order has never been tested
                                                        If start > MaxOffs - 2 Then
                                                            RaiseEvent DataStreamError(NegativeResponse.Premature_Data_Stream_Termination)
                                                            Return False
                                                        End If
                                                        Dim len As Integer = (buf(start) << 8) + buf(start + 1)
                                                        start += 2
                                                        If start > MaxOffs - len Then
                                                            RaiseEvent DataStreamError(NegativeResponse.Premature_Data_Stream_Termination)
                                                            Return False
                                                        End If
                                                        For i As Integer = 0 To len - 1
                                                            If Me.Screen.GetTextBufferAddress > Me.Screen.TextBuffer.Length - 1 Then
                                                                RaiseEvent DataStreamError(NegativeResponse.Write_Past_End_Of_Display)
                                                                Return False
                                                            End If
                                                            Me.Screen.WriteTextBuffer(buf(start))
                                                            start += 1
                                                        Next
                                                        StringsChanged = True

                                                    Case EmulatorScreen.WTD_Order.Write_Extended_Attribute
                                                        'XXX we don't currently support extended attributes
                                                        Dim AttrType As Byte = buf(start)
                                                        Dim Attr As Byte = buf(start + 1)
                                                        start += 2
                                                        Logger.Trace(vbTab & vbTab & "Attribute Type: &H" & Hex(AttrType) & "(unimplemented)")
                                                        Logger.Trace(vbTab & vbTab & "Attribute: &H" & Hex(Attr) & "(unimplemented)")
                                                        RaiseEvent DataStreamError(NegativeResponse.Extended_Attribute_Type_Not_Valid)
                                                        Return False

                                                    Case EmulatorScreen.WTD_Order.Start_Of_Field
                                                        'The 5494 can accommodate up to 256 input fields for each workstation. The maximum number of fields is not restricted by the number of FCWs
                                                        'defined. The 5494 processes each SF order in the following manner:
                                                        '
                                                        'Upon detection of the SF code, the 5494 first sets the SF address. This address is either determined by the contents of a preceding SBA
                                                        'order or calculated from the field length parameter in the last SF order received.
                                                        '
                                                        'The 5494 examines the first 2 bits after the code to identify which word is present. If an attribute follows immediately after the code, an output
                                                        'field is being defined, and such fields require no entry in the format table.
                                                        '
                                                        'If the 5494 detects an FFW in the SF order, it takes the following actions:
                                                        '- Locks the keyboard.
                                                        '- Clears Insert mode and the corresponding display on the keyboard.
                                                        '- Clears Command mode and any outstanding AID request.
                                                        '- Examines the format table for an entry that begins at the current starting address plus 1. If such an entry is detected, the 5494 modifies the
                                                        '  existing field in accordance with the new FFW and leading field attribute.
                                                        '- If no field previously existed at the current starting address, and this field is after the last field on the display screen, the 5494 enters a new
                                                        '  field in the format table in accordance with the FFW, and writes the leading and ending field attributes.
                                                        Dim hdr As New EmulatorScreen.StartOfField_Header(buf, start, Screen.DefaultForeColor, Screen.BackColor)
                                                        start += hdr.Length
                                                        Logger.Trace(vbTab & vbTab & "Length: " & hdr.Length.ToString)

                                                        Logger.Trace(vbTab & vbTab & "Field Format Word: &H" & Hex(hdr.FieldFormatWord.Raw))
                                                        Logger.Trace(vbTab & vbTab & "    Bypass: " & hdr.FieldFormatWord.Bypass.ToString)
                                                        Logger.Trace(vbTab & vbTab & "    Dup or Field Mark Enable: " & hdr.FieldFormatWord.Dup_Or_Field_Mark_Enabled.ToString)
                                                        Logger.Trace(vbTab & vbTab & "    Modified Data Tag: " & hdr.FieldFormatWord.Modified.ToString)
                                                        Logger.Trace(vbTab & vbTab & "    Field Shift/Edit Spec: " & hdr.FieldFormatWord.Shift_Edit_Spec.ToString)
                                                        Logger.Trace(vbTab & vbTab & "    Auto Enter: " & hdr.FieldFormatWord.Auto_Enter_On_Exit.ToString)
                                                        Logger.Trace(vbTab & vbTab & "    Field Exit Required: " & hdr.FieldFormatWord.Field_Exit_Key_Required.ToString)
                                                        Logger.Trace(vbTab & vbTab & "    Monocase: " & hdr.FieldFormatWord.UpperCase.ToString)
                                                        Logger.Trace(vbTab & vbTab & "    Mandatory Enter: " & hdr.FieldFormatWord.Mandatory_Enter.ToString)
                                                        Logger.Trace(vbTab & vbTab & "    Mandatory Fill: " & hdr.FieldFormatWord.Mandatory_Fill.ToString)

                                                        For Each fcw As EmulatorScreen.StartOfField_Header.FCW In hdr.FieldControlWords
                                                            Logger.Trace(vbTab & vbTab & "Field Control Word: &H" & Hex(fcw.Raw))
                                                            Logger.Trace(vbTab & vbTab & "    Type: " & fcw.Type.ToString)
                                                            Select Case fcw.Type
                                                                Case EmulatorScreen.StartOfField_Header.FCW_Type.Highlighted
                                                                    Dim a As New EmulatorScreen.FieldAttribute(fcw.Data)
                                                                    Logger.Trace(vbTab & vbTab & "    Attribute: " & a.Attribute.ToString)
                                                                Case Else
                                                                    Logger.Trace(vbTab & vbTab & "    Data: &H" & Hex(fcw.Data))
                                                            End Select
                                                            'The first FCW of any type is used. Subsequent FCWs of the same type are ignored. 
                                                            'The 5494 does not check to determine if the FCWs are formatted correctly or if 
                                                            'the requested function is installed. 
                                                            'The 5494 can detect and report these errors to the AS/400 system if the FCW is
                                                            'required during subsequent command and keystroke processing.

                                                            'These lines removed due to above information from SC30-3533-04.
                                                            '
                                                            'If (fcw >> 8) = &H82 Then 'it's an ideographic field
                                                            '    RaiseEvent DataStreamError(NegativeResponse.Start_Of_Field_Attribute_Not_Valid)
                                                            '    Return False
                                                            'End If
                                                        Next
                                                        Logger.Trace(vbTab & vbTab & "Leading Field Attribute: " & hdr.LeadingFieldAttribute.Attribute.ToString)
                                                        Logger.Trace(vbTab & vbTab & "Field Length: " & hdr.FieldLength.ToString)
                                                        Logger.Trace(vbTab & vbTab & "Is Input Field: " & hdr.IsInputField.ToString)

                                                        'It's OK to start a field at (1,0) as a special case per documentation.
                                                        'It's OK to start a field on the last column because the first byte is the leading attribute, so the field will start on the next line.
                                                        If ((Screen.Row = 1) And (Screen.Column = 0)) OrElse
                                                            (((Screen.Row > 0) And (Screen.Column > 0)) And ((Screen.Row <= Screen.Rows) And (Screen.Column <= Screen.Columns))) Then
                                                            Select Case hdr.FieldFormatWord.Shift_Edit_Spec
                                                                Case EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.AlphaOnly, EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.AlphaShift
                                                                    If hdr.FieldLength < 1 Then
                                                                        RaiseEvent DataStreamError(NegativeResponse.Start_Of_Field_Length_Not_Valid)
                                                                        Return False
                                                                    End If
                                                                Case EmulatorScreen.StartOfField_Header.FFW_ShiftEditSpec.SignedNumeric
                                                                    If hdr.FieldLength < 2 Then
                                                                        RaiseEvent DataStreamError(NegativeResponse.Start_Of_Field_Length_Not_Valid)
                                                                        Return False
                                                                    End If
                                                                Case Else
                                                                    'XXX The documentation doesn't specify limits for any other field type.  Is 0 ok?
                                                            End Select

                                                            Handle_Text(" ", hdr.LeadingFieldAttribute)
                                                            Me.Screen.WriteTextBuffer(hdr.LeadingFieldAttribute.Attribute)

                                                            'XXX this is a crappy workaround for wrapping fields
                                                            If (Me.Screen.Column + hdr.FieldLength - 1) > Me.Screen.Columns Then
                                                                hdr.FieldLength = Me.Screen.Columns - Me.Screen.Column - 1
                                                            End If


                                                            If (Me.Screen.Column + hdr.FieldLength - 1) > Me.Screen.Columns Then
                                                                'RaiseEvent DataStreamError(NegativeResponse.Start_Of_Field_Length_Not_Valid)
                                                                RaiseEvent DataStreamError(NegativeResponse.Field_Extends_Past_End_Of_Display)
                                                                Return False
                                                            End If

                                                            If hdr.IsInputField Then Screen.UpdateField(hdr, Nothing, True) 'XXX if it's not an input field, what is it for?

                                                            FieldsChanged = True
                                                        Else
                                                            RaiseEvent DataStreamError(NegativeResponse.Start_Of_Field_Address_Not_Valid)
                                                            Return False
                                                        End If

                                                    Case EmulatorScreen.WTD_Order.Write_To_Display_Structured_Field
                                                        Dim sf As New EmulatorScreen.StructuredField(buf, start, Me.Screen)
                                                        Logger.Trace(vbTab & vbTab & "Length: " & sf.Length.ToString)
                                                        Logger.Trace(vbTab & vbTab & "Command: " & sf.Command.ToString)
                                                        Select Case sf.Command
                                                            Case EmulatorScreen.StructuredField.WSFCommand.Define_Selection_Field
                                                                'XXX we don't currently support selection fields
                                                                RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Type_Not_Valid)
                                                                Return False
                                                            Case EmulatorScreen.StructuredField.WSFCommand.Create_Window
                                                                If sf.Length > 8 Then
                                                                    Dim cwh As New EmulatorScreen.StructuredField.CreateWindowHeader(buf, start + (sf.Length - sf.Data.Length), sf.Data.Length, sf)
                                                                    Logger.Trace(vbTab & vbTab & vbTab & "Rows: " & cwh.Rows.ToString)
                                                                    Logger.Trace(vbTab & vbTab & vbTab & "Columns: " & cwh.Columns.ToString)
                                                                    Logger.Trace(vbTab & vbTab & vbTab & "IsPullDownMenuBar: " & cwh.IsPullDownMenuBar.ToString)
                                                                    Logger.Trace(vbTab & vbTab & vbTab & "CursorRestrictedToWindow: " & cwh.CursorRestrictedToWindow.ToString)
                                                                    Logger.Trace(vbTab & vbTab & vbTab & "Minor Structures: " & cwh.MinorStructures.Length.ToString)

                                                                    Dim RemainingRows As Integer = Me.Screen.Rows - Me.Screen.Row
                                                                    Dim RemainingCols As Integer = Me.Screen.Columns - Me.Screen.Column
                                                                    If (cwh.Rows < 1) Or (cwh.Rows > RemainingRows - 1) Or
                                                                        (cwh.Columns < 1) Or (cwh.Columns > RemainingCols - 3) Then
                                                                        RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Parameter_Not_Valid)
                                                                        Return False
                                                                    End If

                                                                    For i As Integer = 0 To cwh.MinorStructures.Length - 1
                                                                        Select Case cwh.MinorStructures(i).GetType
                                                                            Case GetType(Emulator.EmulatorScreen.StructuredField.CreateWindowHeader.WindowTitle_Or_Footer)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & "Window Title or Footer:")
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "Length: " & cwh.MinorStructures(i).Length.ToString)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "Orientation: " & cwh.MinorStructures(i).Orientation.ToString)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "Element: " & cwh.MinorStructures(i).Element.ToString)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "Monochrome Attribute: " & cwh.MinorStructures(i).MonochromeAttribute.Attribute.ToString)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "Color Attribute: " & cwh.MinorStructures(i).ColorAttribute.Attribute.ToString)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "Text: " & cwh.MinorStructures(i).Text.ToString)
                                                                                If (cwh.MinorStructures(i).Length < 7) Then
                                                                                    RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Minor_Structure_Length_Not_Valid)
                                                                                    Return False
                                                                                End If
                                                                            Case GetType(Emulator.EmulatorScreen.StructuredField.CreateWindowHeader.BorderPresentation)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & "Border Presentation:")
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "Length: " & cwh.MinorStructures(i).Length.ToString)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "UseBorderPresentationCharacters: " & cwh.MinorStructures(i).UseBorderPresentationCharacters.ToString)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "Element: " & cwh.MinorStructures(i).Element.ToString)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "Monochrome Attribute: " & cwh.MinorStructures(i).MonochromeAttribute.Attribute.ToString)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "Color Attribute: " & cwh.MinorStructures(i).ColorAttribute.Attribute.ToString)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "TopLeftChar: " & cwh.MinorStructures(i).TopLeftChar)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "TopChar: " & cwh.MinorStructures(i).TopChar)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "TopRightChar: " & cwh.MinorStructures(i).TopRightChar)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "LeftChar: " & cwh.MinorStructures(i).LeftChar)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "RightChar: " & cwh.MinorStructures(i).RightChar)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "BottomLeftChar: " & cwh.MinorStructures(i).BottomLeftChar)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "BottomChar: " & cwh.MinorStructures(i).BottomChar)
                                                                                Logger.Trace(vbTab & vbTab & vbTab & vbTab & "BottomRightChar: " & cwh.MinorStructures(i).BottomRightChar)
                                                                                If (cwh.MinorStructures(i).Length < 4) Or (cwh.MinorStructures(i).Length > 13) Then
                                                                                    RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Minor_Structure_Length_Not_Valid)
                                                                                    Return False
                                                                                End If
                                                                            Case Else
                                                                                Logger.Trace(vbTab & vbTab & vbTab & "Unknown Minor Structure type!")
                                                                                RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Minor_Structure_Parameter_Not_Valid)
                                                                                Return False
                                                                        End Select
                                                                    Next

                                                                    If cwh.IsPullDownMenuBar Then
                                                                        'XXX we don't currently support pulldown menus
                                                                        RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Parameter_Not_Valid)
                                                                        Return False
                                                                    Else
                                                                        Me.Screen.AddPopup(cwh)
                                                                    End If
                                                                Else
                                                                    RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Length_Not_Valid)
                                                                    Return False
                                                                End If
                                                            Case EmulatorScreen.StructuredField.WSFCommand.Unrestricted_Window_Cursor_Movement
                                                                'This command is currently meaningless to us.  Perhaps it could be used (along with CreateWindowHeader.CursorRestrictedToWindow) 
                                                                '   to control whether the popup is modal or not.
                                                            Case EmulatorScreen.StructuredField.WSFCommand.Define_Scroll_Bar_Field
                                                                'XXX we don't currently support scroll bar fields
                                                                RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Type_Not_Valid)
                                                                Return False
                                                            Case EmulatorScreen.StructuredField.WSFCommand.Write_Data
                                                                'XXX we don't currently support Write_Data.  This should be implemented at the same time as wrap fields and continuation fields.
                                                                RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Type_Not_Valid)
                                                                Return False
                                                            Case EmulatorScreen.StructuredField.WSFCommand.Programmable_Mouse_Buttons
                                                                'XXX we don't currently support programmable mouse buttons
                                                                RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Type_Not_Valid)
                                                                Return False
                                                            Case EmulatorScreen.StructuredField.WSFCommand.Remove_GUI_Selection_Field
                                                                'XXX we don't currently support Define_Selection_Field, so we should never get this.  
                                                                '   Ignore it rather than returning a negative response.
                                                            Case EmulatorScreen.StructuredField.WSFCommand.Remove_GUI_Window
                                                                'When using the REMOVE GUI WINDOW structured field, the current display address must be the same as the address used when the window
                                                                'was created by a CREATE WINDOW structured field. The 5494 attempts to find a match (current display address and a GUI window). When a
                                                                'match is found, the GUI construct is removed, but the display screen is not cleared.
                                                                'XXX This operation has not been tested.
                                                                For Each pop As KeyValuePair(Of String, IBM5250.Emulator.EmulatorScreen.EmulatorPopup) In Me.Screen.PopUps
                                                                    If pop.Value.Top = Screen.Row And pop.Value.Left = Screen.Column Then
                                                                        Me.Screen.RemovePopup(pop.Key)
                                                                    End If
                                                                Next
                                                            Case EmulatorScreen.StructuredField.WSFCommand.Remove_GUI_Scroll_Bar_Field
                                                                'XXX we don't currently support Define_Scroll_Bar_Field, so we should never get this.  
                                                                '   Ignore it rather than returning a negative response.
                                                            Case EmulatorScreen.StructuredField.WSFCommand.Remove_All_GUI_Constructs
                                                                'When this structured field is issued, the 5494 and PWS:
                                                                'Process all of the functions of a REMOVE GUI WINDOW structured field against all of the GUI windows on the display screen (see "REMOVE
                                                                'GUI WINDOW Structured Field" in topic 15.6.13.3).
                                                                'Process all of the functions of a REMOVE GUI SELECTION FIELD structured field against all of the GUI selection fields on the display screen
                                                                '(see "REMOVE GUI SELECTION FIELD Structured Field" in topic 15.6.13.6).
                                                                'Process all of the functions of a REMOVE GUI SCROLL BAR FIELD structured field against all of the GUI selection fields on the display
                                                                'screen (see "REMOVE GUI SCROLL BAR FIELD Structured Field" in topic 15.6.13.8).
                                                                '
                                                                'XXX Since we currently only support GUI windows, that's all we can remove.
                                                                For Each pop As KeyValuePair(Of String, IBM5250.Emulator.EmulatorScreen.EmulatorPopup) In Me.Screen.PopUps
                                                                    Me.Screen.RemovePopup(pop.Key)
                                                                Next
                                                            Case Else
                                                                'XXX more structured field types to deal with here
                                                                RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Type_Not_Valid)
                                                                Return False
                                                        End Select

                                                        start += sf.Length

                                                    Case Else
                                                        RaiseEvent DataStreamError(NegativeResponse.Command_Not_Valid)
                                                        Return False
                                                End Select
                                                Logger.Trace(vbTab & "----------")
                                            Else
                                                Logger.Trace(vbTab & "ERROR: Unknown WTD Order: &H" & Hex(b))
                                                RaiseEvent DataStreamError(NegativeResponse.Command_Not_Valid)
                                                Return False
                                            End If

                                        End If

                                        'If (Not ThisByteWasText) And (text IsNot Nothing) Then
                                        '    Logger.Trace(vbTab & "Text: '" & text & "'")
                                        '    text = Nothing
                                        'End If

                                    Loop
                                    Handle_Text(text, CurrentAttribute)

                                    'XXX if we're in an error state, defer this cursor move until after the user hits the error reset key.
                                    If Got_InsertCursor Then
                                        If cc.Flags.Post.Cursor_Moves_When_Keyboard_Unlocks Then
                                            Me.Screen.Row = Me.Screen.HomeCoordinates.Row
                                            Me.Screen.Column = Me.Screen.HomeCoordinates.Column
                                        End If

                                    Else
                                        Dim LowestAddress As Integer = Integer.MaxValue
                                        For idx As Integer = 0 To Me.Screen.Fields.Length - 1
                                            If Me.Screen.Fields(idx).Allocated Then
                                                If Not Me.Screen.Fields(idx).Flags.Bypass Then
                                                    If Me.Screen.Fields(idx).Location.Position < LowestAddress Then
                                                        If Me.Screen.Fields(idx).PopupKey = Me.Screen.PopupKeyOfAddress(Me.Screen.Row, Me.Screen.Column) Then
                                                            LowestAddress = Me.Screen.Fields(idx).Location.Position
                                                            Me.Screen.Row = Me.Screen.Fields(idx).Location.Row
                                                            Me.Screen.Column = Me.Screen.Fields(idx).Location.Column
                                                        End If
                                                    End If
                                                End If
                                            Else
                                                Exit For
                                            End If
                                        Next
                                        If LowestAddress = Integer.MaxValue Then 'no non-bypass fields are allocated
                                            Me.Screen.Row = 1
                                            Me.Screen.Column = 1
                                        End If
                                    End If

                                    If StringsChanged Then
                                        Screen.UpdateStrings(False)
                                    End If
                                    Screen.UpdateFieldValues()

                                    'Prevent FieldsChanged() handler from moving the cursor
                                    Dim rw As Integer = Me.Screen.Row
                                    Dim cl As Integer = Me.Screen.Column
                                    If FieldsChanged Then
                                        RaiseEvent FieldsChanged()
                                    End If
                                    RaiseEvent CursorPositionSet(rw, cl)
                                    '

                                    If cc.Flags.Post.Unlock_Keyboard_And_Reset_Pending_AID Then
                                        Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.Normal_Unlocked
                                    End If

                                    If cc.Flags.Post.Set_Message_Waiting_Indicator_Off Then
                                        RaiseEvent MessageWaitingChanged(False)
                                    End If
                                    If cc.Flags.Post.Set_Message_Waiting_Indicator_On Then
                                        RaiseEvent MessageWaitingChanged(True)
                                    End If
                                End If
                            Case Command.READ_MDT_FIELDS
                                Me.Invited = True 'RFC1205 says "A work station is said to be 'invited' when the server has sent a read command to the client."
                                Dim cc As New EmulatorScreen.ControlCharacter(buf(start), buf(start + 1))
                                start += 2
                                Logger.Trace(vbTab & "---Pre flags---")
                                Logger.Trace(vbTab & "Non-Stream Data: " & cc.Flags.Pre.Non_Stream_Data.ToString)
                                Logger.Trace(vbTab & "Null all non-bypass fields: " & cc.Flags.Pre.Null_All_Non_Bypass_Fields.ToString)
                                Logger.Trace(vbTab & "Null all non-bypass fields with MDT on: " & cc.Flags.Pre.Null_All_Non_Bypass_Fields_With_MDT_On.ToString)
                                Logger.Trace(vbTab & "Reset MDT flags in all fields: " & cc.Flags.Pre.Reset_MDT_Flags_In_All_Fields.ToString)
                                Logger.Trace(vbTab & "Reset MDT flags in non-bypass fields: " & cc.Flags.Pre.Reset_MDT_Flags_In_Non_Bypass_Fields.ToString)
                                Logger.Trace(vbTab & "Reset pending AID and lock keyboard: " & cc.Flags.Pre.Reset_Pending_AID_And_Lock_Keyboard.ToString)
                                Logger.Trace(vbTab & "---Post flags---")
                                Logger.Trace(vbTab & "Cursor moves when keyboard unlocks: " & cc.Flags.Post.Cursor_Moves_When_Keyboard_Unlocks.ToString)
                                Logger.Trace(vbTab & "Reset blinking cursor: " & cc.Flags.Post.Reset_Blinking_Cursor.ToString)
                                Logger.Trace(vbTab & "Set blinking cursor: " & cc.Flags.Post.Set_Blinking_Cursor.ToString)
                                Logger.Trace(vbTab & "Set message waiting indicator off: " & cc.Flags.Post.Set_Message_Waiting_Indicator_Off.ToString)
                                Logger.Trace(vbTab & "Set message waiting indicator on: " & cc.Flags.Post.Set_Message_Waiting_Indicator_On.ToString)
                                Logger.Trace(vbTab & "Sound alarm: " & cc.Flags.Post.Sound_Alarm.ToString)
                                Logger.Trace(vbTab & "Reset pending AID and unlock keyboard: " & cc.Flags.Post.Unlock_Keyboard_And_Reset_Pending_AID.ToString)

                                '﻿The format of the control character following the READ MDT FIELDS command is identical to that in the WTD command. The 5494 completes 	 
                                'the actions indicated by this character after servicing the READ MDT FIELDS command. 	 
                                'When the 5494 receives a READ MDT FIELDS command, the command remains pending until the workstation operator presses an AID key. 	 
                                'Once this requirement has been satisfied, the 5494 services the command and clears the AID request immediately or upon receipt of the next CD 	 
                                'bit. 	 
                                'The format of data returned in response to a READ MDT FIELDS command is: 	 
                                '+------------------------------------------------------------------------+ 	 
                                '¦ Cursor   ¦ 	AID 	¦ 	SBA 	¦ 	Field 	¦ 	Field   ¦ 	SBA 	¦ 	Field   ¦ 	Field   ¦ 	 
                                '¦ 	Row/ 	¦ 	Code 	¦ 	        ¦ 	Row/ 	¦ 	Data 	¦ 	        ¦ 	Row/ 	¦ 	Data 	¦ 	 
                                '¦ 	        ¦ 	        ¦ 	X'11'   ¦ 	Column  ¦ 	        ¦ 	X'11'   ¦ Column    ¦ 	        ¦ 	 
                                '¦ Column   ¦ 1 byte    ¦ 	        ¦ 	        ¦ 	        ¦ 	        ¦ 	        ¦ 	        ¦ 	 
                                '¦ 	        ¦ 	        ¦ 	        ¦ 2 bytes   ¦ 	        ¦ 	        ¦ 	2 	    ¦ 	        ¦ 	 
                                '¦ 	2 	    ¦ 	        ¦ 	        ¦ 	        ¦ 	        ¦ 	        ¦ 	bytes   ¦ 	        ¦ 	 
                                '¦ 	bytes   ¦ 	        ¦ 	        ¦ 	        ¦ 	        ¦ 	        ¦ 	        ¦ 	        ¦ 	 
                                '+------------------------------------------------------------------------+ 	 
                                'Field data is returned only when at least one field has its MDT set to ON and the AID is one of the following keys: 	 
                                '   Enter or Record Advance 	 
                                '   Roll(Up / Down)
                                '   PF 1--24 (unless masked in SOH order) 
                                '
                                'If field data is not returned, only the cursor address and AID code are returned. 	 
                                'The cursor address and AID code fields contain the same information as an input data stream for a READ INPUT FIELDS command. However, 	 
                                'because this data stream only includes those fields having the MDT bit on, the 5494 inserts an SBA code and the field address before the field 	 
                                'data. The SBA codes serve as a delimiter between successive fields, and the field addresses enable the AS/400 system to determine which fields 	 
                                'are included in the transmission. 


                                'XXX
                                Screen.Read.Command = Cmd
                                Screen.Read.Pending = True

                            Case Command.WRITE_STRUCTURED_FIELD
                                Do
                                    Dim sf As New EmulatorScreen.StructuredField(buf, start, Me.Screen)
                                    start += sf.Length

                                    Logger.Trace(vbTab & "Length: " & sf.Length.ToString)
                                    Logger.Trace(vbTab & "Command: " & sf.Command.ToString)

                                    Select Case sf.Command
                                        Case EmulatorScreen.StructuredField.WSFCommand.Query
                                            If sf.Length <> 5 Then
                                                RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Length_Not_Valid)
                                                Return False
                                            Else
                                                Dim Response As New EmulatorScreen.StructuredField(Me.Screen)
                                                Response.Command = EmulatorScreen.StructuredField.WSFCommand.Query
                                                ReDim Response.Flags(0)
                                                Response.Flags(0) = &H80 'Is a query response

                                                Dim qr As New EmulatorScreen.StructuredField.QueryResponse(Response)
                                                qr.TerminalType = Me.TerminalType
                                                qr.TerminalModel = Me.TerminalModel
                                                If Me.ScreenSize27x132_Allowed Then _
                                                    qr.DisplayCapabilities = qr.DisplayCapabilities Or
                                                    EmulatorScreen.StructuredField.QueryResponse.Device_Capabilities_Byte1_Bits.ScreenSize_27x132
                                                Response.Data = qr.ToBytes

                                                Dim ReplyData() As Byte = Response.ToBytes
                                                Dim Reply(3 + ReplyData.Length - 1) As Byte
                                                Reply(0) = 0 'Row
                                                Reply(1) = 0 'Col
                                                Reply(2) = &H88 'Fake AID code to indicate QUERY reply
                                                Array.Copy(ReplyData, 0, Reply, 3, ReplyData.Length)
                                                RaiseEvent DataReady(Reply, IBM5250.TN5250.OpCodes.PutOrGet)
                                            End If
                                        Case Else
                                            'XXX
                                            Logger.Trace(vbTab & "***UNIMPLEMENTED***")
                                            RaiseEvent DataStreamError(NegativeResponse.Structured_Field_Type_Not_Valid)
                                            Return False
                                    End Select
                                Loop While (start <= MaxOffs) AndAlso (buf(start) <> Emulator.ESC)

                            Case Command.WRITE_ERROR_CODE, Command.WRITE_ERROR_CODE_TO_WINDOW
                                'XXX there's a lot more to think about here
                                'XXX WRITE_ERROR_CODE_TO_WINDOW should behave differently, but this is how Client Access does it and it's easy.
                                Logger.Trace(vbTab & "----------")
                                Select Case Cmd
                                    Case Command.WRITE_ERROR_CODE
                                        If buf(start) = EmulatorScreen.WTD_Order.Insert_Cursor Then
                                            Screen.Row = buf(start + 1) 'XXX this command is not supposed to alter the address set by Write To Display.
                                            Screen.Column = buf(start + 2)
                                            start += 2
                                        End If
                                        Logger.Trace(vbTab & "Insert Cursor: " & Screen.Row & "," & Screen.Column)
                                    Case Command.WRITE_ERROR_CODE_TO_WINDOW
                                        Dim StartColumn As Byte = buf(start)
                                        Dim EndColumn As Byte = buf(start + 1)
                                        start += 2
                                        Logger.Trace(vbTab & "Start Column: " & StartColumn.ToString)
                                        Logger.Trace(vbTab & "End Column: " & EndColumn.ToString)
                                        'There are two negative responses associated with this command:
                                        '   NegativeResponse.Write_Error_Code_To_Window_Not_Valid_With_Current_Error_Line
                                        '   NegativeResponse.Write_Error_Code_To_Window_Row_Col_Address_Not_Valid
                                        'We're currently accepting anything and writing it to the error line just like WRITE_ERROR_CODE.
                                End Select

                                Dim ErrorText As String = ""
                                Dim Got_Trailing_Attribute As Boolean = False
                                Dim error_text_start As Integer = start
                                For i As Integer = error_text_start To MaxOffs
                                    If buf(i) = ESC Then
                                        Exit For
                                    ElseIf IsAttribute(buf(i)) Then
                                        Dim a As New EmulatorScreen.FieldAttribute(buf(i), Me.Screen.DefaultForeColor, Screen.BackColor)
                                        Logger.Trace(vbTab & "Attribute: " & a.Attribute.ToString)
                                        start += 1
                                        If i > error_text_start Then
                                            Got_Trailing_Attribute = True
                                        End If
                                        'XXX need to apply the attribute to the error textbox
                                    Else
                                        Dim e As Byte = EBCDIC_To_UTF8(New Byte() {buf(i)})(0)

                                        'sometimes the AS400 sends strange characters following the trailing attribute, so ignore them.
                                        'XXX are the strange characters supposed to control the OIA display?
                                        If Not Got_Trailing_Attribute Then ErrorText += ChrW(e)

                                        start += 1
                                    End If
                                Next
                                Screen.ErrorText = ErrorText
                                RaiseEvent ErrorTextChanged()

                                Logger.Trace(vbTab & "Error: " & ErrorText)
                                Logger.Trace(vbTab & "----------")

                            Case Command.READ_SCREEN
                                If (Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.SS_Message) Or
                                    (Me.Keyboard.State = EmulatorKeyboard.Keyboard_State.System_Request) Then
                                    RaiseEvent DataStreamError(NegativeResponse.Command_Not_Valid)
                                    Return False
                                Else
                                    'Me.Invited = True 'RFC1205 says "A work station is said to be 'invited' when the server has sent a read command to the client."
                                    Dim Reply(Screen.TextBuffer.Length - 1) As Byte
                                    Array.Copy(Screen.TextBuffer, 0, Reply, 0, Reply.Length)
                                    RaiseEvent DataReady(Reply, IBM5250.TN5250.OpCodes.ReadScreen)
                                End If

                            Case Command.SAVE_SCREEN, Command.SAVE_PARTIAL_SCREEN
                                'XXX Need to check docs for SAVE_SCREEN requirements.  We're probably missing a few things here.

                                'XXX We should serialize any open popups here.
                                '    Instead, for now, close them all.
                                'Dim pops As New List(Of String)
                                'For Each kvp As KeyValuePair(Of String, IBM5250.Emulator.EmulatorScreen.EmulatorPopup) In Me.Screen.PopUps
                                '    pops.Add(kvp.Key)
                                'Next
                                'For Each key As String In pops
                                '    Me.Screen.RemovePopup(key)
                                'Next
                                'pops = Nothing

                                Dim Reply As New System.IO.MemoryStream
                                Reply.WriteByte(ESC)
                                Select Case Cmd
                                    Case Command.SAVE_SCREEN
                                        Reply.WriteByte(Command.RESTORE_SCREEN)
                                    Case Command.SAVE_PARTIAL_SCREEN
                                        Reply.WriteByte(Command.RESTORE_PARTIAL_SCREEN)
                                        'XXX SAVE_PARTIAL_SCREEN should save the screen data and queue it to be sent to the AS400 after processing the rest of the data stream.
                                        Dim Flag As Byte = buf(start)
                                        Logger.Trace(vbTab & "Flag: " & Flag.ToString)
                                        Dim TopRow As Byte = buf(start + 1)
                                        Logger.Trace(vbTab & "TopRow: " & TopRow.ToString)
                                        Dim LeftColumn As Byte = buf(start + 2)
                                        Logger.Trace(vbTab & "LeftColumn: " & LeftColumn.ToString)
                                        Dim WindowDepth As Byte = buf(start + 3)
                                        Logger.Trace(vbTab & "WindowDepth: " & WindowDepth.ToString)
                                        Dim WindowWidth As Byte = buf(start + 4)
                                        Logger.Trace(vbTab & "WindowWidth: " & WindowWidth.ToString)
                                        start += 5
                                End Select

                                'Arbitrary data structure for reply.  We'll need to be able to restore this later.
                                Reply.WriteByte(Me.Screen.Row)
                                Reply.WriteByte(Me.Screen.Column)

                                'serialize the fields array
                                Dim js As New System.Web.Script.Serialization.JavaScriptSerializer
                                Dim FieldCount As Integer = 0
                                For i As Integer = 0 To Screen.Fields.Length - 1
                                    If Screen.Fields(i).Allocated Then
                                        FieldCount += 1
                                    Else
                                        Exit For
                                    End If
                                Next
                                Dim f(FieldCount - 1) As IBM5250.Emulator.EmulatorScreen.Field
                                Array.Copy(Screen.Fields, 0, f, 0, f.Length)
                                Dim s As String = js.Serialize(f)

                                s = StringCompressor.Compress(s)

                                Dim b() As Byte = System.Text.Encoding.UTF8.GetBytes(s)

                                Dim FieldsLength() As Byte = BitConverter.GetBytes(b.Length) 'Array.Length is an int32
                                Reply.Write(FieldsLength, 0, FieldsLength.Length)

                                Reply.Write(b, 0, b.Length)

                                Reply.Write(Me.Screen.TextBuffer, 0, Me.Screen.TextBuffer.Length)  'XXX this is supposed to be compressed in a specific way

                                RaiseEvent DataReady(Reply.ToArray, TN5250.OpCodes.SaveScreen)

                            Case Command.RESTORE_SCREEN, Command.RESTORE_PARTIAL_SCREEN
                                'XXX Need to check docs for RESTORE_SCREEN requirements.  We're probably missing a few things here.

                                'XXX SC30-3533-04 Page 15.24-1 states that there are 2 bytes here holding the length of the data to restore.  That is empirically false.
                                'If Cmd = Command.RESTORE_PARTIAL_SCREEN Then
                                '    Dim DataLen As UShort = (CUShort(buf(start)) << 8) + buf(start + 1)
                                '    start += 2
                                '    DataLen -= 2 'The 2 length bytes are included in the length value.
                                'End If

                                If (start + Me.Screen.TextBuffer.Length + 2 - 1) <= MaxOffs Then
                                    Dim r As Byte = buf(start)
                                    Dim c As Byte = buf(start + 1)
                                    start += 2

                                    Dim FieldsLength As Int32 = BitConverter.ToInt32(buf, start)
                                    start += 4 'int32

                                    Dim f(FieldsLength - 1) As Byte
                                    Array.Copy(buf, start, f, 0, f.Length)
                                    start += FieldsLength
                                    Dim s As String = System.Text.Encoding.UTF8.GetString(f)

                                    s = StringCompressor.Decompress(s)

                                    Dim js As New System.Web.Script.Serialization.JavaScriptSerializer
                                    Dim Fields() As IBM5250.Emulator.EmulatorScreen.Field = js.Deserialize(Of IBM5250.Emulator.EmulatorScreen.Field())(s)

                                    'For some reason the deserializer fails on System.Drawing.Color, so we have to fix it up here.
                                    For i As Integer = 0 To Fields.Length - 1
                                        Fields(i).Attribute = New IBM5250.Emulator.EmulatorScreen.FieldAttribute(Fields(i).Attribute.Attribute, Screen.DefaultForeColor, Screen.BackColor)
                                    Next

                                    Array.Copy(Fields, 0, Screen.Fields, 0, Fields.Length)

                                    Array.Copy(buf, start, Me.Screen.TextBuffer, 0, Me.Screen.TextBuffer.Length)
                                    start += Me.Screen.TextBuffer.Length

                                    Screen.UpdateStrings(False)
                                    Screen.UpdateFieldValues()
                                    RaiseEvent FieldsChanged()
                                    Me.Screen.Row = r
                                    Me.Screen.Column = c
                                    RaiseEvent CursorPositionSet(r, c)

                                Else
                                    RaiseEvent DataStreamError(NegativeResponse.Restore_Data_Not_Valid)
                                    Return False
                                End If
                            Case Command.UNDOCUMENTED_1
                                Logger.Trace(vbTab & "***Undocumented command ignored***")
                                'XXX All known occurrences of this command have contained no additional bytes, but since it's undocumented, read until the next ESC.  
                                Do While (start <= MaxOffs) AndAlso (buf(start) <> Emulator.ESC)
                                    start += 1
                                Loop
                            Case Else
                                RaiseEvent DataStreamError(NegativeResponse.Command_Not_Valid)
                                Return False
                        End Select
                    ElseIf buf(start) = &HC0 Then
                        If Length = 62 Then 'looks like a startup response record, poorly documented in RFC4777.
                            'first 5 bytes of response record should be &HC0 00 3D 00 00
                            start += 5
                            'next 4 bytes are the response code as an EBCDIC string
                            Dim RespCode(3) As Byte
                            Array.Copy(buf, start, RespCode, 0, RespCode.Length)
                            start += 4
                            RespCode = EBCDIC_To_UTF8(RespCode)
                            Dim RespCodeString As String = System.Text.Encoding.UTF8.GetString(RespCode)
                            RaiseEvent StartupResponseReceived(RespCodeString)
                            'next 8 bytes are the hostname of the AS400 in EBCDIC
                            start += 8
                            'next 10 bytes are the workstation name in EBCDIC
                            start += 10
                            'remaining bytes are zeros
                            start += 35
                        End If
                    Else
                        RaiseEvent DataStreamError(NegativeResponse.Escape_Code_Expected_But_Not_Found)
                        Return False
                    End If
                Loop While start <= MaxOffs 'start < length
                'Return pds.PDS_OKAY_NO_OUTPUT 'XXX what does this do vs. PDS_OKAY_OUTPUT?
                Return True
            Catch ex As IndexOutOfRangeException
                'This is just a guess.  This exception could be due to a bug instead.
                RaiseEvent DataStreamError(NegativeResponse.Premature_Data_Stream_Termination)
                Return False
            Catch ex As Exception
                Logger.Error(ex.Message, ex)
                MsgBox(ex.Message, MsgBoxStyle.Critical, "Error reading Telnet data")
                Return False
            Finally
                Screen.Complete = True
            End Try
        Else
            Return True 'Nothing to do
        End If
    End Function

    Public Sub New(Allow_ScreenSize_27x132 As Boolean)
        Me.ScreenSize27x132_Allowed = Allow_ScreenSize_27x132
        If Me.LocaleInfo Is Nothing Then Me.LocaleInfo = New Localization
        Me.Locale = LocaleInfo.Locales("Default")
    End Sub
End Class 'Emulator
