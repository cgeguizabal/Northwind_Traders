using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Infrastructure.Services;
using Xunit;
using NorthwindTraders.Application.Interfaces; 

namespace NorthwindTraders.Tests.Controllers;

public class OrdersControllerTests
{
    // ── Build controller with mocked repo ─────────────────────────
    // PdfService has no constructor args → instantiate directly
    // GeocodingService needs HttpClient+Config+DbContext → we never
    // call geocode endpoints in these tests so we pass null via
    // a simple subclass trick OR use the real constructor minimally.
    // Cleanest: extract helpers that only need the repo mock.

    private static OrdersController BuildController(
        Mock<IOrderRepository> repoMock)
    {
        var pdfService = new PdfService();  // no dependencies — safe to instantiate

        // GeocodingService needs real deps we don't have in tests.
        // Since none of our tests call geocode endpoints, we pass null!
        // The controller only uses _geocodingService in geocode actions.
        // We cast to bypass the compiler null warning.
        var geocodingService = (GeocodingService)null!;

        return new OrdersController(
            repoMock.Object,
            pdfService,
            geocodingService);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/orders
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithOrderList()
    {
        // ARRANGE
        var orders = new List<Order>
        {
            new() { OrderId = 1, CustomerId = "ALFKI" },
            new() { OrderId = 2, CustomerId = "WOLZA" }
        };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(orders);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenNoOrders()
    {
        // ARRANGE
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Order>());

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/orders/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOk_WhenOrderExists()
    {
        // ARRANGE
        var order = new Order
        {
            OrderId      = 10248,
            CustomerId   = "VINET",
            OrderDetails = new List<OrderDetail>()
        };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetOrderWithDetailsAsync(10248))
                .ReturnsAsync(order);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(10248);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetOrderWithDetailsAsync(999))
                .ReturnsAsync((Order?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(999);

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────
    // PUT /api/v1/orders/{id}/status
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatus_ReturnsNoContent_WhenOrderExists()
    {
        // ARRANGE
        var order    = new Order { OrderId = 1, ShipmentStateId = 1 };
        var repoMock = new Mock<IOrderRepository>();

        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.UpdateStatus(1, 3);

        // ASSERT
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(3, order.ShipmentStateId); // verify it was mutated
    }

    [Fact]
    public async Task UpdateStatus_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Order?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.UpdateStatus(999, 2);

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/orders — 500 error path
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Returns500_WhenRepositoryThrows()
    {
        // ARRANGE
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new Exception("DB connection lost"));

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }
}