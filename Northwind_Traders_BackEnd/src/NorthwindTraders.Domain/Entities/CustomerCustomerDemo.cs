namespace NorthwindTraders.Domain.Entities;

/// Maps to dbo.CustomerCustomerDemo (junction table).
/// Composite PK: CustomerID + CustomerTypeID.
public class CustomerCustomerDemo
{
    /// FK to Customer (nchar(5)).
    public string CustomerId { get; set; } = string.Empty;
    /// FK to CustomerDemographic (nchar(10)).
    public string CustomerTypeId { get; set; } = string.Empty;

    /// Navigation to the related Customer.
    public Customer? Customer { get; set; }
    /// Navigation to the related demographic.
    public CustomerDemographic? CustomerDemographic { get; set; }
}