using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TvShop.Data;
using TvShop.Models;
using TvShop.Services;
using TvShop.ViewModels;

namespace TvShop.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PaymentService _paymentService;

        public CheckoutController(CartService cartService, ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, PaymentService paymentService)
        {
            _cartService = cartService;
            _context = context;
            _userManager = userManager;
            _paymentService = paymentService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Checkout") });
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            
            if (!cartItems.Any()) 
                return RedirectToAction("Index", "Cart");

            // Create a temporary payment record
            var payment = await _paymentService.InitializePaymentAsync(cartItems.Sum(i => i.Price * i.Quantity));

            var order = new Order
            {
                UserId = userId,
                Email = model.Email,
                CustomerName = model.FullName,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                TotalAmount = cartItems.Sum(i => i.Price * i.Quantity),
                PaymentId = payment.TransactionId,  // Set the PaymentId as TransactionId
                OrderItems = cartItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    MainImageUrl = i.MainImageUrl,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _cartService.ClearCartAsync(userId);

            return RedirectToAction("FakePayment", new { id = order.Id });
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderItems)
                                             .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        public async Task<IActionResult> FakePayment(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> CompletePayment(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            var payment = await _paymentService.ProcessFakePaymentAsync(
                order.UserId, order.Id, order.TotalAmount);

            if (payment.PaymentStatus == "Failed")
            {
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                return RedirectToAction("PaymentFailed", new { id = order.Id });
            }

            order.Status = OrderStatus.Processing;
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { id });
        }
    }
}
