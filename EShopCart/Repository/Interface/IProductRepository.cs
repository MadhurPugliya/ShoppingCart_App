using System.Threading.Tasks;
using EShopCart.DTOs;
using EShopCart.Models;

namespace EShopCart.Repositories
{
public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync();

        Task<List<Product>> GetProductsByCategoryAsync(int categoryId);  // Fetch products by CategoryId
    Task<List<Product>> GetProductsByCategoryAsync(string category); // Ensure this exists
    Task<Product> GetProductByIdAsync(int id);

        Task<List<Product>> GetProductsByUserIdAsync(int userId);  // Add this method to filter products by UserId
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);

    //For Mercahnt statistics
    Task<Dictionary<string, int>> GetOrderSummaryAsync(int id);
}

}













