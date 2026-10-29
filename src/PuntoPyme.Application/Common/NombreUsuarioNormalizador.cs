namespace PuntoPyme.Application.Common;

/// <summary>Regla compartida con HU-09/HU-10: sin espacios y en minúsculas.</summary>
public static class NombreUsuarioNormalizador
{
    public static string Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? string.Empty
            : string.Concat(valor.Where(c => !char.IsWhiteSpace(c))).ToLowerInvariant();
}
