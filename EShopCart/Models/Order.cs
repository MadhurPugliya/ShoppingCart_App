using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EShopCart.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        // Order placement date
        public DateTime OrderDate { get; set; }

        // Total amount of the order (sum of all order items)
        public decimal TotalAmount { get; set; }

        // Status of the order (e.g., "Pending", "Paid")
        public string Status { get; set; }

        // Shipping address for the order
        public string ShippingAddress { get; set; }

        // Pin code for the order's shipping address
        public string PinCode { get; set; }

        // Foreign key to User
        public int UserId { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }

        // One-to-Many Relationship with OrderItems
        [JsonIgnore]
        public virtual ICollection<OrderItem> OrderItems { get; set; }

        // One-to-Many Relationship with Payments
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
