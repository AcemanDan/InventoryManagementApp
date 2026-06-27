using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public EditModel(InventoryDbContext db) => _db = db;

        [BindProperty]
        public Category Category { get; set; } = null!;

        public int ProductCount { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category is null) return NotFound();

            Category = category;
            ProductCount = await _db.Products.CountAsync(p => p.CategoryId == id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ProductCount = await _db.Products.CountAsync(p => p.CategoryId == Category.Id);
                return Page();
            }

            _db.Attach(Category).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Categories.AnyAsync(c => c.Id == Category.Id))
                    return NotFound();
                throw;
            }

            TempData["SuccessMessage"] = $"Category \"{Category.Name}\" was updated.";
            return RedirectToPage("Index");
        }
    }
}
