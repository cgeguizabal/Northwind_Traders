using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindTraders.Domain.Interfaces;

namespace NorthwindTraders.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class GeocodingController : ControllerBase
{
    private readonly IGeocodingService _geocodingService;

    public GeocodingController(IGeocodingService geocodingService)
    {
        _geocodingService = geocodingService;
    }

    // POST api/v1/geocoding/validate-address
    // Validates a free-form address string and returns coordinates
    [HttpPost("validate-address")]
    public async Task<IActionResult> ValidateAddress([FromBody] ValidateAddressRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Address))
            return BadRequest("Address is required.");

        var result = await _geocodingService.GeocodeAddressAsync(request.Address);

        if (!result.IsSuccess)
            return UnprocessableEntity(new { message = result.Error });

        return Ok(new
        {
            validatedAddress = result.Value.validatedAddress,
            lat              = result.Value.lat,
            lng              = result.Value.lng
        });
    }

    public record ValidateAddressRequest(string Address);
}
