using PuntoPyme.Application.Common;

namespace PuntoPyme.Application.Reportes;

public record ProductoVendidoDto(int ProductoId, string Nombre, int CantidadVendida);

public record ReporteVentasDto(
    DateTime DesdeUtc, DateTime HastaUtc, decimal TotalVentas, int NumeroTransacciones,
    decimal TicketPromedio, IReadOnlyList<ProductoVendidoDto> ProductosMasVendidos);

public record ResumenVentasCrudo(decimal Total, int Transacciones, IReadOnlyList<ProductoVendidoDto> PorProducto);

public interface IReporteRepository
{
    /// <summary>Ventas con FechaVentaUtc >= desde y &lt; hasta, con la cantidad vendida por producto (sin ordenar).</summary>
    Task<ResumenVentasCrudo> ObtenerResumenAsync(DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct = default);
}

/// <summary>HU-08. El cliente manda el rango en UTC (D6).</summary>
public class ReporteService(IReporteRepository repositorio)
{
    public const int DiasMaximos = 366;
    public const int TopProductos = 10;

    public async Task<ReporteVentasDto> VentasAsync(DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct = default)
    {
        if (hastaUtc <= desdeUtc)
            throw AppException.Validacion("hastaUtc", "La fecha final debe ser mayor que la inicial.");
        if ((hastaUtc - desdeUtc).TotalDays > DiasMaximos)
            throw AppException.Validacion("hastaUtc", $"El rango no puede pasar de {DiasMaximos} días.");

        var crudo = await repositorio.ObtenerResumenAsync(desdeUtc, hastaUtc, ct);

        var ticket = crudo.Transacciones == 0
            ? 0m
            : Math.Round(crudo.Total / crudo.Transacciones, 2, MidpointRounding.AwayFromZero);

        var top = crudo.PorProducto
            .OrderByDescending(p => p.CantidadVendida)
            .ThenBy(p => p.Nombre, StringComparer.CurrentCulture)
            .Take(TopProductos)
            .ToList();

        return new ReporteVentasDto(desdeUtc, hastaUtc, crudo.Total, crudo.Transacciones, ticket, top);
    }
}
