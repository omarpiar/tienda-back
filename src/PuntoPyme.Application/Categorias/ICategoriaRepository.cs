using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Categorias;

public interface ICategoriaRepository
{
    Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default);
    Task<Categoria?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<Categoria?> ObtenerPorIdParaEditarAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct = default);
    Task<bool> TieneProductosAsync(int id, CancellationToken ct = default);
    Task AgregarAsync(Categoria categoria, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
    Task EliminarAsync(Categoria categoria, CancellationToken ct = default);
}
