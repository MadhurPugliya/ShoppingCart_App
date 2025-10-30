using EShopCart.Models;
using System.Threading.Tasks;

namespace EShopCart.Repositories
{

public interface IPaymentRepository
{
    Task<Payment> GetPaymentByOrderIdAsync(int orderId);
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task UpdatePaymentAsync(Payment payment);
    Task SaveChangesAsync();
}
}
