using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoPyme.Api.Seguridad;
using PuntoPyme.Application.Reportes;

namespace PuntoPyme.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Policy = Politicas.SoloAdministrador)]
public class ReportesController(ReporteService reportes) : ControllerBase
{
    /// <summary>API-08 (HU-08): el cliente manda el rango en UTC.</summary>
    [HttpGet("ventas")]
    public async Task<ActionResult<ReporteVentasDto>> Ventas(
        [FromQuery] DateTimeOffset desdeUtc, [FromQuery] DateTimeOffset hastaUtc, CancellationToken ct) =>
        Ok(await reportes.VentasAsync(desdeUtc.UtcDateTime, hastaUtc.UtcDateTime, ct));
}
