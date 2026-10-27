using System.Globalization;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using NorthWind.Sales.Entities.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NorthWind.Sales.WebApi.Invoices;

public sealed class InvoicePdfExporter : IInvoicePdfExporter
{
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("es-EC");
    public byte[] Export(InvoiceDto invoice)
    {
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(36); page.DefaultTextStyle(s => s.FontSize(10));
            page.Header().Text($"NORTHWIND / FACTURA DE VENTA #{invoice.Id}").FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
            page.Content().PaddingVertical(20).Column(column =>
            {
                column.Spacing(7);
                column.Item().Text($"Fecha (UTC): {invoice.OrderDate:dd/MM/yyyy HH:mm} | Creada por: {invoice.CreatedBy}");
                column.Item().Text($"Cliente: {invoice.CustomerId} - {invoice.CustomerName}").Bold();
                column.Item().Text($"Dirección de envío: {invoice.ShipAddress}, {invoice.ShipCity}, {invoice.ShipCountry} {invoice.ShipPostalCode}");
                column.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(c => { c.ConstantColumn(45); c.RelativeColumn(); c.ConstantColumn(60); c.ConstantColumn(45); c.ConstantColumn(65); });
                    table.Header(h => { foreach (var title in new[] { "Código", "Producto", "Precio unitario", "Cant.", "Importe" }) h.Cell().BorderBottom(1).Padding(5).Text(title).Bold(); });
                    foreach (var line in invoice.Details)
                    {
                        table.Cell().Padding(5).Text(line.ProductId.ToString());
                        table.Cell().Padding(5).Text(line.ProductName);
                        table.Cell().Padding(5).AlignRight().Text(line.UnitPrice.ToString("F2", DisplayCulture));
                        table.Cell().Padding(5).AlignRight().Text(line.Quantity.ToString());
                        table.Cell().Padding(5).AlignRight().Text(line.Amount.ToString("F2", DisplayCulture));
                    }
                });
                foreach (var total in new[] { ("Subtotal",invoice.Subtotal), ($"Descuento ({invoice.DiscountPercent:0.##}%)",invoice.DiscountAmount),
                    ("Base imponible",invoice.TaxableAmount), ($"IVA ({invoice.VatRate * 100:0.##}%)",invoice.VatAmount), ("TOTAL",invoice.Total) })
                    column.Item().AlignRight().Text($"{total.Item1}: USD {total.Item2.ToString("F2", DisplayCulture)}").Bold();
                column.Item().PaddingTop(15).Text("Comprobante académico de venta. No es una factura electrónica autorizada.").FontSize(9);
            });
            page.Footer().AlignCenter().Text(text => { text.Span("NorthWind | Página "); text.CurrentPageNumber(); text.Span(" / "); text.TotalPages(); });
        })).GeneratePdf();
    }
}
