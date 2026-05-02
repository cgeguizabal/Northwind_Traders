using QuestPDF.Fluent;          // QuestPDF — Document, Column, Row, Table, etc.
using QuestPDF.Helpers;         // QuestPDF — Colors, PageSizes
using QuestPDF.Infrastructure;  // QuestPDF — IDocument, LicenseType
using NorthwindTraders.Application.DTOs.Order;

namespace NorthwindTraders.Infrastructure.Services;

public class PdfService
{
    // Returns the PDF as byte[] — raw binary of the file in memory
    public byte[] GenerateOrderPdf(OrderDetailDto order)
    {
        // QuestPDF requires a license declaration — Community is free
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                // ── PAGE SETTINGS ─────────────────────────────────────────────
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                // ── HEADER ────────────────────────────────────────────────────
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Northwind Traders")
                            .FontSize(20).Bold()
                            .FontColor(Color.FromHex("#1a1a2e"));

                        row.ConstantItem(150).AlignRight()
                            .Text($"Order #{order.OrderId}")
                            .FontSize(14).Bold()
                            .FontColor(Color.FromHex("#16213e"));
                    });

                    col.Item().PaddingTop(5)
                        .LineHorizontal(1)
                        .LineColor(Color.FromHex("#0f3460"));
                });

                // ── CONTENT ───────────────────────────────────────────────────
                page.Content().PaddingTop(20).Column(col =>
                {
                    // ── ORDER INFO — 3 COLUMNS ────────────────────────────────
                    col.Item().Row(row =>
                    {
                        // Left — Customer + Dates
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("Customer").Bold().FontSize(11);
                            left.Item().Text(order.CustomerName ?? "—");

                            left.Item().PaddingTop(10).Text("Order Date").Bold().FontSize(11);
                            left.Item().Text(order.OrderDate?.ToString("MMM dd, yyyy") ?? "—");

                            left.Item().PaddingTop(5).Text("Required Date").Bold().FontSize(11);
                            left.Item().Text(order.RequiredDate?.ToString("MMM dd, yyyy") ?? "—");

                            left.Item().PaddingTop(5).Text("Shipped Date").Bold().FontSize(11);
                            left.Item().Text(order.ShippedDate?.ToString("MMM dd, yyyy") ?? "Not shipped yet");
                        });

                        // Middle — Ship To
                        row.RelativeItem().Column(mid =>
                        {
                            mid.Item().Text("Ship To").Bold().FontSize(11);
                            mid.Item().Text(order.ShipName ?? "—");
                            mid.Item().Text(order.ShipAddress ?? "—");
                            mid.Item().Text($"{order.ShipCity}, {order.ShipRegion} {order.ShipPostalCode}");
                            mid.Item().Text(order.ShipCountry ?? "—");

                            mid.Item().PaddingTop(10).Text("Shipper").Bold().FontSize(11);
                            mid.Item().Text(order.ShipperName ?? "—");
                        });

                        // Right — Employee + Status badge
                        row.RelativeItem().Column(right =>
                        {
                            right.Item().Text("Handled By").Bold().FontSize(11);
                            right.Item().Text(order.EmployeeName ?? "—");

                            right.Item().PaddingTop(10).Text("Status").Bold().FontSize(11);

                            // C# switch expression — picks color based on status
                            var statusColor = order.ShipmentStatus switch
                            {
                                "Shipped"   => "#27ae60",  // green
                                "Completed" => "#2980b9",  // blue
                                "Cancelled" => "#e74c3c",  // red
                                "Pending"   => "#f39c12",  // orange
                                _           => "#7f8c8d"   // grey — default
                            };

                            right.Item()
                                .Background(Color.FromHex(statusColor))
                                .Padding(5)
                                .Text(order.ShipmentStatus ?? "—")
                                .FontColor(Colors.White)
                                .Bold();
                        });
                    });

                    // ── DIVIDER ───────────────────────────────────────────────
                    col.Item().PaddingVertical(15)
                        .LineHorizontal(0.5f)
                        .LineColor(Color.FromHex("#cccccc"));

                    // ── LINE ITEMS TABLE ──────────────────────────────────────
                    col.Item().Text("Order Lines").Bold().FontSize(12);

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        // Column widths
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);   // Product name — widest
                            columns.RelativeColumn(2);   // Unit Price
                            columns.RelativeColumn(1);   // Qty
                            columns.RelativeColumn(1);   // Discount
                            columns.RelativeColumn(2);   // Line Total
                        });

                        // Header row
                        table.Header(header =>
                        {
                            var style = TextStyle.Default.Bold().FontColor(Colors.White);

                            header.Cell().Background(Color.FromHex("#1a1a2e")).Padding(6).Text("Product").Style(style);
                            header.Cell().Background(Color.FromHex("#1a1a2e")).Padding(6).Text("Unit Price").Style(style);
                            header.Cell().Background(Color.FromHex("#1a1a2e")).Padding(6).Text("Qty").Style(style);
                            header.Cell().Background(Color.FromHex("#1a1a2e")).Padding(6).Text("Discount").Style(style);
                            header.Cell().Background(Color.FromHex("#1a1a2e")).Padding(6).Text("Total").Style(style);
                        });

                        // Data rows — alternating background color
                        var rowIndex = 0;
                        foreach (var line in order.Lines)
                        {
                            var bg = rowIndex % 2 == 0
                                ? Colors.White
                                : Color.FromHex("#f5f5f5");

                            table.Cell().Background(bg).Padding(6).Text(line.ProductName);
                            table.Cell().Background(bg).Padding(6).Text($"${line.UnitPrice:F2}");
                            table.Cell().Background(bg).Padding(6).Text(line.Quantity.ToString());
                            table.Cell().Background(bg).Padding(6).Text($"{line.Discount * 100:F0}%");
                            table.Cell().Background(bg).Padding(6).Text($"${line.LineTotal:F2}");

                            rowIndex++;
                        }
                    });

                    // ── TOTALS ────────────────────────────────────────────────
                    col.Item().PaddingTop(15).AlignRight().Column(totals =>
                    {
                        var subtotal = order.Lines.Sum(l => l.LineTotal);
                        var freight  = order.Freight ?? 0;
                        var total    = subtotal + freight;

                        totals.Item().Row(row =>
                        {
                            row.ConstantItem(120).Text("Subtotal:").Bold();
                            row.ConstantItem(80).AlignRight().Text($"${subtotal:F2}");
                        });

                        totals.Item().Row(row =>
                        {
                            row.ConstantItem(120).Text("Freight:").Bold();
                            row.ConstantItem(80).AlignRight().Text($"${freight:F2}");
                        });

                        totals.Item().PaddingVertical(4)
                            .LineHorizontal(1)
                            .LineColor(Colors.Black);

                        totals.Item().Row(row =>
                        {
                            row.ConstantItem(120).Text("Total:").Bold().FontSize(13);
                            row.ConstantItem(80).AlignRight().Text($"${total:F2}").Bold().FontSize(13);
                        });
                    });
                });

                // ── FOOTER ────────────────────────────────────────────────────
                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Northwind Traders  •  Generated on ");
                    txt.Span(DateTime.UtcNow.ToString("MMM dd, yyyy HH:mm") + " UTC");
                    txt.Span("  •  Page ");
                    txt.CurrentPageNumber();   // QuestPDF — auto filled
                    txt.Span(" of ");
                    txt.TotalPages();          // QuestPDF — auto filled
                });
            });
        })
        .GeneratePdf();    // QuestPDF Method — returns byte[]
    }
}