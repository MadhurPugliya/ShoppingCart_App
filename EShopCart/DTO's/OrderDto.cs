 using System.ComponentModel.DataAnnotations;
using EShopCart.Models;


namespace EShopCart.DTOs
{
    // DTO for returning order details and listing orders
    public class OrderDto
    {
        public int OrderId { get; set; }          // Order ID
        public DateTime OrderDate { get; set; }    // Order placement date
        public string Status { get; set; }         // Status of the order (e.g., "Pending", "Completed", etc.)
        public decimal TotalAmount { get; set; }   // Total amount of the order
        public string ShippingAddress { get; set; } // Shipping address of the order
        public string PinCode { get; set; }  
        
        public string Name {get; set;}   // Pin code associated with the shipping address
        public List<OrderItemDto> OrderItems { get; set; }  // List of items in the order
    }

    // DTO for individual order items
    public class OrderItemDto
    {
        public int ProductId { get; set; }   // Product ID of the item
        // public string ProductName { get; set; } // Name of the product
        public int Quantity { get; set; }     // Quantity of the product ordered
        // public decimal Price { get; set; }    // Price of the product per unit
        public decimal TotalPrice { get; set; } 
        public Product Product{get;set;}   // Total price for the ordered quantity (Quantity * Price)
    }
public class OrderCreateDto
{
    [Required(ErrorMessage = "User ID is required.")]
    public int UserId { get; set; }  // User ID placing the order

    [Required(ErrorMessage = "Shipping address is required.")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Shipping address must be between 10 and 200 characters.")]
    public string ShippingAddress { get; set; } // Shipping address for the order

    [Required(ErrorMessage = "Pin code is required.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Pin code must be exactly 6 digits.")]
    public string PinCode { get; set; }  // Pin code associated with the shipping address
}



    // DTO for updating order details (e.g., address, pin code)
    public class OrderUpdateDto
    {
        public string ShippingAddress { get; set; } // New shipping address
        public string PinCode { get; set; }        // New pin code for the shipping address
    }

    // DTO for updating the status of an order
    public class OrderStatusUpdateDto
    {
        public string Status { get; set; } // New status of the order (e.g., "Pending", "Completed", "Canceled")
    }

}
