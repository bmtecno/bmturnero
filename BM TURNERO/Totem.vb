Imports System.Data.SqlClient

Public Class Totem

    Private Sub btnSalir_Click(sender As Object, e As EventArgs) Handles btnSalir.Click

        Application.Exit()

    End Sub

    Private Sub Totem_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        AsignarAreasABotones(Me)

    End Sub


    Private Async Sub OnAreaClick(sender As Object, e As EventArgs)

        Dim btn = DirectCast(sender, Button)
        Dim bind = TryCast(btn.Tag, ButtonAreaBinding)
        If bind Is Nothing Then
            MessageBox.Show("Botón no asignado a un área.", "TOTEM", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Me.Enabled = False
        Cursor = Cursors.WaitCursor
        Try

            Dim turno = Await Task.Run(Function()
                                           Return RetryHelper.ExecuteWithRetry("sp_TurnoTomar",
                Function() TomarTurno(bind.IdArea, bind.Nombre))
                                       End Function)

            If turno Is Nothing OrElse String.IsNullOrEmpty(turno.Codigo) Then
                Throw New ApplicationException("Resultado vacío de sp_TurnoTomar")
            End If

            'lblArea.Text = $"Área: {turno.Area}"
            'lblCodigo.Text = turno.Codigo

            'Dim prn As String = Nothing
            'If Not PrinterWatchdog.IsPrinterReady(prn) Then
            '    Logger.LogWarn($"Impresora no disponible ({prn}).")
            '    MessageBox.Show("La impresora no está disponible. Se emitió el turno sin impresión.", "Impresora",
            '                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            'Else
            '    Dim tp As New TicketPrinter(turno)
            '    tp.Imprimir()
            'End If

            Console.Beep(800, 120)

        Catch ex As Exception

            Dim msg = "ERROR: " & ex.Message
            If String.IsNullOrWhiteSpace(msg) Then msg = "Servicio no disponible. Reintente en unos minutos."
            MessageBox.Show(msg, "TOTEM", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Cursor = Cursors.Default
            Me.Enabled = True
        End Try

    End Sub

    Private Sub AsignarAreasABotones(contenedor As Control)
        Dim areas = ObtenerAreasActivas()
        Dim byId = areas.ToDictionary(Function(a) a.ID_AREA, Function(a) a)
        Dim byNombre = areas.ToDictionary(Function(a) a.NOMBRE.ToUpperInvariant(), Function(a) a)
        Dim byPrefijo = areas.ToDictionary(Function(a) a.PREFIJO.ToUpperInvariant(), Function(a) a)

        For Each btn In contenedor.Controls.OfType(Of Button)()
            Dim keyVal = ParseButtonTag(btn.Tag)
            Dim area As AreaRow = Nothing

            Select Case keyVal.Key
                Case "ID", "ID_AREA"
                    Dim id As Integer
                    If Integer.TryParse(keyVal.Value, id) AndAlso byId.ContainsKey(id) Then
                        area = byId(id)
                    End If
                Case "AREA"
                    Dim k = keyVal.Value.ToUpperInvariant()
                    If byNombre.ContainsKey(k) Then area = byNombre(k)
                Case "PREFIJO"
                    Dim k = keyVal.Value.ToUpperInvariant()
                    If byPrefijo.ContainsKey(k) Then area = byPrefijo(k)
                Case Else
                    ' Sin clave: intentamos como nombre y como prefijo
                    Dim k = keyVal.Value.ToUpperInvariant()
                    If byNombre.ContainsKey(k) Then
                        area = byNombre(k)
                    ElseIf byPrefijo.ContainsKey(k) Then
                        area = byPrefijo(k)
                    End If
            End Select

            If area IsNot Nothing Then
                btn.Text = $"{area.NOMBRE} ({area.PREFIJO})"
                btn.Enabled = True
                btn.Visible = True
                ' Guardamos binding completo en Tag para usar después en el Click
                btn.Tag = New ButtonAreaBinding With {
                .IdArea = area.ID_AREA,
                .Nombre = area.NOMBRE,
                .Prefijo = area.PREFIJO,
                .OriginalTag = $"{keyVal.Key}={keyVal.Value}"
            }
                RemoveHandler btn.Click, AddressOf OnAreaClick
                AddHandler btn.Click, AddressOf OnAreaClick
            Else

                If btn.Name = "btnSalir" Then
                    ' Botón de salir: lo dejo habilitado
                    btn.Enabled = True
                Else
                    ' Botón sin área: lo deshabilito (o poné Visible=False)
                    btn.Enabled = False
                    ' btn.Visible = False
                    ' Si querés marcar visualmente:
                    ' btn.Text = "(no disponible)"
                End If

            End If
        Next
    End Sub


End Class
