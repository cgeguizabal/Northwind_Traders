using Microsoft.AspNetCore.Mvc;                      // C#
using NorthwindTraders.Application.DTOs.Order;
using NorthwindTraders.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using NorthwindTraders.Infrastructure.Services;
using NorthwindTraders.Domain.Entities;          // ← Order, OrderDetail


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
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred while retrieving orders: {ex.Message}");
        }
    }

    // GET api/orders/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred while retrieving order {id}: {ex.Message}");
        }
    }

    // GET api/orders/customer/ALFKI
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(string customerId)
    {
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred while retrieving orders for customer '{customerId}': {ex.Message}");
        }
    }

    // GET api/orders/status/3
    [HttpGet("status/{shipmentStateId}")]
    public async Task<IActionResult> GetByStatus(int shipmentStateId)
    {
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred while retrieving orders with status {shipmentStateId}: {ex.Message}");
        }
    }

    // PUT api/orders/5/status
    // Updates only the shipment status — used in the tracking dashboard
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] int shipmentStateId)
    {
        try
        {
            var order = await _repository.GetByIdAsync(id);

            if (order is null)
                return NotFound($"Order with id {id} was not found.");

            order.ShipmentStateId = shipmentStateId;
            _repository.Update(order);
            await _repository.SaveChangesAsync();

            return NoContent();                          // C# — HTTP 204
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred while updating the status of order {id}: {ex.Message}");
        }
    }


    // GET api/v1/orders/10248/pdf
    [HttpGet("{id}/pdf")]
    
    public async Task<IActionResult> GetPdf(int id)
    {
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred while generating the PDF for order {id}: {ex.Message}");
        }
    }

// POST api/v1/orders
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
{
    try
    {
        if (dto.Lines is null || dto.Lines.Count == 0)
            return BadRequest("An order must have at least one product line.");

        var order = new Order
        {
            CustomerId      = dto.CustomerId,
            EmployeeId      = dto.EmployeeId,
            ShipVia         = dto.ShipVia,
            ShipmentStateId = dto.ShipmentStateId,
            OrderDate       = dto.OrderDate ?? DateTime.UtcNow,
            RequiredDate    = dto.RequiredDate,
            ShippedDate     = dto.ShippedDate,
            Freight         = dto.Freight,
            Notes           = dto.Notes,
            ShipName        = dto.ShipName,
            ShipAddress     = dto.ShipAddress,
            ShipCity        = dto.ShipCity,
            ShipRegion      = dto.ShipRegion,
            ShipPostalCode  = dto.ShipPostalCode,
            ShipCountry     = dto.ShipCountry,
            BillAddress     = dto.BillAddress,
            BillCity        = dto.BillCity,
            BillRegion      = dto.BillRegion,
            BillPostalCode  = dto.BillPostalCode,
            BillCountry     = dto.BillCountry,
            OrderDetails    = dto.Lines.Select(l => new OrderDetail
            {
                ProductId = l.ProductId,
                UnitPrice = l.UnitPrice,
                Quantity  = l.Quantity,
                Discount  = l.Discount
            }).ToList()
        };

        await _repository.AddAsync(order);
        await _repository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, new { order.OrderId });
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Unexpected error while creating order: {ex.Message}");
    }
}

// PUT api/v1/orders/5
[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto dto)
{
    try
    {
        var order = await _repository.GetOrderWithDetailsAsync(id);

        if (order is null)
            return NotFound($"Order with id {id} was not found.");

        // Only update fields that were provided
        if (dto.CustomerId      is not null) order.CustomerId      = dto.CustomerId;
        if (dto.EmployeeId      is not null) order.EmployeeId      = dto.EmployeeId;
        if (dto.ShipVia         is not null) order.ShipVia         = dto.ShipVia;
        if (dto.ShipmentStateId is not null) order.ShipmentStateId = dto.ShipmentStateId;
        if (dto.OrderDate       is not null) order.OrderDate       = dto.OrderDate;
        if (dto.RequiredDate    is not null) order.RequiredDate    = dto.RequiredDate;
        if (dto.ShippedDate     is not null) order.ShippedDate     = dto.ShippedDate;
        if (dto.Freight         is not null) order.Freight         = dto.Freight;
        if (dto.Notes           is not null) order.Notes           = dto.Notes;
        if (dto.ShipName        is not null) order.ShipName        = dto.ShipName;
        if (dto.ShipAddress     is not null) order.ShipAddress     = dto.ShipAddress;
        if (dto.ShipCity        is not null) order.ShipCity        = dto.ShipCity;
        if (dto.ShipRegion      is not null) order.ShipRegion      = dto.ShipRegion;
        if (dto.ShipPostalCode  is not null) order.ShipPostalCode  = dto.ShipPostalCode;
        if (dto.ShipCountry     is not null) order.ShipCountry     = dto.ShipCountry;
        if (dto.BillAddress     is not null) order.BillAddress     = dto.BillAddress;
        if (dto.BillCity        is not null) order.BillCity        = dto.BillCity;
        if (dto.BillRegion      is not null) order.BillRegion      = dto.BillRegion;
        if (dto.BillPostalCode  is not null) order.BillPostalCode  = dto.BillPostalCode;
        if (dto.BillCountry     is not null) order.BillCountry     = dto.BillCountry;

        // Replace order lines if provided
        if (dto.Lines is not null && dto.Lines.Count > 0)
        {
            order.OrderDetails.Clear();
            foreach (var l in dto.Lines)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    OrderId   = id,
                    ProductId = l.ProductId,
                    UnitPrice = l.UnitPrice,
                    Quantity  = l.Quantity,
                    Discount  = l.Discount
                });
            }
        }

        _repository.Update(order);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Unexpected error while updating order {id}: {ex.Message}");
    }
}



    // GET api/v1/orders/export/excel
