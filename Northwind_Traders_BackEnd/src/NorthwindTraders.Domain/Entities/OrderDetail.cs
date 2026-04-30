namespace NorthwindTraders.Domain.Entities;



public class OrderDetail
{
    
    /// Part 1 of composite PK: Order identifier.
    public int OrderId { get; set; }

    /// Part 2 of composite PK: Product identifier.
    public int ProductId { get; set; }

    /// Price per unit (money → decimal).
    public decimal UnitPrice { get; set; }
    /// Number of units ordered.
    public short Quantity { get; set; }
    /// Line-level discount (0.0 - 1.0).
    public float Discount { get; set; }

    /// Navigation to parent Order.
    public Order? Order { get; set; }
    /// Navigation to the Product ordered.
    public Product? Product { get; set; }
}