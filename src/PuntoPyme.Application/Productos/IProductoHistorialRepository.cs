using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Productos;

public interface IProductoHistorialRepository
{
    Task<IReadOnlyList<ProductoHistorial>> ListarAsync(int productoId, CancellationToken ct = default);
    Task AgregarAsync(ProductoHistorial historial, CancellationToken ct = default);
}
