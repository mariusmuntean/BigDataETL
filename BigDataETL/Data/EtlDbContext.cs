using Microsoft.EntityFrameworkCore;

namespace BigDataETL.Data;

public class EtlDbContext : DbContext
{
    public EtlDbContext(DbContextOptions<EtlDbContext> options) : base(options)
    {
    }
}