using GestionObras.Core.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionObras.Web.Services;

public class ExportPdfService
{
    public byte[] GenerarInformeFacturas(List<Factura> facturas, string titulo = "Informe de Facturas")
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComponerCabecera(c, titulo));
                page.Content().Element(c => ComponerTablaFacturas(c, facturas));
                page.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    public byte[] GenerarInformePresupuestos(List<Presupuesto> presupuestos, string titulo = "Informe de Presupuestos")
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComponerCabecera(c, titulo));
                page.Content().Element(c => ComponerTablaPresupuestos(c, presupuestos));
                page.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    public byte[] GenerarInformeEmpleados(List<Empleado> empleados, string titulo = "Informe de Empleados")
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComponerCabecera(c, titulo));
                page.Content().Element(c => ComponerTablaEmpleados(c, empleados));
                page.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    public byte[] GenerarInformeTareas(List<Tarea> tareas, string titulo = "Informe de Tareas")
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComponerCabecera(c, titulo));
                page.Content().Element(c => ComponerTablaTareas(c, tareas));
                page.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    private void ComponerCabecera(IContainer container, string titulo)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Gestión de Obras").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                    c.Item().Text(titulo).FontSize(12).FontColor(Colors.Grey.Darken1);
                });
                row.ConstantItem(200).AlignRight().Column(c =>
                {
                    c.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(9);
                    c.Item().Text($"Hora: {DateTime.Now:HH:mm}").FontSize(9);
                });
            });
            col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
        });
    }

    private void ComponerPie(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("Gestión de Proyectos de Construcción para PYMEs").FontSize(8).FontColor(Colors.Grey.Medium);
            row.ConstantItem(100).AlignRight().Text(text =>
            {
                text.Span("Página ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
                text.Span(" de ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        });
    }

    private void ComponerTablaFacturas(IContainer container, List<Factura> facturas)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);  // Número
                    columns.ConstantColumn(70);  // Fecha
                    columns.RelativeColumn(2);   // Concepto
                    columns.RelativeColumn(1);   // Proveedor
                    columns.ConstantColumn(70);  // Base
                    columns.ConstantColumn(55);  // IVA
                    columns.ConstantColumn(75);  // Total
                    columns.ConstantColumn(65);  // Estado
                });

                // Cabecera
                table.Header(header =>
                {
                    foreach (var h in new[] { "Nº Factura", "Fecha", "Concepto", "Proveedor", "Base Imp.", "IVA", "Total", "Estado" })
                        header.Cell().Background(Colors.Blue.Darken2).Padding(4)
                            .Text(h).FontColor(Colors.White).Bold().FontSize(8);
                });

                // Filas de datos
                foreach (var f in facturas)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(f.NumeroFactura).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(f.FechaEmision.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(f.Concepto).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(f.Proveedor?.Nombre ?? "—").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(f.BaseImponible.ToString("N2") + " €").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(f.IVA.ToString("N2") + " €").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(f.ImporteTotal.ToString("N2") + " €").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(f.Estado.ToString()).FontSize(8);
                }

                // Totales
                table.Cell().ColumnSpan(4).Padding(4).AlignRight().Text("TOTALES:").Bold().FontSize(9);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(facturas.Sum(f => f.BaseImponible).ToString("N2") + " €").Bold().FontSize(9);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(facturas.Sum(f => f.IVA).ToString("N2") + " €").Bold().FontSize(9);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(facturas.Sum(f => f.ImporteTotal).ToString("N2") + " €").Bold().FontSize(9);
                table.Cell().Padding(4).Text("");
            });

            col.Item().PaddingTop(15).Column(resumen =>
            {
                resumen.Item().Text("Resumen").Bold().FontSize(11);
                resumen.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Pendientes: {facturas.Count(f => f.Estado == EstadoFactura.Pendiente)}").FontSize(9);
                    row.RelativeItem().Text($"Pagadas: {facturas.Count(f => f.Estado == EstadoFactura.Pagada)}").FontSize(9);
                    row.RelativeItem().Text($"Vencidas: {facturas.Count(f => f.Estado == EstadoFactura.Vencida)}").FontSize(9);
                    row.RelativeItem().Text($"Total facturas: {facturas.Count}").FontSize(9);
                });
            });
        });
    }

    private void ComponerTablaPresupuestos(IContainer container, List<Presupuesto> presupuestos)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(80);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(100);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    foreach (var h in new[] { "ID", "Fecha", "Proyecto", "Total", "Observaciones" })
                        header.Cell().Background(Colors.Blue.Darken2).Padding(4)
                            .Text(h).FontColor(Colors.White).Bold().FontSize(8);
                });

                foreach (var p in presupuestos)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"PRE-{p.Id:000}").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(p.FechaElaboracion.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(p.Proyecto?.Nombre ?? $"Proyecto #{p.ProyectoId}").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(p.Total.ToString("N2") + " €").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(p.Observaciones ?? "").FontSize(8);
                }

                table.Cell().ColumnSpan(3).Padding(4).AlignRight().Text("TOTAL:").Bold().FontSize(9);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(presupuestos.Sum(p => p.Total).ToString("N2") + " €").Bold().FontSize(9);
                table.Cell().Padding(4).Text("");
            });

            col.Item().PaddingTop(15).Column(resumen =>
            {
                resumen.Item().Text("Resumen").Bold().FontSize(11);
                resumen.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Total presupuestos: {presupuestos.Count}").FontSize(9);
                    row.RelativeItem().Text($"Promedio: {(presupuestos.Any() ? presupuestos.Average(p => p.Total) : 0):N2} €").FontSize(9);
                    row.RelativeItem().Text($"Mayor: {(presupuestos.Any() ? presupuestos.Max(p => p.Total) : 0):N2} €").FontSize(9);
                });
            });
        });
    }

    private void ComponerTablaEmpleados(IContainer container, List<Empleado> empleados)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(80);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(80);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.ConstantColumn(55);
                });

                table.Header(header =>
                {
                    foreach (var h in new[] { "Nombre", "DNI", "Email", "Teléfono", "Departamento", "Cargo", "Estado" })
                        header.Cell().Background(Colors.Blue.Darken2).Padding(4)
                            .Text(h).FontColor(Colors.White).Bold().FontSize(8);
                });

                foreach (var e in empleados)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{e.Nombre} {e.Apellidos}").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(e.DNI).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(e.Email).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(e.Telefono).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(e.Departamento).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(e.Cargo).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(e.Activo ? "Activo" : "Inactivo").FontSize(8);
                }
            });

            col.Item().PaddingTop(15).Column(resumen =>
            {
                resumen.Item().Text("Resumen").Bold().FontSize(11);
                resumen.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Total empleados: {empleados.Count}").FontSize(9);
                    row.RelativeItem().Text($"Activos: {empleados.Count(e => e.Activo)}").FontSize(9);
                    row.RelativeItem().Text($"Inactivos: {empleados.Count(e => !e.Activo)}").FontSize(9);
                });
            });
        });

    }

    private void ComponerTablaTareas(IContainer container, List<Tarea> tareas)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(55);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(75);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    foreach (var h in new[] { "ID", "Proyecto", "Tarea", "Estado", "Prioridad", "Inicio", "Fin", "Responsables" })
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(4)
                            .Text(h).FontColor(Colors.White).Bold().FontSize(8);
                    }
                });

                foreach (var t in tareas.OrderBy(t => t.Prioridad).ThenBy(t => t.FechaInicio))
                {
                    var responsables = t.UsuariosAsignados != null && t.UsuariosAsignados.Any()
                        ? string.Join(", ", t.UsuariosAsignados.Select(u => u.NombreCompleto).Where(n => !string.IsNullOrWhiteSpace(n)))
                        : "Sin asignar";

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"T-{t.Id:000}").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(t.Proyecto?.Nombre ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(t.Nombre).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(t.Estado.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(t.Prioridad.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(t.FechaInicio.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(t.FechaFin?.ToString("dd/MM/yyyy") ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(responsables).FontSize(8);
                }
            });

            col.Item().PaddingTop(15).Column(resumen =>
            {
                resumen.Item().Text("Resumen").Bold().FontSize(11);
                resumen.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Total tareas: {tareas.Count}").FontSize(9);
                    row.RelativeItem().Text($"Pendientes: {tareas.Count(t => t.Estado == EstadoTarea.Pendiente)}").FontSize(9);
                    row.RelativeItem().Text($"En curso: {tareas.Count(t => t.Estado == EstadoTarea.EnCurso)}").FontSize(9);
                    row.RelativeItem().Text($"Bloqueadas: {tareas.Count(t => t.Estado == EstadoTarea.Bloqueado)}").FontSize(9);
                    row.RelativeItem().Text($"Finalizadas: {tareas.Count(t => t.Estado == EstadoTarea.Finalizado)}").FontSize(9);
                });
            });
        });
    }

    /// <summary>
    /// Genera un PDF con los detalles de una factura individual
    /// </summary>
    public byte[] GenerarInformeFacturaIndividual(Factura factura)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComponerCabecera(c, $"Factura {factura.NumeroFactura}"));
                page.Content().Element(c => ComponerDetalleFactura(c, factura));
                page.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    /// <summary>
    /// Genera un PDF con los detalles de una tarea individual
    /// </summary>
    public byte[] GenerarInformeTareaIndividual(Tarea tarea)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComponerCabecera(c, $"Tarea: {tarea.Nombre}"));
                page.Content().Element(c => ComponerDetalleTarea(c, tarea));
                page.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    private void ComponerDetalleFactura(IContainer container, Factura factura)
    {
        container.Column(col =>
        {
            // Información básica
            col.Item().Column(c =>
            {
                c.Item().Text("Información General").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                c.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(cc =>
                    {
                        cc.Item().Text("Número de Factura:").Bold();
                        cc.Item().Text(factura.NumeroFactura).FontSize(11);
                    });
                    row.RelativeItem().Column(cc =>
                    {
                        cc.Item().Text("Fecha de Emisión:").Bold();
                        cc.Item().Text(factura.FechaEmision.ToString("dd/MM/yyyy")).FontSize(11);
                    });
                });
            });

            // Detalles económicos
            col.Item().PaddingTop(15).Column(c =>
            {
                c.Item().Text("Detalles Económicos").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                c.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(120);
                    });

                    table.Cell().Padding(5).Text("Concepto:").Bold();
                    table.Cell().Padding(5).Text(factura.Concepto);

                    table.Cell().Padding(5).Text("Proveedor:").Bold();
                    table.Cell().Padding(5).Text(factura.Proveedor?.Nombre ?? "No especificado");

                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Base Imponible:").Bold();
                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text($"{factura.BaseImponible:N2} €");

                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("IVA:").Bold();
                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text($"{factura.IVA:N2} €");

                    table.Cell().Background(Colors.Blue.Lighten4).Padding(5).Text("TOTAL:").Bold().FontSize(11);
                    table.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text($"{factura.ImporteTotal:N2} €").Bold().FontSize(11);
                });
            });

            // Estado
            col.Item().PaddingTop(15).Column(c =>
            {
                c.Item().Text("Estado").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                c.Item().PaddingTop(10).Text($"Estado Actual: {factura.Estado}").FontSize(11);
            });

            // Pie de página con información del documento
            col.Item().PaddingTop(30).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComponerDetalleTarea(IContainer container, Tarea tarea)
    {
        container.Column(col =>
        {
            // Información básica
            col.Item().Column(c =>
            {
                c.Item().Text("Información General").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                c.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(cc =>
                    {
                        cc.Item().Text("ID Tarea:").Bold();
                        cc.Item().Text($"T-{tarea.Id:000}").FontSize(11);
                    });
                    row.RelativeItem().Column(cc =>
                    {
                        cc.Item().Text("Proyecto:").Bold();
                        cc.Item().Text(tarea.Proyecto?.Nombre ?? "Sin asignar").FontSize(11);
                    });
                });
            });

            // Descripción
            if (!string.IsNullOrWhiteSpace(tarea.Descripcion))
            {
                col.Item().PaddingTop(15).Column(c =>
                {
                    c.Item().Text("Descripción").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                    c.Item().PaddingTop(10).Text(tarea.Descripcion).FontSize(10);
                });
            }

            // Detalles de ejecución
            col.Item().PaddingTop(15).Column(c =>
            {
                c.Item().Text("Detalles de Ejecución").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                c.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Estado:").Bold();
                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text(tarea.Estado.ToString());

                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Prioridad:").Bold();
                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text(tarea.Prioridad.ToString());

                    table.Cell().Padding(5).Text("Fecha Inicio:").Bold();
                    table.Cell().Padding(5).Text(tarea.FechaInicio.ToString("dd/MM/yyyy"));

                    table.Cell().Padding(5).Text("Fecha Fin:").Bold();
                    table.Cell().Padding(5).Text(tarea.FechaFin?.ToString("dd/MM/yyyy") ?? "No especificada");
                });
            });

            // Presupuesto
            if (tarea.PresupuestoEstimado > 0 || tarea.CostesReales > 0)
            {
                col.Item().PaddingTop(15).Column(c =>
                {
                    c.Item().Text("Presupuesto").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                    c.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.ConstantColumn(100);
                        });

                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Presupuesto Estimado:").Bold();
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text($"{tarea.PresupuestoEstimado:N2} €");

                        table.Cell().Background(Colors.Blue.Lighten4).Padding(5).Text("Costes Reales:").Bold();
                        table.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text($"{tarea.CostesReales:N2} €").Bold();
                    });
                });
            }

            // Responsables
            if (tarea.UsuariosAsignados?.Any() == true)
            {
                col.Item().PaddingTop(15).Column(c =>
                {
                    c.Item().Text("Responsables Asignados").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                    c.Item().PaddingTop(10).Text(string.Join(", ", tarea.UsuariosAsignados.Select(u => u.NombreCompleto))).FontSize(10);
                });
            }

            // Pie de página
            col.Item().PaddingTop(30).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    /// <summary>
    /// Genera un PDF con la lista de contratos laborales
    /// </summary>
    public byte[] GenerarInformeContratos(List<Contrato> contratos, string titulo = "Informe de Contratos Laborales")
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComponerCabecera(c, titulo));
                page.Content().Element(c => ComponerTablaContratos(c, contratos));
                page.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    /// <summary>
    /// Genera un PDF con la lista de horarios asignados
    /// </summary>
    public byte[] GenerarInformeHorarios(List<HorarioAsignado> horarios, string titulo = "Informe de Horarios")
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComponerCabecera(c, titulo));
                page.Content().Element(c => ComponerTablaHorarios(c, horarios));
                page.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    /// <summary>
    /// Genera un PDF con la lista de fichajes
    /// </summary>
    public byte[] GenerarInformeFichajes(List<RegistroFichaje> fichajes, string titulo = "Informe de Fichajes")
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComponerCabecera(c, titulo));
                page.Content().Element(c => ComponerTablaFichajes(c, fichajes));
                page.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    private void ComponerTablaContratos(IContainer container, List<Contrato> contratos)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);    // Trabajador
                    columns.ConstantColumn(80);   // Tipo
                    columns.ConstantColumn(75);   // Inicio
                    columns.ConstantColumn(75);   // Fin
                    columns.ConstantColumn(80);   // Jornada
                    columns.ConstantColumn(70);   // Salario
                    columns.ConstantColumn(55);   // Estado
                });

                table.Header(header =>
                {
                    foreach (var h in new[] { "Trabajador", "Tipo Contrato", "Fecha Inicio", "Fecha Fin", "Jornada (h/sem)", "Salario Bruto", "Estado" })
                        header.Cell().Background(Colors.Blue.Darken2).Padding(4)
                            .Text(h).FontColor(Colors.White).Bold().FontSize(8);
                });

                foreach (var c in contratos.OrderBy(x => x.Usuario?.NombreCompleto ?? ""))
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.Usuario?.NombreCompleto ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.TipoContrato.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.FechaInicio.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.FechaFin?.ToString("dd/MM/yyyy") ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{c.HorasSemanales:#,##0.00}").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{c.SalarioBrutoAnual:N0} €").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.Activo ? "Activo" : "Finalizado").FontSize(8);
                }
            });

            col.Item().PaddingTop(15).Column(resumen =>
            {
                resumen.Item().Text("Resumen").Bold().FontSize(11);
                resumen.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Total contratos: {contratos.Count}").FontSize(9);
                    row.RelativeItem().Text($"Activos: {contratos.Count(c => c.Activo)}").FontSize(9);
                    row.RelativeItem().Text($"Finalizados: {contratos.Count(c => !c.Activo)}").FontSize(9);
                });
            });
        });
    }

    private void ComponerTablaHorarios(IContainer container, List<HorarioAsignado> horarios)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);    // Usuario
                    columns.RelativeColumn(2);    // Proyecto
                    columns.ConstantColumn(70);   // Día
                    columns.ConstantColumn(80);   // Turno
                    columns.ConstantColumn(90);   // Vigencia Desde
                    columns.ConstantColumn(90);   // Vigencia Hasta
                });

                table.Header(header =>
                {
                    foreach (var h in new[] { "Usuario", "Proyecto", "Día Semana", "Turno", "Vigencia Desde", "Vigencia Hasta" })
                        header.Cell().Background(Colors.Blue.Darken2).Padding(4)
                            .Text(h).FontColor(Colors.White).Bold().FontSize(8);
                });

                foreach (var h in horarios.OrderBy(x => x.Usuario?.NombreCompleto).ThenBy(x => x.DiaSemana))
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(h.Usuario?.NombreCompleto ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(h.Proyecto?.Nombre ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(h.DiaSemana.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(h.TipoTurno.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(h.VigenteDesde.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(h.VigenteHasta?.ToString("dd/MM/yyyy") ?? "-").FontSize(8);
                }
            });

            col.Item().PaddingTop(15).Column(resumen =>
            {
                resumen.Item().Text("Resumen").Bold().FontSize(11);
                resumen.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Total asignaciones: {horarios.Count}").FontSize(9);
                    row.RelativeItem().Text($"Usuarios únicos: {horarios.Select(h => h.UsuarioId).Distinct().Count()}").FontSize(9);
                    row.RelativeItem().Text($"Proyectos: {horarios.Select(h => h.ProyectoId).Distinct().Count()}").FontSize(9);
                });
            });
        });
    }

    private void ComponerTablaFichajes(IContainer container, List<RegistroFichaje> fichajes)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);    // Usuario
                    columns.ConstantColumn(80);   // Fecha
                    columns.ConstantColumn(85);   // Hora Entrada
                    columns.ConstantColumn(85);   // Hora Salida
                    columns.ConstantColumn(80);   // Horas Totales
                    columns.ConstantColumn(70);   // Estado
                });

                table.Header(header =>
                {
                    foreach (var h in new[] { "Usuario", "Fecha", "Hora Entrada", "Hora Salida", "Horas Totales", "Estado" })
                        header.Cell().Background(Colors.Blue.Darken2).Padding(4)
                            .Text(h).FontColor(Colors.White).Bold().FontSize(8);
                });

                foreach (var f in fichajes.OrderBy(x => x.Usuario?.NombreCompleto).ThenByDescending(x => x.Fecha))
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(f.Usuario?.NombreCompleto ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(f.Fecha.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(f.HoraEntrada.ToString("HH:mm")).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(f.HoraSalida?.ToString("HH:mm") ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{(f.TotalHoras?.ToString("F2") ?? "-")} h").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(f.Estado.ToString()).FontSize(8);
                }
            });

            col.Item().PaddingTop(15).Column(resumen =>
            {
                resumen.Item().Text("Resumen").Bold().FontSize(11);
                resumen.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Total fichajes: {fichajes.Count}").FontSize(9);
                    row.RelativeItem().Text($"Horas totales: {fichajes.Where(f => f.TotalHoras.HasValue).Sum(f => f.TotalHoras.Value):F1}h").FontSize(9);
                    row.RelativeItem().Text($"Pendientes: {fichajes.Count(f => f.Estado == EstadoFichaje.Pendiente)}").FontSize(9);
                });
            });
        });
    }
}
