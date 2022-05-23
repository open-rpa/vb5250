Partial Class Emulator
    Public Class Localization
        Public Locales As SortedList(Of String, Locale)
        Public Sub New()
            Locales = New SortedList(Of String, Locale)
            Locales.Add("Default", New Locale("Default", System.Text.Encoding.GetEncoding("IBM037"), "37", "697", "USB"))
            Dim sr As System.IO.StreamReader = Nothing
            Try
                Dim fname As String = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "Locales.dat")
                If System.IO.File.Exists(fname) Then
                    sr = New System.IO.StreamReader(fname) ', System.Text.Encoding.GetEncoding("UTF-8"))
                    Dim LineNumber As Integer = 0
                    Dim s As String = sr.ReadLine
                    Do While s IsNot Nothing
                        LineNumber += 1
                        s = s.Trim
                        If s.Length > 0 Then
                            If s(0) <> ";" And s(0) <> "#" Then 'ignore comments
                                Dim loc() As String = s.Split(",")
                                For i As Integer = 0 To loc.Length - 1
                                    loc(i) = loc(i).Trim.Replace("""", "")
                                Next
                                If loc.Length = 5 Then
                                    Try
                                        Locales.Add(loc(0), New Locale(loc(0), System.Text.Encoding.GetEncoding(loc(1)), loc(2), loc(3), loc(4)))
                                    Catch ex As Exception
                                    End Try
                                End If
                            End If
                        End If
                        s = sr.ReadLine
                    Loop
                End If
            Catch ex As Exception
            Finally
                If sr IsNot Nothing Then sr.Close()
            End Try
        End Sub

        Public Class Locale
            Private _Description As String
            Private _Encoding As System.Text.Encoding
            Private _CodePage As String
            Private _CharacterSet As String
            Private _KeyboardType As String
            Public ReadOnly Property Description As String
                Get
                    Return _Description
                End Get
            End Property
            Public ReadOnly Property Encoding As System.Text.Encoding
                Get
                    Return _Encoding
                End Get
            End Property
            Public ReadOnly Property CODEPAGE As String
                Get
                    Return _CodePage
                End Get
            End Property
            Public ReadOnly Property CHARSET As String
                Get
                    Return _CharacterSet
                End Get
            End Property
            Public ReadOnly Property KBDTYPE As String
                Get
                    Return _KeyboardType
                End Get
            End Property
            Public Sub New(Description As String, Encoding As System.Text.Encoding, CodePage As String, CharSet As String, KbdType As String)
                If String.IsNullOrWhiteSpace(Description) Then Throw New ArgumentNullException("Description")
                If Encoding Is Nothing Then Throw New ArgumentNullException("Encoding")
                If String.IsNullOrWhiteSpace(CodePage) Then Throw New ArgumentNullException("CodePage")
                If String.IsNullOrWhiteSpace(CharSet) Then Throw New ArgumentNullException("CharSet")
                If String.IsNullOrWhiteSpace(KbdType) Then Throw New ArgumentNullException("KbdType")
                _Description = Description
                _Encoding = Encoding
                _CodePage = CodePage
                _CharacterSet = CharSet
                _KeyboardType = KbdType
            End Sub
            Public Function ConvertFrom(ByVal b() As Byte, ByVal SourceEncoding As System.Text.Encoding) As Byte()
                If SourceEncoding Is Nothing Then Throw New ArgumentNullException("SourceEncoding")
                Return System.Text.Encoding.Convert(SourceEncoding, _Encoding, b)
            End Function
            Public Function ConvertTo(ByVal b() As Byte, ByVal DestinationEncoding As System.Text.Encoding) As Byte()
                If DestinationEncoding Is Nothing Then Throw New ArgumentNullException("DestinationEncoding")
                Return System.Text.Encoding.Convert(_Encoding, DestinationEncoding, b)
            End Function
            Public Function GetString(ByVal b() As Byte) As String
                Return System.Text.Encoding.UTF8.GetString(b)
            End Function
            Public Function GetString(ByVal b() As Byte, ByVal SourceEncoding As System.Text.Encoding) As String
                Dim c() As Byte = ConvertFrom(b, SourceEncoding)
                Return System.Text.Encoding.UTF8.GetString(c)
            End Function
        End Class
    End Class
End Class
