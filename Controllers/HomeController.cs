using Microsoft.AspNetCore.Mvc;
using TvShop.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TvShop.Models;

namespace TvShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Get regular products (for featured section)
            var products = _context.Products
                .Include(p => p.ProductImages)
                .ToList();
            
            // Get products with discounts, ordered by highest discount first
            var discountedProducts = _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.DiscountPercentage != null && p.DiscountPercentage > 0)
                .OrderByDescending(p => p.DiscountPercentage)
                .ToList();
                
            ViewBag.DiscountedProducts = discountedProducts;
            
            return View(products);
        }

        public IActionResult Search(string query)
        {
            var results = _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.Name.Contains(query) || p.Brand.Contains(query))
                .ToList();

            if (results.Count == 0)
            {
                TempData["Message"] = "No products found.";
            }

            return View("Index", results);
        }
        
        public IActionResult About()
        {
            return View();
        }
    }
}