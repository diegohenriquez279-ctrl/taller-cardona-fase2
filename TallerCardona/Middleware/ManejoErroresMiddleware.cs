using Microsoft.AspNetCore.Mvc;

namespace TallerCardona.Middleware;

// Atrapa cualquier error que no se haya controlado en un controlador o servicio.
//
// 1) Registra el error en el log con un "id de seguimiento".
// 2) Responde según quién hizo la petición:
//    - Rutas /api  -> JSON (ProblemDetails) con código 500 y un mensaje genérico.
//    - Parte MVC   -> la página /Home/Error, que muestra el id de seguimiento.
// Al usuario NUNCA se le muestran detalles técnicos; con el id se busca el error en Logs/.
public class ManejoErroresMiddleware
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejoErroresMiddleware> _logger;

    // Clave con la que se pasa el id a la vista de error (HttpContext.Items).
    public const string ClaveIdSeguimiento = "IdSeguimiento";

    public ManejoErroresMiddleware(RequestDelegate siguiente, ILogger<ManejoErroresMiddleware> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (Exception ex)
        {
            // Id corto y fácil de dictar por teléfono: 8 caracteres.
            var idSeguimiento = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

            _logger.LogError(ex, "Error no controlado [{IdSeguimiento}] en {Metodo} {Ruta}",
                idSeguimiento, contexto.Request.Method, contexto.Request.Path);

            // Si ya se empezó a enviar la respuesta no se puede cambiar: se relanza.
            if (contexto.Response.HasStarted) throw;

            if (contexto.Request.Path.StartsWithSegments("/api"))
                await ResponderJsonAsync(contexto, idSeguimiento);
            else
                await MostrarPaginaErrorAsync(contexto, idSeguimiento);
        }
    }

    private static async Task ResponderJsonAsync(HttpContext contexto, string idSeguimiento)
    {
        var problema = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Error interno del servidor",
            Detail = "Ocurrió un error inesperado. Si el problema continúa, comunique este id de seguimiento al administrador.",
            Instance = contexto.Request.Path
        };
        problema.Extensions["idSeguimiento"] = idSeguimiento;

        contexto.Response.Clear();
        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await contexto.Response.WriteAsJsonAsync(problema, options: null, contentType: "application/problem+json");
    }

    // Vuelve a ejecutar la aplicación, pero ahora pidiendo /Home/Error (sin redirigir el navegador).
    private async Task MostrarPaginaErrorAsync(HttpContext contexto, string idSeguimiento)
    {
        contexto.Items[ClaveIdSeguimiento] = idSeguimiento;

        contexto.Response.Clear();
        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        contexto.SetEndpoint(null);
        contexto.Request.RouteValues.Clear();
        contexto.Request.Path = "/Home/Error";
        contexto.Request.QueryString = QueryString.Empty;
        contexto.Request.Method = HttpMethods.Get;

        try
        {
            await _siguiente(contexto);
        }
        catch (Exception ex)
        {
            // Si hasta la página de error falla, se responde texto simple.
            _logger.LogError(ex, "Falló la página de error [{IdSeguimiento}]", idSeguimiento);
            if (!contexto.Response.HasStarted)
            {
                contexto.Response.Clear();
                contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
                contexto.Response.ContentType = "text/plain; charset=utf-8";
                await contexto.Response.WriteAsync($"Ocurrió un error inesperado. Id de seguimiento: {idSeguimiento}");
            }
        }
    }
}
