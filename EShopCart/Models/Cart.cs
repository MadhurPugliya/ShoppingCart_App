using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EShopCart.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        [Required]
        public int CustomerId { get; set; }  // This should be linked to the User
         public decimal TotalPrice { get; set; }


        // Navigation property for the User (One User has one Cart)
        public User User { get; set; }

        // Navigation property for CartItems (One Cart can have many CartItems)
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }

    public class CartItem
    {
        public int CartItemId { get; set; }   // Primary key for CartItem
        public int CartId { get; set; }        // Foreign key for Cart
        public int ProductId { get; set; }     // Foreign key for Product
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public bool IsOrdered { get; set; }    // Property to track if the item is ordered
        
        // Navigation property for Cart
        [JsonIgnore]
        public Cart Cart { get; set; }
        
        // Navigation property for Product
        public Product Product { get; set; }
    }
}


