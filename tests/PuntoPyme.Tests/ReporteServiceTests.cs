using NSubstitute;
using PuntoPyme.Application.Common;
using PuntoPyme.Application.Reportes;

namespace PuntoPyme.Tests;

public class ReporteServiceTests
{
    private readonly IReporteRepository _repo = Substitute.For<IReporteRepository>();
    private readonly ReporteService _sut;
    private static readonly DateTime Desde = new(2026, 10, 26, 6, 0, 0, DateTimeKind.Utc);

    public ReporteServiceTests() => _sut = new ReporteService(_repo);

    [Fact]
    public async Task Calcula_ticket_promedio_y_ordena_por_cantidad_con_desempate_por_nombre()
    {
        _repo.ObtenerResumenAsync(Desde, Desde.AddDays(1), Arg.Any<CancellationToken>())
            .Returns(new ResumenVentasCrudo(100m, 3, [new(1, "Pan", 5), new(2, "Aceite", 5), new(3, "Agua", 9)]));

        var r = await _sut.VentasAsync(Desde, Desde.AddDays(1));

        Assert.Equal(33.33m, r.TicketPromedio);
        Assert.Equal(["Agua", "Aceite", "Pan"], r.ProductosMasVendidos.Select(p => p.Nombre).ToArray());
    }

    [Fact]
    public async Task Sin_ventas_el_ticket_promedio_es_cero()
    {
        _repo.ObtenerResumenAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new ResumenVentasCrudo(0m, 0, []));

        var r = await _sut.VentasAsync(Desde, Desde.AddDays(1));

        Assert.Equal(0m, r.TicketPromedio);
        Assert.Empty(r.ProductosMasVendidos);
    }

    [Fact]
    public async Task Devuelve_como_maximo_los_10_mas_vendidos()
    {
        var muchos = Enumerable.Range(1, 15).Select(i => new ProductoVendidoDto(i, $"P{i:00}", i)).ToList();
        _repo.ObtenerResumenAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new ResumenVentasCrudo(1m, 1, muchos));

        var r = await _sut.VentasAsync(Desde, Desde.AddDays(1));

        Assert.Equal(10, r.ProductosMasVendidos.Count);
        Assert.Equal(15, r.ProductosMasVendidos[0].CantidadVendida);
    }

    [Fact]
    public async Task Rango_invertido_o_mayor_a_366_dias_responde_400()
    {
        var a = await Assert.ThrowsAsync<AppException>(() => _sut.VentasAsync(Desde, Desde.AddDays(-1)));
        var b = await Assert.ThrowsAsync<AppException>(() => _sut.VentasAsync(Desde, Desde.AddDays(400)));
        Assert.Equal(400, a.StatusCode);
        Assert.Equal(400, b.StatusCode);
    }
}
