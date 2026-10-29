using Microsoft.EntityFrameworkCore;
using PuntoPyme.Application.Auth;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;
using PuntoPyme.Infrastructure.Data;

namespace PuntoPyme.Infrastructure.Repositories;

public class UsuarioRepository(PuntoPymeDbContext db) : IUsuarioRepository
{
    public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuarioNormalizado, CancellationToken ct = default) =>
        db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuarioNormalizado, ct);

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(string? buscar, string? rol, bool? activo, CancellationToken ct = default)
    {
        IQueryable<Usuario> q = db.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var patron = $"%{buscar.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%";
            q = q.Where(u => EF.Functions.ILike(u.NombreCompleto, patron, "\\") ||
                             EF.Functions.ILike(u.NombreUsuario, patron, "\\"));
        }
        if (rol is not null)
        {
            var rolId = Roles.IdDe(rol);
            q = q.Where(u => u.RolId == rolId);
        }
        if (activo is not null) q = q.Where(u => u.Activo == activo);

        return await q.OrderBy(u => u.NombreCompleto).ThenBy(u => u.Id).ToListAsync(ct);
    }

    public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuarioNormalizado, int? excluirId, CancellationToken ct = default) =>
        db.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuarioNormalizado && (excluirId == null || u.Id != excluirId), ct);

    public Task<int> ContarAdministradoresActivosAsync(CancellationToken ct = default) =>
        db.Usuarios.CountAsync(u => u.RolId == Roles.AdministradorId && u.Activo, ct);

    public async Task AgregarAsync(Usuario usuario, CancellationToken ct = default)
    {
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Usuario usuario, CancellationToken ct = default) =>
        await db.SaveChangesAsync(ct); // la entidad ya viene rastreada
}
