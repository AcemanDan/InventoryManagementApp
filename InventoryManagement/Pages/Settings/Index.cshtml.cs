using InventoryManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.Settings
{
    public class DbStats
    {
        public int ProductCount { get; set; }
        public int CategoryCount { get; set; }
        public int SupplierCount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public IndexModel(InventoryDbContext db) => _db = db;

        [BindProperty]
        public AppSettings Settings { get; set; } = null!;

        public DbStats Stats { get; private set; } = new();

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadStatsAsync();
                return Page();
            }

            Settings.LastUpdated = DateTime.UtcNow;
            _db.Attach(Settings).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Settings saved successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostClearTransactionsAsync()
        {
            var all = await _db.StockTransactions.AsQueryable().ToListAsync();
            _db.StockTransactions.RemoveRange(all);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{all.Count} transaction(s) deleted.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetDatabaseAsync()
        {
            // Wipe all data in dependency order
            _db.StockTransactions.RemoveRange(
                await _db.StockTransactions.AsQueryable().ToListAsync());
            await _db.SaveChangesAsync();

            _db.Products.RemoveRange(
                await _db.Products.AsQueryable().ToListAsync());
            await _db.SaveChangesAsync();

            _db.Suppliers.RemoveRange(
                await _db.Suppliers.AsQueryable().ToListAsync());
            _db.Categories.RemoveRange(
                await _db.Categories.AsQueryable().ToListAsync());
            await _db.SaveChangesAsync();

            // Re-seed
            await DbInitializer.SeedAsync(_db);

            TempData["SuccessMessage"] = "Database reset to seed data successfully.";
            return RedirectToPage();
        }

        // ── Helpers ──
        private async Task LoadAsync()
        {
            Settings = await _db.AppSettings
                           .AsQueryable()
                           .FirstOrDefaultAsync()
                       ?? new AppSettings();

            await LoadStatsAsync();
        }

        private async Task LoadStatsAsync()
        {
            Stats = new DbStats
            {
                ProductCount = await _db.Products.AsQueryable().CountAsync(),
                CategoryCount = await _db.Categories.AsQueryable().CountAsync(),
                SupplierCount = await _db.Suppliers.AsQueryable().CountAsync(),
                TransactionCount = await _db.StockTransactions.AsQueryable().CountAsync()
            };
        }
    }
}
