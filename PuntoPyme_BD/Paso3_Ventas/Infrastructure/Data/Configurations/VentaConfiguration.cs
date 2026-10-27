using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas", tabla =>
        {
            tabla.HasCheckConstraint("CK_Ventas_Total_NoNegativo", "\"Total\" >= 0");
            tabla.HasCheckConstraint("CK_Ventas_MetodoPago_Valido", "\"MetodoPago\" IN ('Efectivo', 'Tarjeta')");
        });

        builder.HasKey(v => v.Id);

        builder.Property(v => v.IdLocal).IsRequired();
        builder.Property(v => v.MetodoPago).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(v => v.Total).HasPrecision(12, 2);
        builder.Property(v => v.MontoRecibido).HasPrecision(12, 2);
        builder.Property(v => v.Cambio).HasPrecision(12, 2);

        builder.HasIndex(v => v.IdLocal).IsUnique();
        builder.HasIndex(v => v.FechaVentaUtc);

        builder.HasOne(v => v.Usuario)
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
