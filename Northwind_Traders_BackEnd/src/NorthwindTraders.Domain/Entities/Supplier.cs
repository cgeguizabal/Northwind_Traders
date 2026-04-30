namespace NorthwindTraders.Domain.Entities;


public class Supplier
{
    /// Primary key for supplier.
    public int SupplierId { get; set; }
    /// Company or business name (required).
    public required string CompanyName { get; set; }
    /// Contact person name (optional).
    public string? ContactName { get; set; }
    /// Contact person's job title (optional).
    public string? ContactTitle { get; set; }
    /// Street address (optional).
    public string? Address { get; set; }
    /// City (optional).
    public string? City { get; set; }
    /// Region/state (optional).
    public string? Region { get; set; }
    /// Postal code (optional).
    public string? PostalCode { get; set; }
    /// Country (optional).
    public string? Country { get; set; }
    /// Phone number (optional).
    public string? Phone { get; set; }
    /// Fax number (optional).
    public string? Fax { get; set; }
    /// Supplier website or homepage (optional).
    public string? HomePage { get; set; }      

    /// Products supplied by this supplier.
    public ICollection<Product> Products { get; set; } = [];
}