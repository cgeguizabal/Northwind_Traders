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

    // GET api/v1/customers?page=1&pageSize=10&search=text
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var (customers, totalCount) = await _repository.GetPagedAsync(page, pageSize, search);

        var items = customers.Select(c => new CustomerSummaryDto
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

        return Ok(new PagedResult<CustomerSummaryDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    // GET api/v1/customers/ALFKI
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
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

    // GET api/v1/customers/ALFKI/map
    // Returns all geocoded order locations for a customer — used to drop map pins
    [HttpGet("{id}/map")]
    public async Task<IActionResult> GetMap(string id)
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
}