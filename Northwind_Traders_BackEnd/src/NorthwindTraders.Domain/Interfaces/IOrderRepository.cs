using NorthwindTraders.Domain.Entities;

namespace NorthwindTraders.Domain.Interfaces;

// Order-specific data access contract
// Orders are the most complex entity — they need
// special queries beyond basic CRUD

public interface IOrderRepository : IRepository<Order>
{
    // Get a FULL order — with Customer, Employee,
    // Shipper, ShipmentState, and all OrderDetails + Products
    // This is what we need for the PDF report
    Task<Order?> GetOrderWithDetailsAsync(int orderId);

    // Get all orders for a specific customer
    Task<IReadOnlyList<Order>> GetByCustomerAsync(string customerId);

    // Get all orders processed by a specific employee
    Task<IReadOnlyList<Order>> GetByEmployeeAsync(int employeeId);

    // Get orders filtered by shipment status
    // Used in the Orders dashboard view
    Task<IReadOnlyList<Order>> GetByShipmentStatusAsync(int shipmentStateId);

    // Get orders within a date range
    // Used for reporting and dashboard metrics
    Task<IReadOnlyList<Order>> GetByDateRangeAsync(DateTime from, DateTime to);
}