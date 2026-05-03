namespace NorthwindTraders.Application.DTOs.Product;

// Summary — used in GET /api/v1/products (list)
public class ProductSummaryDto
{
    public int      ProductId       { get; set; }
    public string?  ProductName     { get; set; }
    public string?  CategoryName    { get; set; }
    public string?  SupplierName    { get; set; }
    public decimal? UnitPrice       { get; set; }
    public short?   UnitsInStock    { get; set; }
    public short?   UnitsOnOrder    { get; set; }
    public short?   ReorderLevel    { get; set; }
    public bool     Discontinued    { get; set; }
}

// Detail — used in GET /api/v1/products/{id}
public class ProductDetailDto
{
    public int      ProductId        { get; set; }
    public string?  ProductName      { get; set; }
    public string?  QuantityPerUnit  { get; set; }
    public string?  CategoryName     { get; set; }
    public string?  SupplierName     { get; set; }
    public decimal? UnitPrice        { get; set; }
    public short?   UnitsInStock     { get; set; }
    public short?   UnitsOnOrder     { get; set; }
    public short?   ReorderLevel     { get; set; }
    public bool     Discontinued     { get; set; }
    public bool     LowStock         { get; set; }   // UnitsInStock <= ReorderLevel
}