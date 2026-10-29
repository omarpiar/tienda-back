namespace PuntoPyme.Infrastructure.Seguridad;

public class JwtOptions
{
    public const string Seccion = "Jwt";

    /// <summary>Mínimo 32 caracteres. Va en user-secrets, nunca en el repositorio.</summary>
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "PuntoPyme";
    public string Audience { get; set; } = "PuntoPyme.Pwa";
    public int ExpiresMinutes { get; set; } = 480;
}
