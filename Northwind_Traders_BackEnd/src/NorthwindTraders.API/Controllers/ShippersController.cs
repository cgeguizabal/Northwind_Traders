using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindTraders.Application.DTOs.Shipper;
using NorthwindTraders.Domain.Interfaces;

namespace NorthwindTraders.API.Controllers;

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

    // GET api/v1/shippers
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
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
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving shippers: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving shippers: {ex.Message}");
        }
    }

    // GET api/v1/shippers/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
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
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving shipper {id}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving shipper {id}: {ex.Message}");
        }
    }
}