using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NorthwindTraders.Application.DTOs.Auth;
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Application.Interfaces;

namespace NorthwindTraders.API.Controllers;

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
        try
        {
            var employee = await _employeeRepository.GetByEmailAsync(request.Email);
            if (employee is null)
                return Unauthorized("Invalid email or password.");

            var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash);
            if (!passwordValid)
                return Unauthorized("Invalid email or password.");

            var token      = _jwtService.GenerateToken(employee);
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
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred during login: {ex.Message}");
        }
    }

    // POST api/v1/auth/change-password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest("New password is required.");

            if (request.NewPassword.Length < 8)
                return BadRequest("Password must be at least 8 characters.");

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("nameid")?.Value;

            if (employeeIdClaim is null || !int.TryParse(employeeIdClaim, out int employeeId))
                return Unauthorized("Invalid token.");

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee is null)
                return NotFound("Employee not found.");

            employee.PasswordHash      = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            employee.MustChangePassword = false;
            _employeeRepository.Update(employee);
            await _employeeRepository.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
        }
    }
}