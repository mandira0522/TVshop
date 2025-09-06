using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TvShop.Data;
using TvShop.Models;

namespace TvShop.Services
{
    public class CheckoutService
    {
        private readonly ApplicationDbContext _context;
        private readonly PaymentService _paymentService;

        public CheckoutService(ApplicationDbContext context, PaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        public async Task<Order> ProcessCheckoutAsync(string userId)
        {
            var cartItems = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            if (!cartItems.Any()) return null;

            var order = new Order
            {
                UserId = userId,
                Email = await _context.Users.Where(u => u.Id == userId).Select(u => u.Email).FirstOrDefaultAsync(),
                CustomerName = await _context.Users.Where(u => u.Id == userId).Select(u => u.UserName).FirstOrDefaultAsync(),
                Address = "Default Address", // Consider adding address to user profile or getting from a separate method
                TotalAmount = cartItems.Sum(c => c.Quantity * c.Price),
                PaymentId = "PENDING", // Will be updated after payment
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductName = c.ProductName,
                    MainImageUrl = _context.Products.Where(p => p.Id == c.ProductId).Select(p => p.MainImageUrl).FirstOrDefault(),
                    Price = c.Price,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var payment = await _paymentService.ProcessFakePaymentAsync(userId, order.Id, order.TotalAmount);
            if (payment.PaymentStatus == "Failed")
            {
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                return null;
            }

            order.Status = OrderStatus.Processing;
            order.PaymentId = payment.TransactionId;
            await _context.SaveChangesAsync();

            await GenerateInvoiceAsync(order);

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return order;
        }

        private async Task GenerateInvoiceAsync(Order order)
        {
            var invoice = new Invoice
            {
                OrderId = order.Id,
                UserId = order.UserId,
                InvoiceNumber = $"INV-{DateTime.UtcNow.Ticks}",
                TotalAmount = order.TotalAmount
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
        }
    }
}
