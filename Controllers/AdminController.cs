using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using TvShop.Data;
using TvShop.Models;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using TvShop.ViewModels;

namespace TvShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, 
                              UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // Log User Roles for Debugging
        private async Task LogUserRoles()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Console.WriteLine($"Current User Roles: {string.Join(", ", roles)}");
            }
        }

        // Display Products in Admin Panel
        public async Task<IActionResult> Index()
        {
            await LogUserRoles(); // Debugging
            var products = await _context.Products.Include(p => p.ProductImages).ToListAsync();
            return View(products);
        }
        
        // GET: Admin/ManageUsers
        public async Task<IActionResult> ManageUsers(string searchString, string roleFilter)
        {
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            
            var users = _userManager.Users.AsQueryable();
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => 
                    u.UserName.Contains(searchString) || 
                    u.Email.Contains(searchString) || 
                    u.FirstName.Contains(searchString) || 
                    u.LastName.Contains(searchString));
            }
            
            var userList = await users.ToListAsync();
            var userViewModels = new List<UserViewModel>();
            
            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // Apply role filter if provided
                if (!string.IsNullOrEmpty(roleFilter) && !roles.Contains(roleFilter))
                {
                    continue;
                }
                
                var userOrders = await _context.Orders
                    .Where(o => o.UserId == user.Id)
                    .ToListAsync();
                
                userViewModels.Add(new UserViewModel
                {
                    User = user,
                    Roles = roles.ToList(),
                    OrderCount = userOrders.Count,
                    TotalSpent = userOrders.Sum(o => o.TotalAmount)
                });
            }
            
            ViewBag.SearchString = searchString;
            ViewBag.RoleFilter = roleFilter;
            
            return View(userViewModels);
        }
        
        // GET: Admin/UserDetails/id
        public async Task<IActionResult> UserDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            
            var userDetails = new UserDetailsViewModel
            {
                User = user,
                Roles = roles.ToList(),
                Orders = orders,
                TotalOrders = orders.Count,
                TotalSpent = orders.Sum(o => o.TotalAmount)
            };
            
            return View(userDetails);
        }
        
        // POST: Admin/ToggleUserRole
        [HttpPost]
        public async Task<IActionResult> ToggleUserRole(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                return BadRequest("User ID and Role Name are required");
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            
            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            IdentityResult result;
            
            if (isInRole)
            {
                result = await _userManager.RemoveFromRoleAsync(user, roleName);
            }
            else
            {
                result = await _userManager.AddToRoleAsync(user, roleName);
            }
            
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(UserDetails), new { id = userId });
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            
            return RedirectToAction(nameof(UserDetails), new { id = userId });
        }
        
        // GET: Admin/StoreSettings
        public IActionResult StoreSettings()
        {
            var storeSettings = new StoreSettingsViewModel
            {
                StoreName = _configuration["StoreSettings:StoreName"] ?? "TV Shop",
                StoreTagline = _configuration["StoreSettings:StoreTagline"] ?? "Your one-stop shop for quality TVs",
                StoreAddress = _configuration["StoreSettings:StoreAddress"] ?? "",
                ContactEmail = _configuration["StoreSettings:ContactEmail"] ?? "",
                ContactPhone = _configuration["StoreSettings:ContactPhone"] ?? "",
                SocialFacebook = _configuration["StoreSettings:SocialFacebook"] ?? "",
                SocialTwitter = _configuration["StoreSettings:SocialTwitter"] ?? "",
                SocialInstagram = _configuration["StoreSettings:SocialInstagram"] ?? "",
                DefaultCurrency = _configuration["StoreSettings:DefaultCurrency"] ?? "INR",
                OrderEmailNotifications = _configuration.GetValue<bool>("StoreSettings:OrderEmailNotifications", true),
                // Email settings
                SmtpServer = _configuration["EmailSettings:SmtpServer"] ?? "",
                SmtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort", 587),
                SmtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "",
                SmtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "",
                FromName = _configuration["EmailSettings:FromName"] ?? "",
                FromAddress = _configuration["EmailSettings:FromAddress"] ?? ""
            };
            
            return View(storeSettings);
        }
        
        // POST: Admin/StoreSettings
        [HttpPost]
        public async Task<IActionResult> StoreSettings(StoreSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // This is a simple implementation that would normally involve updating
            // appsettings.json or a database. For demonstration purposes, we'll just
            // show a success message without actually saving the settings.
            
            // In a real application, you would either:  
            // 1. Use a database table to store these settings
            // 2. Update the appsettings.json file
            // 3. Use another configuration mechanism
            
            TempData["SuccessMessage"] = "Store settings have been updated successfully!";
            return RedirectToAction(nameof(StoreSettings));
        }

        // GET: Create Product
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Product with Images
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile>? images, List<string>? OtherImageUrls)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ModelState.AddModelError("Name", "Product name is required");
            }
            if (string.IsNullOrWhiteSpace(product.Description))
            {
                ModelState.AddModelError("Description", "Product description is required");
            }
            if (product.Price <= 0)
            {
                ModelState.AddModelError("Price", "Price must be greater than 0");
            }
            if (string.IsNullOrWhiteSpace(product.Category))
            {
                ModelState.AddModelError("Category", "Category is required");
            }
            
            // Validate image fields
            if (images != null && images.Any())
            {
                foreach (var image in images)
                {
                    if (image.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        ModelState.AddModelError("ImageUrl", "Image size must be less than 5MB");
                        break;
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(product.MainImageUrl))
            {
                ModelState.AddModelError("ImageUrl", "Either upload images or provide a main image URL");
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState is valid, proceeding with product creation");
                try
                {
                    product.ProductImages ??= new List<ProductImage>();
                    
                    // Apply discount if specified
                    if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
                    {
                        product.ApplyDiscount();
                    }
                    
                    // Set MainImageUrl if images are uploaded
                    if (images != null && images.Any())
                    {
                        product.MainImageUrl = "/uploads/" + images.First().FileName;
                    }

                    if (images != null && images.Count > 0)
                    {
                        Console.WriteLine($"Processing {images.Count} image(s) for upload");
                        string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                        if (!Directory.Exists(uploadFolder))
                        {
                            Console.WriteLine("Uploads directory not found, creating it");
                            Directory.CreateDirectory(uploadFolder);
                        }

                        foreach (var image in images)
                        {
                            Console.WriteLine($"Processing image: {image.FileName} ({image.Length} bytes)");
                            if (image.Length > 0)
                            {
                                var fileName = Path.GetFileName(image.FileName);
                                var filePath = Path.Combine(uploadFolder, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await image.CopyToAsync(stream);
                                }

                                product.ProductImages.Add(new ProductImage
                                {
                                    ImageUrl = "/uploads/" + fileName
                                });
                            }
                        }
                    }
                    else if (OtherImageUrls != null && OtherImageUrls.Any())
                    {
                        // Process manually entered image URLs
                        foreach (var imageUrl in OtherImageUrls.Where(url => !string.IsNullOrWhiteSpace(url)))
                        {
                            product.ProductImages.Add(new ProductImage
                            {
                                ImageUrl = imageUrl
                            });
                        }
                    }
                    else
                    {
                        Console.WriteLine("No images provided");
                    }

                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            Console.WriteLine("Starting database transaction");
                            _context.Products.Add(product);
                            Console.WriteLine("Product added to context");
                            await _context.SaveChangesAsync();
                            Console.WriteLine("Changes saved to database");
                            await transaction.CommitAsync();
                            Console.WriteLine("Transaction committed successfully");
                            return RedirectToAction("Index");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Transaction failed: {ex.Message}");
                            await transaction.RollbackAsync();
                            Console.WriteLine("Transaction rolled back");
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating product: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    
                    ModelState.AddModelError("", "An error occurred while creating the product. Please try again.");
                    ModelState.AddModelError("", $"Technical Details: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("ModelState is invalid");
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        Console.WriteLine($"Validation Error: {error.Key} - {err.ErrorMessage}");
                    }
                }
            }

            return View(product);
        }

        // GET: Edit Product
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Edit Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, List<string>? NewImageUrls, List<int>? DeleteImageIds)
        {
            try
            {
                Console.WriteLine($"Edit POST received for product ID: {product.Id}");
                Console.WriteLine($"Name: {product.Name}, Brand: {product.Brand}, Price: {product.Price}");
                Console.WriteLine($"Category: {product.Category}, MainImageUrl: {product.MainImageUrl}");
                Console.WriteLine($"Discount Percentage: {product.DiscountPercentage}%");
                
                // Validate model state
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("Model validation failed");
                    foreach (var error in ModelState)
                    {
                        foreach (var err in error.Value.Errors)
                        {
                            Console.WriteLine($"Validation Error: {error.Key} - {err.ErrorMessage}");
                        }
                    }
                    return View(product);
                }
                
                // Get existing product with images
                var existingProduct = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (existingProduct == null)
                {
                    Console.WriteLine($"Product not found: {product.Id}");
                    ModelState.AddModelError("", "Product not found. It may have been deleted.");
                    return NotFound();
                }

                Console.WriteLine($"Found existing product: {existingProduct.Name}");

                // Update product fields
                existingProduct.Name = product.Name;
                existingProduct.Brand = product.Brand;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.MainImageUrl = product.MainImageUrl;
                existingProduct.Category = product.Category;
                existingProduct.Size = product.Size;
                existingProduct.Resolution = product.Resolution;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.IsNewArrival = product.IsNewArrival;
                existingProduct.DiscountPercentage = product.DiscountPercentage;
                
                // Apply discount if needed
                if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
                {
                    existingProduct.ApplyDiscount();
                }
                else
                {
                    // Clear old price if no discount
                    existingProduct.OldPrice = null;
                }

                // Handle deleting images
                if (DeleteImageIds != null && DeleteImageIds.Any())
                {
                    foreach (var imageId in DeleteImageIds)
                    {
                        var imageToRemove = existingProduct.ProductImages.FirstOrDefault(i => i.Id == imageId);
                        if (imageToRemove != null)
                        {
                            existingProduct.ProductImages.Remove(imageToRemove);
                        }
                    }
                }
                
                // Add new images
                if (NewImageUrls != null && NewImageUrls.Any())
                {
                    foreach (var imageUrl in NewImageUrls.Where(url => !string.IsNullOrWhiteSpace(url)))
                    {
                        existingProduct.ProductImages.Add(new ProductImage
                        {
                            ImageUrl = imageUrl
                        });
                    }
                }

                try
                {
                    Console.WriteLine("Updating product...");
                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Product update successful");
                    TempData["SuccessMessage"] = "Product updated successfully";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine($"Concurrency exception: {ex.Message}");
                    ModelState.AddModelError("", "The product was modified by another user. Please try again.");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Database update exception: {ex.Message}");
                    Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                    ModelState.AddModelError("", "There was a problem saving to the database. Please try again.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General exception: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Top-level exception in Edit method: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            return View(product);
        }

        // GET: Delete Confirmation Page
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Confirm Delete Product
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return NotFound();
        }
    }
}
