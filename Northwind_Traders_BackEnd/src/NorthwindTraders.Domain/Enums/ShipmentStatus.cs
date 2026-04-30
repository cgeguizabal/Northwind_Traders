namespace NorthwindTraders.Domain.Enums;

/// 
/// Represents the shipment lifecycle state for an order.
/// 
public enum shipmentStatus
{
    /// Order created and awaiting processing.
    Pending = 1,
    ///Order is being prepared or processed.
    Processing = 2,
    /// Order has been shipped from the warehouse.
    Shipped = 3,
    /// An invoice has been issued for the order.
    Invoiced = 4,
    /// Order processing is finished and closed.
    Complete = 5,
    /// Order has been cancelled and will not ship.
    Cancelled = 6,
    /// Order has been delivered to the customer.
    Delivered = 7
}