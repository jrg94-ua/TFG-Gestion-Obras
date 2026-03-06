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
        container.PaddingTop(10).Table(table =>
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

        container.PaddingTop(15).Column(col =>
        {
            col.Item().Text("Resumen").Bold().FontSize(11);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Pendientes: {facturas.Count(f => f.Estado == EstadoFactura.Pendiente)}").FontSize(9);
                row.RelativeItem().Text($"Pagadas: {facturas.Count(f => f.Estado == EstadoFactura.Pagada)}").FontSize(9);
                row.RelativeItem().Text($"Vencidas: {facturas.Count(f => f.Estado == EstadoFactura.Vencida)}").FontSize(9);
                row.RelativeItem().Text($"Total facturas: {facturas.Count}").FontSize(9);
            });
        });
    }

    private void ComponerTablaPresupuestos(IContainer container, List<Presupuesto> presupuestos)
    {
        container.PaddingTop(10).Table(table =>
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

        container.PaddingTop(15).Column(col =>
        {
            col.Item().Text("Resumen").Bold().FontSize(11);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Total presupuestos: {presupuestos.Count}").FontSize(9);
                row.RelativeItem().Text($"Promedio: {(presupuestos.Any() ? presupuestos.Average(p => p.Total) : 0):N2} €").FontSize(9);
                row.RelativeItem().Text($"Mayor: {(presupuestos.Any() ? presupuestos.Max(p => p.Total) : 0):N2} €").FontSize(9);
            });
        });
    }

    private void ComponerTablaEmpleados(IContainer container, List<Empleado> empleados)
    {
        container.PaddingTop(10).Table(table =>
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

        container.PaddingTop(15).Column(col =>
        {
            col.Item().Text("Resumen").Bold().FontSize(11);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Total empleados: {empleados.Count}").FontSize(9);
                row.RelativeItem().Text($"Activos: {empleados.Count(e => e.Activo)}").FontSize(9);
                row.RelativeItem().Text($"Inactivos: {empleados.Count(e => !e.Activo)}").FontSize(9);
            });
        });
    }
}
