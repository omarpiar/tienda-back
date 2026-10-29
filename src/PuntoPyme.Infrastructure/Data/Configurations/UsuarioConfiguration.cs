using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuarios");
        b.HasKey(u => u.Id);

        b.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(150);
        b.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
        b.Property(u => u.PinHash).IsRequired().HasMaxLength(200);
        b.Property(u => u.Activo).IsRequired().HasDefaultValue(true);
        b.Property(u => u.IntentosFallidos).IsRequired().HasDefaultValue(0);
        b.Property(u => u.BloqueadoHastaUtc).HasColumnType("timestamp with time zone");
        b.Property(u => u.FechaCreacionUtc).IsRequired().HasColumnType("timestamp with time zone");

        b.HasIndex(u => u.NombreUsuario).IsUnique();

        b.HasOne(u => u.Rol)
         .WithMany(r => r.Usuarios)
         .HasForeignKey(u => u.RolId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
