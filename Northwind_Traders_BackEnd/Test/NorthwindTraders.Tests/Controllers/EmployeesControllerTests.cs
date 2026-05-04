using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Application.DTOs.Employee;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class EmployeesControllerTests
{
    private static EmployeesController BuildController(
        Mock<IEmployeeRepository>? repoMock = null)
    {
        repoMock ??= new Mock<IEmployeeRepository>();
        return new EmployeesController(repoMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/employees
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithEmployeeList()
    {
        // ARRANGE
        var employees = new List<Employee>
        {
            new() { EmployeeId = 1, FirstName = "Nancy", LastName = "Davolio" },
            new() { EmployeeId = 2, FirstName = "Andrew", LastName = "Fuller"  }
        };

        var repoMock = new Mock<IEmployeeRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(employees);

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
        var repoMock = new Mock<IEmployeeRepository>();
        repoMock.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new Exception("DB error"));

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetAll();

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/v1/employees/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOk_WhenEmployeeExists()
    {
        // ARRANGE
        var employee = new Employee
        {
            EmployeeId = 1,
            FirstName  = "Nancy",
            LastName   = "Davolio"
        };

        var repoMock = new Mock<IEmployeeRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(1);

        // ASSERT
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenEmployeeDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<IEmployeeRepository>();
        repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Employee?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.GetById(999);

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────
    // PUT /api/v1/employees/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsNoContent_WhenEmployeeExists()
    {
        // ARRANGE
        var employee = new Employee { EmployeeId = 1, FirstName = "Nancy", LastName = "Davolio" };
        var dto      = new UpdateEmployeeDto { Title = "Sales Manager", City = "Seattle" };

        var repoMock = new Mock<IEmployeeRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);
        repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.Update(1, dto);

        // ASSERT
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Sales Manager", employee.Title); // verify field was updated
        Assert.Equal("Seattle", employee.City);        // verify field was updated
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenEmployeeDoesNotExist()
    {
        // ARRANGE
        var repoMock = new Mock<IEmployeeRepository>();
        repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Employee?)null);

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.Update(999, new UpdateEmployeeDto());

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_Returns500_WhenRepositoryThrows()
    {
        // ARRANGE
        var repoMock = new Mock<IEmployeeRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1))
                .ThrowsAsync(new Exception("DB error"));

        var controller = BuildController(repoMock);

        // ACT
        var result = await controller.Update(1, new UpdateEmployeeDto());

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }
}