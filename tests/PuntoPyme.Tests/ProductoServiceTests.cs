using NSubstitute;
using PuntoPyme.Application.Common;
using PuntoPyme.Application.Productos;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Tests;

public class ProductoServiceTests
{
    private readonly IProductoRepository _repo = Substitute.For<IProductoRepository>();
    private readonly ProductoService _sut;

    public ProductoServiceTests() => _sut = new ProductoService(_repo);

    [Fact]
    public async Task Crear_guarda_el_producto_con_minimo_por_defecto_y_devuelve_margen()
    {
        Producto? guardado = null;
        await _repo.AgregarAsync(Arg.Do<Producto>(p => guardado = p), Arg.Any<CancellationToken>());

        var d = await _sut.CrearAsync(new("Galletas", " G-1 ", null, 22m, 16m, 30, null));

        Assert.Equal("G-1", guardado!.Sku);
        Assert.Equal(5, guardado.StockMinimo);
        Assert.Equal(30, guardado.Stock);
        Assert.Equal(27.3m, d.MargenPorcentaje);
    }

    [Fact]
    public async Task Crear_con_sku_repetido_responde_409()
    {
        _repo.ExisteSkuAsync("G-1", null, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.CrearAsync(new("Galletas", "G-1", null, 22m, 16m, 0, null)));

        Assert.Equal(409, ex.StatusCode);
        Assert.Equal("SKU_DUPLICADO", ex.Code);
        await _repo.DidNotReceive().AgregarAsync(Arg.Any<Producto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_con_precio_menor_al_costo_responde_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.CrearAsync(new("X", "X-1", null, 10m, 20m, 0, null)));
        Assert.Equal("PRECIO_MENOR_AL_COSTO", ex.Code);
    }

    [Fact]
    public async Task Crear_sin_nombre_ni_precio_lista_todos_los_errores()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.CrearAsync(new(" ", "X-1", null, null, null, null, null)));
        Assert.Equal("VALIDACION", ex.Code);
        Assert.Contains("nombre", ex.Errores!.Keys);
        Assert.Contains("precioVenta", ex.Errores.Keys);
    }

    [Fact]
    public async Task Editar_no_modifica_el_stock_aunque_el_cliente_lo_mande()
    {
        var p = new Producto { Id = 1, Nombre = "Viejo", Sku = "A", PrecioVenta = 10, CostoCompra = 5, Stock = 30, StockMinimo = 5 };
        _repo.ObtenerPorIdParaEditarAsync(1, Arg.Any<CancellationToken>()).Returns(p);

        await _sut.EditarAsync(1, new("Nuevo", "A", "Cat", 12m, 6m, 9999, 3));

        Assert.Equal("Nuevo", p.Nombre);
        Assert.Equal(12m, p.PrecioVenta);
        Assert.Equal(3, p.StockMinimo);
        Assert.Equal(30, p.Stock);
        await _repo.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Editar_con_sku_de_otro_producto_responde_409()
    {
        _repo.ObtenerPorIdParaEditarAsync(1, Arg.Any<CancellationToken>()).Returns(new Producto { Id = 1, Sku = "A" });
        _repo.ExisteSkuAsync("B", 1, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.EditarAsync(1, new("N", "B", null, 10m, 5m, null, null)));
        Assert.Equal("SKU_DUPLICADO", ex.Code);
    }

    [Fact]
    public async Task Editar_producto_inexistente_responde_404()
    {
        _repo.ObtenerPorIdParaEditarAsync(9, Arg.Any<CancellationToken>()).Returns((Producto?)null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.EditarAsync(9, new("N", "B", null, 10m, 5m, null, null)));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task Por_sku_devuelve_el_producto_o_404()
    {
        _repo.ObtenerPorSkuAsync("7501", Arg.Any<CancellationToken>())
            .Returns(new Producto { Id = 3, Nombre = "Agua", Sku = "7501", PrecioVenta = 12, Stock = 0, StockMinimo = 5 });

        var ok = await _sut.ObtenerPorSkuAsync(" 7501 ");
        Assert.Equal("Agotado", ok.EstadoStock);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.ObtenerPorSkuAsync("nope"));
        Assert.Equal("PRODUCTO_NO_ENCONTRADO", ex.Code);
    }
}
