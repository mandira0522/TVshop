using System.ComponentModel.DataAnnotations;

namespace TvShop.ViewModels
{
    public class StoreSettingsViewModel
    {
        [Required]
        [Display(Name = "Store Name")]
        public string StoreName { get; set; } = string.Empty;
        
        [Display(Name = "Store Tagline")]
        public string StoreTagline { get; set; } = string.Empty;
        
        [Display(Name = "Store Address")]
        public string StoreAddress { get; set; } = string.Empty;
        
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;
        
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = string.Empty;
        
        [Display(Name = "Facebook URL")]
        public string SocialFacebook { get; set; } = string.Empty;
        
        [Display(Name = "Twitter URL")]
        public string SocialTwitter { get; set; } = string.Empty;
        
        [Display(Name = "Instagram URL")]
        public string SocialInstagram { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Default Currency")]
        public string DefaultCurrency { get; set; } = "INR";
        
        [Display(Name = "Enable Order Email Notifications")]
        public bool OrderEmailNotifications { get; set; } = true;
        
        // Email Settings
        [Display(Name = "SMTP Server")]
        public string SmtpServer { get; set; } = string.Empty;
        
        [Display(Name = "SMTP Port")]
        public int SmtpPort { get; set; } = 587;
        
        [Display(Name = "SMTP Username")]
        public string SmtpUsername { get; set; } = string.Empty;
        
        [Display(Name = "SMTP Password")]
        [DataType(DataType.Password)]
        public string SmtpPassword { get; set; } = string.Empty;
        
        [Display(Name = "Email From Name")]
        public string FromName { get; set; } = string.Empty;
        
        [EmailAddress]
        [Display(Name = "Email From Address")]
        public string FromAddress { get; set; } = string.Empty;
    }
}
