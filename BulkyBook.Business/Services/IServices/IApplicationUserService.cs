using BulkyBook.Models;

namespace BulkyBook.Business.Services.IServices
{
    public interface IApplicationUserService
    {
        Task<ApplicationUser?> GetUserByIdAsync(String userId);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    }
}
