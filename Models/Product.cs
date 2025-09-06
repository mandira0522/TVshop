using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TvShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        [Required]
        [StringLength(255)]
        public required string MainImageUrl { get; set; } // Stores the main product image URL

        [Required]
        [StringLength(100)]
        public required string Brand { get; set; } // Stores the brand name

        [Required]
        public required string Description { get; set; } // Detailed product description

        [Required]
        [Range(0.01, 1000000)]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Price { get; set; } // Product price with validation

        [Range(0.01, 1000000)]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? OldPrice { get; set; }

        [Range(0, 100)]
        public decimal? DiscountPercentage { get; set; } // Discount percentage for the product

        [StringLength(100)]
        public string? Category { get; set; } // Category of the product (e.g., LED, OLED, QLED)

        [NotMapped]
        public bool IsOnSale => OldPrice.HasValue && OldPrice.Value > Price;

        public bool IsNewArrival { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        [StringLength(50)]
        public string? Resolution { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>(); // List of extra images
        
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>(); // List of product ratings
        
        /// <summary>
        /// Calculates the final price after applying the discount percentage (if any)
        /// </summary>
        [NotMapped]
        public decimal FinalPrice 
        { 
            get
            {
                if (DiscountPercentage.HasValue && DiscountPercentage.Value > 0)
                {
                    // If we already applied the discount to Price and saved OldPrice, return Price
                    if (OldPrice.HasValue && OldPrice.Value > Price)
                        return Price;
                    
                    // Otherwise calculate the discount
                    decimal discount = (DiscountPercentage.Value / 100) * Price;
                    return Price - discount;
                }
                return Price;
            }
        }

        /// <summary>
        /// Updates the OldPrice based on the current price and applies discount
        /// </summary>
        public void ApplyDiscount()
        {
            if (DiscountPercentage.HasValue && DiscountPercentage.Value > 0)
            {
                // Store the original price as OldPrice if not already set
                if (!OldPrice.HasValue || OldPrice.Value <= Price)
                {
                    OldPrice = Price;
                }
                
                // Apply the discount to Price
                decimal discount = (DiscountPercentage.Value / 100) * OldPrice.Value;
                Price = OldPrice.Value - discount;
            }
        }
        
        /// <summary>
        /// Gets the safe image URL, returning a placeholder if the main image URL is empty
        /// </summary>
        [NotMapped]
        public string SafeMainImageUrl => string.IsNullOrEmpty(MainImageUrl) ? "/images/placeholder.jpg" : MainImageUrl;
        
        /// <summary>
        /// Gets the average rating value of the product
        /// </summary>
        [NotMapped]
        public double AverageRating => Ratings != null && Ratings.Any() ? Ratings.Average(r => r.Value) : 0;
        
        /// <summary>
        /// Gets the total number of ratings for the product
        /// </summary>
        [NotMapped]
        public int RatingCount => Ratings?.Count ?? 0;
    }
}
