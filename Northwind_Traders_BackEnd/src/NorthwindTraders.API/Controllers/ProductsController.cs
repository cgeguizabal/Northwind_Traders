using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindTraders.Application.DTOs.Product;
using NorthwindTraders.Domain.Interfaces;

namespace NorthwindTraders.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;

    public ProductsController(IProductRepository repository)
    {
        _repository = repository;
    }

    // GET api/v1/products
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var products = await _repository.GetAllAsync();

            var dtos = products.Select(p => new ProductSummaryDto
            {
                ProductId    = p.ProductId,
                ProductName  = p.ProductName,
                CategoryName = p.Category?.CategoryName,
                SupplierName = p.Supplier?.CompanyName,
                UnitPrice    = p.UnitPrice,
                UnitsInStock = p.UnitsInStock,
                UnitsOnOrder = p.UnitsOnOrder,
                ReorderLevel = p.ReorderLevel,
                Discontinued = p.Discontinued
            }).ToList();

            return Ok(dtos);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving products: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving products: {ex.Message}");
        }
    }

    // GET api/v1/products/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _repository.GetByIdAsync(id);

            if (product is null)
                return NotFound($"Product with id {id} was not found.");

            var dto = new ProductDetailDto
            {
                ProductId       = product.ProductId,
                ProductName     = product.ProductName,
                QuantityPerUnit = product.QuantityPerUnit,
                CategoryName    = product.Category?.CategoryName,
                SupplierName    = product.Supplier?.CompanyName,
                UnitPrice       = product.UnitPrice,
                UnitsInStock    = product.UnitsInStock,
                UnitsOnOrder    = product.UnitsOnOrder,
                ReorderLevel    = product.ReorderLevel,
                Discontinued    = product.Discontinued,
                // LowStock — true when stock is at or below the reorder threshold
                LowStock        = product.UnitsInStock <= product.ReorderLevel
            };

            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving product {id}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving product {id}: {ex.Message}");
        }
    }

    // GET api/v1/products/category/1
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        try
        {
            var products = await _repository.GetByCategoryAsync(categoryId);

            var dtos = products.Select(p => new ProductSummaryDto
            {
                ProductId    = p.ProductId,
                ProductName  = p.ProductName,
                CategoryName = p.Category?.CategoryName,
                SupplierName = p.Supplier?.CompanyName,
                UnitPrice    = p.UnitPrice,
                UnitsInStock = p.UnitsInStock,
                UnitsOnOrder = p.UnitsOnOrder,
                ReorderLevel = p.ReorderLevel,
                Discontinued = p.Discontinued
            }).ToList();

            return Ok(dtos);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving products for category {categoryId}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving products for category {categoryId}: {ex.Message}");
        }
    }

    // GET api/v1/products/active
    // Returns only non-discontinued products — used in order forms
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var products = await _repository.GetActiveProductsAsync();

            var dtos = products.Select(p => new ProductSummaryDto
            {
                ProductId    = p.ProductId,
                ProductName  = p.ProductName,
                CategoryName = p.Category?.CategoryName,
                SupplierName = p.Supplier?.CompanyName,
                UnitPrice    = p.UnitPrice,
                UnitsInStock = p.UnitsInStock,
                UnitsOnOrder = p.UnitsOnOrder,
                ReorderLevel = p.ReorderLevel,
                Discontinued = p.Discontinued
            }).ToList();

            return Ok(dtos);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving active products: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving active products: {ex.Message}");
        }
    }
}