using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class ProductsControllerTests
{
    private static ProductsController BuildController(
        Mock<IProductRepository>? repoMock = null)
    {
        repoMock ??= new Mock<IProductRepository>();
        return new ProductsController(repoMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/products
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithProductList()
    {
        // ARRANGE
        var products = new List<Product>
        {
            new() { ProductId = 1, ProductName = "Chai"  },
            new() { ProductId = 2, ProductName = "Chang" }
        };

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

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
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new InvalidOperationException("DB error"));

        var controller = BuildController(repoMock);

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetAll());
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/products/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOk_WhenProductExists()
    {
        // ARRANGE
        var product = new Product
        {
            ProductId    = 1,
            ProductName  = "Chai",
            UnitsInStock = 5,
            ReorderLevel = 10  // UnitsInStock <= ReorderLevel → LowStock = true
        };

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(1);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(999);

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/products/category/{categoryId}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCategory_ReturnsOk_WithProducts()
    {
        // ARRANGE
        var products = new List<Product>
        {
            new() { ProductId = 1, ProductName = "Chai" }
        };

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetByCategoryAsync(1)).ReturnsAsync(products);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetByCategory(1);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetByCategory_ReturnsOk_WhenNoProducts()
    {
        // ARRANGE
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetByCategoryAsync(99)).ReturnsAsync(new List<Product>());

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetByCategory(99);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/products/active
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetActive_ReturnsOk_WithActiveProducts()
    {
        // ARRANGE
        var products = new List<Product>
        {
            new() { ProductId = 1, ProductName = "Chai",  Discontinued = false },
            new() { ProductId = 2, ProductName = "Chang", Discontinued = false }
        };

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetActiveProductsAsync()).ReturnsAsync(products);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetActive();

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetActive_Returns500_WhenRepositoryThrows()
    {
        // ARRANGE
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetActiveProductsAsync())
                .ThrowsAsync(new Exception("DB error"));

        var controller = BuildController(repoMock);

        // ACT & ASSERT
        await Assert.ThrowsAsync<Exception>(() => controller.GetActive());
    }
}