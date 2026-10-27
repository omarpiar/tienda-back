using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(150);
        builder.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
        builder.Property(u => u.PinHash).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Activo).HasDefaultValue(true);
        builder.Property(u => u.IntentosFallidos).HasDefaultValue(0);

        builder.HasIndex(u => u.NombreUsuario).IsUnique();

        builder.HasOne(u => u.Rol)
            .WithMany()
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
