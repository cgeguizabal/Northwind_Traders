using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using NorthwindTraders.Infrastructure.Services;
using NorthwindTraders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace NorthwindTraders.Tests.Services;

public class GeocodingServiceTests
{
    // ── Builds a fake HttpClient that returns a scripted response ─
    private static HttpClient BuildHttpClient(string jsonResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        // HttpMessageHandler is what HttpClient uses internally to send requests
        // We mock it so no real HTTP call is made
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected() // access the protected SendAsync method
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content    = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handlerMock.Object);
    }

    // ── Builds an EF Core in-memory DB ───────────────────────────
    private static ApplicationDbContext BuildDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName) // no SQL Server needed
            .Options;

        return new ApplicationDbContext(options);
    }

    // ── Builds a real IConfiguration with fake API key ───────────
    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleMaps:ApiKey"] = "fake-api-key-for-testing"
            })
            .Build();

    // ── Sample Google Maps success response ──────────────────────
    private static string SuccessJson(double lat = 40.71, double lng = -74.00) => $$"""
        {
            "status": "OK",
            "results": [{
                "formatted_address": "123 Main St, New York, NY 10001, USA",
                "geometry": {
                    "location": {
                        "lat": {{lat}},
                        "lng": {{lng}}
                    }
                }
            }]
        }
        """;

    // ── Google Maps ZERO_RESULTS response ────────────────────────
    private const string ZeroResultsJson = """
        {
            "status": "ZERO_RESULTS",
            "results": []
        }
        """;

    // ─────────────────────────────────────────────────────────────
    // GeocodeAddressAsync — happy path
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GeocodeAddressAsync_ReturnsSuccess_WhenGoogleRespondsOk()
    {
        // ARRANGE
        var client  = BuildHttpClient(SuccessJson());
        var context = BuildDbContext("geocode-success");
        var config  = BuildConfiguration();
        var service = new GeocodingService(client, config, context);

        // ACT
        var result = await service.GeocodeAddressAsync("123 Main St, New York");

        // ASSERT
        Assert.True(result.IsSuccess);
        Assert.Equal("123 Main St, New York, NY 10001, USA", result.Value!.validatedAddress);
        Assert.Equal(40.71m, result.Value.lat);
        Assert.Equal(-74.00m, result.Value.lng);
    }

    [Fact]
    public async Task GeocodeAddressAsync_ReturnsFailure_WhenStatusIsZeroResults()
    {
        // ARRANGE
        var client  = BuildHttpClient(ZeroResultsJson);
        var context = BuildDbContext("geocode-zero");
        var config  = BuildConfiguration();
        var service = new GeocodingService(client, config, context);

        // ACT
        var result = await service.GeocodeAddressAsync("NoWhere Land");

        // ASSERT
        Assert.False(result.IsSuccess);
        Assert.Contains("ZERO_RESULTS", result.Error);
    }

    [Fact]
    public async Task GeocodeAddressAsync_ReturnsFailure_WhenHttpFails()
{
    // ARRANGE — Google returns 500
    var client  = BuildHttpClient("{}", HttpStatusCode.InternalServerError);
    var context = BuildDbContext("geocode-http-fail");
    var config  = BuildConfiguration();
    var service = new GeocodingService(client, config, context);

    // ACT
    var result = await service.GeocodeAddressAsync("123 Main St");

    // ASSERT
    Assert.False(result.IsSuccess);
    Assert.Contains("InternalServerError", result.Error); // ← enum name not number
}

    // ─────────────────────────────────────────────────────────────
    // GeocodeOrderAsync — happy path
    // ────────────────────────────────────────��────────────────────

    [Fact]
    public async Task GeocodeOrderAsync_ReturnsSuccess_WhenOrderExists()
    {
        // ARRANGE — seed an order into the in-memory DB
        var context = BuildDbContext("geocode-order-success");
        context.Orders.Add(new Domain.Entities.Order
        {
            OrderId     = 1,
            ShipAddress = "123 Main St",
            ShipCity    = "New York",
            ShipCountry = "USA",
            IsActive    = "Y"
        });
        await context.SaveChangesAsync();

        var client  = BuildHttpClient(SuccessJson());
        var config  = BuildConfiguration();
        var service = new GeocodingService(client, config, context);

        // ACT
        var result = await service.GeocodeOrderAsync(1);

        // ASSERT
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Item1); // validatedShipAddress
    }

    [Fact]
    public async Task GeocodeOrderAsync_ReturnsFailure_WhenOrderNotFound()
    {
        // ARRANGE — empty DB, order 999 does not exist
        var context = BuildDbContext("geocode-order-notfound");
        var client  = BuildHttpClient(SuccessJson());
        var config  = BuildConfiguration();
        var service = new GeocodingService(client, config, context);

        // ACT
        var result = await service.GeocodeOrderAsync(999);

        // ASSERT
        Assert.False(result.IsSuccess);
        Assert.Contains("999", result.Error);
    }
}