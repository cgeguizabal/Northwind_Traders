namespace NorthwindTraders.Domain.Entities;

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