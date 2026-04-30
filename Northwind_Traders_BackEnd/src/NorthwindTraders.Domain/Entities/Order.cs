using NorthwindTraders.Domain.Enums;

namespace NorthwindTraders.Domain.Entities;

/// Maps to dbo.Orders — central entity for sales.
public class Order
{
    /// Primary key.
    public int OrderId { get; set; }
    /// When the order was placed.
    public DateTime? OrderDate { get; set; }
    /// Date the customer requested delivery.
    public DateTime? RequiredDate { get; set; }
    /// Date the order was shipped.
    public DateTime? ShippedDate { get; set; }
    /// Freight charge (money → decimal).
    public decimal? Freight { get; set; }

    /// Existing shipping name.
    public string? ShipName { get; set; }
    /// Existing shipping street address.
    public string? ShipAddress { get; set; }
    /// Shipping city.
    public string? ShipCity { get; set; }
    /// Shipping region/state.
    public string? ShipRegion { get; set; }
    /// Shipping postal code.
    public string? ShipPostalCode { get; set; }
    /// Shipping country.
    public string? ShipCountry { get; set; }

    /// Original address text entered by user.
    public string? OriginalShipAddress { get; set; }
    /// Address normalized/validated by external service.
    public string? ValidatedShipAddress { get; set; }
    /// Validated shipping latitude.
    public decimal? ShipLatitude { get; set; }
    /// Validated shipping longitude.
    public decimal? ShipLongitude { get; set; }

    /// Billing street address.
    public string? BillAddress { get; set; }
    /// Billing city.
    public string? BillCity { get; set; }
    /// Billing region/state.
    public string? BillRegion { get; set; }
    /// Billing postal code.
    public string? BillPostalCode { get; set; }
    /// Billing country.
    public string? BillCountry { get; set; }
    /// Original billing address text.
    public string? OriginalBillAddress { get; set; }
    /// Validated billing address.
    public string? ValidatedBillAddress { get; set; }
    /// Billing latitude.
    public decimal? BillLatitude { get; set; }
    /// Billing longitude.
    public decimal? BillLongitude { get; set; }

    /// Additional notes for the order.
    public string? Notes { get; set; }

    /// FK to Customer (nchar(5)).
    public string? CustomerId { get; set; }
    /// FK to Employee handling the order.
    public int? EmployeeId { get; set; }
    /// FK to Shipper (ShipVia).
    public int? ShipVia { get; set; }

    /// FK to ShipmentState (new table).
    public int? ShipmentStateId { get; set; }

    /// Navigation to customer.
    public Customer? Customer { get; set; }
    /// Navigation to employee.
    public Employee? Employee { get; set; }
    /// Navigation to shipper.
    public Shipper? Shipper { get; set; }
    /// Navigation to shipment state.
    public ShipmentState? ShipmentState { get; set; }

    /// Line items for this order.
    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
}