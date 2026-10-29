using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Ventas;

public record DetalleVentaRequest(int ProductoId, int Cantidad);

public record RegistrarVentaRequest(
    Guid? IdLocal, DateTime? FechaVentaUtc, string? MetodoPago, decimal? MontoRecibido,
    List<DetalleVentaRequest>? Detalles);

public record VentaDetalleDto(int ProductoId, string Nombre, int Cantidad, decimal PrecioUnitario, decimal Subtotal);

public record VentaDto(
    int Id, string Folio, Guid IdLocal, DateTime FechaVentaUtc, DateTime FechaRegistroUtc,
    string Cajero, string MetodoPago, decimal Total, decimal? MontoRecibido, decimal? Cambio,
    IReadOnlyList<VentaDetalleDto> Detalles);

/// <summary>Creada = false cuando el idLocal ya existía (la sincronización repetida no duplica).</summary>
public record ResultadoVenta(VentaDto Venta, bool Creada);

public record StockFaltante(int ProductoId, string Nombre, int Disponible, int Solicitado);

/// <summary>Venta es null cuando faltó stock (no se guardó nada).</summary>
public record RegistroVentaResultado(Venta? Venta, bool Creada, IReadOnlyList<StockFaltante> Faltantes);

public static class VentaMapeo
{
    public static string Folio(int id) => $"V-{id:D6}";

    /// <summary>Requiere Usuario y Detalles.Producto cargados.</summary>
    public static VentaDto ADto(Venta v) => new(
        v.Id, Folio(v.Id), v.IdLocal, v.FechaVentaUtc, v.FechaRegistroUtc,
        v.Usuario.NombreCompleto, v.MetodoPago, v.Total, v.MontoRecibido, v.Cambio,
        v.Detalles.Select(d => new VentaDetalleDto(d.ProductoId, d.Producto.Nombre, d.Cantidad, d.PrecioUnitario, d.Subtotal)).ToList());
}
