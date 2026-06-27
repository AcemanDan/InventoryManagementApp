using InventoryManagement.Models;
using InventoryManagement.Pages.Settings;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
        public DbSet<AppSettings> AppSettings => Set<AppSettings>();
        public DbSet<AdminProfile> AdminProfiles => Set<AdminProfile>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product → Category (required)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product → Supplier (optional)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            // StockTransaction → Product
            modelBuilder.Entity<StockTransaction>()
                .HasOne(t => t.Product)
                .WithMany(p => p.StockTransactions)
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique SKU (nullable filtered index)
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique()
                .HasFilter("[SKU] IS NOT NULL");

            // Enum stored as string
            modelBuilder.Entity<StockTransaction>()
                .Property(t => t.Type)
                .HasConversion<string>();

            // AppSettings — enforce single row
            modelBuilder.Entity<AppSettings>()
                .HasData(new AppSettings
                {
                    Id = 1,
                    CompanyName = "My Company",
                    DefaultLowStockThreshold = 10,
                    ProductsPerPage = 15,
                    CurrencySymbol = "$",
                    CurrencyCode = "USD",
                    ShowLowStockAlerts = true,
                    ShowInactiveProducts = false,
                    LastUpdated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
        }
    }
}
