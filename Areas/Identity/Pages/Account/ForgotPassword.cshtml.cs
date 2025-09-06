using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TvShop.Models;
using TvShop.Services;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailService _emailService;

    public ForgotPasswordModel(
        UserManager<ApplicationUser> userManager,
        EmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
        Input = new InputModel();
        StatusMessage = "";
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string StatusMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                StatusMessage = "Password reset link has been sent. Please check your email.";
                return Page();
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Create reset link
            var resetLink = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { email = Input.Email, token },
                protocol: Request.Scheme);

            // Send email
            var subject = "Reset Your TvShop Password";
            var message = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.";
            await _emailService.SendEmailAsync(Input.Email, subject, message);

            StatusMessage = "Password reset link has been sent. Please check your email.";
            return Page();
        }
        return Page();
    }
}
