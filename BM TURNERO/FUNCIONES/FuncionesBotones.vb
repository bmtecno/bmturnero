Imports System.Data.SqlClient
Imports System.Linq

Module FuncionesBotones

    Public Function ObtenerAreasActivas() As List(Of AreaRow)
        Dim lista As New List(Of AreaRow)()
        Dim cs = My.Settings.CONEXION
        Using cn As New SqlConnection(cs)
            cn.Open()
            Using cmd As New SqlCommand("
            SELECT ID_AREA, NOMBRE, PREFIJO, ACTIVO
            FROM AREAS
            WHERE ACTIVO = 1
            ORDER BY NOMBRE;", cn)
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        lista.Add(New AreaRow With {
                            .ID_AREA = rd.GetInt32(0),
                            .NOMBRE = rd.GetString(1),
                            .PREFIJO = rd.GetString(2),
                            .ACTIVO = rd.GetBoolean(3)
                        })
                    End While
                End Using
            End Using
        End Using
        Return lista
    End Function

    Public Function ParseButtonTag(tagObj As Object) As (Key As String, Value As String)
        If tagObj Is Nothing Then Return ("", "")
        Dim s = tagObj.ToString().Trim()
        If s.Contains("=") OrElse s.Contains(":") Then
            Dim sep = If(s.Contains("="), "=", ":")
            Dim parts = s.Split(New Char() {CChar(sep)}, 2)
            Return (parts(0).Trim().ToUpperInvariant(), parts(1).Trim())
        Else
            If s.Length <= 3 Then
                Return ("PREFIJO", s)
            Else
                Return ("AREA", s)
            End If
        End If
    End Function



End Module
