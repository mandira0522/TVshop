using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TvShop.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        [Required]
        [StringLength(255)]
        public required string ImageUrl { get; set; }

        // OtherImageUrl is now the main storage field for images
        public string? OtherImageUrl 
        { 
            get => ImageUrl; 
            set => ImageUrl = value ?? string.Empty; 
        }

        public bool IsPrimary { get; set; } = false;
    }
}
