using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos", tabla =>
        {
            tabla.HasCheckConstraint("CK_Productos_PrecioVenta_MayorOIgualCosto", "\"PrecioVenta\" >= \"CostoCompra\"");
            tabla.HasCheckConstraint("CK_Productos_Stock_NoNegativo", "\"Stock\" >= 0");
            tabla.HasCheckConstraint("CK_Productos_StockMinimo_NoNegativo", "\"StockMinimo\" >= 0");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Categoria).HasMaxLength(100);
        builder.Property(p => p.PrecioVenta).HasPrecision(12, 2);
        builder.Property(p => p.CostoCompra).HasPrecision(12, 2);
        builder.Property(p => p.StockMinimo).HasDefaultValue(5);

        // Ya existe en la migracion CrearIndiceUnicoSkuProductos (IX_Productos_Sku).
        builder.HasIndex(p => p.Sku).IsUnique();
    }
}
