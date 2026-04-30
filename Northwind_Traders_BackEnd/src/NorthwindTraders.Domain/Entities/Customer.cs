namespace NorthwindTraders.Domain.Entities;


/// Customer who purchases products.
public class  Customer
{
    /// Primary key (fixed-length identifier).
    public string CustomerId { get; set; } = string.Empty;    
    /// Company or business name (required).
    public required string CompanyName { get; set; }
    /// Contact person's full name (optional).
    public string? ContactName { get; set; }
    /// Contact person's job title (optional).
    public string? ContactTitle { get; set; }
    /// Street address (optional).
    public string? Address { get; set; }
    /// City of the customer (optional).
    public string? City { get; set; }
    /// Region or state (optional).
    public string? Region { get; set; }
    /// Postal or ZIP code (optional).
    public string? PostalCode { get; set; }
    /// Country name (optional).
    public string? Country { get; set; }
    /// Primary phone number (optional).
    public string? Phone { get; set; }
    /// Fax number (optional).
    public string? Fax { get; set; }

    /// Orders placed by this customer.
    public ICollection<Order> Orders { get; set; } = [];
    /// Link table entries for customer demographics.
    public ICollection<CustomerCustomerDemo> CustomerCustomerDemos { get; set; } = [];
}