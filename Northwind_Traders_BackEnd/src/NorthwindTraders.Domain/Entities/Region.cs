namespace NorthwindTraders.Domain.Entities;

public class Region
{
    /// Primary key for region.
    public int RegionId {get; set;}
    /// Human-readable description of the region.
    public required string RegionDescription {get; set;}

    /// Territories contained in this region.
    public ICollection<Territory> Territories {get; set;} = [];
}