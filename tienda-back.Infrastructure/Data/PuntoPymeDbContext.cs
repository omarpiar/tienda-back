using Microsoft.EntityFrameworkCore;
using tienda_back.Domain.Entities;

namespace tienda_back.Infrastructure.Data;

public class PuntoPymeDbContext : DbContext
{
    public PuntoPymeDbContext(DbContextOptions<PuntoPymeDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PuntoPymeDbContext).Assembly);
    }
}
