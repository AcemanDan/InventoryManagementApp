using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryManagement.Pages.Suppliers
{
    public class CreateModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public CreateModel(InventoryDbContext db) => _db = db;

        [BindProperty]
        public Supplier Supplier { get; set; } = new() { IsActive = true };

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _db.Suppliers.Add(Supplier);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{Supplier.Name}\" was added successfully.";
            return RedirectToPage("Index");
        }
    }
}
