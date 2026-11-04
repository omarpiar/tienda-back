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
    public async Task Eliminar_una_categoria_en_uso_responde_409()
    {
        _repo.ObtenerPorIdParaEditarAsync(1, Arg.Any<CancellationToken>()).Returns(new Categoria { Id = 1, Nombre = "Bebidas" });
        _repo.TieneProductosAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.EliminarAsync(1));

        Assert.Equal("CATEGORIA_EN_USO", ex.Code);
        await _repo.DidNotReceive().EliminarAsync(Arg.Any<Categoria>(), Arg.Any<CancellationToken>());
    }
}
