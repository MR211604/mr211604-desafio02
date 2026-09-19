using System.Diagnostics;

namespace APIGateway.Middleware
{
    /// <summary>
    /// Auditoría de las solicitudes recibidas y las respuestas generadas por el API Gateway.
    /// Registra método, ruta, IP y usuario de la petición entrante, y el código de estado y
    /// tiempo de procesamiento de la respuesta saliente, asociándolos al TraceId de la petición.
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            _logger.LogInformation(
                "Solicitud entrante {Method} {Path}{QueryString} | TraceId: {TraceId} | IP: {ClientIp} | Usuario: {User}",
                request.Method,
                request.Path,
                request.QueryString,
                context.TraceIdentifier,
                context.Connection.RemoteIpAddress,
                context.User?.Identity?.Name ?? "anónimo");

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "Respuesta saliente {StatusCode} para {Method} {Path} en {ElapsedMilliseconds} ms | TraceId: {TraceId}",
                    context.Response.StatusCode,
                    request.Method,
                    request.Path,
                    stopwatch.ElapsedMilliseconds,
                    context.TraceIdentifier);
            }
        }
    }
}
