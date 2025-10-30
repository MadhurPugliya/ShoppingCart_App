using EShopCart.Models;
using System.Threading.Tasks;

namespace EShopCart.Repositories
{
    public interface IUserRepository
    {
        Task<bool> UserExistsAsync(string username);
        Task AddUserAsync(User user);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByIdAsync(int id); // Add this method
        Task UpdateUserAsync(User user); // Add this method for updating the user
    }
}
