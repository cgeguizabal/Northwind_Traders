using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindTraders.Domain.Interfaces;

namespace NorthwindTraders.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ShipmentStatesController : ControllerBase
{
    private readonly IShipmentStateRepository _repository;

    public ShipmentStatesController(IShipmentStateRepository repository)
    {
        _repository = repository;
    }

    // GET api/v1/shipmentstates
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var states = await _repository.GetAllAsync();
            return Ok(states);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error while retrieving shipment states: {ex.Message}");
        }
    }
}