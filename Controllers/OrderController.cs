using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TvShop.Data;
using TvShop.Models;

namespace TvShop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UsersOrders(string? statusFilter = null, int? days = null, string? search = null, string? userId = null)
        {
            // Start with a fresh query and build it up with filters before executing
            var baseQuery = _context.Orders.AsQueryable();
            
            // Include order items first
            baseQuery = baseQuery.Include(o => o.OrderItems);

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(statusFilter))
            {
                baseQuery = baseQuery.Where(o => o.Status == Enum.Parse<OrderStatus>(statusFilter));
            }

            // Apply date filter if provided
            if (days.HasValue)
            {
                var cutoffDate = DateTime.Now.AddDays(-days.Value);
                baseQuery = baseQuery.Where(o => o.OrderDate >= cutoffDate);
            }
            
            // Apply user filter if provided
            if (!string.IsNullOrEmpty(userId))
            {
                baseQuery = baseQuery.Where(o => o.UserId == userId);
            }
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                baseQuery = baseQuery.Where(o => 
                    o.OrderItems.Any(oi => oi.ProductName != null && oi.ProductName.Contains(search)) ||
                    o.Id.ToString().Contains(search) ||
                    o.Email.Contains(search) ||
                    (o.CustomerName != null && o.CustomerName.Contains(search))
                );
            }
            
            // Always sort by most recent first
            baseQuery = baseQuery.OrderByDescending(o => o.OrderDate);

            // Execute the query and get the final list
            var orders = baseQuery.ToList();
            
            // Get list of users with orders for the filter dropdown
            ViewBag.Users = _context.Users
                .Where(u => _context.Orders.Any(o => o.UserId == u.Id))
                .Select(u => new { Id = u.Id, Email = u.Email })
                .ToList();
            
            ViewBag.StatusFilter = statusFilter;
            ViewBag.DaysFilter = days;
            ViewBag.SearchTerm = search;
            ViewBag.UserIdFilter = userId;

            return View("~/Views/Admin/UsersOrders.cshtml", orders);
        }

        [Authorize]
        public IActionResult MyOrders(string? statusFilter = null, int? days = null, string? search = null)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Add debug information
            Console.WriteLine($"Looking for orders for user: {userId}");
            
            var ordersQuery = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId);

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(statusFilter))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == Enum.Parse<OrderStatus>(statusFilter));
            }


            // Apply date filter if provided
            if (days.HasValue)
            {
                var cutoffDate = DateTime.Now.AddDays(-days.Value);
                ordersQuery = ordersQuery.Where(o => o.OrderDate >= cutoffDate);
            }
            
            // Always sort by most recent first
            ordersQuery = ordersQuery.OrderByDescending(o => o.OrderDate);
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                ordersQuery = ordersQuery.Where(o => 
                    o.OrderItems.Any(oi => oi.ProductName != null && oi.ProductName.Contains(search)) ||
                    o.Id.ToString().Contains(search)
                );
            }
            
            // Debug: Check SQL query
            var sql = ordersQuery.ToQueryString();
            Console.WriteLine($"SQL Query: {sql}");

            var orders = ordersQuery.ToList();
            Console.WriteLine($"Found {orders.Count} orders for user {userId}");

            if (orders.Count == 0)
            {
                ViewBag.Message = "You have no orders yet. Start shopping to place your first order!";
            }

            return View("~/Views/orders/MyOrders.cshtml", orders);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UpdateStatus(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = OrderStatus.Shipped;
            _context.SaveChanges();

            return RedirectToAction("AllOrders");
        }

        [Authorize]
        public IActionResult ViewInvoice(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // First check if the order exists and belongs to the current user
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                ViewBag.ErrorMessage = "Order not found or you don't have access to this order.";
                return RedirectToAction("MyOrders");
            }

            // Check if invoice exists
            var invoice = _context.Invoices.FirstOrDefault(i => i.OrderId == id);
            
            // If invoice doesn't exist, create it
            if (invoice == null)
            {
                invoice = new Invoice
                {
                    OrderId = order.Id,
                    UserId = userId,
                    InvoiceDate = DateTime.Now,
                    InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{order.Id}",
                    TotalAmount = order.TotalAmount,
                    Order = order
                };
                
                _context.Invoices.Add(invoice);
                _context.SaveChanges();
            }
            else
            {
                // Ensure we have the order details for the view
                invoice.Order = order;
            }

            return View("~/Views/Checkout/Invoice.cshtml", invoice);
        }

        [Authorize]
        public IActionResult TrackOrder(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                ViewBag.ErrorMessage = "Order not found or you're not authorized to view it.";
                return RedirectToAction("MyOrders");
            }

            // For now, just show order details with a nicer tracking UI
            return View("~/Views/Orders/TrackOrder.cshtml", order);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BuyNow(int productId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Clear any existing cart items to ensure only this product is in the cart
            var existingCartItems = _context.CartItems.Where(c => c.UserId == userId);
            _context.CartItems.RemoveRange(existingCartItems);
            await _context.SaveChangesAsync();

            // Add the product to cart with quantity 1
            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = productId,
                ProductName = product.Name,
                Price = product.Price,
                MainImageUrl = product.MainImageUrl,
                Quantity = 1
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            // Redirect to checkout
            return RedirectToAction("Index", "Checkout");
        }
    }
}
