using BulkyBook.Models;

namespace BulkyBook.Business.Services.IServices
{
    public interface ICategoryService
    {
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllCategoryAsync();
        Task<Category> CreateCategoryAsync(Category category);
        //Task<Category> UpdateCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
        Task<bool> IsCategoryNameUniqueAsync(string name, int? categoryId = null);
    }
}
