using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.ProductPages;

public class EditModel : PageModel
{
    private readonly InventoryDbContext _db;
    public EditModel(InventoryDbContext db) => _db = db;

    [BindProperty]
    public Product Product { get; set; } = null!;

    public SelectList CategoryList { get; private set; } = null!;
    public SelectList SupplierList { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        Product = product;
        await PopulateListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateListsAsync();
            return Page();
        }

        Product.LastUpdated = DateTime.UtcNow;
        _db.Attach(Product).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Products.AnyAsync(p => p.Id == Product.Id))
                return NotFound();
            throw;
        }

        TempData["SuccessMessage"] = $"{Product.Name} was updated successfully.";
        return RedirectToPage("Details", new { id = Product.Id });
    }

    private async Task PopulateListsAsync()
    {
        CategoryList = new SelectList(
            await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        SupplierList = new SelectList(
            await _db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
    }
}
