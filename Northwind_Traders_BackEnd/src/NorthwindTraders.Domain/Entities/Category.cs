namespace NorthwindTraders.Domain.Entities;


/// Category of products.
public class Category
{
    /// Primary key for the category.
    public int CategoryId {get; set;}
    /// Name of the category (required).
    public required string CategoryName {get; set;}
    /// Optional longer description of the category.
    public string? Description {get; set;}

    /// Products that belong to this category.
    public ICollection<Product> Products {get; set;} = [];
}