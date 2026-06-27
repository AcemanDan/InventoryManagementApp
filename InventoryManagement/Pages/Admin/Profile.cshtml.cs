using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace InventoryManagement.Pages.Admin
{
    public class ProfileModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public ProfileModel(InventoryDbContext db) => _db = db;

        [BindProperty]
        public AdminProfile Profile { get; set; } = null!;

        public async Task OnGetAsync()
        {
            await LoadProfileAsync();
        }

        // ── Save personal info ──
        public async Task<IActionResult> OnPostProfileAsync()
        {
            // Only validate profile fields, not password fields
            ModelState.Remove(nameof(Profile.CurrentPassword));
            ModelState.Remove(nameof(Profile.NewPassword));
            ModelState.Remove(nameof(Profile.ConfirmPassword));

            if (!ModelState.IsValid)
                return Page();

            var existing = await _db.AdminProfiles.AsQueryable().FirstOrDefaultAsync();
            if (existing is null) return NotFound();

            existing.FirstName = Profile.FirstName;
            existing.LastName = Profile.LastName;
            existing.Email = Profile.Email;
            existing.Phone = Profile.Phone;
            existing.JobTitle = Profile.JobTitle;
            existing.Bio = Profile.Bio;
            existing.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToPage();
        }

        // ── Change password ──
        public async Task<IActionResult> OnPostPasswordAsync()
        {
            // Only validate password fields
            ModelState.Remove(nameof(Profile.FirstName));
            ModelState.Remove(nameof(Profile.LastName));
            ModelState.Remove(nameof(Profile.Email));

            var existing = await _db.AdminProfiles.AsQueryable().FirstOrDefaultAsync();
            if (existing is null) return NotFound();

            // If a password is already set, verify the current one
            if (!string.IsNullOrEmpty(existing.PasswordHash))
            {
                if (string.IsNullOrEmpty(Profile.CurrentPassword) ||
                    HashPassword(Profile.CurrentPassword) != existing.PasswordHash)
                {
                    TempData["PasswordError"] = "Current password is incorrect.";
                    await LoadProfileAsync();
                    return Page();
                }
            }

            if (string.IsNullOrEmpty(Profile.NewPassword) ||
                Profile.NewPassword.Length < 8)
            {
                TempData["PasswordError"] = "New password must be at least 8 characters.";
                await LoadProfileAsync();
                return Page();
            }

            if (Profile.NewPassword != Profile.ConfirmPassword)
            {
                TempData["PasswordError"] = "Passwords do not match.";
                await LoadProfileAsync();
                return Page();
            }

            existing.PasswordHash = HashPassword(Profile.NewPassword);
            existing.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password updated successfully.";
            return RedirectToPage();
        }

        // ── Helpers ──
        private async Task LoadProfileAsync()
        {
            Profile = await _db.AdminProfiles.AsQueryable().FirstOrDefaultAsync()
                      ?? new AdminProfile
                      {
                          FirstName = "Admin",
                          LastName = "User",
                          Email = "admin@example.com"
                      };
        }

        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
