using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace TvShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        
        [StringLength(200)]
        public string? ProfileImageUrl { get; set; }
        
        [StringLength(500)]
        public string? Bio { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        
        [StringLength(50)]
        public string? Location { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; } = DateTime.Now;
        
        // Helper property to get full name
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        // Helper property to get profile image or default avatar
        public string ProfileImage => string.IsNullOrEmpty(ProfileImageUrl) ? "/images/default-avatar.svg" : ProfileImageUrl;
    }
}
