namespace PuntoPyme.Application.Productos;

public record ProductoListaDto(
    int Id, string Nombre, string Sku, string? Categoria,
    decimal PrecioVenta, int Stock, int StockMinimo, string EstadoStock);

/// <summary>CostoCompra, Ganancia y MargenPorcentaje solo se llenan para el rol Administrador.</summary>
public record ProductoDetalleDto(
    int Id, string Nombre, string Sku, string? Categoria,
    decimal PrecioVenta, decimal? CostoCompra, decimal? Ganancia, decimal? MargenPorcentaje,
    int Stock, int StockMinimo, string EstadoStock);

public record PaginaResultado<T>(IReadOnlyList<T> Items, int Total, int Pagina, int TamanoPagina);
