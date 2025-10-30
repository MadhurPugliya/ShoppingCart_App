using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShopCart.Models;
using EShopCart.Repositories;
using System.Security.Claims;

namespace EShopCart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class WalletController : ControllerBase
    {
        #region Fields

        private readonly IWalletRepository _walletRepository;

        #endregion

        #region Constructor

        public WalletController(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        #endregion

        #region Endpoints

      [HttpPost("top-up/{amount}")]
public async Task<IActionResult> TopUpWallet(int amount)
{
    // Get user ID from the authenticated user claims
    var userIdStr = User.FindFirstValue("userId");
    if (string.IsNullOrEmpty(userIdStr))
    {
        return Unauthorized(new { message = "User not authenticated." });
    }

    if (!int.TryParse(userIdStr, out int userId))
    {
        return BadRequest(new { message = "Invalid user ID." });
    }

    // Validate the amount
    if (amount <= 0)
    {
        return BadRequest(new { message = "Amount must be greater than zero." });
    }
    // Retrieve the user
    var user = await _walletRepository.GetUserByIdAsync(userId);
    if (user == null)
    {
        return NotFound(new { message = "User not found." });
    }

    // Update wallet balance
    user.WalletBalance += amount;
    await _walletRepository.UpdateUserAsync(user);

    return Ok(new { message = $"Wallet top-up successful. New balance: {user.WalletBalance}" });
}

        /// <summary>
        /// Get the current wallet balance.
        /// </summary>
        /// <returns>Wallet balance or error message.</returns>
        [HttpGet("balance")]
        public async Task<IActionResult> GetWalletBalance()
        {
            // Get user ID from the authenticated user claims
            var userIdStr = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized("User not authenticated.");
            }

            if (!int.TryParse(userIdStr, out int userId))
            {
                return BadRequest("Invalid user ID.");
            }

            // Retrieve the user
            var user = await _walletRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new { user.WalletBalance });
        }

        #endregion
    }
}
