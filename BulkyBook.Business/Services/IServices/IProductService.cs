using BulkyBook.Models;

namespace BulkyBook.Business.Services.IServices
{
    public interface IProductService
    {
        Task<Product?> GetProductByIdAsync(int id, bool includeCategory = false);
        Task<IEnumerable<Product>> GetAllProductAsync(bool includeCategory = false);
        Task<Product> CreateProductAsync(Product product);
        //Task<Product> UpdateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);

    }
}
