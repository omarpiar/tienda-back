using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> b)
    {
        b.ToTable("Roles");
        b.HasKey(r => r.Id);
        b.Property(r => r.Nombre).IsRequired().HasMaxLength(30);
        b.HasIndex(r => r.Nombre).IsUnique();

        b.HasData(
            new Rol { Id = Roles.AdministradorId, Nombre = Roles.Administrador },
            new Rol { Id = Roles.CajeroId, Nombre = Roles.Cajero });
    }
}
