using System.Text;
using System.Text.Json;
using APIGateway.Models;
using Ocelot.Middleware;

namespace APIGateway.Middleware
{
    /// <summary>
    /// Manejo centralizado de errores del API Gateway.
    /// Captura las excepciones no controladas y transforma cualquier respuesta con
    /// código HTTP de error en un cuerpo JSON estandarizado (<see cref="ApiErrorResponse"/>),
    /// tanto si el error lo generó Ocelot (401, 403, 404, 429, 502, ...) como si provino
    /// de una Web API interna.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBody = context.Response.Body;
            await using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Excepción no controlada procesando {Method} {Path}. TraceId: {TraceId}",
                        context.Request.Method, context.Request.Path, context.TraceIdentifier);

                    if (!context.Response.HasStarted)
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    }
                }

                if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
                {
                    await WriteStandardErrorAsync(context, buffer);
                }

                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(originalBody);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }

        private async Task WriteStandardErrorAsync(HttpContext context, MemoryStream buffer)
        {
            var downstreamBody = await ReadDownstreamBodyAsync(context, buffer);

            var ocelotErrors = context.Items.Errors()
                .Select(e => e.Message)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct()
                .ToArray();

            var payload = new ApiErrorResponse
            {
                Status = context.Response.StatusCode,
                Message = BuildMessage(context.Response.StatusCode),
                Errors = ocelotErrors,
                Details = downstreamBody,
                TraceId = context.TraceIdentifier,
                Path = context.Request.Path,
                Timestamp = DateTime.UtcNow
            };

            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, SerializerOptions);

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentLength = bytes.Length;

            await context.Response.Body.WriteAsync(bytes, context.RequestAborted);
        }

        private static async Task<string?> ReadDownstreamBodyAsync(HttpContext context, MemoryStream buffer)
        {
            if (buffer.Length == 0)
            {
                return null;
            }

            var contentType = context.Response.ContentType ?? string.Empty;
            var isTextContent = contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
                || contentType.Contains("text", StringComparison.OrdinalIgnoreCase);

            buffer.Seek(0, SeekOrigin.Begin);

            if (!isTextContent)
            {
                buffer.SetLength(0);
                return null;
            }

            using var reader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            buffer.SetLength(0);

            return string.IsNullOrWhiteSpace(body) ? null : body;
        }

        private static string BuildMessage(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "La solicitud es inválida.",
            StatusCodes.Status401Unauthorized => "No está autenticado. Se requiere un token de acceso válido.",
            StatusCodes.Status403Forbidden => "No tiene permisos para acceder a este recurso.",
            StatusCodes.Status404NotFound => "El recurso solicitado no fue encontrado.",
            StatusCodes.Status413PayloadTooLarge => "El contenido de la solicitud es demasiado grande.",
            StatusCodes.Status429TooManyRequests => "Se superó el límite de solicitudes. Intente nuevamente más tarde.",
            StatusCodes.Status499ClientClosedRequest => "La solicitud fue cancelada por el cliente.",
            StatusCodes.Status500InternalServerError => "Ocurrió un error interno en el servidor.",
            StatusCodes.Status502BadGateway => "El servicio interno no está disponible.",
            StatusCodes.Status503ServiceUnavailable => "El servicio no está disponible temporalmente.",
            StatusCodes.Status504GatewayTimeout => "El servicio interno no respondió a tiempo.",
            _ => "Ocurrió un error al procesar la solicitud."
        };
    }
}
