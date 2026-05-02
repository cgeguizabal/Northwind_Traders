using Microsoft.AspNetCore.Mvc;                      // C# — ControllerBase, HTTP attributes
using NorthwindTraders.Application.DTOs.Employee;
using NorthwindTraders.Domain.Interfaces;

namespace NorthwindTraders.API.Controllers;

// [ApiController] — C# attribute — enables automatic 400 responses for invalid input
// [Route] — C# attribute — sets base URL for all endpoints in this controller
// "api/[controller]" → "api/employees" — [controller] is replaced with the class name minus "Controller"
[ApiController]
[Route("api/v1/[controller]")]
public class EmployeesController : ControllerBase   // C# — base class for API controllers, no view support
{
    private readonly IEmployeeRepository _repository;

    // DI injects IEmployeeRepository — controller never knows about EmployeeRepository or EF Core
    // This is DIP — high level (controller) depends on abstraction (interface), not implementation
    public EmployeesController(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    // GET api/employees
    [HttpGet]                                        // C# attribute — maps HTTP GET to this method
    public async Task<IActionResult> GetAll()
    {
        var employees = await _repository.GetAllAsync();

        // Manual mapping — Entity to DTO
        // .Select() — C# LINQ Method — projects each item in the list into a new shape
        var dtos = employees.Select(e => new EmployeeDto
        {
            EmployeeId      = e.EmployeeId,
            FirstName       = e.FirstName,
            LastName        = e.LastName,
            Title           = e.Title,
            TitleOfCourtesy = e.TitleOfCourtesy,
            City            = e.City,
            Country         = e.Country,
            HomePhone       = e.HomePhone,
            Email           = e.Email,
            PhotoPath       = e.PhotoPath,
            // ?. — C# null conditional operator — safe access, returns null if Manager is null
            // $"" — C# string interpolation
            ManagerName     = e.Manager is not null
                                ? $"{e.Manager.FirstName} {e.Manager.LastName}"
                                : null
        }).ToList();                                 // C# LINQ Method — materializes the projection

        return Ok(dtos);                             // C# — HTTP 200 with JSON body
    }

    // GET api/employees/5
    [HttpGet("{id}")]                                // C# attribute — {id} is a route parameter
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _repository.GetByIdAsync(id);

        if (employee is null)
            return NotFound($"Employee with id {id} was not found.");  // C# — HTTP 404

        var dto = new EmployeeDto
        {
            EmployeeId      = employee.EmployeeId,
            FirstName       = employee.FirstName,
            LastName        = employee.LastName,
            Title           = employee.Title,
            TitleOfCourtesy = employee.TitleOfCourtesy,
            City            = employee.City,
            Country         = employee.Country,
            HomePhone       = employee.HomePhone,
            Email           = employee.Email,
            PhotoPath       = employee.PhotoPath,
            ManagerName     = employee.Manager is not null
                                ? $"{employee.Manager.FirstName} {employee.Manager.LastName}"
                                : null
        };

        return Ok(dto);
    }

    // PUT api/employees/5
    // Updates only the fields a client is allowed to change
    [HttpPut("{id}")]                                // C# attribute — maps HTTP PUT
    public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto)
    {
        var employee = await _repository.GetByIdAsync(id);

        if (employee is null)
            return NotFound($"Employee with id {id} was not found.");

        // Map only the allowed fields — PasswordHash and Email are untouched
        employee.Title           = dto.Title;
        employee.TitleOfCourtesy = dto.TitleOfCourtesy;
        employee.Address         = dto.Address;
        employee.City            = dto.City;
        employee.Region          = dto.Region;
        employee.PostalCode      = dto.PostalCode;
        employee.Country         = dto.Country;
        employee.HomePhone       = dto.HomePhone;
        employee.Extension       = dto.Extension;
        employee.Notes           = dto.Notes;
        employee.PhotoPath       = dto.PhotoPath;

        _repository.Update(employee);
        await _repository.SaveChangesAsync();

        return NoContent();                          // C# — HTTP 204 — success, no body needed
    }
}