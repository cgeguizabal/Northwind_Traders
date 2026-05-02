namespace NorthwindTraders.Application.DTOs.Employee;

// What the API sends back when a client requests an employee
// No PasswordHash — never expose this
// No navigation collections — avoids circular reference crashes
public class EmployeeDto
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? TitleOfCourtesy { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? HomePhone { get; set; }
    public string? Email { get; set; }
    public string? PhotoPath { get; set; }

    // Only the manager's full name — not the full Manager object
    // This is what breaks the circular reference
    public string? ManagerName { get; set; }
}