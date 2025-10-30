using EShopCart.Models;
using System.Threading.Tasks;

namespace EShopCart.Repositories
{    
    public interface ICategoryRepository
    {
        Task<Categories> GetCategoryByIdAsync(int id);
        Task<List<Categories>> GetAllCategoriesAsync();
        Task<Categories> GetCategoryByNameAsync(string categoryName);
    }

    
}