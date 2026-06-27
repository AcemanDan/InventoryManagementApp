using InventoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(InventoryDbContext context)
        {
            await context.Database.MigrateAsync();

            // ---------- Categories ----------
            if (!await context.Categories.AsQueryable().AnyAsync())
            {
                context.Categories.AddRange(
                    new() { Name = "Electronics", Description = "Gadgets and electronic components" },
                    new() { Name = "Office Supplies", Description = "Stationery and desk accessories" },
                    new() { Name = "Furniture", Description = "Desks, chairs, and shelving" },
                    new() { Name = "Consumables", Description = "Items used and replaced regularly" }
                );
                await context.SaveChangesAsync();
            }

            // ---------- Suppliers ----------
            if (!await context.Suppliers.AsQueryable().AnyAsync())
            {
                context.Suppliers.AddRange(
                    new() { Name = "TechSource Inc.", Email = "orders@techsource.example", Phone = "555-0100", IsActive = true },
                    new() { Name = "OfficeWorld Co.", Email = "supply@officeworld.example", Phone = "555-0200", IsActive = true },
                    new() { Name = "FurniturePlus Ltd.", Email = "sales@furniplus.example", Phone = "555-0300", IsActive = true }
                );
                await context.SaveChangesAsync();
            }

            // ---------- Products ----------
            if (!await context.Products.AsQueryable().AnyAsync())
            {
                var elec = await context.Categories.AsQueryable().FirstAsync(c => c.Name == "Electronics");
                var off = await context.Categories.AsQueryable().FirstAsync(c => c.Name == "Office Supplies");
                var furn = await context.Categories.AsQueryable().FirstAsync(c => c.Name == "Furniture");
                var cons = await context.Categories.AsQueryable().FirstAsync(c => c.Name == "Consumables");

                var tech = await context.Suppliers.AsQueryable().FirstAsync(s => s.Name == "TechSource Inc.");
                var ow = await context.Suppliers.AsQueryable().FirstAsync(s => s.Name == "OfficeWorld Co.");
                var fp = await context.Suppliers.AsQueryable().FirstAsync(s => s.Name == "FurniturePlus Ltd.");

                context.Products.AddRange(
                    new() { Name = "Wireless Keyboard", SKU = "ELEC-001", Price = 49.99m, StockQuantity = 35, LowStockThreshold = 10, CategoryId = elec.Id, SupplierId = tech.Id },
                    new() { Name = "USB-C Hub 7-Port", SKU = "ELEC-002", Price = 34.99m, StockQuantity = 8, LowStockThreshold = 10, CategoryId = elec.Id, SupplierId = tech.Id },
                    new() { Name = "27\" Monitor", SKU = "ELEC-003", Price = 299.99m, StockQuantity = 12, LowStockThreshold = 5, CategoryId = elec.Id, SupplierId = tech.Id },
                    new() { Name = "Ballpoint Pens x20", SKU = "OFFC-001", Price = 5.99m, StockQuantity = 200, LowStockThreshold = 50, CategoryId = off.Id, SupplierId = ow.Id },
                    new() { Name = "Legal Pads x5", SKU = "OFFC-002", Price = 9.49m, StockQuantity = 4, LowStockThreshold = 20, CategoryId = off.Id, SupplierId = ow.Id },
                    new() { Name = "Ergonomic Chair", SKU = "FURN-001", Price = 449.00m, StockQuantity = 7, LowStockThreshold = 3, CategoryId = furn.Id, SupplierId = fp.Id },
                    new() { Name = "Standing Desk", SKU = "FURN-002", Price = 699.00m, StockQuantity = 2, LowStockThreshold = 2, CategoryId = furn.Id, SupplierId = fp.Id },
                    new() { Name = "AA Batteries x10", SKU = "CONS-001", Price = 8.99m, StockQuantity = 150, LowStockThreshold = 30, CategoryId = cons.Id, SupplierId = null },
                    new() { Name = "Printer Paper Ream", SKU = "CONS-002", Price = 12.99m, StockQuantity = 9, LowStockThreshold = 15, CategoryId = cons.Id, SupplierId = ow.Id }
                );
                await context.SaveChangesAsync();
            }

            // ---------- Stock Transactions ----------
            if (!await context.StockTransactions.AsQueryable().AnyAsync())
            {
                var kbd = await context.Products.AsQueryable().FirstAsync(p => p.SKU == "ELEC-001");
                var hub = await context.Products.AsQueryable().FirstAsync(p => p.SKU == "ELEC-002");
                var mon = await context.Products.AsQueryable().FirstAsync(p => p.SKU == "ELEC-003");
                var pens = await context.Products.AsQueryable().FirstAsync(p => p.SKU == "OFFC-001");
                var pads = await context.Products.AsQueryable().FirstAsync(p => p.SKU == "OFFC-002");
                var chair = await context.Products.AsQueryable().FirstAsync(p => p.SKU == "FURN-001");
                var desk = await context.Products.AsQueryable().FirstAsync(p => p.SKU == "FURN-002");
                var batt = await context.Products.AsQueryable().FirstAsync(p => p.SKU == "CONS-001");
                var paper = await context.Products.AsQueryable().FirstAsync(p => p.SKU == "CONS-002");

                var now = DateTime.UtcNow;

                context.StockTransactions.AddRange(

                    // ── Wireless Keyboard ────────────────────────────────────
                    new() { ProductId = kbd.Id, Type = TransactionType.Restock, QuantityChange = 50, StockAfter = 50, Reference = "PO-2024-001", Notes = "Initial stock order", TransactionDate = now.AddDays(-90) },
                    new() { ProductId = kbd.Id, Type = TransactionType.Sale, QuantityChange = -10, StockAfter = 40, Reference = "ORD-1001", Notes = "Bulk order — marketing dept", TransactionDate = now.AddDays(-60) },
                    new() { ProductId = kbd.Id, Type = TransactionType.Sale, QuantityChange = -5, StockAfter = 35, Reference = "ORD-1045", Notes = null, TransactionDate = now.AddDays(-14) },

                    // ── USB-C Hub ────────────────────────────────────────────
                    new() { ProductId = hub.Id, Type = TransactionType.Restock, QuantityChange = 30, StockAfter = 30, Reference = "PO-2024-002", Notes = "Initial stock order", TransactionDate = now.AddDays(-90) },
                    new() { ProductId = hub.Id, Type = TransactionType.Sale, QuantityChange = -12, StockAfter = 18, Reference = "ORD-1012", Notes = null, TransactionDate = now.AddDays(-45) },
                    new() { ProductId = hub.Id, Type = TransactionType.Sale, QuantityChange = -10, StockAfter = 8, Reference = "ORD-1078", Notes = "Running low — reorder soon", TransactionDate = now.AddDays(-7) },

                    // ── 27" Monitor ──────────────────────────────────────────
                    new() { ProductId = mon.Id, Type = TransactionType.Restock, QuantityChange = 15, StockAfter = 15, Reference = "PO-2024-003", Notes = "Initial stock order", TransactionDate = now.AddDays(-80) },
                    new() { ProductId = mon.Id, Type = TransactionType.Sale, QuantityChange = -2, StockAfter = 13, Reference = "ORD-1020", Notes = null, TransactionDate = now.AddDays(-50) },
                    new() { ProductId = mon.Id, Type = TransactionType.Return, QuantityChange = 1, StockAfter = 14, Reference = "RET-0045", Notes = "Customer return — unopened", TransactionDate = now.AddDays(-30) },
                    new() { ProductId = mon.Id, Type = TransactionType.Sale, QuantityChange = -2, StockAfter = 12, Reference = "ORD-1099", Notes = null, TransactionDate = now.AddDays(-5) },

                    // ── Ballpoint Pens ───────────────────────────────────────
                    new() { ProductId = pens.Id, Type = TransactionType.Restock, QuantityChange = 300, StockAfter = 300, Reference = "PO-2024-010", Notes = "Bulk order", TransactionDate = now.AddDays(-120) },
                    new() { ProductId = pens.Id, Type = TransactionType.Sale, QuantityChange = -50, StockAfter = 250, Reference = "ORD-1005", Notes = "Office restock — HR dept", TransactionDate = now.AddDays(-90) },
                    new() { ProductId = pens.Id, Type = TransactionType.Sale, QuantityChange = -50, StockAfter = 200, Reference = "ORD-1055", Notes = null, TransactionDate = now.AddDays(-30) },

                    // ── Legal Pads ───────────────────────────────────────────
                    new() { ProductId = pads.Id, Type = TransactionType.Restock, QuantityChange = 40, StockAfter = 40, Reference = "PO-2024-011", Notes = "Initial stock order", TransactionDate = now.AddDays(-100) },
                    new() { ProductId = pads.Id, Type = TransactionType.Sale, QuantityChange = -20, StockAfter = 20, Reference = "ORD-1008", Notes = null, TransactionDate = now.AddDays(-70) },
                    new() { ProductId = pads.Id, Type = TransactionType.Sale, QuantityChange = -16, StockAfter = 4, Reference = "ORD-1091", Notes = "Low stock — order more", TransactionDate = now.AddDays(-10) },

                    // ── Ergonomic Chair ──────────────────────────────────────
                    new() { ProductId = chair.Id, Type = TransactionType.Restock, QuantityChange = 10, StockAfter = 10, Reference = "PO-2024-020", Notes = "Initial stock order", TransactionDate = now.AddDays(-110) },
                    new() { ProductId = chair.Id, Type = TransactionType.Sale, QuantityChange = -2, StockAfter = 8, Reference = "ORD-1003", Notes = null, TransactionDate = now.AddDays(-80) },
                    new() { ProductId = chair.Id, Type = TransactionType.Sale, QuantityChange = -1, StockAfter = 7, Reference = "ORD-1067", Notes = null, TransactionDate = now.AddDays(-20) },

                    // ── Standing Desk ────────────────────────────────────────
                    new() { ProductId = desk.Id, Type = TransactionType.Restock, QuantityChange = 5, StockAfter = 5, Reference = "PO-2024-021", Notes = "Initial stock order", TransactionDate = now.AddDays(-110) },
                    new() { ProductId = desk.Id, Type = TransactionType.Sale, QuantityChange = -2, StockAfter = 3, Reference = "ORD-1004", Notes = null, TransactionDate = now.AddDays(-75) },
                    new() { ProductId = desk.Id, Type = TransactionType.Adjustment, QuantityChange = -1, StockAfter = 2, Reference = null, Notes = "Damaged in warehouse", TransactionDate = now.AddDays(-15) },

                    // ── AA Batteries ─────────────────────────────────────────
                    new() { ProductId = batt.Id, Type = TransactionType.Restock, QuantityChange = 200, StockAfter = 200, Reference = "PO-2024-030", Notes = "Bulk order", TransactionDate = now.AddDays(-60) },
                    new() { ProductId = batt.Id, Type = TransactionType.Sale, QuantityChange = -30, StockAfter = 170, Reference = "ORD-1022", Notes = null, TransactionDate = now.AddDays(-40) },
                    new() { ProductId = batt.Id, Type = TransactionType.Sale, QuantityChange = -20, StockAfter = 150, Reference = "ORD-1080", Notes = null, TransactionDate = now.AddDays(-10) },

                    // ── Printer Paper ────────────────────────────────────────
                    new() { ProductId = paper.Id, Type = TransactionType.Restock, QuantityChange = 40, StockAfter = 40, Reference = "PO-2024-031", Notes = "Initial stock order", TransactionDate = now.AddDays(-95) },
                    new() { ProductId = paper.Id, Type = TransactionType.Sale, QuantityChange = -15, StockAfter = 25, Reference = "ORD-1015", Notes = null, TransactionDate = now.AddDays(-60) },
                    new() { ProductId = paper.Id, Type = TransactionType.Sale, QuantityChange = -10, StockAfter = 15, Reference = "ORD-1050", Notes = null, TransactionDate = now.AddDays(-30) },
                    new() { ProductId = paper.Id, Type = TransactionType.Sale, QuantityChange = -6, StockAfter = 9, Reference = "ORD-1095", Notes = "Stock getting low", TransactionDate = now.AddDays(-3) }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
