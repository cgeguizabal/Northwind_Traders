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

    // GET api/v1/categories/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
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
}