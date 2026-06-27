using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InventoryManagement.Pages.Reports
{
    public class ProductActivityRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public int UnitsIn { get; set; }
        public int UnitsOut { get; set; }
        public int NetChange { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public IndexModel(InventoryDbContext db) => _db = db;

        // ── Date range ────────────────────────────────────────────────────────
        [BindProperty(SupportsGet = true)] public DateTime DateFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime DateTo { get; set; }

        // ── KPIs ──────────────────────────────────────────────────────────────
        public decimal InventoryValue { get; private set; }
        public int UnitsSold { get; private set; }
        public int UnitsRestocked { get; private set; }
        public int LowStockCount { get; private set; }

        // ── Tables ────────────────────────────────────────────────────────────
        public List<Product> LowStockProducts { get; private set; } = new();
        public List<ProductActivityRow> ProductActivity { get; private set; } = new();

        // ── Chart JSON ────────────────────────────────────────────────────────
        public string ActivityLabelsJson { get; private set; } = "[]";
        public string ActivityInJson { get; private set; } = "[]";
        public string ActivityOutJson { get; private set; } = "[]";
        public string TypeLabelsJson { get; private set; } = "[]";
        public string TypeCountsJson { get; private set; } = "[]";
        public string CategoryLabelsJson { get; private set; } = "[]";
        public string CategoryValuesJson { get; private set; } = "[]";
        public string TopProductLabelsJson { get; private set; } = "[]";
        public string TopProductValuesJson { get; private set; } = "[]";

        public async Task OnGetAsync()
        {
            ModelState.Clear();

            // Default: last 90 days
            if (DateFrom == default) DateFrom = DateTime.UtcNow.AddDays(-90).Date;
            if (DateTo == default) DateTo = DateTime.UtcNow.Date;

            var periodEnd = DateTo.AddDays(1);

            // ── KPIs ──────────────────────────────────────────────────────────
            InventoryValue = await _db.Products
                .AsQueryable()
                .Where(p => p.IsActive)
                .SumAsync(p => p.StockQuantity * p.Price);

            var periodTx = await _db.StockTransactions
                .AsQueryable()
                .Where(t => t.TransactionDate >= DateFrom && t.TransactionDate < periodEnd)
                .ToListAsync();

            UnitsSold = periodTx.Where(t => t.QuantityChange < 0).Sum(t => Math.Abs(t.QuantityChange));
            UnitsRestocked = periodTx.Where(t => t.Type == TransactionType.Restock).Sum(t => t.QuantityChange);

            LowStockCount = await _db.Products
                .AsQueryable()
                .CountAsync(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold);

            // ── Low stock table ───────────────────────────────────────────────
            LowStockProducts = await _db.Products
                .AsQueryable()
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            // ── Product activity table ────────────────────────────────────────
            var activityRaw = periodTx
                .GroupBy(t => t.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TransactionCount = g.Count(),
                    UnitsIn = g.Where(t => t.QuantityChange > 0).Sum(t => t.QuantityChange),
                    UnitsOut = g.Where(t => t.QuantityChange < 0).Sum(t => t.QuantityChange)
                })
                .OrderByDescending(x => x.TransactionCount)
                .Take(10)
                .ToList();

            var productIds = activityRaw.Select(x => x.ProductId).ToList();
            var productNames = await _db.Products
                .AsQueryable()
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            var nameMap = productNames.ToDictionary(p => p.Id, p => p.Name);

            ProductActivity = activityRaw.Select(x => new ProductActivityRow
            {
                ProductId = x.ProductId,
                ProductName = nameMap.GetValueOrDefault(x.ProductId, "Unknown"),
                TransactionCount = x.TransactionCount,
                UnitsIn = x.UnitsIn,
                UnitsOut = Math.Abs(x.UnitsOut),
                NetChange = x.UnitsIn + x.UnitsOut
            }).ToList();

            // ── Activity chart: group by week ─────────────────────────────────
            var weeks = periodTx
                .GroupBy(t => t.TransactionDate.Date.AddDays(
                    -(int)t.TransactionDate.DayOfWeek))   // week start (Sunday)
                .OrderBy(g => g.Key)
                .ToList();

            ActivityLabelsJson = JsonSerializer.Serialize(
                weeks.Select(w => w.Key.ToString("MMM d")));
            ActivityInJson = JsonSerializer.Serialize(
                weeks.Select(w => w.Where(t => t.QuantityChange > 0).Sum(t => t.QuantityChange)));
            ActivityOutJson = JsonSerializer.Serialize(
                weeks.Select(w => w.Where(t => t.QuantityChange < 0).Sum(t => Math.Abs(t.QuantityChange))));

            // ── Type breakdown doughnut ───────────────────────────────────────
            var typeCounts = periodTx
                .GroupBy(t => t.Type.ToString())
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            TypeLabelsJson = JsonSerializer.Serialize(typeCounts.Select(x => x.Type));
            TypeCountsJson = JsonSerializer.Serialize(typeCounts.Select(x => x.Count));

            // ── Inventory value by category ───────────────────────────────────
            var catValues = await _db.Categories
                .AsQueryable()
                .Select(c => new
                {
                    c.Name,
                    Value = c.Products
                        .Where(p => p.IsActive)
                        .Sum(p => (decimal?)(p.StockQuantity * p.Price)) ?? 0m
                })
                .Where(x => x.Value > 0)
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            CategoryLabelsJson = JsonSerializer.Serialize(catValues.Select(x => x.Name));
            CategoryValuesJson = JsonSerializer.Serialize(catValues.Select(x => Math.Round(x.Value, 2)));

            // ── Top 5 products by value ───────────────────────────────────────
            var topProducts = await _db.Products
                .AsQueryable()
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Name,
                    Value = p.StockQuantity * p.Price
                })
                .OrderByDescending(p => p.Value)
                .Take(5)
                .ToListAsync();

            TopProductLabelsJson = JsonSerializer.Serialize(topProducts.Select(x => x.Name));
            TopProductValuesJson = JsonSerializer.Serialize(topProducts.Select(x => Math.Round(x.Value, 2)));
        }
    }
}
