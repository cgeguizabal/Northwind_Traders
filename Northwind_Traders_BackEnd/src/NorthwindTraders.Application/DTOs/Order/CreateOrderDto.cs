namespace NorthwindTraders.Application.DTOs.Order;

public class CreateOrderDto
{
    public string?   CustomerId     { get; set; }
    public int?      EmployeeId     { get; set; }
    public int?      ShipVia        { get; set; }         // shipperId
    public int?      ShipmentStateId { get; set; }
    public DateTime? OrderDate      { get; set; }
    public DateTime? RequiredDate   { get; set; }
    public DateTime? ShippedDate    { get; set; }
    public decimal?  Freight        { get; set; }
    public string?   Notes          { get; set; }

    // Shipping address
    public string?   ShipName       { get; set; }
    public string?   ShipAddress    { get; set; }
    public string?   ShipCity       { get; set; }
    public string?   ShipRegion     { get; set; }
    public string?   ShipPostalCode { get; set; }
    public string?   ShipCountry    { get; set; }

    // Billing address
    public string?   BillAddress    { get; set; }
    public string?   BillCity       { get; set; }
    public string?   BillRegion     { get; set; }
    public string?   BillPostalCode { get; set; }
    public string?   BillCountry    { get; set; }

    // Order lines — at least one required
    public List<OrderLineInputDto> Lines { get; set; } = [];
}

public class OrderLineInputDto
{
    public int     ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public short   Quantity  { get; set; }
    public float   Discount  { get; set; }
}