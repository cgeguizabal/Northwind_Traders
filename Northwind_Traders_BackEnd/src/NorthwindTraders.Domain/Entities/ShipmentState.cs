namespace NorthwindTraders.Domain.Entities;

/// Lookup table for order shipment states (e.g. Pending, Shipped, Completed, Cancelled).
public class ShipmentState
{
    /// Primary key for shipment state.
    public int ShipmentStateId{get; set;}
    /// Short name of the state (required).
    public required string Name {get; set;}

    /// Optional description of the state.
    public string? Description {get; set;}

    /// Orders currently in this shipment state.
    public ICollection<Order> Orders {get; set;} = [];
}