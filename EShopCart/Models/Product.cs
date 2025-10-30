using System.ComponentModel.DataAnnotations;

namespace EShopCart.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }  // Foreign key to Category

        public Categories Category { get; set; }  // Navigation property to Category

        [StringLength(200)] // Max length for the image URL
        public string ImageUrl { get; set; }  // URL for the product image

        // Foreign key to User
        [Required]
        public int UserId { get; set; }

        // Navigation property to User
        public User User { get; set; }
    }
}
