using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly InventoryDbContext _db;
        private const int PageSize = 20;

        public IndexModel(InventoryDbContext db) => _db = db;

        // ── Query params ──────────────────────────────────────────────────────
        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public string? TypeFilter { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;

        // ── Output ────────────────────────────────────────────────────────────
        public List<StockTransaction> Transactions { get; private set; } = new();
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalAdded { get; private set; }
        public int TotalRemoved { get; private set; }

        public async Task OnGetAsync()
        {
            ModelState.Clear();

            var query = _db.StockTransactions
                .Include(t => t.Product)
                .AsQueryable();

            // Search by product name or SKU
            if (!string.IsNullOrWhiteSpace(Search))
                query = query.Where(t =>
                    t.Product.Name.Contains(Search) ||
                    (t.Product.SKU != null && t.Product.SKU.Contains(Search)));

            // Transaction type filter
            if (!string.IsNullOrWhiteSpace(TypeFilter) &&
                Enum.TryParse<TransactionType>(TypeFilter, out var parsedType))
                query = query.Where(t => t.Type == parsedType);

            // Date range
            if (DateFrom.HasValue)
                query = query.Where(t => t.TransactionDate >= DateFrom.Value);

            if (DateTo.HasValue)
                query = query.Where(t => t.TransactionDate < DateTo.Value.AddDays(1));

            // Summary stats across the filtered set
            var stats = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Added = g.Where(t => t.QuantityChange > 0).Sum(t => (int?)t.QuantityChange) ?? 0,
                    Removed = g.Where(t => t.QuantityChange < 0).Sum(t => (int?)t.QuantityChange) ?? 0
                })
                .FirstOrDefaultAsync();

            TotalCount = stats?.Total ?? 0;
            TotalAdded = stats?.Added ?? 0;
            TotalRemoved = stats?.Removed ?? 0;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(CurrentPage, Math.Max(1, TotalPages)));

            Transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
