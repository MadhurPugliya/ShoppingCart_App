using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EShopCart.DTOs;
using EShopCart.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EShopCart.Repositories;
using MimeKit;
using MailKit.Security;
using MimeKit.Text;

namespace EShopCart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderController> _logger;

    private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrderController(ApplicationDbContext context, IOrderRepository orderRepository, ILogger<OrderController> logger, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _mapper = mapper;
            _context= context;
        }

        #region Create Order
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder(OrderCreateDto orderDto)
        {
            // Validate UserId
            if (orderDto.UserId <= 0)
            {
                return BadRequest("Invalid UserId.");
            }

            // Retrieve cart items
            var cartItems = await _context.CartItems
                .Where(ci => ci.Cart.CustomerId == orderDto.UserId && !ci.IsOrdered)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return BadRequest("No items in the cart to place an order.");
            }

            var order = new Order
            {
                UserId = orderDto.UserId,
                ShippingAddress = orderDto.ShippingAddress,
                PinCode = orderDto.PinCode,
                OrderDate = DateTime.UtcNow,
                Status = "Pending Payment",
            };

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Price,
                    TotalPrice = cartItem.Quantity * cartItem.Price
                };
                orderItems.Add(orderItem);
                totalAmount += orderItem.TotalPrice;
            }

            order.TotalAmount = totalAmount;
            order.OrderItems = orderItems;

            // Create order via repository
            await _orderRepository.CreateOrderAsync(order);

            // Mark cart items as ordered
            foreach (var cartItem in cartItems)
            {
                cartItem.IsOrdered = true;
            }

            var userCart = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == orderDto.UserId);
            if (userCart != null)
            {
                _context.CartItems.RemoveRange(userCart.CartItems);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Order placed successfully. Proceed to payment.",
                Order = _mapper.Map<OrderDto>(order)
            });
        }
        #endregion

      

        #region Get Order
        [HttpGet("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            try
            {
                var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("User ID is not available in the claims.");
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid User ID.");
                }

                var order = await _orderRepository.GetOrderByIdAsync(id, userId);

                if (order == null)
                {
                    return NotFound("Order not found or you are not authorized to view this order.");
                }

                var orderDto = _mapper.Map<OrderDto>(order);
                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching the order.");
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region Delete Order
        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id, int.Parse(User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value));

                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                await _orderRepository.DeleteOrderAsync(id);

                return Ok("Your order has been successfully cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling the order.");
                return StatusCode(500, "Internal server error.");
            }
        }
        #endregion

          #region Get Orders by UserId
        [HttpGet("user-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetOrdersByUserId()
        {
            try
            {
                var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("User ID is not available in the claims.");
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid User ID.");
                }

                var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);

                if (orders == null || !orders.Any())
                {
                    return NotFound("No orders found for this user.");
                }

                var orderDtos = _mapper.Map<List<OrderDto>>(orders);
                return Ok(orderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching orders.");
                return StatusCode(500, "Internal server error.");
            }
        }

         #endregion

        
    }
}

