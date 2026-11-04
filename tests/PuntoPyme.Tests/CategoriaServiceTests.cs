using NSubstitute;
using PuntoPyme.Application.Categorias;
using PuntoPyme.Application.Common;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Tests;

public class CategoriaServiceTests
{
    private readonly ICategoriaRepository _repo = Substitute.For<ICategoriaRepository>();
    private readonly CategoriaService _sut;

    public CategoriaServiceTests() => _sut = new CategoriaService(_repo);

    [Fact]
    public async Task Crear_guarda_el_nombre_limpio()
    {
        Categoria? guardada = null;
        await _repo.AgregarAsync(Arg.Do<Categoria>(c => guardada = c), Arg.Any<CancellationToken>());

        var d = await _sut.CrearAsync(new("  Bebidas  "));

        Assert.Equal("Bebidas", guardada!.Nombre);
        Assert.Equal("Bebidas", d.Nombre);
    }

    [Fact]
    public async Task Crear_con_nombre_repetido_responde_409()
    {
        _repo.ExisteNombreAsync("Bebidas", null, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.CrearAsync(new("Bebidas")));

        Assert.Equal("CATEGORIA_DUPLICADA", ex.Code);
        await _repo.DidNotReceive().AgregarAsync(Arg.Any<Categoria>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Listar_cuenta_solo_productos_activos()
    {
        _repo.ListarAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new Categoria
            {
                Id = 1,
                Nombre = "Bebidas",
                Productos = [new Producto(), new Producto { Eliminado = true }]
            },
            new Categoria { Id = 2, Nombre = "Vacía" }
        ]);

        var lista = await _sut.ListarAsync();

        Assert.Equal(2, lista.Count);
        Assert.Equal(1, lista[0].Productos);
        Assert.Equal(0, lista[1].Productos);
    }

    [Fact]
    public async Task Editar_actualiza_el_nombre_y_cuenta_solo_productos_activos()
    {
        var categoria = new Categoria
        {
            Id = 3,
            Nombre = "Viejo",
            Productos = [new Producto(), new Producto { Eliminado = true }]
        };
        _repo.ObtenerPorIdParaEditarAsync(3, Arg.Any<CancellationToken>()).Returns(categoria);

        var d = await _sut.EditarAsync(3, new("  Nuevo  "));

        Assert.Equal("Nuevo", categoria.Nombre);
        Assert.Equal(1, d.Productos);
        await _repo.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Editar_una_categoria_inexistente_responde_404()
    {
        _repo.ObtenerPorIdParaEditarAsync(9, Arg.Any<CancellationToken>()).Returns((Categoria?)null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.EditarAsync(9, new("Bebidas")));

        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("CATEGORIA_NO_ENCONTRADA", ex.Code);
    }

    [Fact]
    public async Task Editar_con_nombre_repetido_responde_409()
    {
        var categoria = new Categoria { Id = 3, Nombre = "Bebidas" };
        _repo.ObtenerPorIdParaEditarAsync(3, Arg.Any<CancellationToken>()).Returns(categoria);
        _repo.ExisteNombreAsync("Bebidas", 3, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.EditarAsync(3, new("Bebidas")));

        Assert.Equal("CATEGORIA_DUPLICADA", ex.Code);
        await _repo.DidNotReceive().GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_con_nombre_vacio_responde_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.CrearAsync(new("   ")));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("nombre", ex.Errores!.Keys);
    }

    [Fact]
    public async Task Crear_con_nombre_muy_largo_responde_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.CrearAsync(new(new string('A', 101))));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("nombre", ex.Errores!.Keys);
    }

    [Fact]
    public async Task Eliminar_una_categoria_sin_productos_la_elimina()
    {
        var categoria = new Categoria { Id = 1, Nombre = "Bebidas" };
        _repo.ObtenerPorIdParaEditarAsync(1, Arg.Any<CancellationToken>()).Returns(categoria);
        _repo.TieneProductosAsync(1, Arg.Any<CancellationToken>()).Returns(false);

        await _sut.EliminarAsync(1);

        await _repo.Received(1).EliminarAsync(categoria, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Eliminar_una_categoria_inexistente_responde_404()
    {
        _repo.ObtenerPorIdParaEditarAsync(9, Arg.Any<CancellationToken>()).Returns((Categoria?)null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.EliminarAsync(9));

        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("CATEGORIA_NO_ENCONTRADA", ex.Code);
    }

    [Fact]
    public async Task Eliminar_una_categoria_en_uso_responde_409()
    {
        _repo.ObtenerPorIdParaEditarAsync(1, Arg.Any<CancellationToken>()).Returns(new Categoria { Id = 1, Nombre = "Bebidas" });
        _repo.TieneProductosAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.EliminarAsync(1));

        Assert.Equal("CATEGORIA_EN_USO", ex.Code);
        await _repo.DidNotReceive().EliminarAsync(Arg.Any<Categoria>(), Arg.Any<CancellationToken>());
    }
}
