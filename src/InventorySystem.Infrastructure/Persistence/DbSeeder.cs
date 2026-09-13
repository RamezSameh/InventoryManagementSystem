using InventorySystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Manager", "Cashier" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var admin = await userManager.FindByEmailAsync("admin@inventory.com");
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = "admin@inventory.com",
                Email = "admin@inventory.com",
                FullName = "System Administrator",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        await SeedReferenceDataAsync(db);
        await SeedProductsAsync(db);
        await SeedStockAsync(db);
        await SeedMovementsAsync(db);
    }

    private static async Task SeedReferenceDataAsync(AppDbContext db)
    {
        var categoryNames = new[]
        {
            ("Electronics", "Laptops, screens, accessories and electronic devices"),
            ("Office Supplies", "Paper, pens, folders and daily office supplies"),
            ("Furniture", "Desks, chairs, cabinets and workplace furniture"),
            ("Networking", "Routers, switches, cables and network equipment"),
            ("Mobile Devices", "Smartphones, tablets and mobile accessories"),
            ("Printers", "Printers, scanners, toner and printing supplies"),
            ("Cleaning", "Cleaning tools and facility supplies"),
            ("Safety", "Protective equipment and safety supplies"),
            ("Packaging", "Boxes, tapes and shipping materials"),
            ("Cameras", "Security and surveillance cameras")
        };

        var existingCategories = await db.Categories.ToDictionaryAsync(x => x.Name);
        foreach (var (name, description) in categoryNames)
        {
            if (!existingCategories.ContainsKey(name))
            {
                db.Categories.Add(new Category { Name = name, Description = description });
            }
        }

        var warehouseData = new[]
        {
            ("Main Warehouse", "Riyadh - Industrial Area"),
            ("Secondary Warehouse", "Jeddah - Al Marwah"),
            ("Eastern Distribution Center", "Dammam - Logistics Park"),
            ("Showroom Stock", "Riyadh - King Fahd Road"),
            ("Returns Warehouse", "Riyadh - Service Center"),
            ("Fast Moving Goods", "Jeddah - Main Branch")
        };

        var existingWarehouses = await db.Warehouses.ToDictionaryAsync(x => x.Name);
        foreach (var (name, location) in warehouseData)
        {
            if (!existingWarehouses.ContainsKey(name))
            {
                db.Warehouses.Add(new Warehouse { Name = name, Location = location });
            }
        }

        var suppliers = new[]
        {
            ("TechSource Arabia", "+966 11 450 1001", "sales@techsource.example.com", "Riyadh"),
            ("Gulf Office Solutions", "+966 12 660 2202", "orders@gulfoffice.example.com", "Jeddah"),
            ("Modern Furniture Co.", "+966 13 820 3303", "contact@modernfurniture.example.com", "Dammam"),
            ("Network World", "+966 11 470 4404", "sales@networkworld.example.com", "Riyadh"),
            ("Print Masters", "+966 12 670 5505", "support@printmasters.example.com", "Jeddah"),
            ("SafeWork Equipment", "+966 13 830 6606", "info@safework.example.com", "Dammam"),
            ("PackRight Logistics", "+966 11 480 7707", "orders@packright.example.com", "Riyadh"),
            ("Vision Security", "+966 12 680 8808", "sales@visionsecurity.example.com", "Jeddah"),
            ("CleanPro Supplies", "+966 13 840 9909", "hello@cleanpro.example.com", "Dammam"),
            ("Mobile Hub", "+966 11 490 1010", "b2b@mobilehub.example.com", "Riyadh")
        };

        var existingSuppliers = await db.Suppliers.ToDictionaryAsync(x => x.Name);
        foreach (var (name, phone, email, address) in suppliers)
        {
            if (!existingSuppliers.ContainsKey(name))
            {
                db.Suppliers.Add(new Supplier { Name = name, Phone = phone, Email = email, Address = address });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(AppDbContext db)
    {
        if (await db.Products.CountAsync() >= 100)
        {
            return;
        }

        var categories = await db.Categories.OrderBy(x => x.Id).ToListAsync();
        var productTemplates = new[]
        {
            ("Laptop Pro 14", "LAP", 700m, 999m), ("Business Laptop 15", "LAP", 850m, 1199m),
            ("27 Inch Monitor", "MON", 180m, 299m), ("Wireless Keyboard", "KEY", 25m, 49m),
            ("Wireless Mouse", "MOU", 12m, 29m), ("USB-C Docking Station", "DOC", 80m, 149m),
            ("A4 Copy Paper Box", "PAP", 18m, 29m), ("Ballpoint Pens Pack", "PEN", 8m, 15m),
            ("Executive Notebook", "NOT", 12m, 24m), ("Document Folders Pack", "FOL", 15m, 27m),
            ("Ergonomic Office Chair", "CHR", 80m, 150m), ("Executive Desk", "DSK", 220m, 399m),
            ("Filing Cabinet", "CAB", 110m, 199m), ("Meeting Table", "TBL", 350m, 599m),
            ("WiFi 6 Router", "RTR", 90m, 159m), ("24 Port Network Switch", "SWT", 145m, 249m),
            ("Cat6 Cable Box", "CAB", 55m, 95m), ("Network Rack 12U", "RCK", 180m, 299m),
            ("Android Smartphone", "PHN", 300m, 499m), ("Business Tablet", "TAB", 260m, 449m),
            ("Laser Printer", "PRN", 180m, 299m), ("Printer Toner Black", "TON", 40m, 75m),
            ("Floor Cleaner 5L", "CLN", 18m, 32m), ("Safety Gloves Box", "GLV", 20m, 39m),
            ("Shipping Carton Medium", "BOX", 2m, 5m), ("Packing Tape Pack", "TAP", 12m, 24m),
            ("Security Camera 4MP", "CAM", 100m, 179m), ("NVR 8 Channel", "NVR", 190m, 329m)
        };

        var existingSkus = (await db.Products.Select(x => x.SKU).ToListAsync()).ToHashSet();
        var random = new Random(20260913);
        var products = new List<Product>();

        for (var i = 0; i < 120; i++)
        {
            var template = productTemplates[i % productTemplates.Length];
            var sku = $"{template.Item2}-{i + 1:000}";
            if (existingSkus.Contains(sku))
            {
                continue;
            }

            var category = categories[i % categories.Count];
            products.Add(new Product
            {
                Name = $"{template.Item1} {((i / productTemplates.Length) + 1)}",
                SKU = sku,
                Barcode = $"6281{random.Next(10000000, 99999999)}",
                Description = $"High quality {template.Item1.ToLowerInvariant()} for business operations.",
                PurchasePrice = template.Item3,
                SellingPrice = template.Item4,
                MinimumStock = 5 + (i % 6) * 5,
                CategoryId = category.Id
            });
        }

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    private static async Task SeedStockAsync(AppDbContext db)
    {
        if (await db.StockItems.AnyAsync())
        {
            return;
        }

        var products = await db.Products.OrderBy(x => x.Id).ToListAsync();
        var warehouses = await db.Warehouses.OrderBy(x => x.Id).ToListAsync();
        var random = new Random(20260914);
        var stockItems = new List<StockItem>();

        foreach (var product in products)
        {
            foreach (var warehouse in warehouses)
            {
                var quantity = random.Next(0, 180);
                if ((product.Id + warehouse.Id) % 11 == 0)
                {
                    quantity = random.Next(0, Math.Max(1, product.MinimumStock));
                }

                stockItems.Add(new StockItem
                {
                    ProductId = product.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = quantity
                });
            }
        }

        db.StockItems.AddRange(stockItems);
        await db.SaveChangesAsync();
    }

    private static async Task SeedMovementsAsync(AppDbContext db)
    {
        if (await db.StockMovements.CountAsync() >= 500)
        {
            return;
        }

        var products = await db.Products.OrderBy(x => x.Id).ToListAsync();
        var warehouses = await db.Warehouses.OrderBy(x => x.Id).ToListAsync();
        var random = new Random(20260915);
        var movements = new List<StockMovement>();
        var types = new[] { MovementType.PurchaseIn, MovementType.SaleOut, MovementType.Adjustment, MovementType.Return };

        for (var i = 0; i < 750; i++)
        {
            var product = products[i % products.Count];
            var warehouse = warehouses[(i * 3) % warehouses.Count];
            var type = types[i % types.Length];
            movements.Add(new StockMovement
            {
                ProductId = product.Id,
                WarehouseId = warehouse.Id,
                Quantity = random.Next(1, 35),
                Type = type,
                Reference = $"SEED-{DateTime.UtcNow.Year}-{i + 1:0000}",
                Date = DateTime.UtcNow.AddDays(-(i % 180)).AddMinutes(-(i * 7)),
                Notes = type == MovementType.PurchaseIn ? "Initial supplier purchase" : "Generated demo movement"
            });
        }

        db.StockMovements.AddRange(movements);
        await db.SaveChangesAsync();
    }
}

public static class SeedDataSummary
{
    public const string Description = "Seed data creates 10 categories, 6 warehouses, 10 suppliers, 120 products, 720 stock balances and 750 movements.";
}
