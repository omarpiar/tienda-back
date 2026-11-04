using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class ProductoHistorialConfiguration : IEntityTypeConfiguration<ProductoHistorial>
{
    public void Configure(EntityTypeBuilder<ProductoHistorial> b)
    {
        b.ToTable("ProductoHistorial");
        b.HasKey(h => h.Id);
        b.Property(h => h.Accion).IsRequired().HasMaxLength(30);
        b.Property(h => h.Descripcion).IsRequired().HasMaxLength(300);
        b.HasOne(h => h.Producto)
            .WithMany(p => p.Historial)
            .HasForeignKey(h => h.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(h => h.Usuario)
            .WithMany()
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(h => h.ProductoId).HasDatabaseName("IX_ProductoHistorial_ProductoId");
        b.HasIndex(h => h.UsuarioId).HasDatabaseName("IX_ProductoHistorial_UsuarioId");
        b.HasIndex(h => new { h.ProductoId, h.FechaUtc }).HasDatabaseName("IX_ProductoHistorial_ProductoId_FechaUtc");
    }
}
