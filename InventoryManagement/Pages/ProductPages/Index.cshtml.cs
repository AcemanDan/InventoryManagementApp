using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.ProductPages;

public class IndexModel : PageModel
{
    private readonly InventoryDbContext _db;
    private const int PageSize = 15;

    public IndexModel(InventoryDbContext db) => _db = db;

    // ── Query params ──
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int? SupplierId { get; set; }
    [BindProperty(SupportsGet = true)] public string? StockFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string SortBy { get; set; } = "name";
    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;

    // ── Output ──
    public List<Product> Products { get; private set; } = new();
    public List<SelectListItem> Categories { get; private set; } = new();
    public List<SelectListItem> Suppliers { get; private set; } = new();
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }

    public async Task OnGetAsync()
    {
        // To prevent optional query string parameters from triggering validation errors.
        ModelState.Clear();

        Categories = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();

        Suppliers = await _db.Suppliers
            .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToListAsync();

        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(Search))
            query = query.Where(p =>
                p.Name.Contains(Search) ||
                (p.SKU != null && p.SKU.Contains(Search)));

        // Category filter
        if (CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == CategoryId);

        // Supplier filter
        if (SupplierId.HasValue)
            query = query.Where(p => p.SupplierId == SupplierId);

        // Stock filter
        query = StockFilter switch
        {
            "low" => query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold),
            "zero" => query.Where(p => p.StockQuantity == 0),
            "ok" => query.Where(p => p.StockQuantity > p.LowStockThreshold),
            _ => query
        };

        // Sort
        query = SortBy switch
        {
            "stock" => query.OrderBy(p => p.StockQuantity),
            "price" => query.OrderByDescending(p => p.Price),
            "date" => query.OrderByDescending(p => p.DateAdded),
            _ => query.OrderBy(p => p.Name)
        };

        TotalCount = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
        CurrentPage = Math.Max(1, Math.Min(CurrentPage, Math.Max(1, TotalPages)));

        Products = await query
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }
}
