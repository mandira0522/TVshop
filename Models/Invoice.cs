using System;

namespace TvShop.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public string UserId { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public string InvoiceNumber { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
