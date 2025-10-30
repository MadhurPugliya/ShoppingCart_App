using System.ComponentModel.DataAnnotations;

namespace EShopCart.DTOs
{
public class ProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }  // ID of the Category to which the product belongs

    public string ImageUrl { get; set; }  // URL for the uploaded image file

    public string CategoryName { get; set; }  // Category name (from Category navigation property)

    public CategoryDto Category { get; set; }  // Optional: Include full category details in the product DTO

    public int UserId { get; set; }  // User who created/owns the product

    // public string Username { get; set; }  // Username of the user who created the product
}

    public class ProductCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // public int UserId {get; set;}

        public IFormFile ImageUrl { get; set; }  // Uploaded image file
    }
}

    public class ProductUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public IFormFile ImageUrl { get; set; }  // Uploaded image file
    }
