using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TvShop.Services;
using TvShop.Models;
using Microsoft.AspNetCore.Http;

namespace TvShop.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        public CartController(CartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User); // Declare userId
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account", new { area = "Identity" }); // Ensure user is logged in

            var cartItems = await _cartService.GetCartItemsAsync(userId); // Pass userId correctly
            return View(cartItems);
        }

        public async Task<IActionResult> AddToCart(int productId)
        {
            string userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                // Always redirect to login for unauthenticated users
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action("AddToCart", "Cart", new { productId }) });
            }

            try
            {
                await _cartService.AddToCartAsync(productId, userId);
                var cartCount = await _cartService.GetCartCountAsync(userId);
                
                return IsAjaxRequest()
                    ? Json(new { success = true, message = "Item added to cart!", cartCount })
                    : RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = ex.Message })
                    : RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            string userId = _userManager.GetUserId(User); // Fetch userId
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account", new { area = "Identity" }); 

            await _cartService.UpdateQuantityAsync(id, quantity, userId); // Pass userId
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            string userId = _userManager.GetUserId(User); // Fetch userId
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account", new { area = "Identity" }); 

            await _cartService.RemoveFromCartAsync(id, userId); // Pass userId
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            string userId = _userManager.GetUserId(User);
            int count = 0;
            
            if (!string.IsNullOrEmpty(userId))
            {
                count = await _cartService.GetCartCountAsync(userId);
            }
            
            return Json(new { count });
        }
    }
}
