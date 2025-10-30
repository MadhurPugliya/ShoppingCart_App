using EShopCart.DTOs;
using EShopCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShopCart.Repositories;
using AutoMapper;

namespace EShopCart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CartController(ApplicationDbContext context, ICartRepository cartRepository, ILogger<CartController> logger, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // Helper method to get logged-in user's ID from the JWT token
        private int GetLoggedInUserId()
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return string.IsNullOrEmpty(userIdClaim) ? 0 : int.Parse(userIdClaim);
        }

        #region Add to Cart

        [Authorize(Roles = "Customer")]
[HttpPost("add-to-cart")]
public async Task<IActionResult> AddToCart([FromBody] CartItemCreateDto request)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int userId = GetLoggedInUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null)
        {
            cart = new Cart
            {
                CustomerId = userId,
                CartItems = new List<CartItem>()
            };

            await _cartRepository.AddCartAsync(cart);
        }

        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
        {
            return NotFound("Product not found");
        }

        if (request.Quantity > product.StockQuantity)
        {
            return BadRequest($"Only {product.StockQuantity} units available in stock.");
        }

        var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
        if (existingCartItem != null)
        {
            existingCartItem.Quantity += request.Quantity;

            if (existingCartItem.Quantity > product.StockQuantity)
            {
                return BadRequest($"Only {product.StockQuantity} units can be added.");
            }
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                CartId = cart.CartId,
                Price = product.Price
            });
        }

        // Attach the cart if it is detached
        if (_context.Entry(cart).State == EntityState.Detached)
        {
            _context.Carts.Attach(cart);
        }

        // Recalculate the total price
        cart.TotalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Price);

        // Log total price for debugging
        _logger.LogInformation($"Cart TotalPrice before save: {cart.TotalPrice}");

        // Save the cart
        await _cartRepository.SaveAsync();

        _logger.LogInformation($"User {userId} added product {request.ProductId} to the cart.");
        return Ok(new 
        { 
            message = "Product added to cart successfully",
            totalPrice = cart.TotalPrice // Return the updated total price
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while adding to cart.");
        return StatusCode(500, "Internal server error");
    }
}


        #endregion

        #region Get Cart Details

        [Authorize(Roles = "Customer")]
        [HttpGet("details")]
        public async Task<IActionResult> GetCartDetails()
        {
            try
            {
                int userId = GetLoggedInUserId();
                if (userId == 0)
                {
                    return Unauthorized();
                }

                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    return NotFound("Cart not found");
                }

                cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price * ci.Quantity);
                var cartDto = _mapper.Map<CartDto>(cart);
                cartDto.TotalPrice = cart.TotalPrice;

                return Ok(cartDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cart details.");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        #region Remove from Cart

        [Authorize(Roles = "Customer")]
        [HttpDelete("remove-from-cart/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                int userId = GetLoggedInUserId();
                if (userId == 0)
                {
                    return Unauthorized();
                }

                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    return NotFound("Cart not found");
                }

                var cartItem = await _cartRepository.GetCartItemAsync(cart.CartId, productId);
                if (cartItem == null)
                {
                    return NotFound("Product not found in the cart");
                }

                await _cartRepository.RemoveCartItemAsync(cartItem);
                _logger.LogInformation($"User {userId} removed product {productId} from the cart.");
                return Ok(new { message = "Product removed from cart successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing product from cart.");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        #region Update Quantity

        [Authorize(Roles = "Customer")]
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartItemUpdateDto request)
        {
            try
            {
                int userId = GetLoggedInUserId();
                if (userId == 0)
                {
                    return Unauthorized();
                }

                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    return NotFound("Cart not found");
                }

                var cartItem = await _cartRepository.GetCartItemAsync(cart.CartId, request.ProductId);
                if (cartItem == null)
                {
                    return NotFound("Product not found in the cart");
                }

                // Get the product from the database
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    return NotFound("Product not found");
                }

                // Check if the requested quantity is within the available stock
                if (request.Quantity > product.StockQuantity)
                {
                    return BadRequest($"Only {product.StockQuantity} units are available in stock.");
                }

                // Update the quantity in the cart item
                cartItem.Quantity = request.Quantity;
                cart.TotalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Price);

                // Save the changes to the database
                await _cartRepository.SaveAsync();

                _logger.LogInformation($"User {userId} updated the quantity of product {request.ProductId} to {request.Quantity}.");
                return Ok(new { message = "Quantity updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product quantity in cart.");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        #region Get Total Amount

        [Authorize(Roles = "Customer")]
        [HttpGet("total")]
        public async Task<IActionResult> GetTotalAmount()
        {
            try
            {
                int userId = GetLoggedInUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not identified");
                }

                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null || cart.CartItems.Count == 0)
                {
                    return NotFound("Cart is empty");
                }

                decimal totalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
                return Ok(new { TotalAmount = totalAmount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calculating the total amount in the cart.");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion
    }
}
