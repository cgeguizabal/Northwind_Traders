namespace NorthwindTraders.Domain.Entities;



public class Product
{
    /// Primary key for product.
    public int ProductId { get; set; }
    /// Product name (required).
    public required string ProductName { get; set; }
    /// Quantity per unit description (optional).
    public string? QuantityPerUnit { get; set; }
    /// Unit price (nullable).
    public decimal? UnitPrice { get; set; }        
    /// Units currently in stock.
    public short? UnitsInStock { get; set; }         
    /// Units currently on order.
    public short? UnitsOnOrder { get; set; }
    /// Reorder threshold.
    public short? ReorderLevel { get; set; }
    /// Whether the product is discontinued.
    public bool Discontinued { get; set; }          

    /// FK to supplier.
    public int? SupplierId { get; set; }
    /// FK to category.
    public int? CategoryId { get; set; }

    /// Navigation to supplier.
    public Supplier? Supplier { get; set; }
    /// Navigation to category.
    public Category? Category { get; set; }
    /// Line items referencing this product.
    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
}