using ClosedXML.Excel;
using GestionObras.Core.Entities;

namespace GestionObras.Web.Services;

public class ExportExcelService
{
    public byte[] GenerarExcelFacturas(List<Factura> facturas, string titulo = "Facturas")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(titulo);

        // Cabecera del informe
        ws.Cell("A1").Value = "Gestión de Obras - Informe de Facturas";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:H1").Merge();

        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;
        ws.Range("A2:H2").Merge();

        // Cabeceras de tabla
        var fila = 4;
        var cabeceras = new[] { "Nº Factura", "Fecha Emisión", "Concepto", "Proveedor", "Base Imponible", "IVA", "Total", "Estado" };
        for (int i = 0; i < cabeceras.Length; i++)
        {
            var celda = ws.Cell(fila, i + 1);
            celda.Value = cabeceras[i];
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Datos
        foreach (var f in facturas)
        {
            fila++;
            ws.Cell(fila, 1).Value = f.NumeroFactura;
            ws.Cell(fila, 2).Value = f.FechaEmision.ToString("dd/MM/yyyy");
            ws.Cell(fila, 3).Value = f.Concepto;
            ws.Cell(fila, 4).Value = f.Proveedor?.Nombre ?? "—";
            ws.Cell(fila, 5).Value = f.BaseImponible;
            ws.Cell(fila, 5).Style.NumberFormat.Format = "#,##0.00 €";
            ws.Cell(fila, 6).Value = f.IVA;
            ws.Cell(fila, 6).Style.NumberFormat.Format = "#,##0.00 €";
            ws.Cell(fila, 7).Value = f.ImporteTotal;
            ws.Cell(fila, 7).Style.NumberFormat.Format = "#,##0.00 €";
            ws.Cell(fila, 8).Value = f.Estado.ToString();
        }

        // Fila de totales
        fila += 2;
        ws.Cell(fila, 4).Value = "TOTALES:";
        ws.Cell(fila, 4).Style.Font.Bold = true;
        ws.Cell(fila, 5).Value = facturas.Sum(f => f.BaseImponible);
        ws.Cell(fila, 5).Style.NumberFormat.Format = "#,##0.00 €";
        ws.Cell(fila, 5).Style.Font.Bold = true;
        ws.Cell(fila, 6).Value = facturas.Sum(f => f.IVA);
        ws.Cell(fila, 6).Style.NumberFormat.Format = "#,##0.00 €";
        ws.Cell(fila, 6).Style.Font.Bold = true;
        ws.Cell(fila, 7).Value = facturas.Sum(f => f.ImporteTotal);
        ws.Cell(fila, 7).Style.NumberFormat.Format = "#,##0.00 €";
        ws.Cell(fila, 7).Style.Font.Bold = true;

        // Resumen
        fila += 2;
        ws.Cell(fila, 1).Value = "Resumen";
        ws.Cell(fila, 1).Style.Font.Bold = true;
        ws.Cell(fila + 1, 1).Value = "Pendientes:";
        ws.Cell(fila + 1, 2).Value = facturas.Count(f => f.Estado == EstadoFactura.Pendiente);
        ws.Cell(fila + 2, 1).Value = "Pagadas:";
        ws.Cell(fila + 2, 2).Value = facturas.Count(f => f.Estado == EstadoFactura.Pagada);
        ws.Cell(fila + 3, 1).Value = "Vencidas:";
        ws.Cell(fila + 3, 2).Value = facturas.Count(f => f.Estado == EstadoFactura.Vencida);

        // Ajustar ancho de columnas
        ws.Columns().AdjustToContents();

        return WorkbookToBytes(workbook);
    }

    public byte[] GenerarExcelPresupuestos(List<Presupuesto> presupuestos, string titulo = "Presupuestos")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(titulo);

        ws.Cell("A1").Value = "Gestión de Obras - Informe de Presupuestos";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:E1").Merge();

        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;
        ws.Range("A2:E2").Merge();

        var fila = 4;
        var cabeceras = new[] { "ID", "Fecha Elaboración", "Proyecto", "Total", "Observaciones" };
        for (int i = 0; i < cabeceras.Length; i++)
        {
            var celda = ws.Cell(fila, i + 1);
            celda.Value = cabeceras[i];
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        foreach (var p in presupuestos)
        {
            fila++;
            ws.Cell(fila, 1).Value = $"PRE-{p.Id:000}";
            ws.Cell(fila, 2).Value = p.FechaElaboracion.ToString("dd/MM/yyyy");
            ws.Cell(fila, 3).Value = p.Proyecto?.Nombre ?? $"Proyecto #{p.ProyectoId}";
            ws.Cell(fila, 4).Value = p.Total;
            ws.Cell(fila, 4).Style.NumberFormat.Format = "#,##0.00 €";
            ws.Cell(fila, 5).Value = p.Observaciones ?? "";
        }

        fila += 2;
        ws.Cell(fila, 3).Value = "TOTAL:";
        ws.Cell(fila, 3).Style.Font.Bold = true;
        ws.Cell(fila, 4).Value = presupuestos.Sum(p => p.Total);
        ws.Cell(fila, 4).Style.NumberFormat.Format = "#,##0.00 €";
        ws.Cell(fila, 4).Style.Font.Bold = true;

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(workbook);
    }

    public byte[] GenerarExcelEmpleados(List<Empleado> empleados, string titulo = "Empleados")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(titulo);

        ws.Cell("A1").Value = "Gestión de Obras - Listado de Empleados";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:G1").Merge();

        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;
        ws.Range("A2:G2").Merge();

        var fila = 4;
        var cabeceras = new[] { "Nombre Completo", "DNI", "Email", "Teléfono", "Departamento", "Cargo", "Estado" };
        for (int i = 0; i < cabeceras.Length; i++)
        {
            var celda = ws.Cell(fila, i + 1);
            celda.Value = cabeceras[i];
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        foreach (var e in empleados)
        {
            fila++;
            ws.Cell(fila, 1).Value = $"{e.Nombre} {e.Apellidos}";
            ws.Cell(fila, 2).Value = e.DNI;
            ws.Cell(fila, 3).Value = e.Email;
            ws.Cell(fila, 4).Value = e.Telefono;
            ws.Cell(fila, 5).Value = e.Departamento;
            ws.Cell(fila, 6).Value = e.Cargo;
            ws.Cell(fila, 7).Value = e.Activo ? "Activo" : "Inactivo";
        }

        fila += 2;
        ws.Cell(fila, 1).Value = "Resumen";
        ws.Cell(fila, 1).Style.Font.Bold = true;
        ws.Cell(fila + 1, 1).Value = "Total empleados:";
        ws.Cell(fila + 1, 2).Value = empleados.Count;
        ws.Cell(fila + 2, 1).Value = "Activos:";
        ws.Cell(fila + 2, 2).Value = empleados.Count(e => e.Activo);
        ws.Cell(fila + 3, 1).Value = "Inactivos:";
        ws.Cell(fila + 3, 2).Value = empleados.Count(e => !e.Activo);

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(workbook);
    }

    private static byte[] WorkbookToBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
