using NSubstitute;
using PuntoPyme.Application.Common;
using PuntoPyme.Application.Productos;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Tests;

public class ProductoConsultaServiceTests
{
    private readonly IProductoRepository _repo = Substitute.For<IProductoRepository>();
    private readonly ProductoConsultaService _sut;

    public ProductoConsultaServiceTests() => _sut = new ProductoConsultaService(_repo);

    private static Producto Aceite(int stock = 3, int minimo = 5) => new()
    {
        Id = 42, Nombre = "Aceite Vegetal 900ml", Sku = "770123456789", Categoria = "Abarrotes",
        PrecioVenta = 75m, CostoCompra = 58m, Stock = stock, StockMinimo = minimo
    };

    [Theory]
    [InlineData(0, 5, "Agotado")]
    [InlineData(3, 5, "Bajo")]
    [InlineData(5, 5, "Bajo")]
    [InlineData(6, 5, "Normal")]
    public void Estado_de_stock_sigue_la_regla_de_HU_04(int stock, int minimo, string esperado) =>
        Assert.Equal(esperado, EstadoStock.Calcular(stock, minimo));

    [Fact]
    public async Task Listar_mapea_los_productos_con_su_estado_y_el_total()
    {
        _repo.BuscarAsync(null, "todos", 1, 50, Arg.Any<CancellationToken>())
            .Returns(new ResultadoBusqueda([Aceite(stock: 0), Aceite(stock: 40)], 148));

        var r = await _sut.ListarAsync(null, null, 1, 50);

        Assert.Equal(148, r.Total);
        Assert.Equal("Agotado", r.Items[0].EstadoStock);
        Assert.Equal("Normal", r.Items[1].EstadoStock);
    }

    [Fact]
    public async Task Listar_acota_la_pagina_y_el_tamano_y_limpia_la_busqueda()
    {
        _repo.BuscarAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ResultadoBusqueda([], 0));

        var r = await _sut.ListarAsync("  aceite ", "STOCKBAJO", pagina: 0, tamanoPagina: 500);

        Assert.Equal(1, r.Pagina);
        Assert.Equal(200, r.TamanoPagina);
        await _repo.Received(1).BuscarAsync("aceite", "stockBajo", 1, 200, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Listar_con_estado_desconocido_responde_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.ListarAsync(null, "casi", 1, 50));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("estado", ex.Errores!.Keys);
    }

    [Fact]
    public async Task Detalle_para_administrador_incluye_ganancia_y_margen_sobre_el_precio()
    {
        _repo.ObtenerPorIdAsync(42, Arg.Any<CancellationToken>()).Returns(Aceite());

        var d = await _sut.ObtenerAsync(42, esAdministrador: true);

        Assert.Equal(58m, d.CostoCompra);
        Assert.Equal(17.00m, d.Ganancia);
        Assert.Equal(22.7m, d.MargenPorcentaje);
    }

    [Fact]
    public async Task Detalle_para_cajero_no_expone_costos()
    {
        _repo.ObtenerPorIdAsync(42, Arg.Any<CancellationToken>()).Returns(Aceite());

        var d = await _sut.ObtenerAsync(42, esAdministrador: false);

        Assert.Null(d.CostoCompra);
        Assert.Null(d.Ganancia);
        Assert.Null(d.MargenPorcentaje);
        Assert.Equal(75m, d.PrecioVenta);
    }

    [Fact]
    public async Task Detalle_de_un_producto_inexistente_responde_404()
    {
        _repo.ObtenerPorIdAsync(999, Arg.Any<CancellationToken>()).Returns((Producto?)null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.ObtenerAsync(999, true));

        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("PRODUCTO_NO_ENCONTRADO", ex.Code);
    }
}
