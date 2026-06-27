using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.Suppliers
{
    public class DeleteModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public DeleteModel(InventoryDbContext db) => _db = db;

        [BindProperty]
        public Supplier Supplier { get; set; } = null!;

        public int ProductCount { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier is null) return NotFound();

            Supplier = supplier;
            ProductCount = await _db.Products
                .AsQueryable()
                .Where(p => p.SupplierId == id)
                .CountAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Re-check before deleting
            var count = await _db.Products
                .AsQueryable()
                .Where(p => p.SupplierId == Supplier.Id)
                .CountAsync();

            if (count > 0)
            {
                TempData["ErrorMessage"] = "Cannot delete a supplier that still has products assigned.";
                return RedirectToPage("Index");
            }

            var supplier = await _db.Suppliers.FindAsync(Supplier.Id);
            if (supplier is null) return NotFound();

            var name = supplier.Name;
            _db.Suppliers.Remove(supplier);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{name}\" was deleted.";
            return RedirectToPage("Index");
        }
    }
}
