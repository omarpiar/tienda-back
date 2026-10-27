using PuntoPyme.Application.Abstractions;

namespace PuntoPyme.Infrastructure.Security;

// Requiere el paquete NuGet BCrypt.Net-Next en PuntoPyme.Infrastructure.
public class BCryptPinHasher : IPinHasher
{
    public string Hash(string pin) => BCrypt.Net.BCrypt.HashPassword(pin);

    public bool Verify(string pin, string hash) => BCrypt.Net.BCrypt.Verify(pin, hash);
}
