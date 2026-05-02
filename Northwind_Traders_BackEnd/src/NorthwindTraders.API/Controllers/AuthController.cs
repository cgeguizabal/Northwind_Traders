using Microsoft.AspNetCore.Mvc;                      // C#
using NorthwindTraders.Application.DTOs.Auth;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Infrastructure.Services;

namespace NorthwindTraders.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]

public class AuthController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly JwtService _jwtService;

    public AuthController(IEmployeeRepository employeeRepository, JwtService jwtService)
    {
        _employeeRepository = employeeRepository;
        _jwtService         = jwtService;
    }

    // POST api/v1/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        // Step 1 — find employee by email
        var employee = await _employeeRepository.GetByEmailAsync(request.Email);

        if (employee is null)
            return Unauthorized("Invalid email or password.");  // C# — HTTP 401

        // Step 2 — verify password
        // BCrypt.Verify — BCrypt.Net-Next package Method
        // Compares the plain text password against the stored hash
        // Never decrypts — re-hashes and compares
        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash);

        if (!passwordValid)
            return Unauthorized("Invalid email or password.");  // same message — never reveal which field failed

        // Step 3 — generate JWT token
        var token     = _jwtService.GenerateToken(employee);
        var expiryMins = int.Parse(HttpContext.RequestServices
                            .GetRequiredService<IConfiguration>()["Jwt:ExpiryMinutes"]!);

        var response = new LoginResponseDto
        {
            Token     = token,
            Email     = employee.Email!,
            FullName  = $"{employee.FirstName} {employee.LastName}",
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMins)
        };

        return Ok(response);   // C# — HTTP 200 with token
    }
}