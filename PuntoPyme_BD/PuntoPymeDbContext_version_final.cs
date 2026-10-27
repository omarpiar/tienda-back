using Microsoft.EntityFrameworkCore;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data;

// Version final. Agrega los DbSet de cada paso solo cuando copies sus archivos:
//   Paso 1: Productos      Paso 2: Roles y Usuarios      Paso 3: Ventas y VentaDetalles
public class PuntoPymeDbContext : DbContext
{
    public PuntoPymeDbContext(DbContextOptions<PuntoPymeDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();           // Paso 1
    public DbSet<Rol> Roles => Set<Rol>();                         // Paso 2
    public DbSet<Usuario> Usuarios => Set<Usuario>();              // Paso 2
    public DbSet<Venta> Ventas => Set<Venta>();                    // Paso 3
    public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>(); // Paso 3

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Toma todas las clases IEntityTypeConfiguration<T> del proyecto.
        // Borra de aqui la configuracion inline anterior de Producto (HasDefaultValue(5), HasIndex Sku).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PuntoPymeDbContext).Assembly);
    }
}
