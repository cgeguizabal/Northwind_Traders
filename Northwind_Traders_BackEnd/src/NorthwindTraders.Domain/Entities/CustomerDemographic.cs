namespace NorthwindTraders.Domain.Entities;



/// Describes a customer demographic type.
public class CustomerDemographic
{
    /// Primary key for demographic type (nchar(10)).
    public string CustomerTypeId { get; set; } = string.Empty;
    /// Optional human-readable description.
    public string? CustomerDesc { get; set; }

    /// Junction entries linking customers to this demographic.
    public ICollection<CustomerCustomerDemo> CustomerCustomerDemos { get; set; } = [];
}