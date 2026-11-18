using PuntoPyme.Application.Common;

namespace PuntoPyme.Application.Ventas;

public class VentaConsultaService(IVentaRepository ventas)
{
    public const int TamanoPorDefecto = 50;
    public const int TamanoMaximo = 200;
    public const int DiasMaximos = 366;

    public async Task<HistorialVentasDto> ListarAsync(
        DateTime? desdeUtc, DateTime? hastaUtc, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        var desde = Normalizar(desdeUtc);
        var hasta = Normalizar(hastaUtc);

        if (desde is not null && hasta is not null)
        {
            if (hasta <= desde)
                throw AppException.Validacion("hastaUtc", "La fecha final debe ser mayor que la inicial.");
            if ((hasta.Value - desde.Value).TotalDays > DiasMaximos)
                throw AppException.Validacion("hastaUtc", $"El rango no puede pasar de {DiasMaximos} días.");
        }

        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina < 1 ? TamanoPorDefecto : Math.Min(tamanoPagina, TamanoMaximo);

        var resultado = await ventas.ListarAsync(desde, hasta, pagina, tamanoPagina, ct);

        return new HistorialVentasDto(
            resultado.Items.Select(VentaMapeo.ADto).ToList(), resultado.Total, pagina, tamanoPagina);
    }

    private static DateTime? Normalizar(DateTime? fecha)
    {
        if (fecha is null) return null;
        var f = fecha.Value;
        return f.Kind switch
        {
            DateTimeKind.Utc => f,
            DateTimeKind.Local => f.ToUniversalTime(),
            _ => DateTime.SpecifyKind(f, DateTimeKind.Utc)
        };
    }
}
