using APIGateway.Middleware;
using JwtAuthenticationManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false,
    reloadOnChange: true);

// Autenticación y validación de tokens JWT para las solicitudes que llegan al Gateway.
builder.Services.AddCustomJwtAuthentication();

builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// Auditoría de solicitudes/respuestas y manejo centralizado de errores.
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();

app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs";
}).UseOcelot().Wait();

app.MapGet("/", () => "API Gateway en ejecución");

app.Run();
