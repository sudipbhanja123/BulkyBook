using BulkyBook.Business.Services.IServices;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<OrderHeader> CreateOrderAsync(OrderHeader orderHeader)
        {
            _dbContext.OrderHeaders.Add(orderHeader);
            await _dbContext.SaveChangesAsync();

            return orderHeader;
        }

        public async Task<IEnumerable<OrderHeader>> GetAllOrderAsync(string? userId = null, string? status = null, bool includeUser = false, bool includeDetails = false)
        {
            var query = _dbContext.OrderHeaders.AsQueryable();
            if (includeUser)
            {
                query = query.Include(u => u.ApplicationUser);
            }
            if (includeDetails)
            {
                query = query.Include(u => u.OrderDetails)
                             .ThenInclude(od => od.Product);
            }
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(u => u.ApplicationUserId == userId);
            }
            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                query = query.Where(u => u.OrderStatus.ToLower() == status.ToLower());
            }
            return await query.ToListAsync();
        }

        public async Task<OrderHeader?> GetOrderByIdAsync(int id, bool includeUser = false, bool includeDetails = false)
        {
            var query = _dbContext.OrderHeaders.AsQueryable();
            if (includeUser)
            {
                query = query.Include(u => u.ApplicationUser);
            }
            if (includeDetails)
            {
                query = query.Include(u => u.OrderDetails)
                             .ThenInclude(od => od.Product);
            }
            return await query.FirstOrDefaultAsync(u => u.Id == id);
        }
        public Task UpdateOrderAsync(OrderHeader orderHeader)
        {
            _dbContext.OrderHeaders.Update(orderHeader);
            return _dbContext.SaveChangesAsync();
        }

        public async Task UpdateOrderStatusAsyncx(int id, string orderStatus, string? carrier = null, string? trackingNumber = null)
        {
            var order = await _dbContext.OrderHeaders.FindAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with id {id} not found.");
            }
            order.OrderStatus = orderStatus;
            if (orderStatus == SD.StatusShipped)
            {
                order.ShippingDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(carrier))
                {
                    order.Carrier = carrier;
                }
                if (!string.IsNullOrEmpty(trackingNumber))
                {
                    order.TrackingNumber = trackingNumber;
                }
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}

