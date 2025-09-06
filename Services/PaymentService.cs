using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TvShop.Models;

namespace TvShop.Services
{
    public class PaymentService
    {
        private readonly ILogger<PaymentService> _logger;
        private readonly Random _random = new();

        public PaymentService(ILogger<PaymentService> logger)
        {
            _logger = logger;
        }

        public async Task<Payment> InitializePaymentAsync(decimal amount)
        {
            _logger.LogInformation("Initializing payment for amount {Amount}", amount);
            
            var payment = new Payment
            {
                Amount = amount,
                PaymentStatus = "Pending",
                TransactionId = Guid.NewGuid().ToString(),
                PaymentDate = DateTime.UtcNow
            };

            return payment;
        }

        public async Task<Payment> ProcessFakePaymentAsync(string userId, int orderId, decimal amount)
        {
            _logger.LogInformation("Processing fake payment for order {OrderId} by user {UserId}", orderId, userId);
            
            // Simulate payment processing with random success/failure
            await Task.Delay(1000);
            
            // 20% chance of payment failure
            var isSuccess = _random.Next(0, 100) < 80;
            var status = isSuccess ? "Success" : "Failed";
            
            _logger.LogInformation("Payment for order {OrderId} completed with status: {Status}", orderId, status);
            
            return new Payment
            {
                PaymentStatus = status,
                OrderId = orderId,
                Amount = amount,
                TransactionId = Guid.NewGuid().ToString(),
                PaymentDate = DateTime.UtcNow
            };
        }
    }
}
