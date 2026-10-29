using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> b)
    {
        b.ToTable("Productos", t =>
        {
            t.HasCheckConstraint("CK_Productos_PrecioVenta_MayorOIgualCosto", "\"PrecioVenta\" >= \"CostoCompra\"");
            t.HasCheckConstraint("CK_Productos_Stock_NoNegativo", "\"Stock\" >= 0");
            t.HasCheckConstraint("CK_Productos_StockMinimo_NoNegativo", "\"StockMinimo\" >= 0");
        });
        b.HasKey(p => p.Id);
        b.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
        b.Property(p => p.Sku).IsRequired().HasMaxLength(50);
        b.Property(p => p.Categoria).HasMaxLength(100);
        b.Property(p => p.PrecioVenta).HasPrecision(12, 2);
        b.Property(p => p.CostoCompra).HasPrecision(12, 2);
        b.Property(p => p.StockMinimo).HasDefaultValue(5);
        b.HasIndex(p => p.Sku).IsUnique().HasDatabaseName("IX_Productos_Sku");
    }
}
