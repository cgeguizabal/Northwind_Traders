using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class SuppliersControllerTests
{
    private static SuppliersController BuildController(
        Mock<ISupplierRepository>? repoMock = null)
    {
        repoMock ??= new Mock<ISupplierRepository>();
        return new SuppliersController(repoMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/suppliers
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithSupplierList()
    {
        // ARRANGE
        var suppliers = new List<Supplier>
        {
            new() { SupplierId = 1, CompanyName = "Exotic Liquids"   },
            new() { SupplierId = 2, CompanyName = "New Orleans Cajun" }
        };

        var repoMock = new Mock<ISupplierRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(suppliers);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenNoSuppliers()
    {
        // ARRANGE
        var repoMock = new Mock<ISupplierRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier>());

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
        var repoMock = new Mock<ISupplierRepository>();
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
    // GET /api/v1/suppliers/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOk_WhenSupplierExists()
    {
        // ARRANGE
        var supplier = new Supplier
        {
            SupplierId  = 1,
            CompanyName = "Exotic Liquids",
            Products    = new List<Product>()
        };

        var repoMock = new Mock<ISupplierRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(1);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<ISupplierRepository>();
        repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Supplier?)null);

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
        var repoMock = new Mock<ISupplierRepository>();
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