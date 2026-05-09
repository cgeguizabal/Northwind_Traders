using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindTraders.Application.DTOs.Shipper;
using NorthwindTraders.Domain.Interfaces;

namespace NorthwindTraders.API.Controllers;

// Read-only endpoints for shipping companies.
// Returns shipper details and their associated orders.
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ShippersController : ControllerBase
{
    private readonly IShipperRepository _repository;

    public ShippersController(IShipperRepository repository)
    {
        _repository = repository;
    }

    // GET api/v1/shippers — returns all shippers with their order count
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var shippers = await _repository.GetAllAsync();

        var dtos = shippers.Select(s => new ShipperSummaryDto
        {
            ShipperId   = s.ShipperId,
            CompanyName = s.CompanyName,
            Phone       = s.Phone,
            TotalOrders = s.Orders?.Count ?? 0
        }).ToList();

        return Ok(dtos);
    }

    // GET api/v1/shippers/{id} — returns a single shipper with full order history
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var shipper = await _repository.GetByIdAsync(id);

        if (shipper is null)
            return NotFound($"Shipper with id {id} was not found.");

        var dto = new ShipperDetailDto
        {
            ShipperId   = shipper.ShipperId,
            CompanyName = shipper.CompanyName,
            Phone       = shipper.Phone,
            Orders      = shipper.Orders?.Select(o => new ShipperOrderDto
            {
                OrderId        = o.OrderId,
                OrderDate      = o.OrderDate,
                ShippedDate    = o.ShippedDate,
                CustomerName   = o.Customer?.CompanyName,
                ShipCountry    = o.ShipCountry,
                ShipmentStatus = o.ShipmentState?.Name
            }).ToList() ?? []
        };

        return Ok(dto);
    }
}