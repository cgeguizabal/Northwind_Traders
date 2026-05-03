using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindTraders.Application.DTOs.Supplier;
using NorthwindTraders.Domain.Interfaces;

namespace NorthwindTraders.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierRepository _repository;

    public SuppliersController(ISupplierRepository repository)
    {
        _repository = repository;
    }

    // GET api/v1/suppliers
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var suppliers = await _repository.GetAllAsync();

            var dtos = suppliers.Select(s => new SupplierSummaryDto
            {
                SupplierId    = s.SupplierId,
                CompanyName   = s.CompanyName,
                ContactName   = s.ContactName,
                ContactTitle  = s.ContactTitle,
                City          = s.City,
                Country       = s.Country,
                Phone         = s.Phone,
                TotalProducts = s.Products?.Count ?? 0
            }).ToList();

            return Ok(dtos);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving suppliers: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving suppliers: {ex.Message}");
        }
    }

    // GET api/v1/suppliers/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var supplier = await _repository.GetByIdAsync(id);

            if (supplier is null)
                return NotFound($"Supplier with id {id} was not found.");

            var dto = new SupplierDetailDto
            {
                SupplierId   = supplier.SupplierId,
                CompanyName  = supplier.CompanyName,
                ContactName  = supplier.ContactName,
                ContactTitle = supplier.ContactTitle,
                Address      = supplier.Address,
                City         = supplier.City,
                Region       = supplier.Region,
                PostalCode   = supplier.PostalCode,
                Country      = supplier.Country,
                Phone        = supplier.Phone,
                Fax          = supplier.Fax,
                HomePage     = supplier.HomePage,
                Products     = supplier.Products?.Select(p => new SupplierProductDto
                {
                    ProductId    = p.ProductId,
                    ProductName  = p.ProductName,
                    UnitPrice    = p.UnitPrice,
                    UnitsInStock = p.UnitsInStock,
                    Discontinued = p.Discontinued
                }).ToList() ?? []
            };

            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving supplier {id}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving supplier {id}: {ex.Message}");
        }
    }
}