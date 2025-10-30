using Microsoft.EntityFrameworkCore;

namespace EShopCart.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Categories> Categories { get; set; } // Added Categories DbSet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);   // Calls the base class's OnModelCreating method before doing any custom configurations

            // Seed predefined categories
            modelBuilder.Entity<Categories>().HasData(
                new Categories { CategoryId = 1, CategoryName = "Electronics" },
                new Categories { CategoryId = 2, CategoryName = "Fashion" },
                new Categories { CategoryId = 3, CategoryName = "Home Appliances" },
                new Categories { CategoryId = 4, CategoryName = "Books" },
                new Categories { CategoryId = 5, CategoryName = "Toys" },
                new Categories { CategoryId = 6, CategoryName = "Beauty & Personal Care" },
                new Categories { CategoryId = 7, CategoryName = "Sports" },
                new Categories { CategoryId = 8, CategoryName = "Groceries" },
                new Categories { CategoryId = 9, CategoryName = "Automotive" },
                new Categories { CategoryId = 10, CategoryName = "Health & Wellness" }
            );

            // User and Cart: One User can have one Cart
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)  // One-to-one relationship
                .HasForeignKey<Cart>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete on User deletion

            // User and Orders: One User can have many Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)  // One-to-many relationship
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete orders if the user is deleted

            // User and Payments: One User can have many Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete on User deletion

            // Cart and CartItem: One Cart can have many CartItems
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem and Product: One CartItem references one Product
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()  // Product does not have a collection of CartItems, so no navigation property on Product
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure IsOrdered is part of the CartItem model
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.IsOrdered)
                .HasDefaultValue(false);  // Default to false

            // Order and OrderItem: One Order can have many OrderItems
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem and Product: One OrderItem references one Product
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order and Payment: One Order can have many Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade); 

            // Ensure WalletBalance is added to User
            modelBuilder.Entity<User>()
                .Property(u => u.WalletBalance)
                .HasDefaultValue(0);  // Set default wallet balance to 0

            // Category and Product: One Category can have many Products (foreign key on Product)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)  // Assuming Product has a Category navigation property
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);  // Restrict delete behavior to prevent cascading

          modelBuilder.Entity<Product>()
        .HasOne(p => p.User)
        .WithMany(u => u.Products)
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.Cascade); // or Restrict

            // base.OnModelCreating(modelBuilder);

        }
    }
}
