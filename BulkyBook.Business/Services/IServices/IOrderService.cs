using BulkyBook.Models;

namespace BulkyBook.Business.Services.IServices
{
    public interface IOrderService
    {
        Task<OrderHeader> CreateOrderAsync(OrderHeader orderHeader);
        Task<OrderHeader?> GetOrderByIdAsync(int id, bool includeUser = false, bool includeDetails = false);
        Task<IEnumerable<OrderHeader>> GetAllOrderAsync(string? userId = null, string? status = null, bool includeUser = false, bool includeDetails = false);
        Task UpdateOrderAsync(OrderHeader orderHeader);
        Task UpdateOrderStatusAsyncx(int id, string orderStatus, string? carrier = null, string? trackingNumber = null);
    }
}
