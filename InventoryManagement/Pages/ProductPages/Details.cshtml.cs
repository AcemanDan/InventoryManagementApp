using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.ProductPages;

public class DetailsModel : PageModel
{
    private readonly InventoryDbContext _db;
    public DetailsModel(InventoryDbContext db) => _db = db;

    public Product Product { get; private set; } = null!;
    public List<StockTransaction> Transactions { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return NotFound();

        Product = product;

        Transactions = await _db.StockTransactions
            .Where(t => t.ProductId == id)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        return Page();
    }
}
