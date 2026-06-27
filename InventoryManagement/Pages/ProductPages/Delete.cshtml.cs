using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.ProductPages;

public class DeleteModel : PageModel
{
    private readonly InventoryDbContext _db;
    public DeleteModel(InventoryDbContext db) => _db = db;

    [BindProperty]
    public Product Product { get; set; } = null!;
    public int TransactionCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return NotFound();

        Product = product;
        TransactionCount = await _db.StockTransactions.CountAsync(t => t.ProductId == id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var product = await _db.Products.FindAsync(Product.Id);
        if (product is null) return NotFound();

        var name = product.Name;
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{name} was deleted.";
        return RedirectToPage("Index");
    }
}
