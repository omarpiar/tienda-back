using System.Globalization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PuntoPyme.Application.Common;

namespace PuntoPyme.Api.Errores;

/// <summary>Convierte AppException en ProblemDetails (RFC 7807) con el campo "code".</summary>
public class ApiExceptionHandler(IProblemDetailsService problemas, ILogger<ApiExceptionHandler> log) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken ct)
    {
        if (excepcion is not AppException app)
        {
            log.LogError(excepcion, "Error no controlado");
            return false; // el manejador por defecto responde 500 en ProblemDetails
        }

        contexto.Response.StatusCode = app.StatusCode;
        if (app.RetryAfterSegundos is int segundos)
            contexto.Response.Headers.RetryAfter = segundos.ToString(CultureInfo.InvariantCulture);

        ProblemDetails detalle = app.Errores is null
            ? new ProblemDetails()
            : new ValidationProblemDetails(app.Errores);

        detalle.Status = app.StatusCode;
        detalle.Title = Titulo(app.StatusCode);
        detalle.Detail = app.Message;
        detalle.Type = $"https://httpstatuses.com/{app.StatusCode}";
        detalle.Extensions["code"] = app.Code;

        if (app.Extensiones is not null)
            foreach (var (clave, valor) in app.Extensiones)
                detalle.Extensions[clave] = valor;

        return await problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            ProblemDetails = detalle,
            Exception = excepcion
        });
    }

    private static string Titulo(int status) => status switch
    {
        400 => "Solicitud inválida",
        401 => "No autorizado",
        403 => "Sin permiso",
        404 => "No encontrado",
        409 => "Conflicto",
        429 => "Demasiados intentos",
        _ => "Error"
    };
}
