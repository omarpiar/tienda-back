using Microsoft.EntityFrameworkCore;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data;

public class PuntoPymeDbContext(DbContextOptions<PuntoPymeDbContext> options) : DbContext(options)
{
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<ProductoHistorial> ProductoHistorial => Set<ProductoHistorial>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PuntoPymeDbContext).Assembly);
    }
}
