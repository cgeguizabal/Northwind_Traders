using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NorthwindTraders.Application.Interfaces;
using NorthwindTraders.Domain.Common;

namespace NorthwindTraders.Infrastructure.Services;

// SRP: Single responsibility — call the Google Maps Geocoding API and parse the response.
// No database access. No order knowledge. Just HTTP + JSON parsing.
public class GoogleMapsGeocodingService : IGeocodingApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GoogleMapsGeocodingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey     = configuration["GoogleMaps:ApiKey"]
                      ?? throw new InvalidOperationException("GoogleMaps:ApiKey is not configured.");
    }

    public async Task<Result<(string validatedAddress, decimal lat, decimal lng)>> GeocodeAddressAsync(string address)
    {
        try
        {
            var url = $"https://maps.googleapis.com/maps/api/geocode/json" +
                      $"?address={Uri.EscapeDataString(address)}" +
                      $"&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return Result<(string, decimal, decimal)>.Failure(
                    $"Google Maps API returned {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root   = doc.RootElement;
            var status = root.GetProperty("status").GetString();

            if (status != "OK")
                return Result<(string, decimal, decimal)>.Failure(
                    $"Geocoding failed with status: {status}");

            var first     = root.GetProperty("results")[0];
            var validated = first.GetProperty("formatted_address").GetString()!;
            var location  = first.GetProperty("geometry").GetProperty("location");
            var lat       = (decimal)location.GetProperty("lat").GetDouble();
            var lng       = (decimal)location.GetProperty("lng").GetDouble();

            return Result<(string, decimal, decimal)>.Success((validated, lat, lng));
        }
        catch (HttpRequestException ex)
        {
            return Result<(string, decimal, decimal)>.Failure(
                $"Failed to reach Google Maps API: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            return Result<(string, decimal, decimal)>.Failure(
                $"Google Maps API request timed out: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Result<(string, decimal, decimal)>.Failure(
                $"Failed to parse the Google Maps API response: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<(string, decimal, decimal)>.Failure(
                $"An unexpected error occurred during geocoding: {ex.Message}");
        }
    }
}
