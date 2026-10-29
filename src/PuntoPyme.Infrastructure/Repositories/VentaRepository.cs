using Microsoft.EntityFrameworkCore;
using Npgsql;
using PuntoPyme.Application.Ventas;
using PuntoPyme.Domain.Entities;
using PuntoPyme.Infrastructure.Data;

namespace PuntoPyme.Infrastructure.Repositories;

public class VentaRepository(PuntoPymeDbContext db) : IVentaRepository
{
    public Task<Venta?> ObtenerPorIdLocalAsync(Guid idLocal, CancellationToken ct = default) =>
        Consulta().FirstOrDefaultAsync(v => v.IdLocal == idLocal, ct);

    public async Task<RegistroVentaResultado> RegistrarAsync(Venta venta, CancellationToken ct = default)
    {
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            // Orden fijo por producto para evitar interbloqueos entre dos cajas.
            foreach (var d in venta.Detalles.OrderBy(d => d.ProductoId))
            {
                var cantidad = d.Cantidad;
                var productoId = d.ProductoId;
                var filas = await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE \"Productos\" SET \"Stock\" = \"Stock\" - {cantidad} WHERE \"Id\" = {productoId} AND \"Stock\" >= {cantidad}",
                    ct);

                if (filas == 0)
                {
                    await tx.RollbackAsync(ct);
                    return new RegistroVentaResultado(null, false, await CalcularFaltantesAsync(venta, ct));
                }
            }

            db.Ventas.Add(venta);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Mismo idLocal llegó dos veces al mismo tiempo: devolvemos la que ganó.
            db.ChangeTracker.Clear();
            var existente = await ObtenerPorIdLocalAsync(venta.IdLocal, ct);
            if (existente is not null) return new RegistroVentaResultado(existente, false, []);
            throw;
        }

        var id = venta.Id;
        db.ChangeTracker.Clear();
        var guardada = await Consulta().FirstAsync(v => v.Id == id, ct);
        return new RegistroVentaResultado(guardada, true, []);
    }

    private IQueryable<Venta> Consulta() =>
        db.Ventas.AsNoTracking()
            .Include(v => v.Usuario)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto);

    private async Task<IReadOnlyList<StockFaltante>> CalcularFaltantesAsync(Venta venta, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        var ids = venta.Detalles.Select(d => d.ProductoId).ToList();
        var productos = await db.Productos.AsNoTracking().Where(p => ids.Contains(p.Id)).ToListAsync(ct);

        return venta.Detalles
            .Join(productos, d => d.ProductoId, p => p.Id, (d, p) => new StockFaltante(p.Id, p.Nombre, p.Stock, d.Cantidad))
            .Where(f => f.Solicitado > f.Disponible)
            .ToList();
    }
}
