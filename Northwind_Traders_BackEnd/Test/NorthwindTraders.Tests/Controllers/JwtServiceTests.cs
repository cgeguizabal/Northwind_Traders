using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Moq;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Infrastructure.Services;
using Xunit;
using System.Security.Claims;

namespace NorthwindTraders.Tests.Services;

public class JwtServiceTests
{
    // ── Builds a real IConfiguration with fake JWT settings ──────
    private static IConfiguration BuildConfiguration(
        string key            = "this-is-a-super-secret-key-for-testing-1234",
        string issuer         = "NorthwindAPI",
        string audience       = "NorthwindClient",
        string expiryMinutes  = "60")
    {
        // ConfigurationBuilder — C# built in — builds IConfiguration from in-memory values
        // No appsettings.json needed in tests
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]           = key,
                ["Jwt:Issuer"]        = issuer,
                ["Jwt:Audience"]      = audience,
                ["Jwt:ExpiryMinutes"] = expiryMinutes
            })
            .Build();

        return config;
    }

    private static Employee BuildEmployee() => new()
    {
        EmployeeId = 1,
        FirstName  = "Nancy",
        LastName   = "Davolio",
        Email      = "nancy@northwind.com"
    };

    // ─────────────────────────────────────────────────────────────
    // GenerateToken — happy path
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        // ARRANGE
        var config  = BuildConfiguration();
        var service = new JwtService(config);

        // ACT
        var token = service.GenerateToken(BuildEmployee());

        // ASSERT
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtFormat()
    {
        // ARRANGE — a valid JWT has 3 parts separated by dots
        var config  = BuildConfiguration();
        var service = new JwtService(config);

        // ACT
        var token = service.GenerateToken(BuildEmployee());

        // ASSERT
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length); // header.payload.signature
    }

    [Fact]
    public void GenerateToken_ContainsCorrectEmail()
{
    // ARRANGE
    var config  = BuildConfiguration();
    var service = new JwtService(config);

    // ACT
    var token = service.GenerateToken(BuildEmployee());

    // ASSERT
    var handler = new JwtSecurityTokenHandler();
    var decoded = handler.ReadJwtToken(token);

    var emailClaim = decoded.Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value; // ← ClaimTypes.Email

    Assert.Equal("nancy@northwind.com", emailClaim);
}

    [Fact]
   public void GenerateToken_ContainsCorrectEmployeeId()
{
    // ARRANGE
    var config  = BuildConfiguration();
    var service = new JwtService(config);

    // ACT
    var token = service.GenerateToken(BuildEmployee());

    // ASSERT
    var handler = new JwtSecurityTokenHandler();
    var decoded = handler.ReadJwtToken(token);

    var idClaim = decoded.Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // ← ClaimTypes.NameIdentifier

    Assert.Equal("1", idClaim);
}

    [Fact]
    public void GenerateToken_HasCorrectIssuerAndAudience()
    {
        // ARRANGE
        var config  = BuildConfiguration();
        var service = new JwtService(config);

        // ACT
        var token = service.GenerateToken(BuildEmployee());

        // ASSERT
        var handler = new JwtSecurityTokenHandler();
        var decoded = handler.ReadJwtToken(token);

        Assert.Equal("NorthwindAPI",    decoded.Issuer);
        Assert.Contains("NorthwindClient", decoded.Audiences);
    }

    [Fact]
    public void GenerateToken_ExpiresInCorrectMinutes()
    {
        // ARRANGE — set expiry to 30 minutes
        var config  = BuildConfiguration(expiryMinutes: "30");
        var service = new JwtService(config);

        var before = DateTime.UtcNow.AddMinutes(29); // should expire AFTER this
        var after  = DateTime.UtcNow.AddMinutes(31); // should expire BEFORE this

        // ACT
        var token = service.GenerateToken(BuildEmployee());

        // ASSERT
        var handler = new JwtSecurityTokenHandler();
        var decoded = handler.ReadJwtToken(token);

        Assert.True(decoded.ValidTo > before);
        Assert.True(decoded.ValidTo < after);
    }

    // ─────────────────────────────────────────────────────────────
    // GenerateToken — error paths
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_ThrowsInvalidOperation_WhenExpiryIsNotANumber()
    {
        // ARRANGE — ExpiryMinutes is not a valid int
        var config  = BuildConfiguration(expiryMinutes: "notanumber");
        var service = new JwtService(config);

        // ACT & ASSERT
        Assert.Throws<InvalidOperationException>(
            () => service.GenerateToken(BuildEmployee()));
    }

    [Fact]
    public void GenerateToken_ThrowsInvalidOperation_WhenKeyIsTooShort()
    {
        // ARRANGE — HMAC-SHA256 requires at least 128 bits (16 chars)
        var config  = BuildConfiguration(key: "short");
        var service = new JwtService(config);

        // ACT & ASSERT
        Assert.Throws<InvalidOperationException>(
            () => service.GenerateToken(BuildEmployee()));
    }
}