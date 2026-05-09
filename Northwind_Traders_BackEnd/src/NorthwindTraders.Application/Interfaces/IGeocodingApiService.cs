using NorthwindTraders.Domain.Common;

namespace NorthwindTraders.Application.Interfaces;

// SRP: Responsible only for communicating with the Google Maps Geocoding HTTP API.
// Does not know about orders, customers, or the database.
public interface IGeocodingApiService
{
    Task<Result<(string validatedAddress, decimal lat, decimal lng)>> GeocodeAddressAsync(string address);
}
