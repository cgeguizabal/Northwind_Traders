namespace NorthwindTraders.Application.DTOs.Category;

// Summary — used in GET /api/v1/categories (list)
public class CategorySummaryDto
{
    public int     CategoryId   { get; set; }
    public string? CategoryName { get; set; }
    public string? Description  { get; set; }
    public int     TotalProducts { get; set; }   // how many products in this category
}

// Detail — used in GET /api/v1/categories/{id}
public class CategoryDetailDto
{
    public int     CategoryId   { get; set; }
    public string? CategoryName { get; set; }
    public string? Description  { get; set; }
    public List<CategoryProductDto> Products { get; set; } = [];
}

// Product summary inside category detail
public class CategoryProductDto
{
    public int      ProductId    { get; set; }
    public string?  ProductName  { get; set; }
    public decimal? UnitPrice    { get; set; }
    public short?   UnitsInStock { get; set; }
    public bool     Discontinued { get; set; }
}