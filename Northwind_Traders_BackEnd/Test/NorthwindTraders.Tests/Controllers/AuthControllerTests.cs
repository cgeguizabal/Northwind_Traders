using Microsoft.AspNetCore.Mvc;
using Moq;
using NorthwindTraders.API.Controllers;
using NorthwindTraders.Application.Interfaces;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using Xunit;

namespace NorthwindTraders.Tests.Controllers;

public class AuthControllerTests
{
    private static AuthController BuildController(
        Mock<IEmployeeRepository>? repoMock = null,
        Mock<IJwtService>?         jwtMock  = null)
    {
        repoMock ??= new Mock<IEmployeeRepository>();
        jwtMock  ??= new Mock<IJwtService>();
        return new AuthController(repoMock.Object, jwtMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // POST /api/v1/auth/login
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenEmployeeNotFound()
    {
        // ARRANGE — no employee with that email
        var repoMock = new Mock<IEmployeeRepository>();
        repoMock.Setup(r => r.GetByEmailAsync("wrong@email.com"))
                .ReturnsAsync((Employee?)null);

        var controller = BuildController(repoMock);

        var request = new Application.DTOs.Auth.LoginRequestDto
        {
            Email    = "wrong@email.com",
            Password = "anypassword"
        };

        // ACT
        var result = await controller.Login(request);

        // ASSERT
        Assert.IsType<UnauthorizedObjectResult>(result); // 401
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        // ARRANGE — employee exists but password hash won't match
        // BCrypt.HashPassword generates a real hash for "correctpassword"
        var employee = new Employee
        {
            EmployeeId   = 1,
            FirstName    = "Nancy",
            LastName     = "Davolio",
            Email        = "nancy@northwind.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };

        var repoMock = new Mock<IEmployeeRepository>();
        repoMock.Setup(r => r.GetByEmailAsync("nancy@northwind.com"))
                .ReturnsAsync(employee);

        var controller = BuildController(repoMock);

        var request = new Application.DTOs.Auth.LoginRequestDto
        {
            Email    = "nancy@northwind.com",
            Password = "wrongpassword" // ← does not match the hash
        };

        // ACT
        var result = await controller.Login(request);

        // ASSERT
        Assert.IsType<UnauthorizedObjectResult>(result); // 401
    }

    [Fact]
    public async Task Login_Returns500_WhenRepositoryThrows()
    {
        // ARRANGE
        var repoMock = new Mock<IEmployeeRepository>();
        repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("DB error"));

        var controller = BuildController(repoMock);

        var request = new Application.DTOs.Auth.LoginRequestDto
        {
            Email    = "nancy@northwind.com",
            Password = "anypassword"
        };

        // ACT
        var result = await controller.Login(request);

        // ASSERT
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }
}