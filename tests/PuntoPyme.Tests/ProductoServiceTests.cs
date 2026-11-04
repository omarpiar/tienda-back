using NSubstitute;
using PuntoPyme.Application.Categorias;
using PuntoPyme.Application.Common;
using PuntoPyme.Application.Productos;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Tests;

public class ProductoServiceTests
{
    private readonly IProductoRepository _repo = Substitute.For<IProductoRepository>();
    private readonly ICategoriaRepository _categorias = Substitute.For<ICategoriaRepository>();
    private readonly IProductoHistorialRepository _historial = Substitute.For<IProductoHistorialRepository>();
    private readonly ProductoService _sut;

    public ProductoServiceTests() =>
        _sut = new ProductoService(_repo, _categorias, _historial, TimeProvider.System);

    [Fact]
    public async Task Crear_guarda_el_producto_con_minimo_por_defecto_y_devuelve_margen()
    {
        Producto? guardado = null;
        await _repo.AgregarAsync(Arg.Do<Producto>(p => guardado = p), Arg.Any<CancellationToken>());

        var d = await _sut.CrearAsync(new("Galletas", " G-1 ", null, 22m, 16m, 30, null), usuarioId: 7);

        Assert.Equal("G-1", guardado!.Sku);
        Assert.Equal(5, guardado.StockMinimo);
        Assert.Equal(30, guardado.Stock);
        Assert.Equal(27.3m, d.MargenPorcentaje);
        await _historial.Received(1).AgregarAsync(
            Arg.Is<ProductoHistorial>(h => h.Accion == AccionesProducto.Creacion && h.UsuarioId == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_con_sku_repetido_responde_409()
    {
        _repo.ExisteSkuAsync("G-1", null, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.CrearAsync(new("Galletas", "G-1", null, 22m, 16m, 0, null), usuarioId: 7));

        Assert.Equal(409, ex.StatusCode);
        Assert.Equal("SKU_DUPLICADO", ex.Code);
        await _repo.DidNotReceive().AgregarAsync(Arg.Any<Producto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_con_categoria_inexistente_responde_400()
    {
        _categorias.ObtenerPorIdParaEditarAsync(99, Arg.Any<CancellationToken>()).Returns((Categoria?)null);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.CrearAsync(new("Galletas", "G-1", 99, 22m, 16m, 0, null), usuarioId: 7));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("categoriaId", ex.Errores!.Keys);
        await _repo.DidNotReceive().AgregarAsync(Arg.Any<Producto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_con_precio_menor_al_costo_responde_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.CrearAsync(new("X", "X-1", null, 10m, 20m, 0, null), usuarioId: 7));
        Assert.Equal("PRECIO_MENOR_AL_COSTO", ex.Code);
    }

    [Fact]
    public async Task Crear_sin_nombre_ni_precio_lista_todos_los_errores()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.CrearAsync(new(" ", "X-1", null, null, null, null, null), usuarioId: 7));
        Assert.Equal("VALIDACION", ex.Code);
        Assert.Contains("nombre", ex.Errores!.Keys);
        Assert.Contains("precioVenta", ex.Errores.Keys);
    }

    [Fact]
    public async Task Editar_no_modifica_el_stock_aunque_el_cliente_lo_mande()
    {
        var p = new Producto { Id = 1, Nombre = "Viejo", Sku = "A", PrecioVenta = 10, CostoCompra = 5, Stock = 30, StockMinimo = 5 };
        _repo.ObtenerPorIdParaEditarAsync(1, Arg.Any<CancellationToken>()).Returns(p);

        await _sut.EditarAsync(1, new("Nuevo", "A", null, 12m, 6m, 9999, 3), usuarioId: 7);

        Assert.Equal("Nuevo", p.Nombre);
        Assert.Equal(12m, p.PrecioVenta);
        Assert.Equal(3, p.StockMinimo);
        Assert.Equal(30, p.Stock);
        await _repo.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
        await _historial.Received(1).AgregarAsync(
            Arg.Is<ProductoHistorial>(h => h.Accion == AccionesProducto.Actualizacion),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Editar_con_sku_de_otro_producto_responde_409()
    {
        _repo.ObtenerPorIdParaEditarAsync(1, Arg.Any<CancellationToken>()).Returns(new Producto { Id = 1, Sku = "A" });
        _repo.ExisteSkuAsync("B", 1, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.EditarAsync(1, new("N", "B", null, 10m, 5m, null, null), usuarioId: 7));
        Assert.Equal("SKU_DUPLICADO", ex.Code);
    }

    [Fact]
    public async Task Editar_producto_inexistente_responde_404()
    {
        _repo.ObtenerPorIdParaEditarAsync(9, Arg.Any<CancellationToken>()).Returns((Producto?)null);
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.EditarAsync(9, new("N", "B", null, 10m, 5m, null, null), usuarioId: 7));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task Reponer_stock_suma_existencias_y_registra_historial()
    {
        var p = new Producto { Id = 3, Nombre = "Agua", Sku = "7501", PrecioVenta = 12, CostoCompra = 8, Stock = 3, StockMinimo = 5 };
        _repo.ObtenerPorIdParaEditarAsync(3, Arg.Any<CancellationToken>()).Returns(p);

        var d = await _sut.ReponerStockAsync(3, new(10), usuarioId: 7);

        Assert.Equal(13, p.Stock);
        Assert.Equal(13, d.Stock);
        await _historial.Received(1).AgregarAsync(
            Arg.Is<ProductoHistorial>(h =>
                h.Accion == AccionesProducto.ReposicionStock && h.StockAnterior == 3 && h.StockNuevo == 13),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reponer_con_cantidad_invalida_responde_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.ReponerStockAsync(3, new(0), usuarioId: 7));
        Assert.Contains("cantidad", ex.Errores!.Keys);
    }

    [Fact]
    public async Task Eliminar_marca_el_producto_sin_borrarlo_y_registra_historial()
    {
        var p = new Producto { Id = 3, Nombre = "Agua", Sku = "7501", Stock = 3 };
        _repo.ObtenerPorIdParaEditarAsync(3, Arg.Any<CancellationToken>()).Returns(p);

        await _sut.EliminarAsync(3, usuarioId: 7);

        Assert.True(p.Eliminado);
        await _repo.DidNotReceive().AgregarAsync(Arg.Any<Producto>(), Arg.Any<CancellationToken>());
        await _historial.Received(1).AgregarAsync(
            Arg.Is<ProductoHistorial>(h => h.Accion == AccionesProducto.Eliminacion),
            Arg.Any<CancellationToken>());
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

    [Fact]
    public async Task Crear_con_categoria_valida_asigna_la_categoria()
    {
        var categoria = new Categoria { Id = 4, Nombre = "Bebidas" };
        _categorias.ObtenerPorIdParaEditarAsync(4, Arg.Any<CancellationToken>()).Returns(categoria);

        var d = await _sut.CrearAsync(new("Agua", "A-1", 4, 12m, 8m, 3, 5), usuarioId: 7);

        Assert.Equal(4, d.CategoriaId);
        Assert.Equal("Bebidas", d.Categoria);
    }

    [Fact]
    public async Task Crear_con_categoria_id_invalido_responde_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.CrearAsync(new("Agua", "A-1", 0, 12m, 8m, 3, 5), usuarioId: 7));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("categoriaId", ex.Errores!.Keys);
    }

    [Fact]
    public async Task Editar_sin_cambios_registra_la_descripcion_correspondiente()
    {
        var p = new Producto
        {
            Id = 1,
            Nombre = "Agua",
            Sku = "A",
            PrecioVenta = 10m,
            CostoCompra = 5m,
            Stock = 30,
            StockMinimo = 5
        };
        _repo.ObtenerPorIdParaEditarAsync(1, Arg.Any<CancellationToken>()).Returns(p);
        ProductoHistorial? registro = null;
        await _historial.AgregarAsync(Arg.Do<ProductoHistorial>(h => registro = h), Arg.Any<CancellationToken>());

        await _sut.EditarAsync(1, new("Agua", "A", null, 10m, 5m, 9999, 5), usuarioId: 7);

        Assert.Equal("Sin cambios en los datos.", registro!.Descripcion);
    }

    [Fact]
    public async Task Obtener_historial_mapea_los_registros_y_el_usuario()
    {
        _repo.ObtenerPorIdAsync(3, Arg.Any<CancellationToken>()).Returns(new Producto { Id = 3 });
        _historial.ListarAsync(3, Arg.Any<CancellationToken>()).Returns(
        [
            new ProductoHistorial
            {
                Id = 10,
                Accion = AccionesProducto.ReposicionStock,
                Descripcion = "Se repusieron 5 unidades.",
                FechaUtc = new DateTime(2026, 11, 4, 8, 0, 0, DateTimeKind.Utc),
                StockAnterior = 1,
                StockNuevo = 6,
                UsuarioId = 7,
                Usuario = new Usuario { NombreCompleto = "Ana Pérez" }
            },
            new ProductoHistorial
            {
                Id = 11,
                Accion = AccionesProducto.Creacion,
                Descripcion = "Producto creado.",
                FechaUtc = new DateTime(2026, 11, 3, 8, 0, 0, DateTimeKind.Utc),
                StockAnterior = 0,
                StockNuevo = 1,
                UsuarioId = 8,
                Usuario = null!
            }
        ]);

        var historial = await _sut.ObtenerHistorialAsync(3);

        Assert.Equal(2, historial.Count);
        Assert.Equal("Ana Pérez", historial[0].Usuario);
        Assert.Equal(6, historial[0].StockNuevo);
        Assert.Equal("", historial[1].Usuario);
    }

    [Fact]
    public async Task Obtener_historial_de_un_producto_inexistente_responde_404()
    {
        _repo.ObtenerPorIdAsync(9, Arg.Any<CancellationToken>()).Returns((Producto?)null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.ObtenerHistorialAsync(9));

        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("PRODUCTO_NO_ENCONTRADO", ex.Code);
    }
}
