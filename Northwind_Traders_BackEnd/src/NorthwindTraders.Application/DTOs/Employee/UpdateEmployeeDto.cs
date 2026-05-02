namespace NorthwindTraders.Application.DTOs.Employee;

// What the client sends in the request body when updating an employee
// Only fields a client is ALLOWED to change
// EmployeeId, Email, PasswordHash, ReportsTo are NOT here — client cannot touch them
public class UpdateEmployeeDto
{
    public string? Title { get; set; }
    public string? TitleOfCourtesy { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? HomePhone { get; set; }
    public string? Extension { get; set; }
    public string? Notes { get; set; }
    public string? PhotoPath { get; set; }
}