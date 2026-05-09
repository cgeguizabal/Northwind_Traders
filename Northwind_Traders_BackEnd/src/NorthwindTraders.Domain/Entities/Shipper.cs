namespace NorthwindTraders.Domain.Entities;

/// Shipping company responsible for delivering orders. Maps to dbo.Shippers.
public class Shipper
{
    /// Primary key for shipper.
    public int ShipperId {get; set;}
    /// Company name (required).
    public required string CompanyName {get; set;}
    /// Contact phone number (optional).
    public string? Phone {get; set;}

    /// Orders shipped by this shipper.
    public ICollection<Order> Orders {get; set;} = [];
}