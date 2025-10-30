using EShopCart.DTOs;
using EShopCart.Models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;

namespace EShopCart.Repositories
{


    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ProductRepository.cs
        public async Task<List<Product>> GetAllProductsAsync()
        {
            // Ensure the related Category is included
            return await _context.Products
                                 .Include(p => p.Category) // Include the category navigation property
                                 .ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                                 .Where(p => p.CategoryId == categoryId)
                                 .ToListAsync();  // Fetch products by CategoryId
        }
        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _context.Products
                .Where(p => p.Category.CategoryName.ToLower() == category.ToLower()) // Ensure this uses ToLower
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsByUserIdAsync(int userId)
        {
            return await _context.Products
                                 .Where(p => p.UserId == userId)  // Filter products by UserId
                                 .Include(p => p.Category)  // Include the category navigation property
                                 .ToListAsync();
        }


        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category) // Include the category to fetch the category details
                .FirstOrDefaultAsync(p => p.ProductId == id); // Use ProductId as it is the correct key
        }


        public async Task AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }


        //For merchant statistics
        public async Task<Dictionary<string, int>> GetOrderSummaryAsync(int userId)
        {
            // Get all product IDs that belong to the merchant (UserId represents Merchant)
            var merchantProductIds = await _context.Products
                .Where(p => p.UserId == userId) // Fetch only the logged-in merchant's products
                .Select(p => p.ProductId)
                .ToListAsync();

            // Get orders that contain at least one product from this merchant
            var customerOrders = await _context.Orders
                .Where(o => o.OrderItems.Any(oi => merchantProductIds.Contains(oi.ProductId))) // Orders that contain merchant's products
                .ToListAsync();

            // Get total orders count
            int totalOrders = customerOrders.Count;

            // Count orders by status
            var statusCounts = customerOrders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Status, x => x.Count);

            // Ensure keys exist for Pending and Successful orders
            int pendingOrders = statusCounts.ContainsKey("Pending Payment") ? statusCounts["Pending Payment"] : 0;
            int successfulOrders = statusCounts.ContainsKey("Paid") ? statusCounts["Paid"] : 0;

            return new Dictionary<string, int>
    {
        { "TotalOrders", totalOrders },
        { "PendingOrders", pendingOrders },
        { "SuccessfulOrders", successfulOrders }
    };
        }

    }
}