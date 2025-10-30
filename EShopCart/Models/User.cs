using System.ComponentModel.DataAnnotations;

namespace EShopCart.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username should have at least 3 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Wallet balance cannot be negative.")]
        public decimal WalletBalance { get; set; }

        // One-to-One Relationship with Cart
        public virtual Cart Cart { get; set; }

        // One-to-Many Relationship with Orders
        public virtual ICollection<Order> Orders { get; set; }

        // One-to-Many Relationship with Payments
        public virtual ICollection<Payment> Payments { get; set; }

            public ICollection<Product> Products { get; set; }

    }
}
