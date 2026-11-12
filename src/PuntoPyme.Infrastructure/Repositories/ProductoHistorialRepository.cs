using Microsoft.EntityFrameworkCore;
using PuntoPyme.Application.Productos;
using PuntoPyme.Domain.Entities;
using PuntoPyme.Infrastructure.Data;

namespace PuntoPyme.Infrastructure.Repositories;

public class ProductoHistorialRepository(PuntoPymeDbContext db) : IProductoHistorialRepository
{
    public async Task<IReadOnlyList<ProductoHistorial>> ListarAsync(int productoId, CancellationToken ct = default) =>
        await db.ProductoHistorial.AsNoTracking()
            .Include(h => h.Usuario)
            .Where(h => h.ProductoId == productoId)
            .OrderByDescending(h => h.FechaUtc).ThenByDescending(h => h.Id)
            .ToListAsync(ct);

    public async Task AgregarAsync(ProductoHistorial historial, CancellationToken ct = default)
    {
        db.ProductoHistorial.Add(historial);
        await db.SaveChangesAsync(ct);
    }
}
