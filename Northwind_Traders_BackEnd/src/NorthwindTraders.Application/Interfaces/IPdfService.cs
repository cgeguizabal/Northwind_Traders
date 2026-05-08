using NorthwindTraders.Application.DTOs.Order;

namespace NorthwindTraders.Application.Interfaces;

public interface IPdfService
{
    byte[] GenerateOrderPdf(OrderDetailDto order);
    byte[] GenerateOrdersReportPdf(IEnumerable<OrderSummaryDto> orders);
}