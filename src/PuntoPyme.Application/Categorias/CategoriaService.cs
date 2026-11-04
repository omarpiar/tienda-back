using PuntoPyme.Application.Common;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Categorias;

public class CategoriaService(ICategoriaRepository categorias)
{
    public async Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken ct = default)
    {
        var lista = await categorias.ListarAsync(ct);
        return lista.Select(c => new CategoriaDto(c.Id, c.Nombre, c.Productos.Count(p => !p.Eliminado))).ToList();
    }

    public async Task<CategoriaDto> CrearAsync(CategoriaRequest s, CancellationToken ct = default)
    {
        var nombre = Validar(s);

        if (await categorias.ExisteNombreAsync(nombre, null, ct))
            throw AppException.Conflicto("CATEGORIA_DUPLICADA", "Ya existe una categoría con ese nombre.");

        var categoria = new Categoria { Nombre = nombre };
        await categorias.AgregarAsync(categoria, ct);
        return new CategoriaDto(categoria.Id, categoria.Nombre, 0);
    }

    public async Task<CategoriaDto> EditarAsync(int id, CategoriaRequest s, CancellationToken ct = default)
    {
        var categoria = await categorias.ObtenerPorIdParaEditarAsync(id, ct)
            ?? throw AppException.NoEncontrado("CATEGORIA_NO_ENCONTRADA", "No encontramos esa categoría.");

        var nombre = Validar(s);

        if (await categorias.ExisteNombreAsync(nombre, id, ct))
            throw AppException.Conflicto("CATEGORIA_DUPLICADA", "Ya existe otra categoría con ese nombre.");

        categoria.Nombre = nombre;
        await categorias.GuardarCambiosAsync(ct);
        return new CategoriaDto(categoria.Id, categoria.Nombre, categoria.Productos.Count(p => !p.Eliminado));
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var categoria = await categorias.ObtenerPorIdParaEditarAsync(id, ct)
            ?? throw AppException.NoEncontrado("CATEGORIA_NO_ENCONTRADA", "No encontramos esa categoría.");

        if (await categorias.TieneProductosAsync(id, ct))
            throw AppException.Conflicto("CATEGORIA_EN_USO", "No puedes eliminar una categoría que todavía tiene productos.");

        await categorias.EliminarAsync(categoria, ct);
    }

    private static string Validar(CategoriaRequest s)
    {
        var nombre = s.Nombre?.Trim() ?? "";
        if (nombre.Length == 0) throw AppException.Validacion("nombre", "Escribe el nombre de la categoría.");
        if (nombre.Length > 100) throw AppException.Validacion("nombre", "El nombre admite hasta 100 caracteres.");
        return nombre;
    }
}
