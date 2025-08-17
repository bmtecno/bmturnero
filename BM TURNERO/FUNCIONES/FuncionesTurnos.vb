Imports System.Data.SqlClient

Module FuncionesTurnos

    Public Function TomarTurno(idArea As Integer, nombreArea As String) As TurnoTomado
        Dim connStr = My.Settings.CONEXION
        Using cn As New SqlClient.SqlConnection(connStr)
            cn.Open()
            Using cmd As New SqlClient.SqlCommand("sp_TurnoTomar", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add(New SqlClient.SqlParameter("@ID_AREA", SqlDbType.Int)).Value = idArea
                cmd.Parameters.Add(New SqlClient.SqlParameter("@FECHA", SqlDbType.Date)).Value = DBNull.Value

                Dim pId As New SqlClient.SqlParameter("@ID_TURNO", SqlDbType.Int) With {.Direction = ParameterDirection.Output}
                Dim pCod As New SqlClient.SqlParameter("@CODIGO", SqlDbType.VarChar, 8) With {.Direction = ParameterDirection.Output}
                cmd.Parameters.Add(pId)
                cmd.Parameters.Add(pCod)

                cmd.ExecuteNonQuery()

                If pId.Value Is Nothing OrElse pId.Value Is DBNull.Value Then
                    Return Nothing
                End If

                Dim codigo = If(pCod.Value Is DBNull.Value, Nothing, pCod.Value.ToString())
                Dim numero As Integer = 0
                If Not String.IsNullOrEmpty(codigo) Then
                    Dim numStr = New String(codigo.Reverse().TakeWhile(Function(ch) Char.IsDigit(ch)).Reverse().ToArray())
                    Integer.TryParse(numStr, numero)
                End If

                Return New TurnoTomado With {
                    .IdTurno = CInt(pId.Value),
                    .Codigo = codigo,
                    .Area = nombreArea,
                    .Numero = numero
                }
            End Using
        End Using
    End Function

    Public Function TomarTurnoAsync(idArea As Integer, nombreArea As String) As Task(Of TurnoTomado)
        Return Task.Run(Function()
                            Using cn As New SqlConnection(My.Settings.CONEXION)
                                cn.Open()
                                Using cmd As New SqlCommand("sp_TurnoTomar", cn)
                                    cmd.CommandType = CommandType.StoredProcedure
                                    cmd.Parameters.Add(New SqlParameter("@ID_AREA", SqlDbType.Int)).Value = idArea
                                    cmd.Parameters.Add(New SqlParameter("@FECHA", SqlDbType.Date)).Value = DBNull.Value

                                    Dim pId As New SqlParameter("@ID_TURNO", SqlDbType.Int)
                                    pId.Direction = ParameterDirection.Output
                                    cmd.Parameters.Add(pId)

                                    Dim pCod As New SqlParameter("@CODIGO", SqlDbType.VarChar, 8)
                                    pCod.Direction = ParameterDirection.Output
                                    cmd.Parameters.Add(pCod)

                                    cmd.ExecuteNonQuery()

                                    If pId.Value Is Nothing OrElse pId.Value Is DBNull.Value Then
                                        Return CType(Nothing, TurnoTomado)
                                    End If

                                    Dim codigo = If(pCod.Value Is DBNull.Value, Nothing, pCod.Value.ToString())
                                    ' Si querés, podés parsear número por si hace falta:
                                    Dim numero As Integer = 0
                                    If Not String.IsNullOrEmpty(codigo) Then
                                        ' prefijo puede ser 1 o 2 letras; extraigo la parte numérica al final
                                        Dim numStr = New String(codigo.Reverse().TakeWhile(Function(ch) Char.IsDigit(ch)).Reverse().ToArray())
                                        Integer.TryParse(numStr, numero)
                                    End If

                                    Return New TurnoTomado With {
                                        .IdTurno = CInt(pId.Value),
                                        .Codigo = codigo,
                                        .Area = nombreArea,
                                        .Prefijo = Nothing, ' opcional; si querés, consultá a DB
                                        .Numero = numero
                                    }
                                End Using
                            End Using
                        End Function)
    End Function

End Module
