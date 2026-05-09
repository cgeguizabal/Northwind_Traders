using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindTraders.Domain.Interfaces;

namespace NorthwindTraders.API.Controllers;

// Read-only lookup for shipment states (e.g. Pending, Shipped, Completed, Cancelled).
// States are seeded in the database and never managed through the API.
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

    // GET api/v1/shipmentstates \u2014 returns all available shipment states
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var states = await _repository.GetAllAsync();
        return Ok(states);
    }
}