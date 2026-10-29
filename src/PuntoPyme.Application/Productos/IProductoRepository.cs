using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Productos;

public record ResultadoBusqueda(IReadOnlyList<Producto> Items, int Total);

public interface IProductoRepository
{
    /// <param name="estado">Ya canónico: todos, stockBajo o agotados.</param>
    Task<ResultadoBusqueda> BuscarAsync(string? buscar, string estado, int pagina, int tamanoPagina, CancellationToken ct = default);

    /// <summary>Sin rastreo. Para consultas.</summary>
    Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Con rastreo, para editar y luego llamar GuardarCambiosAsync.</summary>
    Task<Producto?> ObtenerPorIdParaEditarAsync(int id, CancellationToken ct = default);

    /// <summary>Búsqueda exacta, sin distinguir mayúsculas.</summary>
    Task<Producto?> ObtenerPorSkuAsync(string sku, CancellationToken ct = default);

    Task<bool> ExisteSkuAsync(string sku, int? excluirId, CancellationToken ct = default);
    Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    Task AgregarAsync(Producto producto, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
