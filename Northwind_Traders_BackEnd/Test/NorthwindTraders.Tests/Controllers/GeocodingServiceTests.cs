using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using NorthwindTraders.Infrastructure.Services;
using NorthwindTraders.Infrastructure.Persistence;
using NorthwindTraders.Application.Interfaces;
using NorthwindTraders.Domain.Common;
using NorthwindTraders.Domain.Entities;
using NorthwindTraders.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace NorthwindTraders.Tests.Services;

public class GeocodingServiceTests
{
    // ── Builds a fake HttpClient that returns a scripted response ─
    private static HttpClient BuildHttpClient(string jsonResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
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

    // ── Builds a real IConfiguration with fake API key ───────────
    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleMaps:ApiKey"] = "fake-api-key-for-testing"
            })
            .Build();

    // ── Builds a mock IOrderRepository with optional seeded order ─
    private static Mock<IOrderRepository> BuildOrderRepoMock(Order? order = null)
    {
        var mock = new Mock<IOrderRepository>();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(order);
        mock.Setup(r => r.Update(It.IsAny<Order>()));
        mock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        return mock;
    }

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

    private const string ZeroResultsJson = """
        {
            "status": "ZERO_RESULTS",
            "results": []
        }
        """;

    // ─────────────────────────────────────────────────────────────
    // GeocodeAddressAsync — tests go through GoogleMapsGeocodingService
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GeocodeAddressAsync_ReturnsSuccess_WhenGoogleRespondsOk()
    {
        // ARRANGE
        var apiService = new GoogleMapsGeocodingService(BuildHttpClient(SuccessJson()), BuildConfiguration());
        var service    = new GeocodingService(apiService, BuildOrderRepoMock().Object);

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
        var apiService = new GoogleMapsGeocodingService(BuildHttpClient(ZeroResultsJson), BuildConfiguration());
        var service    = new GeocodingService(apiService, BuildOrderRepoMock().Object);

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
        var apiService = new GoogleMapsGeocodingService(BuildHttpClient("{}", HttpStatusCode.InternalServerError), BuildConfiguration());
        var service    = new GeocodingService(apiService, BuildOrderRepoMock().Object);

        // ACT
        var result = await service.GeocodeAddressAsync("123 Main St");

        // ASSERT
        Assert.False(result.IsSuccess);
        Assert.Contains("InternalServerError", result.Error);
    }

    // ─────────────────────────────────────────────────────────────
    // GeocodeOrderAsync — uses mocked IGeocodingApiService + IOrderRepository
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GeocodeOrderAsync_ReturnsSuccess_WhenOrderExists()
    {
        // ARRANGE
        var order = new Order
        {
            OrderId     = 1,
            ShipAddress = "123 Main St",
            ShipCity    = "New York",
            ShipCountry = "USA",
            IsActive    = "Y"
        };

        var apiMock = new Mock<IGeocodingApiService>();
        apiMock.Setup(a => a.GeocodeAddressAsync(It.IsAny<string>()))
               .ReturnsAsync(Result<(string, decimal, decimal)>.Success(
                   ("123 Main St, New York, NY 10001, USA", 40.71m, -74.00m)));

        var repoMock = BuildOrderRepoMock(order);
        var service  = new GeocodingService(apiMock.Object, repoMock.Object);

        // ACT
        var result = await service.GeocodeOrderAsync(1);

        // ASSERT
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Item1); // validatedShipAddress
    }

    [Fact]
    public async Task GeocodeOrderAsync_ReturnsFailure_WhenOrderNotFound()
    {
        // ARRANGE — repo returns null for any id
        var apiMock  = new Mock<IGeocodingApiService>();
        var repoMock = BuildOrderRepoMock(null);
        var service  = new GeocodingService(apiMock.Object, repoMock.Object);

        // ACT
        var result = await service.GeocodeOrderAsync(999);

        // ASSERT
        Assert.False(result.IsSuccess);
        Assert.Contains("999", result.Error);
    }
}