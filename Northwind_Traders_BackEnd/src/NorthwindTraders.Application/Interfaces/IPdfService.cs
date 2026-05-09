using NorthwindTraders.Application.DTOs.Order;

namespace NorthwindTraders.Application.Interfaces;

// Generates PDF documents from order data using QuestPDF.
public interface IPdfService
{
    // Single-order invoice PDF
    byte[] GenerateOrderPdf(OrderDetailDto order);
    // Multi-order summary report PDF
    byte[] GenerateOrdersReportPdf(IEnumerable<OrderSummaryDto> orders);
}