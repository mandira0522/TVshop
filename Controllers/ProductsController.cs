using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TvShop.Models;
using TvShop.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace TvShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, string category, string brand)
        {
            var products = _context.Products
                .Include(p => p.ProductImages)
                .Select(p => new
                {
                    Product = p,
                    // Calculated properties will be projected from the model
                    FinalPrice = p.FinalPrice
                })
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                products = products.Where(p => p.Product.Name.ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Product.Category == category);
            }

            if (!string.IsNullOrEmpty(brand))
            {
                products = products.Where(p => p.Product.Brand == brand);
            }

            var productList = products.ToList().Select(p => p.Product).ToList();

            if (!productList.Any())
            {
                ViewBag.Message = "No products found.";
            }

            ViewBag.Categories = _context.Products
                                        .Select(p => p.Category)
                                        .Distinct()
                                        .OrderBy(c => c)
                                        .ToList();

            ViewBag.Brands = _context.Products
                                   .Select(p => p.Brand)
                                   .Distinct()
                                   .OrderBy(b => b)
                                   .ToList();

            return View(productList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Ratings)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Ensure we have ImageUrl values for all product images
            foreach (var image in product.ProductImages)
            {
                if (string.IsNullOrEmpty(image.ImageUrl))
                {
                    // Set a default image if ImageUrl is empty
                    image.ImageUrl = "/images/placeholder.jpg";
                }
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,MainImageUrl,Brand,Description,Price,Category,DiscountPercentage")] Product product, List<string>? OtherImageUrls)
        {
            // Ensure MainImageUrl starts with /uploads/
            if (!string.IsNullOrEmpty(product.MainImageUrl) && !product.MainImageUrl.StartsWith("/uploads/"))
            {
                product.MainImageUrl = "/uploads/" + product.MainImageUrl.TrimStart('/');
            }

            // Apply discount if specified
            if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
            {
                // Store the original price before applying discount
                product.OldPrice = product.Price;
                product.ApplyDiscount();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Products
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                    
                ViewBag.Brands = _context.Products
                    .Select(p => p.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToList();
                    
                return View(product);
            }

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (OtherImageUrls != null && OtherImageUrls.Any())
                {
                    foreach (var url in OtherImageUrls)
                    {
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            var formattedUrl = url;
                            if (!formattedUrl.StartsWith("/uploads/"))
                            {
                                formattedUrl = "/uploads/" + formattedUrl.TrimStart('/');
                            }

                            var productImage = new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = formattedUrl
                            };

                            _context.ProductImages.Add(productImage);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Log the error
                Console.WriteLine($"Error occurred while creating product: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return View(product);
            }
        }
    }
}
