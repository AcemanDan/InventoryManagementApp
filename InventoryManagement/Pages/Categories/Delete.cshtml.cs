using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public DeleteModel(InventoryDbContext db) => _db = db;

        [BindProperty]
        public Category Category { get; set; } = null!;

        public int ProductCount { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category is null) return NotFound();

            Category = category;
            ProductCount = await _db.Products
                .AsQueryable()
                .Where(p => p.CategoryId == id)
                .CountAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var count = await _db.Products
                .AsQueryable()
                .Where(p => p.CategoryId == Category.Id)
                .CountAsync();

            if (count > 0)
            {
                TempData["ErrorMessage"] = "Cannot delete a category that still has products assigned.";
                return RedirectToPage("Index");
            }

            var category = await _db.Categories.FindAsync(Category.Id);
            if (category is null) return NotFound();

            var name = category.Name;
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category \"{name}\" was deleted.";
            return RedirectToPage("Index");
        }
    }
}
