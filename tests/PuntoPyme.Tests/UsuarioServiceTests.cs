using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using PuntoPyme.Application.Auth;
using PuntoPyme.Application.Common;
using PuntoPyme.Application.Usuarios;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Tests;

public class UsuarioServiceTests
{
    private sealed class HasherFalso : IPinHasher
    {
        public string Hash(string pin) => "hash:" + pin;
        public bool Verificar(string pin, string hash) => hash == "hash:" + pin;
    }

    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly FakeTimeProvider _reloj = new(new DateTimeOffset(2026, 10, 26, 18, 0, 0, TimeSpan.Zero));
    private readonly UsuarioService _sut;

    public UsuarioServiceTests() => _sut = new UsuarioService(_repo, new HasherFalso(), _reloj);

    private Usuario Existente(int id = 5, int rolId = Roles.CajeroId, bool activo = true)
    {
        var u = new Usuario { Id = id, NombreCompleto = "Laura", NombreUsuario = "laura", PinHash = "hash:1234", RolId = rolId, Activo = activo };
        _repo.ObtenerPorIdAsync(id, Arg.Any<CancellationToken>()).Returns(u);
        return u;
    }

    [Fact]
    public async Task Crear_normaliza_el_usuario_guarda_el_hash_y_nunca_el_pin()
    {
        Usuario? guardado = null;
        await _repo.AgregarAsync(Arg.Do<Usuario>(u => guardado = u), Arg.Any<CancellationToken>());

        var dto = await _sut.CrearAsync(new("Ana Pérez", "  ANA ", "4821", Roles.Cajero));

        Assert.Equal("ana", guardado!.NombreUsuario);
        Assert.Equal("hash:4821", guardado.PinHash);
        Assert.True(guardado.Activo);
        Assert.Equal(Roles.Cajero, dto.Rol);
    }

    [Fact]
    public async Task Crear_con_usuario_repetido_responde_409()
    {
        _repo.ExisteNombreUsuarioAsync("ana", null, Arg.Any<CancellationToken>()).Returns(true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.CrearAsync(new("Ana", "ana", "4821", Roles.Cajero)));
        Assert.Equal("USUARIO_DUPLICADO", ex.Code);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567")]
    [InlineData("12ab")]
    public async Task Crear_con_pin_invalido_responde_400(string pin)
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.CrearAsync(new("Ana", "ana", pin, Roles.Cajero)));
        Assert.Contains("pin", ex.Errores!.Keys);
    }

    [Fact]
    public async Task Editar_sin_pin_conserva_el_actual()
    {
        var u = Existente();
        await _sut.EditarAsync(5, new("Laura G.", "laura", Roles.Cajero, null));
        Assert.Equal("Laura G.", u.NombreCompleto);
        Assert.Equal("hash:1234", u.PinHash);
    }

    [Fact]
    public async Task Editar_con_pin_nuevo_lo_rehashea_y_desbloquea()
    {
        var u = Existente();
        u.IntentosFallidos = 4;
        u.BloqueadoHastaUtc = _reloj.GetUtcNow().UtcDateTime.AddMinutes(1);

        await _sut.EditarAsync(5, new("Laura", "laura", Roles.Cajero, "9999"));

        Assert.Equal("hash:9999", u.PinHash);
        Assert.Equal(0, u.IntentosFallidos);
        Assert.Null(u.BloqueadoHastaUtc);
    }

    [Fact]
    public async Task No_se_puede_quitar_el_rol_al_ultimo_administrador_activo()
    {
        Existente(rolId: Roles.AdministradorId);
        _repo.ContarAdministradoresActivosAsync(Arg.Any<CancellationToken>()).Returns(1);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.EditarAsync(5, new("Laura", "laura", Roles.Cajero, null)));
        Assert.Equal("ULTIMO_ADMINISTRADOR", ex.Code);
    }

    [Fact]
    public async Task Un_administrador_no_puede_desactivarse_a_si_mismo()
    {
        Existente(rolId: Roles.AdministradorId);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.CambiarEstadoAsync(5, false, usuarioActualId: 5));
        Assert.Equal("NO_PUEDES_DESACTIVARTE", ex.Code);
    }

    [Fact]
    public async Task No_se_puede_desactivar_al_ultimo_administrador_activo()
    {
        Existente(rolId: Roles.AdministradorId);
        _repo.ContarAdministradoresActivosAsync(Arg.Any<CancellationToken>()).Returns(1);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.CambiarEstadoAsync(5, false, usuarioActualId: 99));
        Assert.Equal("ULTIMO_ADMINISTRADOR", ex.Code);
    }

    [Fact]
    public async Task Desactivar_a_un_cajero_funciona_y_reactivar_limpia_el_bloqueo()
    {
        var u = Existente();
        var off = await _sut.CambiarEstadoAsync(5, false, usuarioActualId: 1);
        Assert.False(off.Activo);

        u.IntentosFallidos = 3;
        var on = await _sut.CambiarEstadoAsync(5, true, usuarioActualId: 1);
        Assert.True(on.Activo);
        Assert.Equal(0, u.IntentosFallidos);
    }

    [Fact]
    public async Task Usuario_inexistente_responde_404()
    {
        _repo.ObtenerPorIdAsync(77, Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.ObtenerAsync(77));
        Assert.Equal("USUARIO_NO_ENCONTRADO", ex.Code);
    }
}