// Exports all orders to Excel — used in the orders table toolbar
[HttpGet("export/excel")]
public async Task<IActionResult> ExportExcel()
{
    try
    {
        var orders = await _repository.GetAllAsync();

        // XLWorkbook — ClosedXML — creates an in-memory Excel file
        using var workbook  = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Orders");

        // ── HEADER ROW ───────────────────────────────────────────────
        worksheet.Cell(1, 1).Value = "Order ID";
        worksheet.Cell(1, 2).Value = "Customer";
        worksheet.Cell(1, 3).Value = "Employee";
        worksheet.Cell(1, 4).Value = "Order Date";
        worksheet.Cell(1, 5).Value = "Required Date";
        worksheet.Cell(1, 6).Value = "Shipped Date";
        worksheet.Cell(1, 7).Value = "Ship Name";
        worksheet.Cell(1, 8).Value = "Ship City";
        worksheet.Cell(1, 9).Value = "Ship Country";
        worksheet.Cell(1, 10).Value = "Freight";
        worksheet.Cell(1, 11).Value = "Shipment Status";
        worksheet.Cell(1, 12).Value = "Region";

        // Bold header row
        worksheet.Row(1).Style.Font.Bold = true;

        // ── DATA ROWS ────────────────────────────────────────────────
        int row = 2;
        foreach (var o in orders)
        {
            worksheet.Cell(row, 1).Value  = o.OrderId;
            worksheet.Cell(row, 2).Value  = o.Customer?.CompanyName ?? "";
            worksheet.Cell(row, 3).Value  = o.Employee is not null
                                              ? $"{o.Employee.FirstName} {o.Employee.LastName}"
                                              : "";
            worksheet.Cell(row, 4).Value  = o.OrderDate?.ToString("yyyy-MM-dd") ?? "";
            worksheet.Cell(row, 5).Value  = o.RequiredDate?.ToString("yyyy-MM-dd") ?? "";
            worksheet.Cell(row, 6).Value  = o.ShippedDate?.ToString("yyyy-MM-dd") ?? "";
            worksheet.Cell(row, 7).Value  = o.ShipName ?? "";
            worksheet.Cell(row, 8).Value  = o.ShipCity ?? "";
            worksheet.Cell(row, 9).Value  = o.ShipCountry ?? "";
            worksheet.Cell(row, 10).Value = o.Freight ?? 0;
            worksheet.Cell(row, 11).Value = o.ShipmentState?.Name ?? "";
            worksheet.Cell(row, 12).Value = o.ShipRegion ?? "";
            row++;
        }

        // Auto-fit all columns
        worksheet.Columns().AdjustToContents();

        // Write workbook to memory stream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        // Return as downloadable Excel file
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Orders_{DateTime.UtcNow:yyyy-MM-dd}.xlsx"
        );
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Unexpected error while exporting orders to Excel: {ex.Message}");
    }
}

    // POST api/v1/orders/10248/geocode
    [HttpPost("{id}/geocode")]
    [Authorize]
   
    public async Task<IActionResult> Geocode(int id)
    {
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred while geocoding order {id}: {ex.Message}");
        }
    }

    // POST api/v1/orders/geocode-all
    // Geocodes all orders that have no coordinates yet
    [HttpPost("geocode-all")]
    
   
    public async Task<IActionResult> GeocodeAll()
    {
        try
        {
            var (processed, succeeded, failed) = await _geocodingService.GeocodeAllPendingAsync();

            return Ok(new BulkGeocodeResultDto
            {
                Processed = processed,
                Succeeded = succeeded,
                Failed    = failed
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred while geocoding pending orders: {ex.Message}");
        }
    }
}