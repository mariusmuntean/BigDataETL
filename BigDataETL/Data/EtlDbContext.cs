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
}