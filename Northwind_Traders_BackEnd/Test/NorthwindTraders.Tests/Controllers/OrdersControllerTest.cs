using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Application.DTOs.Order;
using NorthwindTraders.Application.Interfaces;
using NorthwindTraders.Domain.Common;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class OrdersControllerTests
{
    // ── Build controller with mocked dependencies ─────────────────
    private static OrdersController BuildController(
        Mock<IOrderRepository>?  repoMock      = null,
        Mock<IPdfService>?       pdfMock       = null,
        Mock<IGeocodingService>? geocodingMock = null)
    {
        repoMock      ??= new Mock<IOrderRepository>();
        pdfMock       ??= new Mock<IPdfService>();
        geocodingMock ??= new Mock<IGeocodingService>();

        return new OrdersController(
            repoMock.Object,
            pdfMock.Object,
            geocodingMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/orders
    // ─────────────────────────────────────────────────────────────

    [Fact] // tells xUnit "this is a test, run it"
    public async Task GetAll_ReturnsOk_WithOrderList()
    {
        // ARRANGE
        var orders = new List<Order>
        {
            new() { OrderId = 1, CustomerId = "ALFKI" },
            new() { OrderId = 2, CustomerId = "WOLZA" }
        };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

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
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Order>());

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
        repoMock.Setup(r => r.GetOrderWithDetailsAsync(10248)).ReturnsAsync(order);

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

    [Fact]
    public async Task GetById_Returns500_WhenRepositoryThrows()
    {
        // ARRANGE
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetOrderWithDetailsAsync(1))
                .ThrowsAsync(new Exception("DB error"));

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(1);

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/orders/customer/{customerId}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCustomer_ReturnsOk_WithOrders()
    {
        // ARRANGE
        var orders = new List<Order>
        {
            new() { OrderId = 1, CustomerId = "ALFKI" },
            new() { OrderId = 2, CustomerId = "ALFKI" }
        };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetByCustomerAsync("ALFKI")).ReturnsAsync(orders);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetByCustomer("ALFKI");

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetByCustomer_ReturnsOk_WhenNoOrders()
    {
        // ARRANGE
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetByCustomerAsync("UNKNOWN"))
                .ReturnsAsync(new List<Order>());

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetByCustomer("UNKNOWN");

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/orders/status/{statusId}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByStatus_ReturnsOk_WithMatchingOrders()
    {
        // ARRANGE
        var orders = new List<Order>
        {
            new() { OrderId = 1, ShipmentStateId = 2 },
            new() { OrderId = 2, ShipmentStateId = 2 }
        };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetByShipmentStatusAsync(2)).ReturnsAsync(orders);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetByStatus(2);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetByStatus_ReturnsOk_WhenNoOrders()
    {
        // ARRANGE
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetByShipmentStatusAsync(99))
                .ReturnsAsync(new List<Order>());

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetByStatus(99);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
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
        repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.UpdateStatus(999, 2);

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/orders/{id}/pdf
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPdf_ReturnsFile_WhenOrderExists()
    {
        // ARRANGE
        var order = new Order
        {
            OrderId      = 1,
            OrderDetails = new List<OrderDetail>()
        };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetOrderWithDetailsAsync(1)).ReturnsAsync(order);

        var pdfMock = new Mock<IPdfService>();
        pdfMock.Setup(p => p.GenerateOrderPdf(It.IsAny<OrderDetailDto>()))
               .Returns(new byte[] { 1, 2, 3 }); // fake PDF bytes

        var controller = BuildController(repoMock, pdfMock);

        // ACT
        var result = await controller.GetPdf(1);

        // ASSERT
        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("Order_1.pdf", file.FileDownloadName);
    }

    [Fact]
    public async Task GetPdf_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetOrderWithDetailsAsync(999))
                .ReturnsAsync((Order?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetPdf(999);

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────
    // POST /api/v1/orders
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsCreated_WhenValidDto()
    {
        // ARRANGE
        var dto = new CreateOrderDto
        {
            CustomerId = "ALFKI",
            EmployeeId = 1,
            Lines      = new List<OrderLineInputDto>
            {
                new() { ProductId = 1, UnitPrice = 18, Quantity = 2, Discount = 0 }
            }
        };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
        repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.Create(dto);

        // ASSERT
        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNoLines()
    {
        // ARRANGE
        var dto = new CreateOrderDto
        {
            CustomerId = "ALFKI",
            Lines      = new List<OrderLineInputDto>() // empty — should fail
        };

        var controller = BuildController();

        // ACT
        var result = await controller.Create(dto);

        // ASSERT
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_Returns500_WhenRepositoryThrows()
    {
        // ARRANGE
        var dto = new CreateOrderDto
        {
            CustomerId = "ALFKI",
            Lines      = new List<OrderLineInputDto>
            {
                new() { ProductId = 1, UnitPrice = 10, Quantity = 1, Discount = 0 }
            }
        };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ThrowsAsync(new Exception("DB error"));

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.Create(dto);

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────
    // PUT /api/v1/orders/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsNoContent_WhenOrderExists()
    {
        // ARRANGE
        var order = new Order
        {
            OrderId      = 1,
            CustomerId   = "ALFKI",
            OrderDetails = new List<OrderDetail>()
        };

        var dto = new UpdateOrderDto { Freight = 99.99m };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetOrderWithDetailsAsync(1)).ReturnsAsync(order);
        repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.Update(1, dto);

        // ASSERT
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(99.99m, order.Freight); // verify field was updated
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetOrderWithDetailsAsync(999))
                .ReturnsAsync((Order?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.Update(999, new UpdateOrderDto());

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────
    // POST /api/v1/orders/{id}/geocode
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Geocode_ReturnsOk_WhenGeocodingSucceeds()
    {
        // ARRANGE
        var geocodingMock = new Mock<IGeocodingService>();
        geocodingMock.Setup(g => g.GeocodeOrderAsync(1))
                     .ReturnsAsync(Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>
                         .Success(("123 Main St", 40.71m, -74.00m, null, null, null)));

        var controller = BuildController(geocodingMock: geocodingMock);

        // ACT
        var result = await controller.Geocode(1);

        // ASSERT
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Geocode_ReturnsBadRequest_WhenGeocodingFails()
    {
        // ARRANGE
        var geocodingMock = new Mock<IGeocodingService>();
        geocodingMock.Setup(g => g.GeocodeOrderAsync(1))
                     .ReturnsAsync(Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>
                         .Failure("Order 1 not found."));

        var controller = BuildController(geocodingMock: geocodingMock);

        // ACT
        var result = await controller.Geocode(1);

        // ASSERT
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────
    // POST /api/v1/orders/geocode-all
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GeocodeAll_ReturnsOk_WithSummary()
    {
        // ARRANGE
        var geocodingMock = new Mock<IGeocodingService>();
        geocodingMock.Setup(g => g.GeocodeAllPendingAsync())
                     .ReturnsAsync((processed: 10, succeeded: 8, failed: 2));

        var controller = BuildController(geocodingMock: geocodingMock);

        // ACT
        var result = await controller.GeocodeAll();

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GeocodeAll_Returns500_WhenServiceThrows()
    {
        // ARRANGE
        var geocodingMock = new Mock<IGeocodingService>();
        geocodingMock.Setup(g => g.GeocodeAllPendingAsync())
                     .ThrowsAsync(new Exception("Google Maps API down"));

        var controller = BuildController(geocodingMock: geocodingMock);

        // ACT
        var result = await controller.GeocodeAll();

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }
}