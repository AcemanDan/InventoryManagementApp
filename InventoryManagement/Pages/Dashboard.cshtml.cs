using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InventoryManagement.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public DashboardModel(InventoryDbContext db) => _db = db;

        // ── Summary card values ───────────────────────────────────────────────
        public int TotalProducts { get; private set; }
        public int TotalStock { get; private set; }
        public int LowStockCount { get; private set; }
        public int ActiveSuppliers { get; private set; }

        // ── Low stock table ───────────────────────────────────────────────────
        public List<Product> LowStockProducts { get; private set; } = new();

        // ── Chart.js JSON payloads ────────────────────────────────────────────
        public string CategoryLabelsJson { get; private set; } = "[]";
        public string CategoryStockJson { get; private set; } = "[]";
        public string CategoryProductCountJson { get; private set; } = "[]";

        public async Task OnGetAsync()
        {
            TotalProducts = await _db.Products.CountAsync(p => p.IsActive);
            TotalStock = await _db.Products.Where(p => p.IsActive).SumAsync(p => p.StockQuantity);
            LowStockCount = await _db.Products.CountAsync(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold);
            ActiveSuppliers = await _db.Suppliers.CountAsync(s => s.IsActive);

            LowStockProducts = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            // Per-category aggregation for charts
            var categoryStats = await _db.Categories
                .Select(c => new
                {
                    c.Name,
                    TotalStock = c.Products.Where(p => p.IsActive).Sum(p => (int?)p.StockQuantity) ?? 0,
                    ProductCount = c.Products.Count(p => p.IsActive)
                })
                .Where(x => x.ProductCount > 0)
                .OrderBy(x => x.Name)
                .ToListAsync();

            CategoryLabelsJson = JsonSerializer.Serialize(categoryStats.Select(x => x.Name));
            CategoryStockJson = JsonSerializer.Serialize(categoryStats.Select(x => x.TotalStock));
            CategoryProductCountJson = JsonSerializer.Serialize(categoryStats.Select(x => x.ProductCount));
        }
    }
}
