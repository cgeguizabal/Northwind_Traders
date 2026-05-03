namespace NorthwindTraders.Application.DTOs.Shipper;

// Summary — used in GET /api/v1/shippers (list)
public class ShipperSummaryDto
{
    public int     ShipperId   { get; set; }
    public string? CompanyName { get; set; }
    public string? Phone       { get; set; }
    public int     TotalOrders { get; set; }   // how many orders this shipper has handled
}

// Detail — used in GET /api/v1/shippers/{id}
public class ShipperDetailDto
{
    public int     ShipperId   { get; set; }
    public string? CompanyName { get; set; }
    public string? Phone       { get; set; }
    public List<ShipperOrderDto> Orders { get; set; } = [];
}

// Order summary inside shipper detail
public class ShipperOrderDto
{
    public int       OrderId    { get; set; }
    public DateTime? OrderDate  { get; set; }
    public DateTime? ShippedDate { get; set; }
    public string?   CustomerName { get; set; }
    public string?   ShipCountry  { get; set; }
    public string?   ShipmentStatus { get; set; }
}