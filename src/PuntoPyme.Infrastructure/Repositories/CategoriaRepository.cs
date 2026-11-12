using Microsoft.EntityFrameworkCore;
using Npgsql;
using PuntoPyme.Application.Categorias;
using PuntoPyme.Application.Common;
using PuntoPyme.Domain.Entities;
using PuntoPyme.Infrastructure.Data;

namespace PuntoPyme.Infrastructure.Repositories;

public class CategoriaRepository(PuntoPymeDbContext db) : ICategoriaRepository
{
    public async Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default) =>
        await db.Categorias.AsNoTracking()
            .Include(c => c.Productos)
            .OrderBy(c => c.Nombre)
            .ToListAsync(ct);

    public Task<Categoria?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        db.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Categoria?> ObtenerPorIdParaEditarAsync(int id, CancellationToken ct = default) =>
        db.Categorias.Include(c => c.Productos).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct = default)
    {
        var normalizado = nombre.ToLower();
        return db.Categorias.AnyAsync(
            c => c.Nombre.ToLower() == normalizado && (excluirId == null || c.Id != excluirId), ct);
    }

    public Task<bool> TieneProductosAsync(int id, CancellationToken ct = default) =>
        db.Productos.AnyAsync(p => p.CategoriaId == id && !p.Eliminado, ct);

    public async Task AgregarAsync(Categoria categoria, CancellationToken ct = default)
    {
        db.Categorias.Add(categoria);
        await GuardarCambiosAsync(ct);
    }

    public async Task GuardarCambiosAsync(CancellationToken ct = default)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw AppException.Conflicto("CATEGORIA_DUPLICADA", "Ya existe una categoría con ese nombre.");
        }
    }

    public async Task EliminarAsync(Categoria categoria, CancellationToken ct = default)
    {
        db.Categorias.Remove(categoria);
        await GuardarCambiosAsync(ct);
    }
}
