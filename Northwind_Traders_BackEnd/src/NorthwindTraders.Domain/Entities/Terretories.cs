namespace NorthwindTraders.Domain.Entities;


public class Territory
{
    /// Territory identifier (string key).
    public string TerritoryId {get; set;} = string.Empty;
    /// Description of the territory (required).
    public required string TerritoryDescription {get; set;}

    /// FK to parent region.
    public int RegionId {get; set;}
    /// Navigation to the region.
    public Region? Region {get; set;}

    /// Employee-territory link entries.
    public ICollection<EmployeeTerritory> EmployeeTerritories {get; set;} = [];
}