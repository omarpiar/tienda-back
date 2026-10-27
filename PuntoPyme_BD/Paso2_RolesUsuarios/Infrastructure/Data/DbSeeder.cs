using Microsoft.EntityFrameworkCore;
using PuntoPyme.Application.Abstractions;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data;

/// <summary>
/// Crea el primer Administrador si todavia no existe ningun usuario.
/// Las credenciales vienen de user-secrets o variables de entorno, nunca del repositorio.
/// </summary>
public static class DbSeeder
{
    public static async Task SembrarAdministradorAsync(
        PuntoPymeDbContext db,
        IPinHasher hasher,
        string? nombreCompleto,
        string? nombreUsuario,
        string? pin,
        CancellationToken ct = default)
    {
        if (await db.Usuarios.AnyAsync(ct)) return;
        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(pin)) return;

        var rolAdmin = await db.Roles.SingleAsync(r => r.Nombre == RolNombres.Administrador, ct);

        db.Usuarios.Add(new Usuario
        {
            NombreCompleto = string.IsNullOrWhiteSpace(nombreCompleto) ? "Administrador" : nombreCompleto.Trim(),
            NombreUsuario = nombreUsuario.Trim().ToLowerInvariant(),
            PinHash = hasher.Hash(pin),
            RolId = rolAdmin.Id,
            Activo = true,
            FechaCreacionUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
    }
}
