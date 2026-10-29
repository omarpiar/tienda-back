using Microsoft.EntityFrameworkCore;
using PuntoPyme.Application.Reportes;
using PuntoPyme.Infrastructure.Data;

namespace PuntoPyme.Infrastructure.Repositories;

public class ReporteRepository(PuntoPymeDbContext db) : IReporteRepository
{
    public async Task<ResumenVentasCrudo> ObtenerResumenAsync(DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct = default)
    {
        var ventas = db.Ventas.AsNoTracking().Where(v => v.FechaVentaUtc >= desdeUtc && v.FechaVentaUtc < hastaUtc);

        var total = await ventas.SumAsync(v => (decimal?)v.Total, ct) ?? 0m;
        var transacciones = await ventas.CountAsync(ct);

        var filas = await db.VentaDetalles.AsNoTracking()
            .Where(d => d.Venta.FechaVentaUtc >= desdeUtc && d.Venta.FechaVentaUtc < hastaUtc)
            .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
            .Select(g => new { g.Key.ProductoId, g.Key.Nombre, Cantidad = g.Sum(x => x.Cantidad) })
            .ToListAsync(ct);

        return new ResumenVentasCrudo(
            total,
            transacciones,
            filas.Select(f => new ProductoVendidoDto(f.ProductoId, f.Nombre, f.Cantidad)).ToList());
    }
}
