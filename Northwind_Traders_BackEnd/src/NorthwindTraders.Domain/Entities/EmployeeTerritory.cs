namespace NorthwindTraders.Domain.Entities;



public class EmployeeTerritory
{
    /// FK to the employee.
    public int EmployeeId { get; set; }
    /// FK to the territory (identifier string).
    public string TerritoryId { get; set; } = string.Empty;

    /// Navigation to the employee.
    public Employee? Employee { get; set; }
    /// Navigation to the territory.
    public Territory? Territory { get; set; }
}