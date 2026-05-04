using NorthwindTraders.Application.DTOs.Order;
using NorthwindTraders.Infrastructure.Services;
using Xunit;

namespace NorthwindTraders.Tests.Services;

public class PdfServiceTests
{
    // ── Builds a minimal but valid OrderDetailDto ─────────────────
    private static OrderDetailDto BuildOrderDto(
        string? shipmentStatus = "Shipped") => new()
    {
        OrderId        = 10248,
        OrderDate      = new DateTime(2024, 1, 15),
        RequiredDate   = new DateTime(2024, 1, 30),
        ShippedDate    = new DateTime(2024, 1, 20),
        Freight        = 32.38m,
        ShipmentStatus = shipmentStatus,
        ShipName       = "Vins et alcools Chevalier",
        ShipAddress    = "59 rue de l'Abbaye",
        ShipCity       = "Reims",
        ShipRegion     = null,
        ShipPostalCode = "51100",
        ShipCountry    = "France",
        CustomerName   = "Vins et alcools Chevalier",
        EmployeeName   = "Steven Buchanan",
        ShipperName    = "Federal Shipping",
        Lines          = new List<OrderLineDto>
        {
            new()
            {
                ProductId   = 11,
                ProductName = "Queso Cabrales",
                UnitPrice   = 14.00m,
                Quantity    = 12,
                Discount    = 0f
            },
            new()
            {
                ProductId   = 42,
                ProductName = "Singaporean Hokkien Fried Mee",
                UnitPrice   = 9.80m,
                Quantity    = 10,
                Discount    = 0f
            }
        }
    };

    // ─────────────────────────────────────────────────────────────
    // GenerateOrderPdf — happy paths
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateOrderPdf_ReturnsByteArray()
    {
        // ARRANGE
        var service = new PdfService();
        var dto     = BuildOrderDto();

        // ACT
        var result = service.GenerateOrderPdf(dto);

        // ASSERT
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateOrderPdf_ReturnsPdfBytes_StartingWithPdfHeader()
    {
        // ARRANGE — every PDF file starts with the magic bytes: %PDF
        var service = new PdfService();
        var dto     = BuildOrderDto();

        // ACT
        var result = service.GenerateOrderPdf(dto);

        // ASSERT — check the PDF magic bytes signature
        Assert.Equal(0x25, result[0]); // %
        Assert.Equal(0x50, result[1]); // P
        Assert.Equal(0x44, result[2]); // D
        Assert.Equal(0x46, result[3]); // F
    }

    [Fact]
    public void GenerateOrderPdf_WorksWithNullOptionalFields()
    {
        // ARRANGE — minimal dto with only required fields
        var service = new PdfService();
        var dto     = new OrderDetailDto
        {
            OrderId  = 1,
            Lines    = new List<OrderLineDto>
            {
                new()
                {
                    ProductId   = 1,
                    ProductName = "Test Product",
                    UnitPrice   = 10m,
                    Quantity    = 1,
                    Discount    = 0f
                }
            }
        };

        // ACT
        var result = service.GenerateOrderPdf(dto);

        // ASSERT — should not throw, should return bytes
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("Shipped")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    [InlineData("Pending")]
    [InlineData("Unknown")]  // → hits the default grey color
    [InlineData(null)]       // → null shipment status
    public void GenerateOrderPdf_HandlesAllShipmentStatuses(string? status)
    {
        // ARRANGE — each status maps to a different badge color in the PDF
        var service = new PdfService();
        var dto     = BuildOrderDto(shipmentStatus: status);

        // ACT
        var result = service.GenerateOrderPdf(dto);

        // ASSERT — none of the statuses should throw
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateOrderPdf_WorksWithMultipleLines()
    {
        // ARRANGE — order with many line items
        var service = new PdfService();
        var dto     = BuildOrderDto();
        dto.Lines   = Enumerable.Range(1, 20).Select(i => new OrderLineDto
        {
            ProductId   = i,
            ProductName = $"Product {i}",
            UnitPrice   = i * 5.00m,
            Quantity    = (short)i,
            Discount    = 0f
        }).ToList();

        // ACT
        var result = service.GenerateOrderPdf(dto);

        // ASSERT
        Assert.NotEmpty(result);
    }

    // ─────────────────────────────────────────────────────────────
    // GenerateOrderPdf — error path
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateOrderPdf_ThrowsInvalidOperation_WhenLinesIsNull()
    {
        // ARRANGE — Lines = null will cause NullReferenceException inside QuestPDF
        var service = new PdfService();
        var dto     = new OrderDetailDto
        {
            OrderId = 1,
            Lines   = null! // ← force null to test error handling
        };

        // ACT & ASSERT
        Assert.Throws<InvalidOperationException>(
            () => service.GenerateOrderPdf(dto));
    }
}