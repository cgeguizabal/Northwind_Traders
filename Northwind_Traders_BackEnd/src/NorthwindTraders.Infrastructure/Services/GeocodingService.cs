using NorthwindTraders.Application.Interfaces;
using NorthwindTraders.Domain.Common;
using NorthwindTraders.Domain.Interfaces;

namespace NorthwindTraders.Infrastructure.Services;

// SRP: Responsible only for the geocoding workflow — loading orders, calling the
// geocoding API, persisting the results, and bulk-processing pending orders.
// All HTTP/API logic lives in IGeocodingApiService (GoogleMapsGeocodingService).
public class GeocodingService : IGeocodingService
{
    private readonly IGeocodingApiService _apiService;
    private readonly IOrderRepository _orderRepository;

    public GeocodingService(IGeocodingApiService apiService, IOrderRepository orderRepository)
    {
        _apiService       = apiService;
        _orderRepository  = orderRepository;
    }

    // Delegates directly to the API service — kept on IGeocodingService so callers
    // that only need a raw geocode don't need to know about IGeocodingApiService.
    public Task<Result<(string validatedAddress, decimal lat, decimal lng)>> GeocodeAddressAsync(string address)
        => _apiService.GeocodeAddressAsync(address);

    // Geocodes both ship and bill addresses for an order and saves to DB
    public async Task<Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>> GeocodeOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order is null)
            return Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>
                .Failure($"Order {orderId} not found.");

        string? validatedShip = null;
        decimal? shipLat = null, shipLng = null;
        string? validatedBill = null;
        decimal? billLat = null, billLng = null;

        // ── GEOCODE SHIP ADDRESS ──────────────────────────────────────────────
        var shipAddress = BuildAddressString(
            order.ShipAddress, order.ShipCity, order.ShipRegion,
            order.ShipPostalCode, order.ShipCountry);

        if (!string.IsNullOrWhiteSpace(shipAddress))
        {
            order.OriginalShipAddress = shipAddress;

            var shipResult = await _apiService.GeocodeAddressAsync(shipAddress);

            if (shipResult.IsSuccess)
            {
                (validatedShip, var sLat, var sLng) = shipResult.Value!;
                order.ValidatedShipAddress = validatedShip;
                order.ShipLatitude         = sLat;
                order.ShipLongitude        = sLng;
                shipLat = sLat;
                shipLng = sLng;
            }
        }

        // ── GEOCODE BILL ADDRESS ──────────────────────────────────────────────
        var billAddress = BuildAddressString(
            order.BillAddress, order.BillCity, order.BillRegion,
            order.BillPostalCode, order.BillCountry);

        if (!string.IsNullOrWhiteSpace(billAddress))
        {
            order.OriginalBillAddress = billAddress;

            var billResult = await _apiService.GeocodeAddressAsync(billAddress);

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

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>
            .Success((validatedShip, shipLat, shipLng, validatedBill, billLat, billLng));
    }

    // Geocodes ALL orders that haven't been geocoded yet
    public async Task<(int processed, int succeeded, int failed)> GeocodeAllPendingAsync()
    {
        var allOrders = await _orderRepository.GetAllAsync();

        var pendingIds = allOrders
            .Where(o => o.ShipLatitude == null
                     && (o.ShipAddress != null || o.ShipCity != null))
            .Select(o => o.OrderId)
            .ToList();

        int succeeded = 0;
        int failed    = 0;

        foreach (var orderId in pendingIds)
        {
            var result = await GeocodeOrderAsync(orderId);

            if (result.IsSuccess) succeeded++;
            else                  failed++;

            // Rate limiting — Google Maps free tier: 50 req/s
            await Task.Delay(50);
        }

        return (pendingIds.Count, succeeded, failed);
    }

    private static string BuildAddressString(
        string? address, string? city, string? region,
        string? postalCode, string? country)
    {
        return string.Join(", ",
            new[] { address, city, region, postalCode, country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
