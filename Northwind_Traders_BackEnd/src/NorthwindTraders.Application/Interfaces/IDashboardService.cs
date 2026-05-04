using NorthwindTraders.Application.DTOs.Dashboard;

namespace NorthwindTraders.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
}