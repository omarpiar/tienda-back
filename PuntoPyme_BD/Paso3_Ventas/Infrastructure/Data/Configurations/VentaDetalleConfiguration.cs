using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> builder)
    {
        builder.ToTable("VentaDetalles", tabla =>
        {
            tabla.HasCheckConstraint("CK_VentaDetalles_Cantidad_Positiva", "\"Cantidad\" > 0");
            tabla.HasCheckConstraint("CK_VentaDetalles_PrecioUnitario_NoNegativo", "\"PrecioUnitario\" >= 0");
        });

        builder.HasKey(d => d.Id);

        builder.Property(d => d.PrecioUnitario).HasPrecision(12, 2);
        builder.Property(d => d.Subtotal).HasPrecision(12, 2);

        builder.HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
