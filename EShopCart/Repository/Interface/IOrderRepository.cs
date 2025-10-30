using EShopCart.Models;
using System.Threading.Tasks;

namespace EShopCart.Repositories
{
public interface IOrderRepository
{
    Task<Order> GetOrderByIdAsync(int id, int userId);
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
    Task CreateOrderAsync(Order order);
    Task UpdateOrderAsync(Order order);
    Task DeleteOrderAsync(int id);
}

}