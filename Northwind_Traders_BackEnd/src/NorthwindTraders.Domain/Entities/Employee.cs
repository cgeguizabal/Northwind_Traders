namespace NorthwindTraders.Domain.Entities;



/// Company employee record.
public class Employee
{
    /// Primary key.
    public int EmployeeId { get; set; }

    /// Employee last name (required).
    public required string LastName { get; set; }
    /// Employee first name (required).
    public required string FirstName { get; set; }

    /// Job title (optional).
    public string? Title { get; set; }

    /// Courtesy title (e.g., Mr./Ms.).
    public string? TitleOfCourtesy { get; set; }
    /// Birth date (optional).
    public DateTime? BirthDate { get; set; }
    /// Hire date (optional).
    public DateTime? HireDate { get; set; }
    /// Street address (optional).
    public string? Address { get; set; }
    /// City (optional).
    public string? City { get; set; }
    /// Region/state (optional).
    public string? Region { get; set; }
    /// Postal code (optional).
    public string? PostalCode { get; set; }
    /// Country (optional).
    public string? Country { get; set; }
    /// Home phone (optional).
    public string? HomePhone { get; set; }
    /// Phone extension (optional).
    public string? Extension { get; set; }
    /// Notes about the employee.
    public string? Notes { get; set; }
    /// Path or URL to photo (optional).
    public string? PhotoPath { get; set; }

    /// FK to manager (employee id) if any.
    public int? ReportsTo { get; set; }

    /// Work email (optional).
    public string? Email { get; set; }

    /// Hashed password (optional, for internal auth).
    public string? PasswordHash { get; set; }

    /// Manager (self-reference navigation).
    public Employee? Manager { get; set; }

    /// Employees who report to this employee.
    public ICollection<Employee> DirectReports { get; set; } = [];

    /// Orders handled by this employee.
    public ICollection<Order> Orders { get; set; } = [];

    /// Territories assigned to the employee.
    public ICollection<EmployeeTerritory> EmployeeTerritories { get; set; } = [];
}