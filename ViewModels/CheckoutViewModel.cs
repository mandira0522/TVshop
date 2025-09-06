using System.ComponentModel.DataAnnotations;

namespace TvShop.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        public required string FullName { get; set; }

        [Required]
        public required string Address { get; set; }

        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        public string? Notes { get; set; }
    }
}
