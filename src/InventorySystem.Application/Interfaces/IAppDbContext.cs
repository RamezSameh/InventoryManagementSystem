using Microsoft.EntityFrameworkCore; using InventorySystem.Domain.Entities;
namespace InventorySystem.Application.Interfaces;
public interface IAppDbContext { DbSet<Product> Products { get; } DbSet<Category> Categories { get; } DbSet<Warehouse> Warehouses { get; } DbSet<StockItem> StockItems { get; } DbSet<StockMovement> StockMovements { get; } DbSet<Supplier> Suppliers { get; } Task<int> SaveChangesAsync(CancellationToken cancellationToken=default); }
