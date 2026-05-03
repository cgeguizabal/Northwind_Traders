namespace NorthwindTraders.Application.DTOs.Order;

// Same fields as Create — all optional for partial updates
public class UpdateOrderDto
{
    public string?   CustomerId      { get; set; }
    public int?      EmployeeId      { get; set; }
    public int?      ShipVia         { get; set; }
    public int?      ShipmentStateId { get; set; }
    public DateTime? OrderDate       { get; set; }
    public DateTime? RequiredDate    { get; set; }
    public DateTime? ShippedDate     { get; set; }
    public decimal?  Freight         { get; set; }
    public string?   Notes           { get; set; }

    public string?   ShipName        { get; set; }
    public string?   ShipAddress     { get; set; }
    public string?   ShipCity        { get; set; }
    public string?   ShipRegion      { get; set; }
    public string?   ShipPostalCode  { get; set; }
    public string?   ShipCountry     { get; set; }

    public string?   BillAddress     { get; set; }
    public string?   BillCity        { get; set; }
    public string?   BillRegion      { get; set; }
    public string?   BillPostalCode  { get; set; }
    public string?   BillCountry     { get; set; }

    // Replaces ALL existing lines when provided
    public List<OrderLineInputDto>? Lines { get; set; }
}