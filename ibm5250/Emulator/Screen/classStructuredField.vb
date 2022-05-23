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
        <Serializable()> Public Class StructuredField
            'SC30-3533-04, 15.27, page 407
            Public Enum WSFCommand As UInt16
                'WRITE STRUCTURED FIELD commands
                Define_Audit_Window_Table = &HD930UI
                Define_Command_Key_Function = &HD931UI
                Read_Text_Screen = &HD932UI
                Define_Pending_Operations = &HD933UI
                Define_Text_Screen_Format = &HD934UI
                Define_Scale_Line = &HD935UI
                Write_Text_Screen = &HD936UI
                Define_Special_Characters = &HD937UI
                Pending_Data = &HD938UI
                Define_Operator_Error_Messages = &HD939UI
                Define_Pitch_Table = &HD93AUI
                Define_Fake_DP_Command_Key_Function = &HD93BUI
                Pass_Through = &HD93FUI
                Query = &HD970UI
                Query_Station_State = &HD972UI
                'WRITE TO DISPLAY-->WRITE TO DISPLAY STRUCTURED FIELD orders
                Define_Selection_Field = &HD950UI
                Create_Window = &HD951UI
                Unrestricted_Window_Cursor_Movement = &HD952UI
                Define_Scroll_Bar_Field = &HD953UI
                Write_Data = &HD954UI
                Programmable_Mouse_Buttons = &HD955UI
                Remove_GUI_Selection_Field = &HD958UI
                Remove_GUI_Window = &HD959UI
                Remove_GUI_Scroll_Bar_Field = &HD95BUI
                Remove_All_GUI_Constructs = &HD95FUI
                Draw_Or_Erase_Grid_Lines = &HD960UI
                Clear_Grid_Line_Buffer = &HD961UI
            End Enum
            'Public Enum WTDSF_Order As UInt16
            '    Define_Selection_Field = &HD950UI
            '    Create_Window = &HD951UI
            '    Unrestricted_Window_Cursor_Movement = &HD952UI
            '    Define_Scroll_Bar_Field = &HD953UI
            '    Write_Data = &HD954UI
            '    Programmable_Mouse_Buttons = &HD955UI
            '    Remove_GUI_Selection_Field = &HD958UI
            '    Remove_GUI_Window = &HD959UI
            '    Remove_GUI_Scroll_Bar_Field = &HD95BUI
            '    Remove_All_GUI_Constructs = &HD95FUI
            '    Draw_Or_Erase_Grid_Lines = &HD960UI
            '    Clear_Grid_Line_Buffer = &HD961UI
            'End Enum
            Private _Length As UInt16
            Public ReadOnly Property Length As UInt16         'LL
                Get
                    _Length = 4 'LL + C + T
                    Select Case Me.Command
                        Case WSFCommand.Define_Scale_Line, WSFCommand.Define_Pending_Operations, WSFCommand.Define_Text_Screen_Format, WSFCommand.Define_Special_Characters, _
                            WSFCommand.Pending_Data, WSFCommand.Define_Fake_DP_Command_Key_Function, WSFCommand.Read_Text_Screen, WSFCommand.Write_Text_Screen
                            'commands using the subaddress byte
                            _Length = +1
                    End Select
                    'XXX what about I?
                    _Length += Flags.Length
                    _Length += Data.Length
                    Return _Length
                End Get
            End Property
            Private _Parent As Object
            Public ReadOnly Property Parent As Object 'Either an EmulatorScreen or EmulatorPopup
                Get
                    Return _Parent
                End Get
            End Property
            Public Command As WSFCommand    'C+T
            Public Subaddress As Byte       'P, should always be 0.
            Public Flags() As Byte          'F, varies from 0 to 3 bytes.
            Public TableVersion As Byte          'I, not described anywhere?
            Public Data() As Byte
            Public BackColor As String
            Public DefaultForeColor As String

            Public Sub New(Parent As Object)
                If Parent.GetType Is GetType(EmulatorScreen) OrElse Parent.GetType Is GetType(EmulatorPopup) Then
                    Me._Parent = Parent
                    Me.BackColor = Parent.BackColor
                    Me.DefaultForeColor = Parent.DefaultForeColor
                Else
                    Throw New Exception("Parent parameter must be an instance of EmulatorScreen or EmulatorPopup")
                End If
            End Sub

            Public Sub New(ByVal buf() As Byte, ByVal Offset As Integer, Parent As Object)
                If Parent.GetType Is GetType(EmulatorScreen) OrElse Parent.GetType Is GetType(EmulatorPopup) Then
                    Me._Parent = Parent
                    Me.BackColor = Parent.BackColor
                    Me.DefaultForeColor = Parent.DefaultForeColor
                Else
                    Throw New Exception("Parent parameter must be an instance of EmulatorScreen or EmulatorPopup")
                End If
                If buf.Length >= Offset + 5 Then
                    Dim OriginalOffset As Integer = Offset
                    Me._Length = (CUShort(buf(Offset)) << 8) + buf(Offset + 1)
                    Me.Command = (CUShort(buf(Offset + 2)) << 8) + buf(Offset + 3)
                    Offset += 4
                    Select Case Me.Command
                        Case WSFCommand.Pass_Through, WSFCommand.Define_Audit_Window_Table, WSFCommand.Define_Command_Key_Function, WSFCommand.Define_Operator_Error_Messages, WSFCommand.Define_Pitch_Table, WSFCommand.Programmable_Mouse_Buttons, WSFCommand.Draw_Or_Erase_Grid_Lines
                            'only data follows
                        Case WSFCommand.Query, WSFCommand.Unrestricted_Window_Cursor_Movement, WSFCommand.Remove_GUI_Selection_Field, WSFCommand.Define_Scroll_Bar_Field, WSFCommand.Remove_GUI_Scroll_Bar_Field, WSFCommand.Write_Data
                            '1 flag byte
                            ReDim Me.Flags(0)
                            Me.Flags(0) = buf(Offset)
                            Offset += 1
                        Case WSFCommand.Query_Station_State, WSFCommand.Create_Window, WSFCommand.Remove_GUI_Window, WSFCommand.Remove_All_GUI_Constructs
                            '2 flag bytes
                            ReDim Me.Flags(1)
                            Me.Flags(0) = buf(Offset)
                            Me.Flags(1) = buf(Offset + 1)
                            Offset += 2
                        Case WSFCommand.Define_Selection_Field
                            '3 flag bytes
                            ReDim Me.Flags(2)
                            Me.Flags(0) = buf(Offset + 1)
                            Me.Flags(1) = buf(Offset + 2)
                            Me.Flags(2) = buf(Offset + 3)
                            Offset += 3
                        Case WSFCommand.Define_Scale_Line
                            'Subaddress byte and 1 flag byte
                            Me.Subaddress = buf(Offset)
                            ReDim Me.Flags(0)
                            Me.Flags(0) = buf(Offset + 1)
                            Offset += 2
                        Case WSFCommand.Define_Pending_Operations, WSFCommand.Define_Text_Screen_Format, WSFCommand.Define_Special_Characters, WSFCommand.Pending_Data, WSFCommand.Define_Fake_DP_Command_Key_Function
                            'Subaddress byte and 2 flag bytes
                            Me.Subaddress = buf(Offset)
                            ReDim Me.Flags(1)
                            Me.Flags(0) = buf(Offset + 1)
                            Me.Flags(1) = buf(Offset + 2)
                            Offset += 3
                        Case WSFCommand.Read_Text_Screen, WSFCommand.Write_Text_Screen
                            'Subaddress byte and 3 flag bytes
                            Me.Subaddress = buf(Offset)
                            ReDim Me.Flags(2)
                            Me.Flags(0) = buf(Offset + 1)
                            Me.Flags(1) = buf(Offset + 2)
                            Me.Flags(2) = buf(Offset + 3)
                            Offset += 4
                    End Select
                    Dim ConsumedBytes As Integer = Offset - OriginalOffset
                    Dim RemainingBytes As Integer = Me._Length - ConsumedBytes
                    ReDim Me.Data(RemainingBytes - 1)
                    Array.Copy(buf, Offset, Me.Data, 0, RemainingBytes)
                Else
                    Throw New ApplicationException("The supplied buffer is too short to contain a complete Structured Field")
                End If
            End Sub

            Public Function ToBytes() As Byte()
                Dim Offs As Integer = 0
                Dim b(Me.Length - 1) As Byte
                b(0) = Me.Length >> 8
                b(1) = Me.Length And &HFF
                b(2) = Me.Command >> 8
                b(3) = Me.Command And &HFF
                Offs += 4
                Select Case Me.Command
                    Case WSFCommand.Define_Scale_Line, WSFCommand.Define_Pending_Operations, WSFCommand.Define_Text_Screen_Format, WSFCommand.Define_Special_Characters, _
                        WSFCommand.Pending_Data, WSFCommand.Define_Fake_DP_Command_Key_Function, WSFCommand.Read_Text_Screen, WSFCommand.Write_Text_Screen
                        'commands using the subaddress byte
                        b(Offs) = Me.Subaddress
                        Offs += 1
                End Select
                Array.Copy(Flags, 0, b, Offs, Flags.Length)
                Offs += Flags.Length
                'XXX what about I?
                Array.Copy(Data, 0, b, Offs, Data.Length)
                Return b
            End Function

            Public Function GetEmulator() As Emulator
                Dim obj As Object = Me
                Do
                    obj = obj.Parent
                    If obj.GetType Is GetType(Emulator) Then Return obj
                Loop
            End Function

            'Public Shared QueryResponseData() As Byte = _
            '   {6, 0, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, &HF3, &HF1, &HF7, &HF9, &HF0, &HF0, &HF2, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, &H7F, &H11, &HD0, &H0, &H5F, &HBC, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            ''above line is taken from a packet capture of Client Access.

            ''stripped down a bit...
            ''  {6, 0, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, &HF3, &HF1, &HF7, &HF9, &HF0, &HF0, &HF2, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, &H11, &HC0, &H0, &H5F, &H88, 0,0,0,0,0,0,0,0,0}
            <Serializable()> Public Class QueryResponse
                'Public Enum CodeQuality As Byte
                '    Release = 4
                'End Enum
                Public Enum Workstation_Type As Byte
                    DisplayStation = 1
                    Printer = 2
                End Enum
                'Public Structure Code_Level
                '    Dim Quality As CodeQuality
                '    Dim VersionMajor As Byte
                '    Dim VersionMinor As Byte
                'End Structure
                Public Structure Code_Level
                    Dim VersionMajor As Byte
                    Dim VersionMinor As Byte
                    Dim VersionBuild As Byte
                End Structure
                Public Enum Control_Unit_Customization_Bits As Byte 'bitwise
                    WSC_CUSTOMIZATION = 128
                    QUERY_STATION_STATE = 64
                    WORKSTATION_CUSTOMIZATION = 32
                    WORKSTATION_CUSTOMIZATION_LONG = 16
                    'bits 4-7 (from the left) are reserved
                End Enum
                Public Enum Device_Capabilities_Byte0_Bits As Byte 'bitwise
                    Row1Col1_LimitedSupport = 64
                    READ_MDT_ALTERNATE = 32
                    PA1_And_PA2 = 16
                    PA3 = 8
                    CursorSelect = 4
                    Move_Cursor_And_Transparent_Data_Orders = 2
                    READ_MODIFIED_IMMEDIATE_ALTERNATE = 1
                End Enum
                Public Enum Device_Capabilities_Byte1_Bits As Byte 'bitwise
                    ScreenSize_27x132 = 32
                    ScreenSize_24x80 = 16
                    ScanningLightPen = 8
                    MagneticStripeReader = 4
                    Color = 1
                End Enum
                Public Enum Device_Capabilities_Byte2_Bits As Byte 'bitwise
                    TextSymbols = 128
                    ExtendedPrimaryAttributes_In_WP_Mode = 64
                    DualLanguage_OfficeEditor = 16
                    SingleLanguage_OfficeEditor = 8
                    ExtendedPrimaryAttributes_In_DP_Mode = 4
                    ExtendedForegroundColorAttributes_14Colors = 2
                    ExtendedForegroundColorAttributes_7Colors = 1
                End Enum
                Public Enum Device_Capabilities_Byte4_Bits As Byte 'bitwise
                    OS2Link_Graphics = 64
                    IBM5292_Graphics = 32
                    Extended3270_Datastream = 16
                    Mouse = 8
                    GUILike_Characters = 4
                    EnhancedUserInterface_Level1 = 2
                    WRITE_ERROR_CODE_TO_WINDOW = 1
                End Enum
                Public Enum Device_Capabilities_Byte5_Bits As Byte 'bitwise
                    EnhancedUserInterface_Level2 = 128
                    AllPointsAddressable_GUI = 64
                    WordPerfect_In_OfficeEditor = 32
                    DynamicStatusLine_In_OfficeEditor = 16
                    EnhancedUserInterface_Level3 = 8 'included left and right justified window titles and footers
                    CursorDraw_In_OfficeEditor = 4
                    IBM5250_VideoDelivery = 1
                End Enum
                Public Enum Device_Capabilities_Byte13_Bits As Byte 'bitwise
                    GridLines_Not_Supported = 0
                    GridLines_Supported = 1
                End Enum

                Dim WorkstationControlUnit(1) As Byte
                Dim CodeLevel As Code_Level
                Dim Reserved1(15) As Byte
                Dim WorkstationType As Workstation_Type
                Dim MachineType(3) As Byte
                Dim ModelNumber(2) As Byte
                Dim KeyboardID As Byte
                Dim ExtendedKeyboardID As Byte
                Dim Reserved2 As Byte
                Dim SerialNumber(3) As Byte
                Dim InputFields(1) As Byte 'always &H100 = 256
                Dim ControlUnitCustomization As Byte
                Dim Reserved3(1) As Byte
                Dim GeneralCapabilities As Device_Capabilities_Byte0_Bits
                Public DisplayCapabilities As Device_Capabilities_Byte1_Bits
                Dim AdvancedTextCapabilities As Device_Capabilities_Byte2_Bits
                Dim IdeographicCapabilities As Byte
                Dim MiscCapabilities As Device_Capabilities_Byte4_Bits
                Dim MiscCapabilities2 As Device_Capabilities_Byte5_Bits
                Dim Reserved4 As Byte
                Dim FaxImageCapabilities(2) As Byte
                Dim PrinterType As Byte
                Dim Reserved5 As Byte
                Dim GridLineBufferCount As Byte
                Dim GridLineSupport As Device_Capabilities_Byte13_Bits
                Dim Reserved6 As Byte
                Dim FaxImageDisplayCount As Byte
                Dim FaxImageScalingGranularityCapabilities As Byte
                Dim FaxImageRotationGranularityCapabilities As Byte
                Dim FaxImageZOrderCapabilities As Byte
                Dim Reserved7(2) As Byte

                Dim _Parent As StructuredField
                Public ReadOnly Property Parent As StructuredField
                    Get
                        Return _Parent
                    End Get
                End Property

                Public Property TerminalType As String
                    Get
                        Return System.Text.Encoding.UTF8.GetString(Parent.GetEmulator.EBCDIC_To_UTF8(Me.MachineType))
                    End Get
                    Set(value As String)
                        If (value.Length < 1) Or (value.Length > 4) Then Throw New ArgumentOutOfRangeException("TerminalType must be between 1 and 4 characters")
                        Me.MachineType = Parent.GetEmulator.UTF8_To_EBCDIC(System.Text.Encoding.UTF8.GetBytes(value.PadLeft(4)))
                    End Set
                End Property

                Public Property TerminalModel As String
                    Get
                        Return System.Text.Encoding.UTF8.GetString(Parent.GetEmulator.EBCDIC_To_UTF8(Me.ModelNumber))
                    End Get
                    Set(value As String)
                        If (value.Length < 1) Or (value.Length > 3) Then Throw New ArgumentOutOfRangeException("TerminalModel must be between 1 and 3 characters")
                        Me.ModelNumber = Parent.GetEmulator.UTF8_To_EBCDIC(System.Text.Encoding.UTF8.GetBytes(value.PadLeft(3)))
                    End Set
                End Property

                Public Sub New(Parent As StructuredField)
                    _Parent = Parent
                    Me.WorkstationControlUnit(0) = 6
                    Me.WorkstationControlUnit(1) = 0
                    Me.CodeLevel.VersionMajor = 3
                    Me.CodeLevel.VersionMinor = 2
                    Me.CodeLevel.VersionBuild = 0
                    Me.WorkstationType = Workstation_Type.DisplayStation
                    Me.TerminalType = "3179"
                    Me.TerminalModel = "002"
                    Me.InputFields(0) = 1 '&H100 = 256
                    Me.InputFields(1) = 0 '
                    Me.GeneralCapabilities = Device_Capabilities_Byte0_Bits.Row1Col1_LimitedSupport
                    Me.DisplayCapabilities = Device_Capabilities_Byte1_Bits.ScreenSize_24x80 Or _
                                             Device_Capabilities_Byte1_Bits.Color
                    Me.MiscCapabilities = Device_Capabilities_Byte4_Bits.WRITE_ERROR_CODE_TO_WINDOW Or _
                                          Device_Capabilities_Byte4_Bits.Mouse Or _
                                          Device_Capabilities_Byte4_Bits.GUILike_Characters Or _
                                          Device_Capabilities_Byte4_Bits.EnhancedUserInterface_Level1
                    Me.GridLineSupport = Device_Capabilities_Byte13_Bits.GridLines_Not_Supported
                End Sub

                Public Function ToBytes() As Byte()
                    Using ms As New System.IO.MemoryStream
                        ms.Write(Me.WorkstationControlUnit, 0, Me.WorkstationControlUnit.Length)
                        ms.WriteByte(Me.CodeLevel.VersionMajor)
                        ms.WriteByte(Me.CodeLevel.VersionMinor)
                        ms.WriteByte(Me.CodeLevel.VersionBuild)
                        ms.Write(Me.Reserved1, 0, Me.Reserved1.Length)
                        ms.WriteByte(Me.WorkstationType)
                        ms.Write(Me.MachineType, 0, Me.MachineType.Length)
                        ms.Write(Me.ModelNumber, 0, Me.ModelNumber.Length)
                        ms.WriteByte(Me.KeyboardID)
                        ms.WriteByte(Me.ExtendedKeyboardID)
                        ms.WriteByte(Me.Reserved2)
                        ms.Write(Me.SerialNumber, 0, Me.SerialNumber.Length)
                        ms.Write(Me.InputFields, 0, Me.InputFields.Length)
                        ms.WriteByte(Me.ControlUnitCustomization)
                        ms.Write(Me.Reserved3, 0, Me.Reserved3.Length)
                        ms.WriteByte(Me.GeneralCapabilities)
                        ms.WriteByte(Me.DisplayCapabilities)
                        ms.WriteByte(Me.AdvancedTextCapabilities)
                        ms.WriteByte(Me.IdeographicCapabilities)
                        ms.WriteByte(Me.MiscCapabilities)
                        ms.WriteByte(Me.MiscCapabilities2)
                        ms.WriteByte(Me.Reserved4)
                        ms.Write(Me.FaxImageCapabilities, 0, Me.FaxImageCapabilities.Length)
                        ms.WriteByte(Me.PrinterType)
                        ms.WriteByte(Me.Reserved5)
                        ms.WriteByte(Me.GridLineBufferCount)
                        ms.WriteByte(Me.GridLineSupport)
                        ms.WriteByte(Me.Reserved6)
                        ms.WriteByte(Me.FaxImageDisplayCount)
                        ms.WriteByte(Me.FaxImageScalingGranularityCapabilities)
                        ms.WriteByte(Me.FaxImageRotationGranularityCapabilities)
                        ms.WriteByte(Me.FaxImageZOrderCapabilities)
                        ms.Write(Me.Reserved7, 0, Me.Reserved7.Length)
                        Return ms.ToArray
                    End Using
                End Function
            End Class

            <Serializable()> Public Class CreateWindowHeader
                Dim _CursorRestrictedToWindow As Boolean = False
                Dim _IsPullDownMenuBar As Boolean = False
                Dim _Rows, _Columns As Integer
                Dim _MinorStructures() As Object
                'Dim _RawFlags(1) As Byte
                Public Enum MinorStructureType As Byte
                    BorderPresentation = 1
                    WindowTitle_Or_Footer = &H10
                End Enum
                'Public ReadOnly Property RawFlags As Byte()
                '    Get
                '        Return _RawFlags
                '    End Get
                'End Property
                Public ReadOnly Property CursorRestrictedToWindow As Boolean
                    Get
                        Return _CursorRestrictedToWindow
                    End Get
                End Property
                Public ReadOnly Property IsPullDownMenuBar As Boolean
                    Get
                        Return _IsPullDownMenuBar
                    End Get
                End Property
                Public ReadOnly Property Rows As Integer
                    Get
                        Return _Rows
                    End Get
                End Property
                Public ReadOnly Property Columns As Integer
                    Get
                        Return _Columns
                    End Get
                End Property
                Public ReadOnly Property MinorStructures() As Object
                    Get
                        Return _MinorStructures
                    End Get
                End Property
                Private _Parent As StructuredField
                Public ReadOnly Property Parent As StructuredField
                    Get
                        Return _Parent
                    End Get
                End Property

                Public Sub New(ByVal buf() As Byte, ByVal Offset As Integer, ByVal Length As Integer, ByVal Parent As StructuredField)
                    _Parent = Parent
                    'Dim Length As Integer = buf(Offset)
                    'If Length < 9 Then
                    '    Throw New ApplicationException("CreateWindow header length is out of range")
                    'End If
                    '_RawFlags(0) = buf(Offset + 2)
                    '_RawFlags(1) = buf(Offset + 3)

                    'The Length, Class, Type, and Flag bytes have already been consumed.
                    'The Length parameter excludes the above.
                    Dim Reserved As Byte = buf(Offset) 'is this ever used?
                    Me._Rows = buf(Offset + 1)
                    Me._Columns = buf(Offset + 2)
                    Length -= 3
                    ReDim Me._MinorStructures(-1)
                    Do While Length > 0
                        'peek ahead to see which minor structure this is.
                        Select Case buf(Offset + 4)
                            Case MinorStructureType.BorderPresentation
                                ReDim Me._MinorStructures(Me._MinorStructures.Length)
                                Me._MinorStructures(Me._MinorStructures.Length - 1) = New BorderPresentation(buf, Offset + 3, Me)
                                Length -= Me._MinorStructures(Me._MinorStructures.Length - 1).Length
                            Case MinorStructureType.WindowTitle_Or_Footer
                                ReDim Me._MinorStructures(Me._MinorStructures.Length)
                                Me._MinorStructures(Me._MinorStructures.Length - 1) = New WindowTitle_Or_Footer(buf, Offset + 3, Me)
                                Length -= Me._MinorStructures(Me._MinorStructures.Length - 1).Length
                            Case Else
                                Throw New ApplicationException("Unrecognized WTD Structured Field minor structure: &H" & Hex(buf(Offset + 4)))
                        End Select
                    Loop
                End Sub

                <Serializable()> Public Class BorderPresentation
                    Public Length As Integer
                    Public RawFlags As Byte
                    Public UseBorderPresentationCharacters As Boolean
                    Public MonochromeAttribute As IBM5250.Emulator.EmulatorScreen.FieldAttribute
                    Public ColorAttribute As IBM5250.Emulator.EmulatorScreen.FieldAttribute
                    Public TopLeftChar As Char
                    Public TopChar As Char
                    Public TopRightChar As Char
                    Public LeftChar As Char
                    Public RightChar As Char
                    Public BottomLeftChar As Char
                    Public BottomChar As Char
                    Public BottomRightChar As Char
                    Private _Parent As CreateWindowHeader
                    Public ReadOnly Property Parent As CreateWindowHeader
                        Get
                            Return _Parent
                        End Get
                    End Property

                    Public Sub New(ByVal buf() As Byte, ByVal Offset As Integer, ByVal Parent As CreateWindowHeader)
                        _Parent = Parent
                        Me.Length = buf(Offset)
                        Dim StructureType As MinorStructureType = buf(Offset + 1)
                        If StructureType <> MinorStructureType.BorderPresentation Then
                            Throw New ApplicationException("Supplied buffer does not contain a BorderPresentation minor structure")
                        End If

                        'Removed lines below so we can send a Negative Response from the data stream parser if the length is wrong.
                        '
                        'If Length < 4 Or Length > 13 Then
                        '    Throw New ApplicationException("BorderPresentation minor structure length is out of range")
                        'End If

                        Me.RawFlags = buf(Offset + 2)
                        Me.UseBorderPresentationCharacters = (Me.RawFlags And 1) 'XXX or are these bits reversed?
                        Me.MonochromeAttribute = New IBM5250.Emulator.EmulatorScreen.FieldAttribute(buf(Offset + 3), Parent.Parent.DefaultForeColor, Parent.Parent.BackColor)
                        Me.ColorAttribute = New IBM5250.Emulator.EmulatorScreen.FieldAttribute(buf(Offset + 4), Parent.Parent.DefaultForeColor, Parent.Parent.BackColor)

                        'Me.TopLeftChar = Chr(Emulator.EBCDIC_To_UTF8(New Byte() {buf(Offset + 3)})(0))
                        'Me.TopChar = Chr(Emulator.EBCDIC_To_UTF8(New Byte() {buf(Offset + 4)})(0))
                        'Me.TopRightChar = Chr(Emulator.EBCDIC_To_UTF8(New Byte() {buf(Offset + 5)})(0))
                        'Me.LeftChar = Chr(Emulator.EBCDIC_To_UTF8(New Byte() {buf(Offset + 6)})(0))
                        'Me.RightChar = Chr(Emulator.EBCDIC_To_UTF8(New Byte() {buf(Offset + 7)})(0))
                        'Me.BottomLeftChar = Chr(Emulator.EBCDIC_To_UTF8(New Byte() {buf(Offset + 8)})(0))
                        'Me.BottomChar = Chr(Emulator.EBCDIC_To_UTF8(New Byte() {buf(Offset + 9)})(0))
                        'Me.BottomRightChar = Chr(Emulator.EBCDIC_To_UTF8(New Byte() {buf(Offset + 10)})(0))

                        Dim b(7) As Byte
                        Array.Copy(buf, Offset + 5, b, 0, b.Length)
                        b = Parent.Parent.GetEmulator.EBCDIC_To_UTF8(b)
                        Me.TopLeftChar = Chr(b(0))
                        Me.TopChar = Chr(b(1))
                        Me.TopRightChar = Chr(b(2))
                        Me.LeftChar = Chr(b(3))
                        Me.RightChar = Chr(b(4))
                        Me.BottomLeftChar = Chr(b(5))
                        Me.BottomChar = Chr(b(6))
                        Me.BottomRightChar = Chr(b(7))

                        If Me.TopLeftChar = Chr(0) Then Me.TopLeftChar = Chr(218)
                        If Me.TopChar = Chr(0) Then Me.TopChar = Chr(196)
                        If Me.TopRightChar = Chr(0) Then Me.TopRightChar = Chr(191)
                        If Me.LeftChar = Chr(0) Then Me.LeftChar = Chr(179)
                        If Me.RightChar = Chr(0) Then Me.RightChar = Chr(179)
                        If Me.BottomLeftChar = Chr(0) Then Me.BottomLeftChar = Chr(192)
                        If Me.BottomChar = Chr(0) Then Me.BottomChar = Chr(196)
                        If Me.BottomRightChar = Chr(0) Then Me.BottomRightChar = Chr(217)
                    End Sub
                End Class 'BorderPresentation
                <Serializable()> Public Class WindowTitle_Or_Footer
                    Public Enum WindowOrientation As Byte
                        Centered = 0
                        RightJustified = 1
                        LeftJustified = &H10
                        Reserved_Centered = &H11
                    End Enum
                    Public Enum WindowElement As Byte
                        Title = 0
                        Footer = 1
                    End Enum
                    Public Length As Integer
                    Public Orientation As WindowOrientation
                    Public Element As WindowElement
                    Public RawFlags As Byte
                    Public MonochromeAttribute As Emulator.EmulatorScreen.FieldAttribute
                    Public ColorAttribute As Emulator.EmulatorScreen.FieldAttribute
                    Public Text As String
                    Private _Parent As CreateWindowHeader
                    Public ReadOnly Property Parent As CreateWindowHeader
                        Get
                            Return _Parent
                        End Get
                    End Property

                    Public Sub New(ByVal buf() As Byte, ByVal Offset As Integer, ByVal Parent As CreateWindowHeader)
                        _Parent = Parent
                        Me.Length = buf(Offset)
                        Dim StructureType As MinorStructureType = buf(Offset + 1)
                        If StructureType <> MinorStructureType.WindowTitle_Or_Footer Then
                            Throw New ApplicationException("Supplied buffer does not contain a WindowTitle_Or_Footer minor structure")
                        End If

                        'Removed lines below so we can send a Negative Response from the data stream parser if the length is wrong.
                        '
                        'If Length < 6 Then
                        '    Throw New ApplicationException("WindowTitle_Or_Footer minor structure length is out of range")
                        'End If

                        Me.RawFlags = buf(Offset + 2)
                        Me.Orientation = CType((Me.RawFlags And 3), WindowOrientation) 'XXX or are the bits reversed?
                        Me.Element = CType(((Me.RawFlags >> 2) And 1), WindowElement) 'XXX ""
                        Me.MonochromeAttribute = New IBM5250.Emulator.EmulatorScreen.FieldAttribute(buf(Offset + 3), Parent.Parent.DefaultForeColor, Parent.Parent.BackColor)
                        Me.ColorAttribute = New IBM5250.Emulator.EmulatorScreen.FieldAttribute(buf(Offset + 4), Parent.Parent.DefaultForeColor, Parent.Parent.BackColor)
                        Dim b(Length - 6 - 1) As Byte
                        Array.Copy(buf, Offset + 6, b, 0, b.Length)

                        Me.Text = System.Text.Encoding.UTF8.GetString(Parent.Parent.GetEmulator.EBCDIC_To_UTF8(b))
                        'XXX last character of string is a trailing attribute?


                    End Sub

                End Class 'WindowTitle

            End Class 'CreateWindowHeader

        End Class 'StructuredField

    End Class 'EmulatorScreen
End Class 'Emulator
