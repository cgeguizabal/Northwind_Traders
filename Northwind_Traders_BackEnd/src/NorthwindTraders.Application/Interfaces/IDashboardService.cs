using NorthwindTraders.Application.DTOs.Dashboard;

namespace NorthwindTraders.Application.Interfaces;

// Aggregates KPI data (revenue, orders, top customers/employees) for the dashboard view.
// Accepts optional date filters so the frontend can scope the metrics to a time range.
public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
}