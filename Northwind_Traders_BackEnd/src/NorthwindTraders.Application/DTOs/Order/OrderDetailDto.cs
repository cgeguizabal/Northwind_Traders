namespace NorthwindTraders.Application.DTOs.Order;

// Full order shape — used for order detail view and future PDF report
public class OrderDetailDto
{
    public int OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public decimal? Freight { get; set; }
    public string? ShipmentStatus { get; set; }

    // Shipping
    public string? ShipName { get; set; }
    public string? ShipAddress { get; set; }
    public string? ShipCity { get; set; }
    public string? ShipRegion { get; set; }
    public string? ShipPostalCode { get; set; }
    public string? ShipCountry { get; set; }

    // Billing
    public string? BillAddress { get; set; }
    public string? BillCity { get; set; }
    public string? BillCountry { get; set; }

    // Related names
    public string? CustomerName { get; set; }
    public string? EmployeeName { get; set; }
    public string? ShipperName { get; set; }

    // Line items
    public List<OrderLineDto> Lines { get; set; } = [];
}

// Each product line inside a full order
public class OrderLineDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public short Quantity { get; set; }
    public float Discount { get; set; }

    // Calculated on the fly — never stored in the database
    public decimal LineTotal => (decimal)(UnitPrice * Quantity * (1 - (decimal)Discount));
}