using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TvShop.Data;
using TvShop.Models;

namespace TvShop.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string userId) // ✅ Accepts userId
        {
            return await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
        }

        public async Task AddToCartAsync(int productId, string userId) // ✅ Accepts userId
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (cartItem != null)
                cartItem.Quantity++;
            else
                _context.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    MainImageUrl = product.MainImageUrl,
                    Price = product.Price,
                    Quantity = 1,
                    UserId = userId
                });

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(int productId, int quantity, string userId) // ✅ Accepts userId
        {
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(int productId, string userId) // ✅ Accepts userId
        {
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string userId) // ✅ Accepts userId
        {
            var cartItems = _context.CartItems.Where(c => c.UserId == userId);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartCountAsync(string userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);
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
