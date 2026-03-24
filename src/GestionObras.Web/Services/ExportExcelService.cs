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

    public byte[] GenerarExcelTareas(List<Tarea> tareas, string titulo = "Tareas")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(titulo);

        ws.Cell("A1").Value = "Gestión de Obras - Informe de Tareas";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:H1").Merge();

        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;
        ws.Range("A2:H2").Merge();

        var fila = 4;
        var cabeceras = new[] { "ID", "Proyecto", "Tarea", "Estado", "Prioridad", "Fecha Inicio", "Fecha Fin", "Responsables" };
        for (int i = 0; i < cabeceras.Length; i++)
        {
            var celda = ws.Cell(fila, i + 1);
            celda.Value = cabeceras[i];
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        foreach (var t in tareas.OrderBy(t => t.Prioridad).ThenBy(t => t.FechaInicio))
        {
            var responsables = t.UsuariosAsignados != null && t.UsuariosAsignados.Any()
                ? string.Join(", ", t.UsuariosAsignados.Select(u => u.NombreCompleto).Where(n => !string.IsNullOrWhiteSpace(n)))
                : "Sin asignar";

            fila++;
            ws.Cell(fila, 1).Value = $"T-{t.Id:000}";
            ws.Cell(fila, 2).Value = t.Proyecto?.Nombre ?? "-";
            ws.Cell(fila, 3).Value = t.Nombre;
            ws.Cell(fila, 4).Value = t.Estado.ToString();
            ws.Cell(fila, 5).Value = t.Prioridad.ToString();
            ws.Cell(fila, 6).Value = t.FechaInicio.ToString("dd/MM/yyyy");
            ws.Cell(fila, 7).Value = t.FechaFin?.ToString("dd/MM/yyyy") ?? "-";
            ws.Cell(fila, 8).Value = responsables;
        }

        fila += 2;
        ws.Cell(fila, 1).Value = "Resumen";
        ws.Cell(fila, 1).Style.Font.Bold = true;
        ws.Cell(fila + 1, 1).Value = "Total tareas:";
        ws.Cell(fila + 1, 2).Value = tareas.Count;
        ws.Cell(fila + 2, 1).Value = "Pendientes:";
        ws.Cell(fila + 2, 2).Value = tareas.Count(t => t.Estado == EstadoTarea.Pendiente);
        ws.Cell(fila + 3, 1).Value = "En curso:";
        ws.Cell(fila + 3, 2).Value = tareas.Count(t => t.Estado == EstadoTarea.EnCurso);
        ws.Cell(fila + 4, 1).Value = "Bloqueadas:";
        ws.Cell(fila + 4, 2).Value = tareas.Count(t => t.Estado == EstadoTarea.Bloqueado);
        ws.Cell(fila + 5, 1).Value = "Finalizadas:";
        ws.Cell(fila + 5, 2).Value = tareas.Count(t => t.Estado == EstadoTarea.Finalizado);

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(workbook);
    }

    /// <summary>
    /// Genera un Excel con los detalles de una factura individual
    /// </summary>
    public byte[] GenerarExcelFacturaIndividual(Factura factura, string titulo = "Factura")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(titulo);

        // Cabecera
        ws.Cell("A1").Value = $"Gestión de Obras - Factura {factura.NumeroFactura}";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:D1").Merge();

        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;

        // Información general
        var fila = 4;
        ws.Cell($"A{fila}").Value = "Información General";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"A{fila}").Style.Font.FontSize = 12;

        fila++;
        ws.Cell($"A{fila}").Value = "Número de Factura:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = factura.NumeroFactura;

        fila++;
        ws.Cell($"A{fila}").Value = "Fecha de Emisión:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = factura.FechaEmision.ToString("dd/MM/yyyy");

        fila++;
        ws.Cell($"A{fila}").Value = "Concepto:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = factura.Concepto;

        fila++;
        ws.Cell($"A{fila}").Value = "Proveedor:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = factura.Proveedor?.Nombre ?? "No especificado";

        // Detalles económicos
        fila += 2;
        ws.Cell($"A{fila}").Value = "Detalles Económicos";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"A{fila}").Style.Font.FontSize = 12;

        fila++;
        var headerRow = fila;
        ws.Cell($"A{fila}").Value = "Concepto";
        ws.Cell($"B{fila}").Value = "Importe";
        foreach (var col in new[] { "A", "B" })
        {
            ws.Cell($"{col}{fila}").Style.Font.Bold = true;
            ws.Cell($"{col}{fila}").Style.Fill.BackgroundColor = XLColor.DarkBlue;
            ws.Cell($"{col}{fila}").Style.Font.FontColor = XLColor.White;
        }

        fila++;
        ws.Cell($"A{fila}").Value = "Base Imponible";
        ws.Cell($"B{fila}").Value = factura.BaseImponible;
        ws.Cell($"B{fila}").Style.NumberFormat.Format = "#,##0.00 €";

        fila++;
        ws.Cell($"A{fila}").Value = "IVA";
        ws.Cell($"B{fila}").Value = factura.IVA;
        ws.Cell($"B{fila}").Style.NumberFormat.Format = "#,##0.00 €";

        fila++;
        ws.Cell($"A{fila}").Value = "TOTAL";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = factura.ImporteTotal;
        ws.Cell($"B{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Style.NumberFormat.Format = "#,##0.00 €";
        ws.Cell($"B{fila}").Style.Fill.BackgroundColor = XLColor.Yellow;

        // Estado
        fila += 2;
        ws.Cell($"A{fila}").Value = "Estado";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"A{fila}").Style.Font.FontSize = 12;

        fila++;
        ws.Cell($"A{fila}").Value = "Estado Actual:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = factura.Estado.ToString();

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(workbook);
    }

    /// <summary>
    /// Genera un Excel con los detalles de una tarea individual
    /// </summary>
    public byte[] GenerarExcelTareaIndividual(Tarea tarea, string titulo = "Tarea")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(titulo);

        // Cabecera
        ws.Cell("A1").Value = $"Gestión de Obras - Tarea: {tarea.Nombre}";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:D1").Merge();

        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;

        // Información general
        var fila = 4;
        ws.Cell($"A{fila}").Value = "Información General";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"A{fila}").Style.Font.FontSize = 12;

        fila++;
        ws.Cell($"A{fila}").Value = "ID Tarea:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = $"T-{tarea.Id:000}";

        fila++;
        ws.Cell($"A{fila}").Value = "Nombre:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = tarea.Nombre;

        fila++;
        ws.Cell($"A{fila}").Value = "Proyecto:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = tarea.Proyecto?.Nombre ?? "Sin asignar";

        // Descripción
        if (!string.IsNullOrWhiteSpace(tarea.Descripcion))
        {
            fila += 2;
            ws.Cell($"A{fila}").Value = "Descripción";
            ws.Cell($"A{fila}").Style.Font.Bold = true;
            fila++;
            ws.Cell($"A{fila}").Value = tarea.Descripcion;
            ws.Range($"A{fila}:D{fila}").Merge();
            ws.Cell($"A{fila}").Style.Alignment.WrapText = true;
        }

        // Detalles de ejecución
        fila += 2;
        ws.Cell($"A{fila}").Value = "Detalles de Ejecución";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"A{fila}").Style.Font.FontSize = 12;

        fila++;
        ws.Cell($"A{fila}").Value = "Estado:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = tarea.Estado.ToString();

        fila++;
        ws.Cell($"A{fila}").Value = "Prioridad:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = tarea.Prioridad.ToString();

        fila++;
        ws.Cell($"A{fila}").Value = "Fecha Inicio:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = tarea.FechaInicio.ToString("dd/MM/yyyy");

        fila++;
        ws.Cell($"A{fila}").Value = "Fecha Fin:";
        ws.Cell($"A{fila}").Style.Font.Bold = true;
        ws.Cell($"B{fila}").Value = tarea.FechaFin?.ToString("dd/MM/yyyy") ?? "No especificada";

        // Presupuesto
        if (tarea.PresupuestoEstimado > 0 || tarea.CostesReales > 0)
        {
            fila += 2;
            ws.Cell($"A{fila}").Value = "Presupuesto";
            ws.Cell($"A{fila}").Style.Font.Bold = true;
            ws.Cell($"A{fila}").Style.Font.FontSize = 12;

            fila++;
            ws.Cell($"A{fila}").Value = "Presupuesto Estimado:";
            ws.Cell($"A{fila}").Style.Font.Bold = true;
            ws.Cell($"B{fila}").Value = tarea.PresupuestoEstimado;
            ws.Cell($"B{fila}").Style.NumberFormat.Format = "#,##0.00 €";

            fila++;
            ws.Cell($"A{fila}").Value = "Costes Reales:";
            ws.Cell($"A{fila}").Style.Font.Bold = true;
            ws.Cell($"B{fila}").Value = tarea.CostesReales;
            ws.Cell($"B{fila}").Style.NumberFormat.Format = "#,##0.00 €";
        }

        // Responsables
        if (tarea.UsuariosAsignados?.Any() == true)
        {
            fila += 2;
            ws.Cell($"A{fila}").Value = "Responsables";
            ws.Cell($"A{fila}").Style.Font.Bold = true;
            ws.Cell($"A{fila}").Style.Font.FontSize = 12;

            fila++;
            var responsables = string.Join(", ", tarea.UsuariosAsignados.Select(u => u.NombreCompleto));
            ws.Cell($"A{fila}").Value = responsables;
            ws.Range($"A{fila}:D{fila}").Merge();
        }

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(workbook);
    }

    /// <summary>
    /// Genera un Excel con la lista de contratos laborales (RRHH)
    /// </summary>
    public byte[] GenerarExcelContratos(List<Contrato> contratos, string titulo = "Contratos Laborales")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(titulo);

        ws.Cell("A1").Value = "Gestión de Obras - Contratos Laborales";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:G1").Merge();

        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;
        ws.Range("A2:G2").Merge();

        var fila = 4;
        var cabeceras = new[] { "Trabajador", "Tipo Contrato", "Fecha Inicio", "Fecha Fin", "Jornada (h/sem)", "Salario Bruto Anual", "Estado" };
        for (int i = 0; i < cabeceras.Length; i++)
        {
            var celda = ws.Cell(fila, i + 1);
            celda.Value = cabeceras[i];
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        foreach (var c in contratos.OrderBy(x => x.Usuario?.NombreCompleto ?? ""))
        {
            fila++;
            ws.Cell(fila, 1).Value = c.Usuario?.NombreCompleto ?? "-";
            ws.Cell(fila, 2).Value = c.TipoContrato.ToString();
            ws.Cell(fila, 3).Value = c.FechaInicio.ToString("dd/MM/yyyy");
            ws.Cell(fila, 4).Value = c.FechaFin?.ToString("dd/MM/yyyy") ?? "-";
            ws.Cell(fila, 5).Value = c.HorasSemanales;
            ws.Cell(fila, 6).Value = c.SalarioBrutoAnual;
            ws.Cell(fila, 6).Style.NumberFormat.Format = "#,##0 €";
            ws.Cell(fila, 7).Value = c.Activo ? "Activo" : "Finalizado";
        }

        fila += 2;
        ws.Cell(fila, 1).Value = "Resumen";
        ws.Cell(fila, 1).Style.Font.Bold = true;
        ws.Cell(fila + 1, 1).Value = "Total contratos:";
        ws.Cell(fila + 1, 2).Value = contratos.Count;
        ws.Cell(fila + 2, 1).Value = "Activos:";
        ws.Cell(fila + 2, 2).Value = contratos.Count(c => c.Activo);
        ws.Cell(fila + 3, 1).Value = "Finalizados:";
        ws.Cell(fila + 3, 2).Value = contratos.Count(c => !c.Activo);

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(workbook);
    }

    /// <summary>
    /// Genera un Excel con la lista de horarios asignados (RRHH)
    /// </summary>
    public byte[] GenerarExcelHorarios(List<HorarioAsignado> horarios, string titulo = "Horarios Asignados")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(titulo);

        ws.Cell("A1").Value = "Gestión de Obras - Horarios Asignados";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:F1").Merge();

        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;
        ws.Range("A2:F2").Merge();

        var fila = 4;
        var cabeceras = new[] { "Usuario", "Proyecto", "Día Semana", "Turno", "Vigencia Desde", "Vigencia Hasta" };
        for (int i = 0; i < cabeceras.Length; i++)
        {
            var celda = ws.Cell(fila, i + 1);
            celda.Value = cabeceras[i];
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        foreach (var h in horarios.OrderBy(x => x.Usuario?.NombreCompleto).ThenBy(x => x.DiaSemana))
        {
            fila++;
            ws.Cell(fila, 1).Value = h.Usuario?.NombreCompleto ?? "-";
            ws.Cell(fila, 2).Value = h.Proyecto?.Nombre ?? "-";
            ws.Cell(fila, 3).Value = h.DiaSemana.ToString();
            ws.Cell(fila, 4).Value = h.TipoTurno.ToString();
            ws.Cell(fila, 5).Value = h.VigenteDesde.ToString("dd/MM/yyyy");
            ws.Cell(fila, 6).Value = h.VigenteHasta?.ToString("dd/MM/yyyy") ?? "-";
        }

        fila += 2;
        ws.Cell(fila, 1).Value = "Resumen";
        ws.Cell(fila, 1).Style.Font.Bold = true;
        ws.Cell(fila + 1, 1).Value = "Total asignaciones:";
        ws.Cell(fila + 1, 2).Value = horarios.Count;
        ws.Cell(fila + 2, 1).Value = "Usuarios únicos:";
        ws.Cell(fila + 2, 2).Value = horarios.Select(h => h.UsuarioId).Distinct().Count();
        ws.Cell(fila + 3, 1).Value = "Proyectos:";
        ws.Cell(fila + 3, 2).Value = horarios.Select(h => h.ProyectoId).Distinct().Count();

        ws.Columns().AdjustToContents();
        return WorkbookToBytes(workbook);
    }

    /// <summary>
    /// Genera un Excel con la lista de fichajes (RRHH)
    /// </summary>
    public byte[] GenerarExcelFichajes(List<RegistroFichaje> fichajes, string titulo = "Fichajes")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(titulo);

        ws.Cell("A1").Value = "Gestión de Obras - Fichajes de Empleados";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:F1").Merge();

        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;
        ws.Range("A2:F2").Merge();

        var fila = 4;
        var cabeceras = new[] { "Usuario", "Fecha", "Hora Entrada", "Hora Salida", "Horas Totales", "Estado" };
        for (int i = 0; i < cabeceras.Length; i++)
        {
            var celda = ws.Cell(fila, i + 1);
            celda.Value = cabeceras[i];
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        foreach (var f in fichajes.OrderBy(x => x.Usuario?.NombreCompleto).ThenByDescending(x => x.Fecha))
        {
            fila++;
            ws.Cell(fila, 1).Value = f.Usuario?.NombreCompleto ?? "-";
            ws.Cell(fila, 2).Value = f.Fecha.ToString("dd/MM/yyyy");
            ws.Cell(fila, 3).Value = f.HoraEntrada.ToString("HH:mm");
            ws.Cell(fila, 4).Value = f.HoraSalida?.ToString("HH:mm") ?? "-";
            ws.Cell(fila, 5).Value = f.TotalHoras?.ToString("F2") ?? "-";
            ws.Cell(fila, 6).Value = f.Estado.ToString();
        }

        fila += 2;
        ws.Cell(fila, 1).Value = "Resumen";
        ws.Cell(fila, 1).Style.Font.Bold = true;
        ws.Cell(fila + 1, 1).Value = "Total fichajes:";
        ws.Cell(fila + 1, 2).Value = fichajes.Count;
        ws.Cell(fila + 2, 1).Value = "Horas totales:";
        ws.Cell(fila + 2, 2).Value = fichajes.Where(f => f.TotalHoras.HasValue).Sum(f => f.TotalHoras.Value);
        ws.Cell(fila + 2, 2).Style.NumberFormat.Format = "0.00";
        ws.Cell(fila + 3, 1).Value = "Pendientes de validar:";
        ws.Cell(fila + 3, 2).Value = fichajes.Count(f => f.Estado == EstadoFichaje.Pendiente);

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
