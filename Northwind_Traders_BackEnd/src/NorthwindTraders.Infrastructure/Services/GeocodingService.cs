using System.Text.Json;                           // C# built in — parse JSON response
using Microsoft.Extensions.Configuration;
using NorthwindTraders.Domain.Common;             // Result<T>
using NorthwindTraders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace NorthwindTraders.Infrastructure.Services;

public class GeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ApplicationDbContext _context;

    // HttpClient — C# built in — sends HTTP requests to external APIs
    // IConfiguration — reads appsettings / User Secrets
    public GeocodingService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext context)
    {
        _httpClient = httpClient;
        _context    = context;
        _apiKey     = configuration["GoogleMaps:ApiKey"]
                      ?? throw new InvalidOperationException("GoogleMaps:ApiKey is not configured.");
    }

    // Geocodes a single address string
    // Returns Result<T> — either Success with coordinates or Failure with error message
    public async Task<Result<(string validatedAddress, decimal lat, decimal lng)>> GeocodeAddressAsync(string address)
    {
        try
        {
            // Build the Google Maps Geocoding API URL
            // encodeURIComponent equivalent in C# — Uri.EscapeDataString
            var url = $"https://maps.googleapis.com/maps/api/geocode/json" +
                      $"?address={Uri.EscapeDataString(address)}" +
                      $"&key={_apiKey}";

            // GetAsync — HttpClient Method — sends GET request to Google
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return Result<(string, decimal, decimal)>.Failure(
                    $"Google Maps API returned {response.StatusCode}");

            // ReadAsStringAsync — HttpClient Method — reads response body as string
            var json = await response.Content.ReadAsStringAsync();

            // JsonDocument.Parse — C# built in — parses raw JSON string into navigable tree
            using var doc = JsonDocument.Parse(json);
            var root   = doc.RootElement;
            var status = root.GetProperty("status").GetString();

            if (status != "OK")
                return Result<(string, decimal, decimal)>.Failure(
                    $"Geocoding failed with status: {status}");

            // Navigate the JSON tree to get results[0]
            var results  = root.GetProperty("results");
            var first    = results[0];

            // formatted_address — the clean validated address Google returns
            var validated = first.GetProperty("formatted_address").GetString()!;

            // geometry.location.lat / lng — the coordinates
            var location = first.GetProperty("geometry").GetProperty("location");
            var lat      = (decimal)location.GetProperty("lat").GetDouble();
            var lng      = (decimal)location.GetProperty("lng").GetDouble();

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
        catch (InvalidOperationException ex)
        {
            return Result<(string, decimal, decimal)>.Failure(
                $"Invalid geocoding operation: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<(string, decimal, decimal)>.Failure(
                $"An unexpected error occurred during geocoding: {ex.Message}");
        }
    }

    // Geocodes both ship and bill addresses for an order and saves to DB
    public async Task<Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>> GeocodeOrderAsync(int orderId)
    {
        try
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order is null)
                return Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>
                    .Failure($"Order {orderId} not found.");

            string? validatedShip = null;
            decimal? shipLat = null, shipLng = null;
            string? validatedBill = null;
            decimal? billLat = null, billLng = null;

            // ── GEOCODE SHIP ADDRESS ──────────────────────────────────────────────
            // Build a full address string from the order fields
            var shipAddress = BuildAddressString(
                order.ShipAddress, order.ShipCity, order.ShipRegion,
                order.ShipPostalCode, order.ShipCountry);

            if (!string.IsNullOrWhiteSpace(shipAddress))
            {
                // Save what the user originally had before we overwrite it
                order.OriginalShipAddress = shipAddress;

                var shipResult = await GeocodeAddressAsync(shipAddress);

                if (shipResult.IsSuccess)
                {
                    (validatedShip, var sLat, var sLng) = shipResult.Value!;
                    order.ValidatedShipAddress = validatedShip;
                    order.ShipLatitude         = sLat;
                    order.ShipLongitude        = sLng;
                    shipLat = sLat;
                    shipLng = sLng;
                }
                // If geocoding fails we don't crash — we just leave the validated fields null
            }

            // ── GEOCODE BILL ADDRESS ──────────────────────────────────────────────
            var billAddress = BuildAddressString(
                order.BillAddress, order.BillCity, order.BillRegion,
                order.BillPostalCode, order.BillCountry);

            if (!string.IsNullOrWhiteSpace(billAddress))
            {
                order.OriginalBillAddress = billAddress;

                var billResult = await GeocodeAddressAsync(billAddress);

                if (billResult.IsSuccess)
                {
                    (validatedBill, var bLat, var bLng) = billResult.Value!;
                    order.ValidatedBillAddress = validatedBill;
                    order.BillLatitude         = bLat;
                    order.BillLongitude        = bLng;
                    billLat = bLat;
                    billLng = bLng;
                }
            }

            // Save both updates in one DB round trip
            await _context.SaveChangesAsync();

            return Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>
                .Success((validatedShip, shipLat, shipLng, validatedBill, billLat, billLng));
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            return Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>
                .Failure($"Failed to save geocoding results for order {orderId}: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            return Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>
                .Failure($"Geocoding operation for order {orderId} was cancelled: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>
                .Failure($"Invalid operation while geocoding order {orderId}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>
                .Failure($"An unexpected error occurred while geocoding order {orderId}: {ex.Message}");
        }
    }

    // Builds a single address string from separate fields
    // Filters out nulls and joins with commas
    private static string BuildAddressString(
        string? address, string? city, string? region,
        string? postalCode, string? country)
    {
        // string.Join — C# built in — joins non-empty parts with ", "
        return string.Join(", ",
            new[] { address, city, region, postalCode, country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    }


    // Geocodes ALL orders that haven't been geocoded yet
    // Returns a summary: how many processed, succeeded, failed
    public async Task<(int processed, int succeeded, int failed)> GeocodeAllPendingAsync()
    {
        // Find all orders where ShipLatitude is null AND ShipAddress exists
        // These are orders that have never been geocoded
        List<int> pendingOrders;
        try
        {
            pendingOrders = await _context.Orders
                .Where(o => o.ShipLatitude == null
                         && (o.ShipAddress != null || o.ShipCity != null))
                .Select(o => o.OrderId)   // only fetch IDs — we load each one individually
                .ToListAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            throw new InvalidOperationException($"Database error while retrieving pending orders for geocoding: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
        catch (OperationCanceledException ex)
        {
            throw new InvalidOperationException($"Geocoding query was cancelled: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve pending orders for geocoding: {ex.Message}", ex);
        }

        int succeeded = 0;
        int failed    = 0;

        foreach (var orderId in pendingOrders)
        {
            // Reuse the existing single-order geocoding logic
            var result = await GeocodeOrderAsync(orderId);

            if (result.IsSuccess)
                succeeded++;
            else
                failed++;

            // ── RATE LIMITING ─────────────────────────────────────────────────
            // Google Maps free tier allows 50 requests/second
            // 50ms delay keeps us safely under the limit
            // Task.Delay — C# built in — async pause, does not block the thread
            await Task.Delay(50);
        }

        return (pendingOrders.Count, succeeded, failed);
    }
}