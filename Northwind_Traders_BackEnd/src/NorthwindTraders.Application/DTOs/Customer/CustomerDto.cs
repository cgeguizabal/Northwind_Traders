namespace NorthwindTraders.Application.DTOs.Customer;

// Paged wrapper — used in GET /api/v1/customers
public class PagedResult<T>
{
    public List<T> Items      { get; set; } = [];
    public int     TotalCount { get; set; }
    public int     Page       { get; set; }
    public int     PageSize   { get; set; }
    public int     TotalPages { get; set; }
}

// Summary — used in GET /api/v1/customers (list)
public class CustomerSummaryDto
{
    public string?  CustomerId   { get; set; }
    public string?  CompanyName  { get; set; }
    public string?  ContactName  { get; set; }
    public string?  ContactTitle { get; set; }
    public string?  City         { get; set; }
    public string?  Country      { get; set; }
    public string?  Phone        { get; set; }
    public int      TotalOrders  { get; set; }   // count of orders
}

// Detail — used in GET /api/v1/customers/{id}
public class CustomerDetailDto
{
    public string?  CustomerId   { get; set; }
    public string?  CompanyName  { get; set; }
    public string?  ContactName  { get; set; }
    public string?  ContactTitle { get; set; }
    public string?  Address      { get; set; }
    public string?  City         { get; set; }
    public string?  Region       { get; set; }
    public string?  PostalCode   { get; set; }
    public string?  Country      { get; set; }
    public string?  Phone        { get; set; }
    public string?  Fax          { get; set; }
    public List<CustomerOrderDto> Orders { get; set; } = [];
}

// Order summary inside customer detail
public class CustomerOrderDto
{
    public int       OrderId        { get; set; }
    public DateTime? OrderDate      { get; set; }
    public DateTime? ShippedDate    { get; set; }
    public decimal?  Freight        { get; set; }
    public string?   ShipmentStatus { get; set; }
    public string?   ShipCountry    { get; set; }
}

// Map pin — used in GET /api/v1/customers/{id}/map
public class CustomerMapDto
{
    public int      OrderId              { get; set; }
    public string?  ShipName             { get; set; }
    public string?  ValidatedShipAddress { get; set; }
    public decimal? ShipLatitude         { get; set; }
    public decimal? ShipLongitude        { get; set; }
    public DateTime? ShippedDate         { get; set; }
    public string?  ShipmentStatus       { get; set; }
}