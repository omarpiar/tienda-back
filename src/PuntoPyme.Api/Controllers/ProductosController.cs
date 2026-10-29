using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoPyme.Api.Seguridad;
using PuntoPyme.Application.Productos;
using PuntoPyme.Domain.Constants;

namespace PuntoPyme.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize(Policy = Politicas.Autenticado)]
public class ProductosController(ProductoConsultaService consulta, ProductoService productos) : ControllerBase
{
    /// <summary>API-02 (HU-04): lista, busca y filtra productos. No devuelve costoCompra.</summary>
    [HttpGet]
    public async Task<ActionResult<PaginaResultado<ProductoListaDto>>> Listar(
        [FromQuery] string? buscar,
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = ProductoConsultaService.TamanoPorDefecto,
        CancellationToken ct = default) =>
        Ok(await consulta.ListarAsync(buscar, estado, pagina, tamanoPagina, ct));

    /// <summary>API-03 (HU-04): detalle. Costo, ganancia y margen solo para el rol activo Administrador.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDetalleDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await consulta.ObtenerAsync(id, User.IsInRole(Roles.Administrador), ct));

    /// <summary>API-04 (HU-02): búsqueda exacta por el código que lee el escáner.</summary>
    [HttpGet("sku/{sku}")]
    public async Task<ActionResult<ProductoListaDto>> PorSku(string sku, CancellationToken ct) =>
        Ok(await productos.ObtenerPorSkuAsync(sku, ct));

    /// <summary>API-05 (HU-05): crea un producto.</summary>
    [HttpPost]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    public async Task<ActionResult<ProductoDetalleDto>> Crear(ProductoRequest solicitud, CancellationToken ct)
    {
        var creado = await productos.CrearAsync(solicitud, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    /// <summary>API-06 (HU-06): edita un producto. No recibe stock.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    public async Task<ActionResult<ProductoDetalleDto>> Editar(int id, ProductoRequest solicitud, CancellationToken ct) =>
        Ok(await productos.EditarAsync(id, solicitud, ct));
}
