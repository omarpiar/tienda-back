using Microsoft.EntityFrameworkCore;
using tienda_back.Domain.Entities;

namespace tienda_back.Infrastructure.Data;

public class PuntoPymeDbContext : DbContext
{
    public PuntoPymeDbContext(DbContextOptions<PuntoPymeDbContext> opciones) : base(opciones) { }

    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>().Property(p => p.StockMinimo).HasDefaultValue(5);
        modelBuilder.Entity<Producto>().HasIndex(p => p.Sku).IsUnique();
    }
}
