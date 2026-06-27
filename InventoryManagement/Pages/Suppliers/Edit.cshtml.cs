using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.Suppliers
{
    public class EditModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public EditModel(InventoryDbContext db) => _db = db;

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
            if (!ModelState.IsValid)
            {
                ProductCount = await _db.Products
                    .AsQueryable()
                    .Where(p => p.SupplierId == Supplier.Id)
                    .CountAsync();
                return Page();
            }

            _db.Attach(Supplier).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Suppliers.AsQueryable().AnyAsync(s => s.Id == Supplier.Id))
                    return NotFound();
                throw;
            }

            TempData["SuccessMessage"] = $"\"{Supplier.Name}\" was updated successfully.";
            return RedirectToPage("Index");
        }
    }
}
