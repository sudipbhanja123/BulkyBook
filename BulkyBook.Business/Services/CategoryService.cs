using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Category>> GetAllCategoryAsync()
        {
            return await _context.Categories.ToListAsync();
        }
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new Exception($"Category with id {id} not found.");

            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

        }

        //Task<Category> ICategoryService.UpdateCategoryAsync(Category category)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<bool> IsCategoryNameUniqueAsync(string name, int? categoryId = null)
        {
            if (categoryId.HasValue)
            {
                return !await _context.Categories.AnyAsync(c =>
                    c.Name.ToLower() == name.ToLower() &&
                    c.Id != categoryId.Value);
            }

            return !await _context.Categories.AnyAsync(c =>
                c.Name.ToLower() == name.ToLower());
        }
    }
}
