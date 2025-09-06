using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TvShop.Data;
using TvShop.Models;

namespace TvShop.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Get user's ratings
            var userRatings = await _context.Ratings
                .Where(r => r.UserId == userId)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.UserRatings = userRatings;

            // Get user's orders
            var userOrders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.UserOrders = userOrders;

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser model, IFormFile profileImage)
        {
            // Keep some ModelState validation errors but ignore certain ones
            // that might be caused by extra properties in the form
            var userErrors = ModelState
                .Where(x => x.Key.StartsWith("FirstName") || 
                           x.Key.StartsWith("LastName") || 
                           x.Key.StartsWith("PhoneNumber") || 
                           x.Key.StartsWith("Bio") ||
                           x.Key.StartsWith("DateOfBirth") ||
                           x.Key.StartsWith("Location"))
                .SelectMany(x => x.Value.Errors)
                .ToList();

            if (userErrors.Any())
            {
                foreach (var error in userErrors)
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            if (!string.IsNullOrEmpty(model.FirstName))
                user.FirstName = model.FirstName;
                
            if (!string.IsNullOrEmpty(model.LastName))
                user.LastName = model.LastName;
                
            user.Bio = model.Bio;
            user.DateOfBirth = model.DateOfBirth;
            user.Location = model.Location;
            user.PhoneNumber = model.PhoneNumber;

            // Handle profile image upload
            if (profileImage != null && profileImage.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.ProfileImageUrl) && !user.ProfileImageUrl.Contains("default-avatar"))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + profileImage.FileName;
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                // Log the path for debugging
                System.Diagnostics.Debug.WriteLine($"Saving profile image to: {uploadsFolder}");
                
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }

                user.ProfileImageUrl = "/images/profiles/" + uniqueFileName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
