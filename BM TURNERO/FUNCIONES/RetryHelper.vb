Imports System.Threading

Public Module RetryHelper
    Public Function GetInt(name As String, defValue As Integer) As Integer
        'Dim v = ConfigurationManager.AppSettings(name)
        'Dim n As Integer
        'If Integer.TryParse(v, n) AndAlso n >= 0 Then Return n
        'Return defValue
    End Function

    Public Function ExecuteWithRetry(Of T)(opName As String, func As Func(Of T)) As T
        Dim maxIntentos = GetInt("REINTENTOS_DB", 3)
        Dim backoff = GetInt("REINTENTOS_BACKOFF_MS", 250)
        Dim intento = 0
        Dim lastEx As Exception = Nothing

        Do
            Try

                Return func()
            Catch ex As Exception
                lastEx = ex

                intento += 1
                If intento >= maxIntentos Then Exit Do
                Thread.Sleep(CInt(backoff * Math.Pow(2, intento - 1)))
            End Try
        Loop


        Throw lastEx
    End Function
End Module
