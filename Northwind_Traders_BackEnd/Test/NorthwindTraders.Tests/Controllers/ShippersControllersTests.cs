using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class ShippersControllerTests
{
    private static ShippersController BuildController(
        Mock<IShipperRepository>? repoMock = null)
    {
        repoMock ??= new Mock<IShipperRepository>();
        return new ShippersController(repoMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/shippers
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithShipperList()
    {
        // ARRANGE
        var shippers = new List<Shipper>
        {
            new() { ShipperId = 1, CompanyName = "Speedy Express" },
            new() { ShipperId = 2, CompanyName = "United Package"  }
        };

        var repoMock = new Mock<IShipperRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(shippers);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenNoShippers()
    {
        // ARRANGE
        var repoMock = new Mock<IShipperRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Shipper>());

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
        var repoMock = new Mock<IShipperRepository>();
        repoMock.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new InvalidOperationException("DB context error"));

        var controller = BuildController(repoMock);

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetAll());
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/shippers/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOk_WhenShipperExists()
    {
        // ARRANGE
        var shipper = new Shipper
        {
            ShipperId   = 1,
            CompanyName = "Speedy Express",
            Phone       = "555-1234",
            Orders      = new List<Order>()
        };

        var repoMock = new Mock<IShipperRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(shipper);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(1);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenShipperDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<IShipperRepository>();
        repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Shipper?)null);

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
        var repoMock = new Mock<IShipperRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1))
                .ThrowsAsync(new Exception("DB error"));

        var controller = BuildController(repoMock);

        // ACT & ASSERT
        await Assert.ThrowsAsync<Exception>(() => controller.GetById(1));
    }
}