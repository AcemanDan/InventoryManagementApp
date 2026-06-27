using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.ProductPages;

public class CreateModel : PageModel
{
    private readonly InventoryDbContext _db;
    public CreateModel(InventoryDbContext db) => _db = db;

    [BindProperty]
    public Product Product { get; set; } = new() { IsActive = true, LowStockThreshold = 10 };

    public SelectList CategoryList { get; private set; } = null!;
    public SelectList SupplierList { get; private set; } = null!;

    public async Task OnGetAsync() => await PopulateListsAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateListsAsync();
            return Page();
        }

        Product.DateAdded = DateTime.UtcNow;
        Product.LastUpdated = DateTime.UtcNow;
        _db.Products.Add(Product);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{Product.Name} was added successfully.";
        return RedirectToPage("Index");
    }

    private async Task PopulateListsAsync()
    {
        CategoryList = new SelectList(
            await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        SupplierList = new SelectList(
            await _db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
    }
}
