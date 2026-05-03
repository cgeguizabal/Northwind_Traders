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

// What the bulk geocode endpoint returns
public class BulkGeocodeResultDto
{
    public int Processed { get; set; }   // total orders attempted
    public int Succeeded { get; set; }   // successfully geocoded
    public int Failed    { get; set; }   // failed (address not found, etc.)
}