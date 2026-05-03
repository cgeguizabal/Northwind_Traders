using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NorthwindTraders.Application.DTOs.Customer;
using NorthwindTraders.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace NorthwindTraders.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;

    public CustomersController(ICustomerRepository repository)
    {
        _repository = repository;
    }

    // GET api/v1/customers
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var customers = await _repository.GetAllAsync();

            var dtos = customers.Select(c => new CustomerSummaryDto
            {
                CustomerId   = c.CustomerId,
                CompanyName  = c.CompanyName,
                ContactName  = c.ContactName,
                ContactTitle = c.ContactTitle,
                City         = c.City,
                Country      = c.Country,
                Phone        = c.Phone,
                TotalOrders  = c.Orders?.Count ?? 0
            }).ToList();

            return Ok(dtos);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving customers: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving customers: {ex.Message}");
        }
    }

    // GET api/v1/customers/ALFKI
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var customer = await _repository.GetByIdAsync(id);

            if (customer is null)
                return NotFound($"Customer with id '{id}' was not found.");

            var dto = new CustomerDetailDto
            {
                CustomerId   = customer.CustomerId,
                CompanyName  = customer.CompanyName,
                ContactName  = customer.ContactName,
                ContactTitle = customer.ContactTitle,
                Address      = customer.Address,
                City         = customer.City,
                Region       = customer.Region,
                PostalCode   = customer.PostalCode,
                Country      = customer.Country,
                Phone        = customer.Phone,
                Fax          = customer.Fax,
                Orders       = customer.Orders?.Select(o => new CustomerOrderDto
                {
                    OrderId        = o.OrderId,
                    OrderDate      = o.OrderDate,
                    ShippedDate    = o.ShippedDate,
                    Freight        = o.Freight,
                    ShipmentStatus = o.ShipmentState?.Name,
                    ShipCountry    = o.ShipCountry
                }).ToList() ?? []
            };

            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving customer '{id}': {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving customer '{id}': {ex.Message}");
        }
    }

    // GET api/v1/customers/ALFKI/map
    // Returns all geocoded order locations for a customer — used to drop map pins
    [HttpGet("{id}/map")]
    public async Task<IActionResult> GetMap(string id)
    {
        try
        {
            var customer = await _repository.GetByIdAsync(id);

            if (customer is null)
                return NotFound($"Customer with id '{id}' was not found.");

            var orders = await _repository.GetOrdersByCustomerAsync(id);

            var pins = orders.Select(o => new CustomerMapDto
            {
                OrderId              = o.OrderId,
                ShipName             = o.ShipName,
                ValidatedShipAddress = o.ValidatedShipAddress,
                ShipLatitude         = o.ShipLatitude,
                ShipLongitude        = o.ShipLongitude,
                ShippedDate          = o.ShippedDate,
                ShipmentStatus       = o.ShipmentState?.Name
            }).ToList();

            return Ok(pins);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving map data for customer '{id}': {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving map data for customer '{id}': {ex.Message}");
        }
    }
}