using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoPyme.Api.Seguridad;
using PuntoPyme.Application.Ventas;

namespace PuntoPyme.Api.Controllers;

[ApiController]
[Route("api/ventas")]
[Authorize(Policy = Politicas.Autenticado)]
public class VentasController(VentaService ventas, VentaConsultaService consulta) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    public async Task<ActionResult<HistorialVentasDto>> Listar(
        [FromQuery] DateTime? desdeUtc, [FromQuery] DateTime? hastaUtc,
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = VentaConsultaService.TamanoPorDefecto,
        CancellationToken ct = default) =>
        Ok(await consulta.ListarAsync(desdeUtc, hastaUtc, pagina, tamanoPagina, ct));

    /// <summary>API-07 (HU-03, HU-07): registra y cobra una venta; también sincroniza las hechas sin conexión.</summary>
    [HttpPost]
    public async Task<ActionResult<VentaDto>> Registrar(RegistrarVentaRequest solicitud, CancellationToken ct)
    {
        var r = await ventas.RegistrarAsync(solicitud, User.Id(), ct);
        return r.Creada ? StatusCode(StatusCodes.Status201Created, r.Venta) : Ok(r.Venta);
    }
}
