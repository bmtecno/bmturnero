Imports System.Drawing.Printing
Imports System.Configuration

Module FuncionesImpresora

    Public Function IsPrinterReady(Optional ByRef printerName As String = Nothing) As Boolean

        Dim cfg = My.Settings.IMPRESORA
        If Not String.IsNullOrWhiteSpace(cfg) Then
            printerName = cfg
            Dim ps As New PrinterSettings() With {.PrinterName = cfg}
            Return ps.IsValid
        End If
        ' Sin nombre => usar predeterminada
        Dim pd As New PrintDocument()
        printerName = pd.PrinterSettings.PrinterName
        Return pd.PrinterSettings.IsValid

    End Function
End Module

