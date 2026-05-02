namespace NorthwindTraders.Application.DTOs.Dashboard;

public class DashboardDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }

    // Orders grouped by shipment status
    public List<OrdersByStatusDto> OrdersByStatus { get; set; } = [];

    // Top 5 customers by order count
    public List<TopCustomerDto> TopCustomers { get; set; } = [];

    // Top 5 employees by orders handled
    public List<TopEmployeeDto> TopEmployees { get; set; } = [];
}

public class OrdersByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TopCustomerDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}

public class TopEmployeeDto
{
    public string FullName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}