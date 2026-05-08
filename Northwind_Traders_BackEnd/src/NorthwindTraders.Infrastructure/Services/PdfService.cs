using QuestPDF.Fluent;          // QuestPDF — Document, Column, Row, Table, etc.
using QuestPDF.Helpers;         // QuestPDF — Colors, PageSizes
using QuestPDF.Infrastructure;  // QuestPDF — IDocument, LicenseType
using NorthwindTraders.Application.DTOs.Order;
using NorthwindTraders.Application.Interfaces;


namespace NorthwindTraders.Infrastructure.Services;

public class PdfService : IPdfService
{
    // Returns the PDF as byte[] — raw binary of the file in memory
    public byte[] GenerateOrderPdf(OrderDetailDto order)
    {
        try
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
        catch (ArgumentNullException ex)
        {
            throw new InvalidOperationException($"A required value was null while generating the PDF: {ex.Message}", ex);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"Invalid argument while generating the PDF: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate PDF document: {ex.Message}", ex);
        }
    }

    // ── BULK ORDERS REPORT ────────────────────────────────────────────────────
    // Generates a landscape summary PDF containing every order in the supplied list.
    public byte[] GenerateOrdersReportPdf(IEnumerable<OrderSummaryDto> orders)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var orderList = orders.ToList();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    // ── HEADER ────────────────────────────────────────────────
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem()
                                .Text("Northwind Traders — Orders Report")
                                .FontSize(16).Bold()
                                .FontColor(Color.FromHex("#1a1a2e"));

                            row.ConstantItem(200).AlignRight()
                                .Text($"Generated {DateTime.UtcNow:MMM dd, yyyy}")
                                .FontSize(9)
                                .FontColor(Color.FromHex("#666666"));
                        });

                        col.Item().PaddingTop(4)
                            .LineHorizontal(1)
                            .LineColor(Color.FromHex("#0f3460"));

                        col.Item().PaddingTop(4)
                            .Text($"Total orders: {orderList.Count}  •  Total freight: ${orderList.Sum(o => o.Freight ?? 0):N2}")
                            .FontSize(9)
                            .FontColor(Color.FromHex("#444444"));
                    });

                    // ── CONTENT ───────────────────────────────────────────────
                    page.Content().PaddingTop(12).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(52);   // Order ID
                            cols.RelativeColumn(3);    // Customer
                            cols.RelativeColumn(3);    // Employee
                            cols.ConstantColumn(72);   // Order Date
                            cols.ConstantColumn(72);   // Shipped Date
                            cols.RelativeColumn(2);    // Ship Country
                            cols.RelativeColumn(2);    // Region
                            cols.ConstantColumn(62);   // Freight
                            cols.ConstantColumn(72);   // Status
                        });

                        // Header row
                        table.Header(header =>
                        {
                            var hs = TextStyle.Default.Bold().FontColor(Colors.White).FontSize(9);
                            var bg = Color.FromHex("#1a1a2e");

                            header.Cell().Background(bg).Padding(5).Text("Order #").Style(hs);
                            header.Cell().Background(bg).Padding(5).Text("Customer").Style(hs);
                            header.Cell().Background(bg).Padding(5).Text("Employee").Style(hs);
                            header.Cell().Background(bg).Padding(5).Text("Order Date").Style(hs);
                            header.Cell().Background(bg).Padding(5).Text("Shipped").Style(hs);
                            header.Cell().Background(bg).Padding(5).Text("Country").Style(hs);
                            header.Cell().Background(bg).Padding(5).Text("Region").Style(hs);
                            header.Cell().Background(bg).Padding(5).AlignRight().Text("Freight").Style(hs);
                            header.Cell().Background(bg).Padding(5).Text("Status").Style(hs);
                        });

                        // Data rows — alternating background
                        var rowIndex = 0;
                        foreach (var o in orderList)
                        {
                            var bg = rowIndex % 2 == 0
                                ? Colors.White
                                : Color.FromHex("#f5f5f5");

                            var statusColor = (o.ShipmentStatus ?? "").ToLower() switch
                            {
                                var s when s.Contains("ship")    => "#27ae60",
                                var s when s.Contains("complet") => "#2980b9",
                                var s when s.Contains("cancel")  => "#e74c3c",
                                var s when s.Contains("pending") => "#f39c12",
                                _                                => "#7f8c8d"
                            };

                            table.Cell().Background(bg).Padding(5).Text(o.OrderId.ToString());
                            table.Cell().Background(bg).Padding(5).Text(o.CustomerName ?? "—");
                            table.Cell().Background(bg).Padding(5).Text(o.EmployeeName ?? "—");
                            table.Cell().Background(bg).Padding(5).Text(o.OrderDate?.ToString("yyyy-MM-dd") ?? "—");
                            table.Cell().Background(bg).Padding(5).Text(o.ShippedDate?.ToString("yyyy-MM-dd") ?? "—");
                            table.Cell().Background(bg).Padding(5).Text(o.ShipCountry ?? "—");
                            table.Cell().Background(bg).Padding(5).Text(o.ShipRegion ?? "—");
                            table.Cell().Background(bg).Padding(5).AlignRight().Text($"${o.Freight ?? 0:N2}");
                            table.Cell().Background(bg).Padding(3)
                                .Background(Color.FromHex(statusColor))
                                .Padding(3)
                                .Text(o.ShipmentStatus ?? "—")
                                .FontColor(Colors.White)
                                .FontSize(8);

                            rowIndex++;
                        }
                    });

                    // ── FOOTER ────────────────────────────────────────────────
                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Northwind Traders  •  Page ");
                        txt.CurrentPageNumber();
                        txt.Span(" of ");
                        txt.TotalPages();
                    });
                });
            })
            .GeneratePdf();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate orders report PDF: {ex.Message}", ex);
        }
    }
}