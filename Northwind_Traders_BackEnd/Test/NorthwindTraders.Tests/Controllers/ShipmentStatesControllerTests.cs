using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class ShipmentStatesControllerTests
{
    private static ShipmentStatesController BuildController(
        Mock<IShipmentStateRepository>? repoMock = null)
    {
        repoMock ??= new Mock<IShipmentStateRepository>();
        return new ShipmentStatesController(repoMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/shipmentstates
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithStateList()
    {
        // ARRANGE
        var states = new List<ShipmentState>
        {
            new() { ShipmentStateId = 1, Name = "Pending"   },
            new() { ShipmentStateId = 2, Name = "Shipped"   },
            new() { ShipmentStateId = 3, Name = "Completed" }
        };

        var repoMock = new Mock<IShipmentStateRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(states);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenNoStates()
    {
        // ARRANGE
        var repoMock = new Mock<IShipmentStateRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ShipmentState>());

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
        var repoMock = new Mock<IShipmentStateRepository>();
        repoMock.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new Exception("DB error"));

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }
}