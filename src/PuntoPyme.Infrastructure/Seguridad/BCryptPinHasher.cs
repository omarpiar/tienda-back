using PuntoPyme.Application.Auth;

namespace PuntoPyme.Infrastructure.Seguridad;

/// <summary>Paquete NuGet: BCrypt.Net-Next</summary>
public class BCryptPinHasher : IPinHasher
{
    public string Hash(string pin) => BCrypt.Net.BCrypt.HashPassword(pin, workFactor: 11);

    public bool Verificar(string pin, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(pin, hash); }
        catch (BCrypt.Net.SaltParseException) { return false; }
    }
}
