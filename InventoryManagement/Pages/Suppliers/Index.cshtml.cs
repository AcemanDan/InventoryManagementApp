using InventoryManagement.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages.Suppliers
{
    public class SupplierSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly InventoryDbContext _db;
        public IndexModel(InventoryDbContext db) => _db = db;

        public List<SupplierSummary> Suppliers { get; private set; } = new();

        public async Task OnGetAsync()
        {
            // To prevent optional query string parameters from triggering validation errors.
            ModelState.Clear();

            var suppliers = await _db.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();

            var countsBySupplier = await _db.Products
                .Where(p => p.SupplierId != null)
                .AsQueryable()
                .GroupBy(p => p.SupplierId)
                .Select(g => new { SupplierId = g.Key, Count = g.Count() })
                .ToListAsync();

            var countMap = countsBySupplier
                .ToDictionary(x => x.SupplierId!.Value, x => x.Count);

            Suppliers = suppliers.Select(s => new SupplierSummary
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                IsActive = s.IsActive,
                ProductCount = countMap.GetValueOrDefault(s.Id, 0)
            }).ToList();
        }
    }
}
