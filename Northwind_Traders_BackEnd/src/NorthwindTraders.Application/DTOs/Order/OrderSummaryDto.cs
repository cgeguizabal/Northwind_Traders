namespace NorthwindTraders.Application.DTOs.Order;

// Lightweight shape — used in list views
// Does NOT include order line items — too heavy for a list
public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public decimal? Freight { get; set; }
    public string? ShipName { get; set; }
    public string? ShipCity { get; set; }
    public string? ShipCountry { get; set; }

    // Human readable names instead of raw FK ids
    public string? CustomerName { get; set; }
    public string? EmployeeName { get; set; }
    public string? ShipmentStatus { get; set; }
}