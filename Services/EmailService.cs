using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace TvShop.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _environment;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // Check if we're in development mode and using the dev mode bypass option
            var emailSettings = _configuration.GetSection("EmailSettings");
            var useDevModeBypass = _environment.IsDevelopment() && 
                                  bool.TryParse(emailSettings["UseDevModeBypass"], out bool devBypass) && 
                                  devBypass;

            if (useDevModeBypass)
            {
                // In development mode with bypass enabled, log the email instead of sending it
                _logger.LogInformation("DEV MODE EMAIL BYPASS: Would have sent email to {Email} with subject: {Subject}", 
                    email, subject);
                _logger.LogInformation("Email content: {Message}", message);
                return;
            }
            
            try
            {
                // Validate SMTP settings before attempting to send
                if (string.IsNullOrEmpty(emailSettings["SmtpUsername"]) || 
                    string.IsNullOrEmpty(emailSettings["SmtpPassword"]) || 
                    emailSettings["SmtpUsername"] == "your-actual-email@gmail.com")
                {
                    throw new InvalidOperationException(
                        "Email settings are not properly configured. Please update the SmtpUsername and SmtpPassword in appsettings.json");
                }

                using var client = new SmtpClient
                {
                    Host = emailSettings["SmtpServer"] ?? "smtp.gmail.com",
                    Port = int.TryParse(emailSettings["SmtpPort"], out int port) ? port : 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        emailSettings["SmtpUsername"],
                        emailSettings["SmtpPassword"])
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        emailSettings["FromAddress"] ?? emailSettings["SmtpUsername"],
                        emailSettings["FromName"] ?? "TvShop Support"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                _logger.LogInformation("Attempting to send email to {Email}", email);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}. Error: {ErrorMessage}", email, ex.Message);
                
                // In development mode, provide more helpful error information
                if (_environment.IsDevelopment())
                {
                    throw new Exception(
                        $"Failed to send email. Please check EMAIL_SETUP_GUIDE.md for configuration instructions. Error: {ex.Message}", ex);
                }
                
                throw; // Re-throw in production to allow the application to handle it appropriately
            }
        }
    }
}
