using NSubstitute;
using PuntoPyme.Application.Common;
using PuntoPyme.Application.Ventas;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Tests;

public class VentaConsultaServiceTests
{
    private readonly IVentaRepository _repo = Substitute.For<IVentaRepository>();
    private readonly VentaConsultaService _sut;

    public VentaConsultaServiceTests() => _sut = new VentaConsultaService(_repo);

    private static Venta UnaVenta(int id) => new()
    {
        Id = id,
        IdLocal = Guid.NewGuid(),
        FechaVentaUtc = new DateTime(2026, 11, id, 12, 0, 0, DateTimeKind.Utc),
        FechaRegistroUtc = new DateTime(2026, 11, id, 12, 1, 0, DateTimeKind.Utc),
        Usuario = new Usuario { NombreCompleto = "Administrador" },
        MetodoPago = "Efectivo",
        Total = 120m,
        Detalles =
        [
            new VentaDetalle
            {
                Producto = new Producto { Nombre = "Refresco" }, Cantidad = 2,
                PrecioUnitario = 60m, Subtotal = 120m
            }
        ]
    };

    [Fact]
    public async Task Listar_devuelve_las_ventas_mapeadas_con_su_total()
    {
        _repo.ListarAsync(null, null, 1, 50, Arg.Any<CancellationToken>())
            .Returns(new ResultadoVentas([UnaVenta(1), UnaVenta(2)], 137));

        var r = await _sut.ListarAsync(null, null, 1, 50);

        Assert.Equal(137, r.Total);
        Assert.Equal(1, r.Pagina);
        Assert.Equal(50, r.TamanoPagina);
        Assert.Equal("V-000001", r.Items[0].Folio);
        Assert.Equal("Refresco", r.Items[0].Detalles[0].Nombre);
    }

    [Fact]
    public async Task Listar_acota_la_pagina_y_el_tamano()
    {
        _repo.ListarAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ResultadoVentas([], 0));

        await _sut.ListarAsync(null, null, pagina: 0, tamanoPagina: 500);

        await _repo.Received(1).ListarAsync(null, null, 1, 200, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Listar_rechaza_un_rango_invalido()
    {
        var desde = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.ListarAsync(desde, desde, 1, 50));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("hastaUtc", ex.Errores!.Keys);
    }
}
