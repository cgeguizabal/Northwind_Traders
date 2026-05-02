using System.IdentityModel.Tokens.Jwt;      // JWT package — JwtSecurityToken, JwtSecurityTokenHandler
using System.Security.Claims;               // C# built in — Claim, ClaimTypes
using System.Text;                          // C# built in — Encoding
using Microsoft.Extensions.Configuration;  // C# built in — IConfiguration
using Microsoft.IdentityModel.Tokens;       // JWT package — SymmetricSecurityKey, SigningCredentials
using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Infrastructure.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    // IConfiguration — C# built in — reads appsettings.json values
    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Employee employee)
    {
        // Read settings from appsettings.json
        var key      = _configuration["Jwt:Key"]!;
        var issuer   = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;
        var expiry   = int.Parse(_configuration["Jwt:ExpiryMinutes"]!);

        // Claims — the data INSIDE the token payload
        // Anyone can READ claims from the token (they are base64 encoded, not encrypted)
        // Nobody can FAKE claims because the signature would break
        var claims = new[]
        {
            // ClaimTypes — C# built in — standard claim name constants
            new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
            new Claim(ClaimTypes.Email,          employee.Email!),
            new Claim(ClaimTypes.Name,           $"{employee.FirstName} {employee.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // unique token id
        };

        // SymmetricSecurityKey — JWT package — creates a key from our secret string
        // Encoding.UTF8.GetBytes — C# built in — converts string to bytes
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        // SigningCredentials — JWT package — pairs the key with the algorithm
        // SecurityAlgorithms.HmacSha256 — JWT package constant — the signing algorithm
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // JwtSecurityToken — JWT package — builds the actual token object
        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: credentials
        );

        // JwtSecurityTokenHandler — JWT package — serializes token object to the string
        // eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIi...
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}