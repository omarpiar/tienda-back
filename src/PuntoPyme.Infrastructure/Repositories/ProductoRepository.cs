using Microsoft.EntityFrameworkCore;
using Npgsql;
using PuntoPyme.Application.Common;
using PuntoPyme.Application.Productos;
using PuntoPyme.Domain.Entities;
using PuntoPyme.Infrastructure.Data;

namespace PuntoPyme.Infrastructure.Repositories;

public class ProductoRepository(PuntoPymeDbContext db) : IProductoRepository
{
    public async Task<ResultadoBusqueda> BuscarAsync(
        string? buscar, string estado, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        IQueryable<Producto> consulta = db.Productos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var patron = $"%{EscaparLike(buscar)}%";
            consulta = consulta.Where(p =>
                EF.Functions.ILike(p.Nombre, patron, "\\") ||
                EF.Functions.ILike(p.Sku, patron, "\\"));
        }

        // D5: stockBajo incluye los agotados; agotados trae solo stock 0.
        if (estado == FiltroEstado.StockBajo)
            consulta = consulta.Where(p => p.Stock <= p.StockMinimo);
        else if (estado == FiltroEstado.Agotados)
            consulta = consulta.Where(p => p.Stock == 0);

        var total = await consulta.CountAsync(ct);

        var items = await consulta
            .OrderBy(p => p.Nombre).ThenBy(p => p.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        return new ResultadoBusqueda(items, total);
    }

    public Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Producto?> ObtenerPorIdParaEditarAsync(int id, CancellationToken ct = default) =>
        db.Productos.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Producto?> ObtenerPorSkuAsync(string sku, CancellationToken ct = default)
    {
        var minusculas = sku.ToLower();
        return db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Sku.ToLower() == minusculas, ct);
    }

    public Task<bool> ExisteSkuAsync(string sku, int? excluirId, CancellationToken ct = default)
    {
        var minusculas = sku.ToLower();
        return db.Productos.AnyAsync(p => p.Sku.ToLower() == minusculas && (excluirId == null || p.Id != excluirId), ct);
    }

    public async Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        await db.Productos.AsNoTracking().Where(p => ids.Contains(p.Id)).ToListAsync(ct);

    public async Task AgregarAsync(Producto producto, CancellationToken ct = default)
    {
        db.Productos.Add(producto);
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
            // Dos peticiones con el mismo SKU a la vez: gana el índice único.
            throw AppException.Conflicto("SKU_DUPLICADO", "Ya existe un producto con ese código (SKU).");
        }
    }

    private static string EscaparLike(string texto) =>
        texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
