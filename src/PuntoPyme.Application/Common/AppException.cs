namespace PuntoPyme.Application.Common;

/// <summary>
/// Error de negocio o validación. El API lo convierte en ProblemDetails
/// con el campo "code" que usa el frontend para decidir qué mostrar.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }
    public int? RetryAfterSegundos { get; init; }
    public IDictionary<string, string[]>? Errores { get; init; }

    /// <summary>Campos extra del ProblemDetails (por ejemplo "faltantes" en STOCK_INSUFICIENTE).</summary>
    public IDictionary<string, object?>? Extensiones { get; init; }

    public AppException(int statusCode, string code, string mensaje) : base(mensaje)
    {
        StatusCode = statusCode;
        Code = code;
    }

    public static AppException Validacion(string campo, string mensaje) =>
        new(400, "VALIDACION", "Los datos enviados no son válidos.")
        {
            Errores = new Dictionary<string, string[]> { [campo] = [mensaje] }
        };

    public static AppException Validacion(IDictionary<string, string[]> errores) =>
        new(400, "VALIDACION", "Los datos enviados no son válidos.") { Errores = errores };

    public static AppException NoEncontrado(string code, string mensaje) => new(404, code, mensaje);
    public static AppException Conflicto(string code, string mensaje) => new(409, code, mensaje);
}
