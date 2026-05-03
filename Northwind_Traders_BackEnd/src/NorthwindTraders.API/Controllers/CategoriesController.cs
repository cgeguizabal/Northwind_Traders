using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindTraders.Application.DTOs.Category;
using NorthwindTraders.Domain.Interfaces;

namespace NorthwindTraders.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _repository;

    public CategoriesController(ICategoryRepository repository)
    {
        _repository = repository;
    }

    // GET api/v1/categories
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var categories = await _repository.GetAllAsync();

            var dtos = categories.Select(c => new CategorySummaryDto
            {
                CategoryId    = c.CategoryId,
                CategoryName  = c.CategoryName,
                Description   = c.Description,
                TotalProducts = c.Products?.Count ?? 0
            }).ToList();

            return Ok(dtos);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, $"Database context error while retrieving categories: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving categories: {ex.Message}");
        }
    }

    // GET api/v1/categories/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var category = await _repository.GetByIdAsync(id);

            if (category is null)
                return NotFound($"Category with id {id} was not found.");

            var dto = new CategoryDetailDto
            {
                CategoryId   = category.CategoryId,
                CategoryName = category.CategoryName,
                Description  = category.Description,
                Products     = category.Products?.Select(p => new CategoryProductDto
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
            return StatusCode(500, $"Database context error while retrieving category {id}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return StatusCode(499, $"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving category {id}: {ex.Message}");
        }
    }
}