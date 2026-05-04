using NorthwindTraders.Domain.Common;

namespace NorthwindTraders.Domain.Interfaces;

public interface IGeocodingService
{
    Task<Result<(string validatedAddress, decimal lat, decimal lng)>> GeocodeAddressAsync(string address);
    Task<Result<(string?, decimal?, decimal?, string?, decimal?, decimal?)>> GeocodeOrderAsync(int orderId);
    Task<(int processed, int succeeded, int failed)> GeocodeAllPendingAsync();
}