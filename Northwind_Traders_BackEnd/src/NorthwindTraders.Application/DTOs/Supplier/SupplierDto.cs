namespace NorthwindTraders.Application.DTOs.Supplier;

// Summary — used in GET /api/v1/suppliers (list)
public class SupplierSummaryDto
{
    public int     SupplierId   { get; set; }
    public string? CompanyName  { get; set; }
    public string? ContactName  { get; set; }
    public string? ContactTitle { get; set; }
    public string? City         { get; set; }
    public string? Country      { get; set; }
    public string? Phone        { get; set; }
    public int     TotalProducts { get; set; }   // how many products this supplier provides
}

// Detail — used in GET /api/v1/suppliers/{id}
public class SupplierDetailDto
{
    public int     SupplierId   { get; set; }
    public string? CompanyName  { get; set; }
    public string? ContactName  { get; set; }
    public string? ContactTitle { get; set; }
    public string? Address      { get; set; }
    public string? City         { get; set; }
    public string? Region       { get; set; }
    public string? PostalCode   { get; set; }
    public string? Country      { get; set; }
    public string? Phone        { get; set; }
    public string? Fax          { get; set; }
    public string? HomePage     { get; set; }
    public List<SupplierProductDto> Products { get; set; } = [];
}

// Product summary inside supplier detail
public class SupplierProductDto
{
    public int      ProductId    { get; set; }
    public string?  ProductName  { get; set; }
    public decimal? UnitPrice    { get; set; }
    public short?   UnitsInStock { get; set; }
    public bool     Discontinued { get; set; }
}