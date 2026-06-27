using InventoryManagement.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.Categories
{
    public class CategorySummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProductCount { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public IndexModel(InventoryDbContext db) => _db = db;

        public List<CategorySummary> Categories { get; private set; } = new();

        public async Task OnGetAsync()
        {
            // Query 1: all categories
            var categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Query 2: active product counts grouped by category
            var countsByCategory = await _db.Products
                .Where(p => p.IsActive)
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync();

            var countMap = countsByCategory.ToDictionary(x => x.CategoryId, x => x.Count);

            // Join in memory
            Categories = categories.Select(c => new CategorySummary
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = countMap.GetValueOrDefault(c.Id, 0)
            }).ToList();
        }
    }
}
