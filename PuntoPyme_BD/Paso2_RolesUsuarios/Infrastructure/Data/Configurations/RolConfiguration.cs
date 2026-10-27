using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre).IsRequired().HasMaxLength(30);
        builder.HasIndex(r => r.Nombre).IsUnique();

        // Datos iniciales: viajan dentro de la migracion (no hay que insertarlos a mano).
        builder.HasData(
            new Rol { Id = 1, Nombre = RolNombres.Administrador },
            new Rol { Id = 2, Nombre = RolNombres.Cajero });
    }
}
