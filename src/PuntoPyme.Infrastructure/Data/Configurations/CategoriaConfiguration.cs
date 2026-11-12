using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> b)
    {
        b.ToTable("Categorias");
        b.HasKey(c => c.Id);
        b.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
        b.HasIndex(c => c.Nombre).IsUnique().HasDatabaseName("IX_Categorias_Nombre");
    }
}
