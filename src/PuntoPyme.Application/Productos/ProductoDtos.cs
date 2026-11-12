namespace PuntoPyme.Application.Productos;

public record ProductoListaDto(
    int Id, string Nombre, string Sku, int? CategoriaId, string? Categoria,
    decimal PrecioVenta, int Stock, int StockMinimo, string EstadoStock);

/// <summary>CostoCompra, Ganancia y MargenPorcentaje solo se llenan para el rol Administrador.</summary>
public record ProductoDetalleDto(
    int Id, string Nombre, string Sku, int? CategoriaId, string? Categoria,
    decimal PrecioVenta, decimal? CostoCompra, decimal? Ganancia, decimal? MargenPorcentaje,
    int Stock, int StockMinimo, string EstadoStock);

public record ProductoHistorialDto(
    int Id, string Accion, string Descripcion, DateTime FechaUtc,
    int StockAnterior, int StockNuevo, int UsuarioId, string Usuario);

public record PaginaResultado<T>(IReadOnlyList<T> Items, int Total, int Pagina, int TamanoPagina);
