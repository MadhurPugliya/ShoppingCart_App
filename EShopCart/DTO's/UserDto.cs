using System.ComponentModel.DataAnnotations;

namespace EShopCart.DTOs
{
    // DTO for returning user details
 public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public decimal WalletBalance { get; set; }
        public string Role { get; set; } // "Merchant" or "Customer"
    }

    // DTO for registering a new user
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        // [RegularExpression(@"^(Customer|Merchant)$", ErrorMessage = "Role must be either 'Customer' or 'Merchant'.")]
        public string Role { get; set; } = string.Empty;// For example, "Customer" or "Merchant"
    }

    // DTO for updating user details
        public class UserUpdateDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public decimal WalletBalance { get; set; } // Updating wallet balance directly
    }
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }
    }
}
