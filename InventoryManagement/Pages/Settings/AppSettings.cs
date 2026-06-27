using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Pages.Settings
{
    public class AppSettings
    {
        public int Id { get; set; } = 1; // always a single row

        [Required, MaxLength(150)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = "My Company";

        [MaxLength(300)]
        [Display(Name = "Company Address")]
        public string? CompanyAddress { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [Display(Name = "Default Low Stock Threshold")]
        [Range(1, 10000)]
        public int DefaultLowStockThreshold { get; set; } = 10;

        [Display(Name = "Products Per Page")]
        [Range(5, 100)]
        public int ProductsPerPage { get; set; } = 15;

        [Required, MaxLength(10)]
        [Display(Name = "Currency Symbol")]
        public string CurrencySymbol { get; set; } = "$";

        [Required, MaxLength(10)]
        [Display(Name = "Currency Code")]
        public string CurrencyCode { get; set; } = "USD";

        [Display(Name = "Show Low Stock Alerts on Dashboard")]
        public bool ShowLowStockAlerts { get; set; } = true;

        [Display(Name = "Show Inactive Products")]
        public bool ShowInactiveProducts { get; set; } = false;

        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
