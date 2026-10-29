using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoPyme.Api.Seguridad;
using PuntoPyme.Application.Usuarios;

namespace PuntoPyme.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Policy = Politicas.SoloAdministrador)]
public class UsuariosController(UsuarioService usuarios) : ControllerBase
{
    /// <summary>API-09 (HU-10): lista usuarios. Nunca devuelve el hash del PIN.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> Listar(
        [FromQuery] string? buscar, [FromQuery] string? rol, [FromQuery] bool? activo, CancellationToken ct) =>
        Ok(await usuarios.ListarAsync(buscar, rol, activo, ct));

    /// <summary>API-10 (HU-10)</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await usuarios.ObtenerAsync(id, ct));

    /// <summary>API-11 (HU-09)</summary>
    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioRequest solicitud, CancellationToken ct)
    {
        var creado = await usuarios.CrearAsync(solicitud, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    /// <summary>API-12 (HU-10): el PIN es opcional.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Editar(int id, EditarUsuarioRequest solicitud, CancellationToken ct) =>
        Ok(await usuarios.EditarAsync(id, solicitud, ct));

    /// <summary>API-13 (HU-10): desactiva o reactiva. No borra nada.</summary>
    [HttpPatch("{id:int}/estado")]
    public async Task<ActionResult<UsuarioDto>> CambiarEstado(int id, CambiarEstadoRequest solicitud, CancellationToken ct) =>
        Ok(await usuarios.CambiarEstadoAsync(id, solicitud.Activo, User.Id(), ct));
}
