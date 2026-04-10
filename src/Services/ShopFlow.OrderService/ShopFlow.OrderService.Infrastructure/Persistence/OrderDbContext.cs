using Microsoft.EntityFrameworkCore;
using ShopFlow.OrderService.Domain.Entities;

namespace ShopFlow.OrderService.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Order ──────────────────────────────────────────
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount)
                  .HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status)
                  .HasConversion<string>();
            entity.Property(e => e.CancellationReason)
                  .HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt)
                  .Metadata
                  .SetAfterSaveBehavior(
                      Microsoft.EntityFrameworkCore.Metadata
                      .PropertySaveBehavior.Ignore);
        });

        // ── OrderLine ──────────────────────────────────────
        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.UnitPrice)
                  .HasColumnType("decimal(18,2)");
            entity.Ignore(e => e.LineTotal);   // computed — not stored
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderLines)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt)
                  .Metadata
                  .SetAfterSaveBehavior(
                      Microsoft.EntityFrameworkCore.Metadata
                      .PropertySaveBehavior.Ignore);
        });

        // ── OutboxMessage ──────────────────────────────────
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Payload)
                  .IsRequired();
            entity.Property(e => e.IsProcessed)
                  .HasDefaultValue(false);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt)
                  .Metadata
                  .SetAfterSaveBehavior(
                      Microsoft.EntityFrameworkCore.Metadata
                      .PropertySaveBehavior.Ignore);

            // Index for OutboxProcessor query performance
            entity.HasIndex(e => e.IsProcessed);
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