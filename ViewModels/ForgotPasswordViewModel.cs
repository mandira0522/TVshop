using System.ComponentModel.DataAnnotations;

namespace TvShop.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}