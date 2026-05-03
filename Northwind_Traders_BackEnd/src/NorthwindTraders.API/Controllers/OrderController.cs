using Microsoft.AspNetCore.Mvc;                      // C#
using NorthwindTraders.Application.DTOs.Order;
using NorthwindTraders.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using NorthwindTraders.Infrastructure.Services;

namespace NorthwindTraders.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Require authentication for all endpoints in this controller
public class OrdersController : ControllerBase      // C#
{
    private readonly IOrderRepository _repository;
    private readonly PdfService _pdfService;
    private readonly GeocodingService _geocodingService; 

    public OrdersController(IOrderRepository repository, PdfService pdfService,  GeocodingService geocodingService)
{
    _repository = repository;
    _pdfService = pdfService;
     _geocodingService  = geocodingService; 
}

    // GET api/orders
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _repository.GetAllAsync();

        var dtos = orders.Select(o => new OrderSummaryDto
        {
            OrderId        = o.OrderId,
            OrderDate      = o.OrderDate,
            RequiredDate   = o.RequiredDate,
            ShippedDate    = o.ShippedDate,
            Freight        = o.Freight,
            ShipName       = o.ShipName,
            ShipCity       = o.ShipCity,
            ShipCountry    = o.ShipCountry,
            CustomerName   = o.Customer?.CompanyName,
            EmployeeName   = o.Employee is not null
                               ? $"{o.Employee.FirstName} {o.Employee.LastName}"
                               : null,
            ShipmentStatus = o.ShipmentState?.Name
        }).ToList();

        return Ok(dtos);
    }

    // GET api/orders/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _repository.GetOrderWithDetailsAsync(id);

        if (order is null)
            return NotFound($"Order with id {id} was not found.");

        var dto = new OrderDetailDto
        {
            OrderId        = order.OrderId,
            OrderDate      = order.OrderDate,
            RequiredDate   = order.RequiredDate,
            ShippedDate    = order.ShippedDate,
            Freight        = order.Freight,
            ShipmentStatus = order.ShipmentState?.Name,
            ShipName       = order.ShipName,
            ShipAddress    = order.ShipAddress,
            ShipCity       = order.ShipCity,
            ShipRegion     = order.ShipRegion,
            ShipPostalCode = order.ShipPostalCode,
            ShipCountry    = order.ShipCountry,
            BillAddress    = order.BillAddress,
            BillCity       = order.BillCity,
            BillCountry    = order.BillCountry,
            CustomerName   = order.Customer?.CompanyName,
            EmployeeName   = order.Employee is not null
                               ? $"{order.Employee.FirstName} {order.Employee.LastName}"
                               : null,
            ShipperName    = order.Shipper?.CompanyName,
            Lines          = order.OrderDetails.Select(od => new OrderLineDto
            {
                ProductName = od.Product?.ProductName ?? "Unknown",
                UnitPrice   = od.UnitPrice,
                Quantity    = od.Quantity,
                Discount    = od.Discount
            }).ToList()
        };

        return Ok(dto);
    }

    // GET api/orders/customer/ALFKI
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(string customerId)
    {
        var orders = await _repository.GetByCustomerAsync(customerId);

        var dtos = orders.Select(o => new OrderSummaryDto
        {
            OrderId        = o.OrderId,
            OrderDate      = o.OrderDate,
            ShippedDate    = o.ShippedDate,
            ShipCountry    = o.ShipCountry,
            ShipmentStatus = o.ShipmentState?.Name
        }).ToList();

        return Ok(dtos);
    }

    // GET api/orders/status/3
    [HttpGet("status/{shipmentStateId}")]
    public async Task<IActionResult> GetByStatus(int shipmentStateId)
    {
        var orders = await _repository.GetByShipmentStatusAsync(shipmentStateId);

        var dtos = orders.Select(o => new OrderSummaryDto
        {
            OrderId        = o.OrderId,
            OrderDate      = o.OrderDate,
            ShippedDate    = o.ShippedDate,
            CustomerName   = o.Customer?.CompanyName,
            ShipmentStatus = o.ShipmentState?.Name
        }).ToList();

        return Ok(dtos);
    }

    // PUT api/orders/5/status
    // Updates only the shipment status — used in the tracking dashboard
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] int shipmentStateId)
    {
        var order = await _repository.GetByIdAsync(id);

        if (order is null)
            return NotFound($"Order with id {id} was not found.");

        order.ShipmentStateId = shipmentStateId;
        _repository.Update(order);
        await _repository.SaveChangesAsync();

        return NoContent();                          // C# — HTTP 204
    }


    // GET api/v1/orders/10248/pdf
[HttpGet("{id}/pdf")]
public async Task<IActionResult> GetPdf(int id)
{
    var order = await _repository.GetOrderWithDetailsAsync(id);

    if (order is null)
        return NotFound($"Order with id {id} was not found.");

    var dto = new OrderDetailDto
    {
        OrderId        = order.OrderId,
        OrderDate      = order.OrderDate,
        RequiredDate   = order.RequiredDate,
        ShippedDate    = order.ShippedDate,
        Freight        = order.Freight,
        ShipmentStatus = order.ShipmentState?.Name,
        ShipName       = order.ShipName,
        ShipAddress    = order.ShipAddress,
        ShipCity       = order.ShipCity,
        ShipRegion     = order.ShipRegion,
        ShipPostalCode = order.ShipPostalCode,
        ShipCountry    = order.ShipCountry,
        BillAddress    = order.BillAddress,
        BillCity       = order.BillCity,
        BillCountry    = order.BillCountry,
        CustomerName   = order.Customer?.CompanyName,
        EmployeeName   = order.Employee is not null
                           ? $"{order.Employee.FirstName} {order.Employee.LastName}"
                           : null,
        ShipperName    = order.Shipper?.CompanyName,
        Lines          = order.OrderDetails.Select(od => new OrderLineDto
        {
            ProductName = od.Product?.ProductName ?? "Unknown",
            UnitPrice   = od.UnitPrice,
            Quantity    = od.Quantity,
            Discount    = od.Discount
        }).ToList()
    };

    var pdfBytes = _pdfService.GenerateOrderPdf(dto);

    // File — ControllerBase Method
    // pdfBytes     = the raw PDF binary
    // application/pdf = MIME type — tells the browser what type of file this is
    // Order_{id}.pdf  = suggested filename when the browser downloads it
    return File(pdfBytes, "application/pdf", $"Order_{id}.pdf");
}

// POST api/v1/orders/10248/geocode
[HttpPost("{id}/geocode")]
[Authorize]   // only logged in employees can trigger geocoding
public async Task<IActionResult> Geocode(int id)
{
    // GeocodingService handles finding the order and saving results
    var result = await _geocodingService.GeocodeOrderAsync(id);

    // Result<T> — check IsSuccess before reading Value
    if (!result.IsSuccess)
        return BadRequest(result.Error);

    var (validatedShip, shipLat, shipLng, validatedBill, billLat, billLng) = result.Value!;

    return Ok(new GeocodeResultDto
    {
        OrderId              = id,
        ValidatedShipAddress = validatedShip,
        ShipLatitude         = shipLat,
        ShipLongitude        = shipLng,
        ValidatedBillAddress = validatedBill,
        BillLatitude         = billLat,
        BillLongitude        = billLng
    });
}
}