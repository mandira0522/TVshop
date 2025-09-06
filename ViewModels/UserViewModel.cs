using TvShop.Models;
using System.Collections.Generic;

namespace TvShop.ViewModels
{
    public class UserViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
