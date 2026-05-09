using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NorthwindTraders.Application.DTOs.Auth;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Application.Interfaces;

namespace NorthwindTraders.API.Controllers;

// Handles employee authentication — login and password change.
// No [Authorize] at class level because the Login endpoint must be publicly accessible.
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IJwtService _jwtService;

    public AuthController(IEmployeeRepository employeeRepository, IJwtService jwtService)
    {
        _employeeRepository = employeeRepository;
        _jwtService         = jwtService;
    }

    // POST api/v1/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var employee = await _employeeRepository.GetByEmailAsync(request.Email);
        if (employee is null)
            return Unauthorized("Invalid email or password.");

        // BCrypt.Verify — compares the plaintext input against the stored hash safely
        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash);
        if (!passwordValid)
            return Unauthorized("Invalid email or password.");

        var token      = _jwtService.GenerateToken(employee);
        // Read expiry from config to align ExpiresAt with the actual token lifetime
        var expiryMins = int.Parse(HttpContext.RequestServices
                            .GetRequiredService<IConfiguration>()["Jwt:ExpiryMinutes"]!);

        var response = new LoginResponseDto
        {
            Token               = token,
            Email               = employee.Email!,
            FullName            = $"{employee.FirstName} {employee.LastName}",
            ExpiresAt           = DateTime.UtcNow.AddMinutes(expiryMins),
            MustChangePassword  = employee.MustChangePassword
        };

        return Ok(response);
    }

    // POST api/v1/auth/change-password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest("New password is required.");

        if (request.NewPassword.Length < 8)
            return BadRequest("Password must be at least 8 characters.");

        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest("Passwords do not match.");

        // Extract the employee id from the JWT claims on the current request
        var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("nameid")?.Value;

        if (employeeIdClaim is null || !int.TryParse(employeeIdClaim, out int employeeId))
            return Unauthorized("Invalid token.");

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee is null)
            return NotFound("Employee not found.");

        // BCrypt.HashPassword — hashes the new password before storing it
        employee.PasswordHash      = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        employee.MustChangePassword = false;
        _employeeRepository.Update(employee);
        await _employeeRepository.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully." });
    }
}