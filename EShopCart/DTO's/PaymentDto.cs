    using System.ComponentModel.DataAnnotations;

namespace EShopCart.DTOs
{
    // DTO for returning payment details
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; }  // "Wallet", "Credit Card", "COD"
        public string Status { get; set; }
        public DateTime PaymentDate { get; set; }
    }

public class PaymentCreateDto
{
    [Required(ErrorMessage = "Order ID is required.")]
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Payment mode is required.")]
    [RegularExpression(@"^(Wallet|CreditCard|COD)$", ErrorMessage = "Payment mode must be either 'Wallet', 'Credit Card', or 'COD'.")]
    public string PaymentMode { get; set; } // "Wallet", "Credit Card", "COD"
}

}
