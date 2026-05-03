namespace NorthwindTraders.Application.DTOs.Order;

// What the geocode endpoint returns after processing
public class GeocodeResultDto
{
    public int OrderId { get; set; }

    // Shipping
    public string? ValidatedShipAddress { get; set; }
    public decimal? ShipLatitude { get; set; }
    public decimal? ShipLongitude { get; set; }

    // Billing
    public string? ValidatedBillAddress { get; set; }
    public decimal? BillLatitude { get; set; }
    public decimal? BillLongitude { get; set; }
}