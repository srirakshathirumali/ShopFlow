using Microsoft.EntityFrameworkCore;
using ShopFlow.InventoryService.Domain.Entities;

namespace ShopFlow.InventoryService.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Product ────────────────────────────────────────
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.SKU)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.HasIndex(e => e.SKU)
                  .IsUnique();
            entity.Property(e => e.Price)
                  .HasColumnType("decimal(18,2)");
            entity.Ignore(e => e.AvailableQuantity); // computed
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt)
                  .Metadata
                  .SetAfterSaveBehavior(
                      Microsoft.EntityFrameworkCore.Metadata
                      .PropertySaveBehavior.Ignore);
        });

        // ── StockReservation ───────────────────────────────
        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status)
                  .HasConversion<string>();
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.OrderId);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt)
                  .Metadata
                  .SetAfterSaveBehavior(
                      Microsoft.EntityFrameworkCore.Metadata
                      .PropertySaveBehavior.Ignore);
        });
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}