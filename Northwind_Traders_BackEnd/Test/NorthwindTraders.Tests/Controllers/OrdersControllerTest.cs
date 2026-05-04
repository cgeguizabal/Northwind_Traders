using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Infrastructure.Services;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class OrdersControllerTests
{
    // ── Helpers: build a controller with mocked dependencies ──────
    private static OrdersController BuildController(
        Mock<IOrderRepository>? repoMock = null)
    {
        repoMock        ??= new Mock<IOrderRepository>();
        var pdfMock       = new Mock<PdfService>();
        var geocodeMock   = new Mock<GeocodingService>();

        return new OrdersController(
            repoMock.Object,
            pdfMock.Object,
            geocodeMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/orders
    // ───────────────────────────────��─────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithOrderList()
    {
        // ARRANGE
        var orders = new List<Order>
        {
            new Order { OrderId = 1, CustomerId = "ALFKI" },
            new Order { OrderId = 2, CustomerId = "WOLZA" }
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
    public async Task GetAll_ReturnsEmpty_WhenNoOrders()
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
            OrderId    = 10248,
            CustomerId = "VINET",
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
                .ReturnsAsync((Order?)null);   // simulate not found

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
        Assert.Equal(3, order.ShipmentStateId); // verify it was actually updated
    }

    [Fact]
    public async Task UpdateStatus_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.UpdateStatus(999, 2);

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }
}