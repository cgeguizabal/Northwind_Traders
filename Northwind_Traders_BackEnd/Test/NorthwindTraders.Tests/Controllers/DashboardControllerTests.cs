using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Application.DTOs.Dashboard;
using NorthwindTraders.Application.Interfaces;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class DashboardControllerTests
{
    // ── IDashboardService is an interface → fully mockable ────────
    private static DashboardController BuildController(
        Mock<IDashboardService>? serviceMock = null)
    {
        serviceMock ??= new Mock<IDashboardService>();
        return new DashboardController(serviceMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/dashboard  (no date filter)
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboard_ReturnsOk_WithDashboardData()
    {
        // ARRANGE
        var dashboard = new DashboardDto
        {
            TotalOrders    = 100,
            TotalRevenue   = 50000m,
            TotalCustomers = 30,
            TotalEmployees = 9,
            OrdersByStatus = new List<OrdersByStatusDto>
            {
                new() { Status = "Shipped",   Count = 60 },
                new() { Status = "Pending",   Count = 30 },
                new() { Status = "Cancelled", Count = 10 }
            },
            TopCustomers = new List<TopCustomerDto>
            {
                new() { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste", OrderCount = 12 }
            },
            TopEmployees = new List<TopEmployeeDto>
            {
                new() { FullName = "Nancy Davolio", OrderCount = 25 }
            }
        };

        var serviceMock = new Mock<IDashboardService>();

        // It.IsAny<DateTime?>() — matches null OR any DateTime value
        serviceMock.Setup(s => s.GetDashboardAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                   .ReturnsAsync(dashboard);

        var controller = BuildController(serviceMock);

        // ACT
        var result = await controller.GetDashboard(null, null);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/dashboard?dateFrom=2024-01-01&dateTo=2024-12-31
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboard_ReturnsOk_WithDateRangeFilter()
    {
        // ARRANGE
        var dateFrom = new DateTime(2024, 1, 1);
        var dateTo   = new DateTime(2024, 12, 31);

        var dashboard = new DashboardDto
        {
            TotalOrders    = 40,
            TotalRevenue   = 20000m,
            TotalCustomers = 30,
            TotalEmployees = 9,
            OrdersByStatus = new List<OrdersByStatusDto>(),
            TopCustomers   = new List<TopCustomerDto>(),
            TopEmployees   = new List<TopEmployeeDto>()
        };

        var serviceMock = new Mock<IDashboardService>();
        serviceMock.Setup(s => s.GetDashboardAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                   .ReturnsAsync(dashboard);

        var controller = BuildController(serviceMock);

        // ACT
        var result = await controller.GetDashboard(dateFrom, dateTo);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/dashboard  — 500 error path
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboard_Returns500_WhenServiceThrows()
    {
        // ARRANGE
        var serviceMock = new Mock<IDashboardService>();
        serviceMock.Setup(s => s.GetDashboardAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                   .ThrowsAsync(new Exception("DB error"));

        var controller = BuildController(serviceMock);

        // ACT
        var result = await controller.GetDashboard(null, null);

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }
}