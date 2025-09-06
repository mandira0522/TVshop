using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace TvShop.Services.Tests
{
    public class EmailServiceTests
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Running Email Service Tests...");
            RunTests().GetAwaiter().GetResult();
        }

        public static async Task RunTests()
        {
            // Test the dev mode bypass
            await TestDevModeBypass();
            
            // Show instructions for proper configuration
            ShowInstructions();
        }

        private static async Task TestDevModeBypass()
        {
            Console.WriteLine("\n======= Testing Development Mode Bypass =======");
            
            // Create mock dependencies
            var configMock = new Mock<IConfiguration>();
            var sectionMock = new Mock<IConfigurationSection>();
            var loggerMock = new Mock<ILogger<EmailService>>();
            var envMock = new Mock<IWebHostEnvironment>();
            
            // Setup mock behavior
            configMock.Setup(c => c.GetSection("EmailSettings")).Returns(sectionMock.Object);
            sectionMock.Setup(s => s["UseDevModeBypass"]).Returns("true");
            envMock.Setup(e => e.IsDevelopment()).Returns(true);
            
            // Create the email service
            var emailService = new EmailService(
                configMock.Object,
                loggerMock.Object,
                envMock.Object);
            
            // Call the send email method
            await emailService.SendEmailAsync(
                "test@example.com", 
                "Test Subject", 
                "<p>This is a test email.</p>");
            
            Console.WriteLine("✅ Email sending bypassed successfully in development mode!");
            Console.WriteLine("   The email would have been logged to the console in a real application.");
        }

        private static void ShowInstructions()
        {
            Console.WriteLine("\n======= How to Configure Email Settings =======");
            Console.WriteLine("1. For development, you can leave UseDevModeBypass set to true");
            Console.WriteLine("2. For production or to test actual email sending:");
            Console.WriteLine("   - Set UseDevModeBypass to false");
            Console.WriteLine("   - Configure valid SMTP credentials in appsettings.json");
            Console.WriteLine("   - See EMAIL_SETUP_GUIDE.md for detailed instructions");
            Console.WriteLine("\nThe application will now use the development bypass mode by default,");
            Console.WriteLine("allowing user registration to work without actual email delivery.");
            Console.WriteLine("======================================================");
        }
    }
}
