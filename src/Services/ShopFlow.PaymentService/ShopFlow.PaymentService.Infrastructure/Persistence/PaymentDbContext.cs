using Microsoft.EntityFrameworkCore;
using ShopFlow.PaymentService.Domain.Entities;

namespace ShopFlow.PaymentService.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount)
                  .HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status)
                  .HasConversion<string>();
            entity.Property(e => e.FailureReason)
                  .HasMaxLength(500);
            entity.HasIndex(e => e.OrderId)
                  .IsUnique();
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