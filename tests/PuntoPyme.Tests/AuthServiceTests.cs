using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using PuntoPyme.Application.Auth;
using PuntoPyme.Application.Common;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Tests;

public class AuthServiceTests
{
    private sealed class HasherFalso : IPinHasher
    {
        public string Hash(string pin) => "hash:" + pin;
        public bool Verificar(string pin, string hash) => hash == "hash:" + pin;
    }

    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly FakeTimeProvider _reloj = new(new DateTimeOffset(2026, 10, 26, 18, 0, 0, TimeSpan.Zero));
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _tokens.Generar(Arg.Any<Usuario>(), Arg.Any<string>())
            .Returns(ci => new TokenGenerado("token-" + ci.ArgAt<string>(1), _reloj.GetUtcNow().UtcDateTime.AddHours(8)));
        _sut = new AuthService(_repo, new HasherFalso(), _tokens, _reloj);
    }

    private Usuario DadoUnUsuario(string rol = Roles.Cajero, bool activo = true, string pin = "1234")
    {
        var u = new Usuario
        {
            Id = 1,
            NombreCompleto = "Laura Gómez",
            NombreUsuario = "laura",
            PinHash = "hash:" + pin,
            RolId = rol == Roles.Administrador ? Roles.AdministradorId : Roles.CajeroId,
            Rol = new Rol { Id = rol == Roles.Administrador ? Roles.AdministradorId : Roles.CajeroId, Nombre = rol },
            Activo = activo
        };
        _repo.ObtenerPorNombreUsuarioAsync("laura", Arg.Any<CancellationToken>()).Returns(u);
        return u;
    }

    [Fact]
    public async Task Credenciales_correctas_devuelven_token_con_el_rol_elegido()
    {
        DadoUnUsuario(Roles.Administrador);

        var r = await _sut.LoginAsync(new LoginRequest("laura", "1234", Roles.Cajero));

        Assert.Equal("token-Cajero", r.Token);
        Assert.Equal(Roles.Cajero, r.Usuario.Rol);
        Assert.Equal("laura", r.Usuario.NombreUsuario);
    }

    [Fact]
    public async Task Pin_incorrecto_suma_un_intento_y_responde_401()
    {
        var u = DadoUnUsuario();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.LoginAsync(new LoginRequest("laura", "9999", Roles.Cajero)));

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("CREDENCIALES_INVALIDAS", ex.Code);
        Assert.Equal(1, u.IntentosFallidos);
        await _repo.Received(1).ActualizarAsync(u, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task El_quinto_intento_fallido_bloquea_un_minuto()
    {
        var u = DadoUnUsuario();
        u.IntentosFallidos = 4;

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.LoginAsync(new LoginRequest("laura", "9999", Roles.Cajero)));

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal(_reloj.GetUtcNow().UtcDateTime.AddMinutes(1), u.BloqueadoHastaUtc);
        Assert.Equal(0, u.IntentosFallidos);
    }

    [Fact]
    public async Task Usuario_bloqueado_no_entra_aunque_el_pin_sea_correcto()
    {
        var u = DadoUnUsuario();
        u.BloqueadoHastaUtc = _reloj.GetUtcNow().UtcDateTime.AddSeconds(30);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.LoginAsync(new LoginRequest("laura", "1234", Roles.Cajero)));

        Assert.Equal(429, ex.StatusCode);
        Assert.Equal("USUARIO_BLOQUEADO", ex.Code);
        Assert.Equal(30, ex.RetryAfterSegundos);
    }

    [Fact]
    public async Task Pasado_el_bloqueo_el_usuario_puede_entrar_y_se_reinician_los_contadores()
    {
        var u = DadoUnUsuario();
        u.BloqueadoHastaUtc = _reloj.GetUtcNow().UtcDateTime.AddSeconds(30);
        _reloj.Advance(TimeSpan.FromSeconds(31));

        var r = await _sut.LoginAsync(new LoginRequest("laura", "1234", Roles.Cajero));

        Assert.Equal("token-Cajero", r.Token);
        Assert.Null(u.BloqueadoHastaUtc);
        Assert.Equal(0, u.IntentosFallidos);
    }

    [Fact]
    public async Task Cajero_que_elige_Administrador_recibe_403()
    {
        DadoUnUsuario(Roles.Cajero);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.LoginAsync(new LoginRequest("laura", "1234", Roles.Administrador)));

        Assert.Equal(403, ex.StatusCode);
        Assert.Equal("ROL_NO_PERMITIDO", ex.Code);
    }

    [Fact]
    public async Task Administrador_puede_entrar_como_Cajero_o_como_Administrador()
    {
        DadoUnUsuario(Roles.Administrador);

        var comoCajero = await _sut.LoginAsync(new LoginRequest("laura", "1234", Roles.Cajero));
        var comoAdmin = await _sut.LoginAsync(new LoginRequest("laura", "1234", Roles.Administrador));

        Assert.Equal(Roles.Cajero, comoCajero.Usuario.Rol);
        Assert.Equal(Roles.Administrador, comoAdmin.Usuario.Rol);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567")]
    [InlineData("12a4")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Pin_con_formato_invalido_se_rechaza_con_400_sin_consultar_la_base(string? pin)
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.LoginAsync(new LoginRequest("laura", pin, Roles.Cajero)));

        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("VALIDACION", ex.Code);
        Assert.Contains("pin", ex.Errores!.Keys);
        await _repo.DidNotReceive().ObtenerPorNombreUsuarioAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rol_invalido_se_rechaza_con_400()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.LoginAsync(new LoginRequest("laura", "1234", "Gerente")));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("rol", ex.Errores!.Keys);
    }

    [Fact]
    public async Task Usuario_inactivo_recibe_el_mismo_error_que_un_pin_incorrecto()
    {
        DadoUnUsuario(activo: false);
        var exInactivo = await Assert.ThrowsAsync<AppException>(() =>
            _sut.LoginAsync(new LoginRequest("laura", "1234", Roles.Cajero)));

        DadoUnUsuario();
        var exPinMalo = await Assert.ThrowsAsync<AppException>(() =>
            _sut.LoginAsync(new LoginRequest("laura", "9999", Roles.Cajero)));

        Assert.Equal(exPinMalo.StatusCode, exInactivo.StatusCode);
        Assert.Equal(exPinMalo.Code, exInactivo.Code);
        Assert.Equal(exPinMalo.Message, exInactivo.Message);
    }

    [Fact]
    public async Task Usuario_inexistente_recibe_el_mismo_error_que_un_pin_incorrecto()
    {
        _repo.ObtenerPorNombreUsuarioAsync("nadie", Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sut.LoginAsync(new LoginRequest("nadie", "1234", Roles.Cajero)));

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal(AuthService.MensajeCredenciales, ex.Message);
    }

    [Fact]
    public async Task El_nombre_de_usuario_se_normaliza_antes_de_buscarlo()
    {
        DadoUnUsuario();

        await _sut.LoginAsync(new LoginRequest("  LAU ra ", "1234", Roles.Cajero));

        await _repo.Received(1).ObtenerPorNombreUsuarioAsync("laura", Arg.Any<CancellationToken>());
    }
}
