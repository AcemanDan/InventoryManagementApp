using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class AdminProfile
    {
        public int Id { get; set; } = 1; // single row

        [Required, MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [MaxLength(100)]
        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [MaxLength(500)]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Not mapped — used only for password change form
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmPassword { get; set; }

        // Stored hash — never exposed to the view
        [MaxLength(256)]
        public string? PasswordHash { get; set; }

        // Computed helper
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
