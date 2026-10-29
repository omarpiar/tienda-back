using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> b)
    {
        b.ToTable("Ventas", t =>
        {
            t.HasCheckConstraint("CK_Ventas_MetodoPago", "\"MetodoPago\" IN ('Efectivo', 'Tarjeta')");
            t.HasCheckConstraint("CK_Ventas_Total_NoNegativo", "\"Total\" >= 0");
        });
        b.HasKey(v => v.Id);
        b.Property(v => v.FechaVentaUtc).HasColumnType("timestamp with time zone");
        b.Property(v => v.FechaRegistroUtc).HasColumnType("timestamp with time zone");
        b.Property(v => v.MetodoPago).IsRequired().HasMaxLength(20);
        b.Property(v => v.Total).HasPrecision(12, 2);
        b.Property(v => v.MontoRecibido).HasPrecision(12, 2);
        b.Property(v => v.Cambio).HasPrecision(12, 2);

        b.HasIndex(v => v.IdLocal).IsUnique();
        b.HasIndex(v => v.FechaVentaUtc);

        b.HasOne(v => v.Usuario).WithMany().HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> b)
    {
        b.ToTable("VentaDetalles", t =>
        {
            t.HasCheckConstraint("CK_VentaDetalles_Cantidad", "\"Cantidad\" > 0");
            t.HasCheckConstraint("CK_VentaDetalles_PrecioUnitario", "\"PrecioUnitario\" >= 0");
        });
        b.HasKey(d => d.Id);
        b.Property(d => d.PrecioUnitario).HasPrecision(12, 2);
        b.Property(d => d.Subtotal).HasPrecision(12, 2);

        b.HasOne(d => d.Venta).WithMany(v => v.Detalles).HasForeignKey(d => d.VentaId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(d => d.Producto).WithMany().HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);
    }
}
