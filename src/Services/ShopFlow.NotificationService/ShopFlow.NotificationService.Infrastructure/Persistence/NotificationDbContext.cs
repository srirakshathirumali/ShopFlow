using Microsoft.EntityFrameworkCore;
using ShopFlow.NotificationService.Domain.Entities;

namespace ShopFlow.NotificationService.Infrastructure.Persistence;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(
        DbContextOptions<NotificationDbContext> options)
        : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message)
                  .IsRequired()
                  .HasMaxLength(1000);
            entity.Property(e => e.Type)
                  .HasConversion<string>();
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.CustomerId);
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
        foreach (var entry in ChangeTracker
            .Entries<ShopFlow.NotificationService.Domain.Entities.BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}