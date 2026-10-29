using PuntoPyme.Application.Common;

namespace PuntoPyme.Application.Productos;

/// <summary>HU-04: consultar inventario y alertas de stock bajo (API-02 y API-03).</summary>
public class ProductoConsultaService(IProductoRepository productos)
{
    public const int TamanoPorDefecto = 50;
    public const int TamanoMaximo = 200;

    public async Task<PaginaResultado<ProductoListaDto>> ListarAsync(
        string? buscar, string? estado, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        var estadoCanonico = FiltroEstado.Canonico(estado)
            ?? throw AppException.Validacion("estado", "Usa todos, stockBajo o agotados.");

        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina < 1 ? TamanoPorDefecto : Math.Min(tamanoPagina, TamanoMaximo);
        var texto = string.IsNullOrWhiteSpace(buscar) ? null : buscar.Trim();

        var resultado = await productos.BuscarAsync(texto, estadoCanonico, pagina, tamanoPagina, ct);

        return new PaginaResultado<ProductoListaDto>(
            resultado.Items.Select(ProductoMapeo.ALista).ToList(),
            resultado.Total, pagina, tamanoPagina);
    }

    public async Task<ProductoDetalleDto> ObtenerAsync(int id, bool esAdministrador, CancellationToken ct = default)
    {
        var producto = await productos.ObtenerPorIdAsync(id, ct)
            ?? throw AppException.NoEncontrado("PRODUCTO_NO_ENCONTRADO", "No encontramos ese producto.");

        return ProductoMapeo.ADetalle(producto, incluirCostos: esAdministrador);
    }
}
