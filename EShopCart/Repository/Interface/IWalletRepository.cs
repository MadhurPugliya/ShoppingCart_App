using System.Threading.Tasks;
using EShopCart.Models;

namespace EShopCart.Repositories
{
    public interface IWalletRepository
    {
        Task<User> GetUserByIdAsync(int userId);
        Task UpdateUserAsync(User user);
    }
}
