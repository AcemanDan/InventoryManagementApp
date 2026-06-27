using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryManagement.Pages.ProductPages
{
    public class RestockModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public RestockModel(InventoryDbContext db) => _db = db;

        public Product Product { get; private set; } = null!;

        [BindProperty]
        public StockTransaction Transaction { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product is null) return NotFound();

            Product = product;
            Transaction = new StockTransaction { ProductId = id };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var product = await _db.Products.FindAsync(Transaction.ProductId);
            if (product is null) return NotFound();

            Product = product;

            if (!ModelState.IsValid)
                return Page();

            // Update stock (floor at zero)
            var newQty = Math.Max(0, product.StockQuantity + Transaction.QuantityChange);
            Transaction.StockAfter = newQty;
            Transaction.TransactionDate = DateTime.UtcNow;

            product.StockQuantity = newQty;
            product.LastUpdated = DateTime.UtcNow;

            _db.StockTransactions.Add(Transaction);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Stock updated. {product.Name} now has {newQty} unit{(newQty == 1 ? "" : "s")} in stock.";

            return RedirectToPage("Details", new { id = product.Id });
        }
    }
}
