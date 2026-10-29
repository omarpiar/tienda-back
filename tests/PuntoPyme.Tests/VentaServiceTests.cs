using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using PuntoPyme.Application.Common;
using PuntoPyme.Application.Productos;
using PuntoPyme.Application.Ventas;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Tests;

public class VentaServiceTests
{
    private readonly IVentaRepository _ventas = Substitute.For<IVentaRepository>();
    private readonly IProductoRepository _productos = Substitute.For<IProductoRepository>();
    private readonly FakeTimeProvider _reloj = new(new DateTimeOffset(2026, 10, 26, 18, 0, 0, TimeSpan.Zero));
    private readonly VentaService _sut;
    private Venta? _capturada;

    private readonly Producto _refresco = new() { Id = 1, Nombre = "Refresco", Sku = "A", PrecioVenta = 18m, Stock = 40 };
    private readonly Producto _aceite = new() { Id = 2, Nombre = "Aceite", Sku = "B", PrecioVenta = 75m, Stock = 3 };

    public VentaServiceTests()
    {
        _sut = new VentaService(_ventas, _productos, _reloj);
        _productos.ObtenerPorIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(ci => (IReadOnlyList<Producto>)new[] { _refresco, _aceite }.Where(p => ci.Arg<IReadOnlyCollection<int>>().Contains(p.Id)).ToList());

        _ventas.RegistrarAsync(Arg.Do<Venta>(v => _capturada = v), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var v = ci.Arg<Venta>();
                v.Id = 842;
                v.Usuario = new Usuario { NombreCompleto = "Laura Gómez" };
                foreach (var d in v.Detalles) d.Producto = d.ProductoId == 1 ? _refresco : _aceite;
                return new RegistroVentaResultado(v, true, []);
            });
    }

    private RegistrarVentaRequest Solicitud(string metodo = "Efectivo", decimal? recibido = 200m, params DetalleVentaRequest[] detalles) =>
        new(Guid.NewGuid(), _reloj.GetUtcNow().UtcDateTime.AddMinutes(-2), metodo, recibido,
            detalles.Length > 0 ? detalles.ToList() : [new(1, 2), new(2, 1)]);

    [Fact]
    public async Task Venta_exitosa_calcula_total_y_cambio_con_el_precio_del_servidor()
    {
        var r = await _sut.RegistrarAsync(Solicitud(), usuarioId: 7);

        Assert.True(r.Creada);
        Assert.Equal(111m, r.Venta.Total);          // 2 x 18 + 75
        Assert.Equal(89m, r.Venta.Cambio);
        Assert.Equal("V-000842", r.Venta.Folio);
        Assert.Equal(7, _capturada!.UsuarioId);
        Assert.Equal(2, _capturada.Detalles.Count);
    }

    [Fact]
    public async Task Renglones_repetidos_del_mismo_producto_se_suman()
    {
        await _sut.RegistrarAsync(Solicitud(recibido: 500m, detalles: [new(1, 2), new(1, 3)]), 7);

        var renglon = Assert.Single(_capturada!.Detalles);
        Assert.Equal(5, renglon.Cantidad);
    }

    [Fact]
    public async Task Con_tarjeta_no_hay_monto_recibido_ni_cambio()
    {
        var r = await _sut.RegistrarAsync(Solicitud("Tarjeta", null), 7);
        Assert.Null(r.Venta.MontoRecibido);
        Assert.Null(r.Venta.Cambio);
    }

    [Fact]
    public async Task Efectivo_con_monto_menor_al_total_responde_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.RegistrarAsync(Solicitud(recibido: 50m), 7));
        Assert.Equal("MONTO_INSUFICIENTE", ex.Code);
        await _ventas.DidNotReceive().RegistrarAsync(Arg.Any<Venta>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cantidad_mayor_al_stock_responde_409_con_los_faltantes_y_no_guarda()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.RegistrarAsync(Solicitud(detalles: [new(2, 5)]), 7));

        Assert.Equal(409, ex.StatusCode);
        Assert.Equal("STOCK_INSUFICIENTE", ex.Code);
        var faltantes = (IReadOnlyList<StockFaltante>)ex.Extensiones!["faltantes"]!;
        Assert.Equal(new StockFaltante(2, "Aceite", 3, 5), faltantes[0]);
        await _ventas.DidNotReceive().RegistrarAsync(Arg.Any<Venta>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Si_el_repositorio_detecta_falta_de_stock_por_carrera_tambien_responde_409()
    {
        _ventas.RegistrarAsync(Arg.Any<Venta>(), Arg.Any<CancellationToken>())
            .Returns(new RegistroVentaResultado(null, false, [new StockFaltante(2, "Aceite", 0, 1)]));

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.RegistrarAsync(Solicitud(), 7));
        Assert.Equal("STOCK_INSUFICIENTE", ex.Code);
    }

    [Fact]
    public async Task Mismo_idLocal_devuelve_la_venta_existente_sin_volver_a_registrar()
    {
        var existente = new Venta
        {
            Id = 10, IdLocal = Guid.NewGuid(), MetodoPago = "Tarjeta", Total = 18m,
            Usuario = new Usuario { NombreCompleto = "Laura" },
            Detalles = [new VentaDetalle { ProductoId = 1, Producto = _refresco, Cantidad = 1, PrecioUnitario = 18m, Subtotal = 18m }]
        };
        _ventas.ObtenerPorIdLocalAsync(existente.IdLocal, Arg.Any<CancellationToken>()).Returns(existente);

        var r = await _sut.RegistrarAsync(Solicitud() with { IdLocal = existente.IdLocal }, 7);

        Assert.False(r.Creada);
        Assert.Equal(10, r.Venta.Id);
        await _ventas.DidNotReceive().RegistrarAsync(Arg.Any<Venta>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Carrito_vacio_responde_400_CARRITO_VACIO()
    {
        var s = Solicitud() with { Detalles = [] };
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.RegistrarAsync(s, 7));
        Assert.Equal("CARRITO_VACIO", ex.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Cantidad_no_positiva_responde_400(int cantidad)
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.RegistrarAsync(Solicitud(detalles: [new(1, cantidad)]), 7));
        Assert.Equal("VALIDACION", ex.Code);
    }

    [Fact]
    public async Task Fecha_de_venta_en_el_futuro_responde_400()
    {
        var s = Solicitud() with { FechaVentaUtc = _reloj.GetUtcNow().UtcDateTime.AddHours(1) };
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.RegistrarAsync(s, 7));
        Assert.Contains("fechaVentaUtc", ex.Errores!.Keys);
    }

    [Fact]
    public async Task Producto_inexistente_responde_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.RegistrarAsync(Solicitud(detalles: [new(99, 1)]), 7));
        Assert.Equal("PRODUCTO_NO_ENCONTRADO", ex.Code);
    }

    [Fact]
    public async Task Metodo_de_pago_invalido_responde_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.RegistrarAsync(Solicitud("Cheque"), 7));
        Assert.Contains("metodoPago", ex.Errores!.Keys);
    }
}
