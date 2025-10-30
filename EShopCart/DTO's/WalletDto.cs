
using System.ComponentModel.DataAnnotations;

namespace EShopCart.DTOs
{
    public class AddWalletDto
    {
        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; } // The ID of the user whose wallet is being updated

        [Required(ErrorMessage = "Amount to add is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount to add must be greater than zero.")]
        public int AmountToAdd { get; set; } // The amount to be added to the wallet
    }
}
