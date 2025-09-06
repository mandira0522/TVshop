using System;

namespace TvShop.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = "Pending"; // "Success", "Failed"
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string TransactionId { get; set; }
    }
}
