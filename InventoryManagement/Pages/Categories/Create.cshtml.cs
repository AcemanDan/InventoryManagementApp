using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryManagement.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public CreateModel(InventoryDbContext db) => _db = db;

        [BindProperty]
        public Category Category { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _db.Categories.Add(Category);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category \"{Category.Name}\" was added.";
            return RedirectToPage("Index");
        }
    }
}
