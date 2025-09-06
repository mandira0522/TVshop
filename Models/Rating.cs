using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TvShop.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Value { get; set; }
        
        [StringLength(500)]
        public string? Comment { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
