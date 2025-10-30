using Microsoft.AspNetCore.Mvc;
using EShopCart.DTOs;
using EShopCart.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MimeKit;
using MailKit.Security;
using MimeKit.Text;
using EShopCart.Repositories; // Import the IUserRepository

namespace EShopCart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository; // Inject IUserRepository

        public PaymentController(ApplicationDbContext context, IMapper mapper, IUserRepository userRepository)
        {
            _context = context;
            _mapper = mapper;
            _userRepository = userRepository; // Initialize IUserRepository
        }

        [HttpPost("make-payment")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MakePayment(PaymentCreateDto paymentDto)
        {
            // Retrieve the order based on the provided OrderId
            var order = await _context.Orders
                .Include(o => o.OrderItems)  // Include OrderItems to update product quantity
                .ThenInclude(oi => oi.Product)  // Ensure Product details are included
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == paymentDto.OrderId);

            if (order == null) return NotFound("Order not found.");

            var user = order.User;  // Associated user with the order
            if (user == null) return NotFound("User not found.");

            // Check if the amount provided by the customer matches the order's total amount
            if (paymentDto.Amount != order.TotalAmount)
            {
                return BadRequest("The amount provided does not match the order total.");
            }

            // Validate the PaymentMode (PaymentMethod)
            var validPaymentModes = new List<string> { "Wallet", "CreditCard", "COD" };
            if (!validPaymentModes.Contains(paymentDto.PaymentMode))
            {
                return BadRequest("Invalid payment method. Accepted methods are: Wallet, CreditCard, COD.");
            }

            // Map the payment DTO to the Payment model
            var payment = _mapper.Map<Payment>(paymentDto);
            payment.PaymentDate = DateTime.UtcNow;
            payment.Status = "Completed";
            payment.UserId = user.UserId;  // Set the UserId of the payment (important for linking payment to the user)
            payment.PaymentMode = paymentDto.PaymentMode;

            if (paymentDto.PaymentMode == "Wallet")
            {
                // Ensure the user has sufficient funds in their wallet
                if (user.WalletBalance < order.TotalAmount)
                {
                    return BadRequest("Insufficient wallet balance.");
                }

                // Calculate the amount to deduct from the user's wallet
                decimal amountToDeduct = order.TotalAmount;
                user.WalletBalance -= amountToDeduct;

                // Update the user’s wallet balance
                await _userRepository.UpdateUserAsync(user);  // Use IUserRepository to update the user
            }
            else if (paymentDto.PaymentMode == "CreditCard" || paymentDto.PaymentMode == "COD")
            {
                // Logic for other payment methods can go here
                // For now, assuming that payment is always successful.
                payment.Status = "Completed";
            }

            // Save the payment to the database
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Update the order status to "Paid"
            order.Status = "Paid";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            // Update product stock quantities
            foreach (var orderItem in order.OrderItems)
            {
                var product = orderItem.Product;  // Get the product for this order item
                if (product != null)
                {
                    // Check if the product has enough stock
                    if (product.StockQuantity < orderItem.Quantity)
                    {
                        return BadRequest($"Not enough stock for product {product.Name}.");
                    }

                    // Reduce the product stock by the quantity ordered
                    product.StockQuantity -= orderItem.Quantity;

                    // Update the product stock in the database
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                }
            }

            // Send Payment Success Email
            try
            {
                await SendPaymentSuccessEmail(user.Email, order.OrderId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending email: {ex.Message}");
            }
            return Ok(new { message = "Payment Successful" });
        }

        // Private method to send a payment success email
        private async Task SendPaymentSuccessEmail(string recipientEmail, int orderId)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("madhurpugliya10@gmail.com")); // Sender's email (your email)
            email.To.Add(MailboxAddress.Parse(recipientEmail));  // Recipient email
            email.Subject = "Payment Successful - EShopCart";

            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"<h1>Payment Successful!</h1><p>Your order with Order ID: {orderId} has been successfully processed.</p><p>Your ordered items will be delivered soon. Thank you for shopping with us!</p>"
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls); // SMTP Gmail server
            await smtp.AuthenticateAsync("madhurpugliya10@gmail.com", "hpqtovtyovojlnrw"); // Gmail credentials (use app password)
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        // Retrieve payment details by OrderId
        [HttpGet("{orderId}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<PaymentDto>> GetPaymentByOrderId(int orderId)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)  // Include order details
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null)
                return NotFound("Payment not found.");

            return Ok(_mapper.Map<PaymentDto>(payment));
        }
    }
}
