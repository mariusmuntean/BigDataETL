using BigDataETL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BigDataETL.Data;

public class EtlDbContext : DbContext
{
    static EtlDbContext()
    {
        NpgsqlConnection.GlobalTypeMapper.MapEnum<OrderStatus>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<LineItemStatus>();
    }

    public EtlDbContext(DbContextOptions<EtlDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderEvent> OrderEvents { get; set; }
    public DbSet<LineItem> LineItems { get; set; }
    public DbSet<LineItemEvent> LineItemEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<OrderStatus>();
        modelBuilder.HasPostgresEnum<LineItemStatus>();
    }

    public override int SaveChanges()
    {
        SetCreationDateTime();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        SetCreationDateTime();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetCreationDateTime()
    {
        foreach (var addedEntityEntry in this.ChangeTracker.Entries<BaseEntity>().Where(entry => entry.State == EntityState.Added))
        {
            addedEntityEntry.Entity.UtcCreatedAt = DateTime.UtcNow;
        }
    }
}