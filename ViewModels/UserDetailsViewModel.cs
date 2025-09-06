using TvShop.Models;
using System.Collections.Generic;

namespace TvShop.ViewModels
{
    public class UserDetailsViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
