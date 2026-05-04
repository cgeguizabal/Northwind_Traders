using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class CategoriesControllerTests
{
    private static CategoriesController BuildController(
        Mock<ICategoryRepository>? repoMock = null)
    {
        repoMock ??= new Mock<ICategoryRepository>();
        return new CategoriesController(repoMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/categories
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithCategoryList()
    {
        // ARRANGE
        var categories = new List<Category>
        {
            new() { CategoryId = 1, CategoryName = "Beverages" },
            new() { CategoryId = 2, CategoryName = "Condiments" }
        };

        var repoMock = new Mock<ICategoryRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenNoCategories()
    {
        // ARRANGE
        var repoMock = new Mock<ICategoryRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetAll_Returns500_WhenRepositoryThrows()
    {
        // ARRANGE
        var repoMock = new Mock<ICategoryRepository>();
        repoMock.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new InvalidOperationException("DB context error"));

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/categories/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOk_WhenCategoryExists()
    {
        // ARRANGE
        var category = new Category
        {
            CategoryId   = 1,
            CategoryName = "Beverages",
            Products     = new List<Product>()
        };

        var repoMock = new Mock<ICategoryRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(1);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<ICategoryRepository>();
        repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Category?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(999);

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_Returns500_WhenRepositoryThrows()
    {
        // ARRANGE
        var repoMock = new Mock<ICategoryRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1))
                .ThrowsAsync(new Exception("DB error"));

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(1);

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }
}