
using System.ComponentModel.DataAnnotations;
using EShopCart.Models;


namespace EShopCart.DTOs
{
    public class CartDto
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public List<CartItemDto> CartItems { get; set; }

        // Add the TotalPrice property here
        public decimal TotalPrice { get; set; }
    }

    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Product Product{get; set;}

        // TotalPrice for this individual item (Price * Quantity)
        public decimal TotalPrice { get; set; }
    }

    public class CartItemCreateDto
    {
        [Required(ErrorMessage = "Product ID is required.")]
        public int ProductId { get; set; }  // ID of the product being added to the cart

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }  // Quantity of the product being added to the cart

        
    }


    public class CartItemUpdateDto
    {
        [Required(ErrorMessage = "Product ID is required.")]
        public int ProductId { get; set; }  // Ensure this is included

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }
    }
}
