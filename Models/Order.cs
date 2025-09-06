using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace TvShop.Models
{

    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }
    
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string CustomerName { get; set; }

        [Required]
        public required string Address { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public required ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        [Required]
        public required string PaymentId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
    }
}
