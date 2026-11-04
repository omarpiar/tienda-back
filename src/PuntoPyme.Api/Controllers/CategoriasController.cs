using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoPyme.Api.Seguridad;
using PuntoPyme.Application.Categorias;

namespace PuntoPyme.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize(Policy = Politicas.Autenticado)]
public class CategoriasController(CategoriaService categorias) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Listar(CancellationToken ct) =>
        Ok(await categorias.ListarAsync(ct));

    [HttpPost]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    public async Task<ActionResult<CategoriaDto>> Crear(CategoriaRequest solicitud, CancellationToken ct)
    {
        var creada = await categorias.CrearAsync(solicitud, ct);
        return Created($"/api/categorias/{creada.Id}", creada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    public async Task<ActionResult<CategoriaDto>> Editar(int id, CategoriaRequest solicitud, CancellationToken ct) =>
        Ok(await categorias.EditarAsync(id, solicitud, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await categorias.EliminarAsync(id, ct);
        return NoContent();
    }
}
