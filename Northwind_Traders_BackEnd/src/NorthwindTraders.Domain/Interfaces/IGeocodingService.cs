using NorthwindTraders.Domain.Common;

namespace NorthwindTraders.Domain.Interfaces;

// Orchestrates geocoding operations — validate a single address, geocode one order,
// or batch-process all orders that still lack coordinates.
public interface IGeocodingService
{
    // Validates a free-form address and returns coordinates + normalized text
    Task<Result<(string validatedAddress, decimal lat, decimal lng)>> GeocodeAddressAsync(string address);

    // Geocodes both ship and bill addresses for a specific order and saves results to DB
    Task<Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>> GeocodeOrderAsync(int orderId);

    // Processes all orders that have no coordinates yet—called on demand or on schedule
    Task<(int processed, int succeeded, int failed)> GeocodeAllPendingAsync();
}