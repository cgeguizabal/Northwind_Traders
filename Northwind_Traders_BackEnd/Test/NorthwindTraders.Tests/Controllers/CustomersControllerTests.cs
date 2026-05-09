using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class CustomersControllerTests
{
    private static CustomersController BuildController(
        Mock<ICustomerRepository>? repoMock = null)
    {
        repoMock ??= new Mock<ICustomerRepository>();
        return new CustomersController(repoMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/customers
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithCustomerList()
    {
        // ARRANGE
        var customers = new List<Customer>
        {
            new() { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste", Orders = new List<Order>() },
            new() { CustomerId = "WOLZA", CompanyName = "Wolski Zajazd",       Orders = new List<Order>() }
        };

        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetPagedAsync(1, 10, null))
                .ReturnsAsync(((IReadOnlyList<Customer>)customers, 2));

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenNoCustomers()
    {
        // ARRANGE
        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetPagedAsync(1, 10, null))
                .ReturnsAsync(((IReadOnlyList<Customer>)new List<Customer>(), 0));

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
        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ThrowsAsync(new InvalidOperationException("DB context error"));

        var controller = BuildController(repoMock);

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetAll());
    }

    [Fact]
    public async Task GetAll_Returns499_WhenRequestCancelled()
    {
        // ARRANGE
        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ThrowsAsync(new OperationCanceledException("Request cancelled"));

        var controller = BuildController(repoMock);

        // ACT & ASSERT
        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.GetAll());
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/customers/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOk_WhenCustomerExists()
    {
        // ARRANGE
        var customer = new Customer
        {
            CustomerId  = "ALFKI",
            CompanyName = "Alfreds Futterkiste",
            Orders      = new List<Order>()
        };

        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetByIdAsync("ALFKI")).ReturnsAsync(customer);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById("ALFKI");

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetByIdAsync("XXXXX")).ReturnsAsync((Customer?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById("XXXXX");

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_Returns500_WhenRepositoryThrows()
    {
        // ARRANGE
        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetByIdAsync("ALFKI"))
                .ThrowsAsync(new InvalidOperationException("DB context error"));

        var controller = BuildController(repoMock);

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetById("ALFKI"));
    }

    [Fact]
    public async Task GetById_Returns499_WhenRequestCancelled()
    {
        // ARRANGE
        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetByIdAsync("ALFKI"))
                .ThrowsAsync(new OperationCanceledException("Request cancelled"));

        var controller = BuildController(repoMock);

        // ACT & ASSERT
        await Assert.ThrowsAsync<OperationCanceledException>(() => controller.GetById("ALFKI"));
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/customers/{id}/map
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMap_ReturnsOk_WithMapPins()
    {
        // ARRANGE
        var customer = new Customer { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste" };

        var orders = new List<Order>
        {
            new() { OrderId = 1, ShipLatitude = 40.71m, ShipLongitude = -74.00m },
            new() { OrderId = 2, ShipLatitude = 51.50m, ShipLongitude = -0.12m  }
        };

        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetByIdAsync("ALFKI")).ReturnsAsync(customer);
        repoMock.Setup(r => r.GetOrdersByCustomerAsync("ALFKI")).ReturnsAsync(orders);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetMap("ALFKI");

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetMap_ReturnsOk_WhenNoGeocodeOrders()
    {
        // ARRANGE — customer exists but has no geocoded orders
        var customer = new Customer { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste" };

        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetByIdAsync("ALFKI")).ReturnsAsync(customer);
        repoMock.Setup(r => r.GetOrdersByCustomerAsync("ALFKI")).ReturnsAsync(new List<Order>());

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetMap("ALFKI");

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetMap_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetByIdAsync("XXXXX")).ReturnsAsync((Customer?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetMap("XXXXX");

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetMap_Returns500_WhenRepositoryThrows()
    {
        // ARRANGE
        var customer = new Customer { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste" };

        var repoMock = new Mock<ICustomerRepository>();
        repoMock.Setup(r => r.GetByIdAsync("ALFKI")).ReturnsAsync(customer);
        repoMock.Setup(r => r.GetOrdersByCustomerAsync("ALFKI"))
                .ThrowsAsync(new InvalidOperationException("DB context error"));

        var controller = BuildController(repoMock);

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetMap("ALFKI"));
    }
}