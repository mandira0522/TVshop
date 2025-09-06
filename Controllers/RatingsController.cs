using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TvShop.Data;
using TvShop.Models;

namespace TvShop.Controllers
{
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddRating(int productId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            // Check if user already rated this product
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.Value = rating;
                existingRating.Comment = comment;
                _context.Ratings.Update(existingRating);
            }
            else
            {
                // Create new rating
                var newRating = new Rating
                {
                    ProductId = productId,
                    UserId = userId,
                    Value = rating,
                    Comment = comment
                };
                _context.Ratings.Add(newRating);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Products", new { id = productId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteRating(int id, int productId)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
            {
                return NotFound("Rating not found.");
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || (rating.UserId != userId && !User.IsInRole("Admin")))
            {
                return Unauthorized("You are not authorized to delete this rating.");
            }

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Products", new { id = productId });
        }

        [HttpGet]
        public async Task<IActionResult> GetProductRatings(int productId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            ViewBag.Product = product;
            return View(ratings);
        }


    }
}
