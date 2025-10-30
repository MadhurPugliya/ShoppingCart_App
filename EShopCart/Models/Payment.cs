using EShopCart.Models;
public class Payment
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; }
    public string PaymentMode { get; set; } // "Wallet", "CreditCard", "COD"
    public decimal Amount { get; set; } // Payment amount
}

