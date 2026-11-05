using Microsoft.EntityFrameworkCore;
using Veterinaria.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<VeterinariaContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Veterinaria")
        ?? throw new InvalidOperationException("Configura ConnectionStrings__Veterinaria con la conexión PostgreSQL.")));

var origen = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:3000";
if (!Uri.TryCreate(origen, UriKind.Absolute, out var uri) ||
    (uri.Scheme != "http" && uri.Scheme != "https") ||
    uri.AbsolutePath != "/" || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
    throw new InvalidOperationException("Cors__AllowedOrigin debe ser un origen HTTP(S) concreto, sin ruta.");

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(uri.GetLeftPart(UriPartial.Authority))
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();
// Origen público fijo para Location detrás de un proxy que termina TLS.
// No se confía en cabeceras de proxy enviadas por clientes arbitrarios.
var publicOrigin = builder.Configuration["PublicOrigin"];
if (!string.IsNullOrWhiteSpace(publicOrigin))
{
    if (!Uri.TryCreate(publicOrigin, UriKind.Absolute, out var publicUri) ||
        publicUri.Scheme != "https" || publicUri.AbsolutePath != "/" ||
        !string.IsNullOrEmpty(publicUri.Query) || !string.IsNullOrEmpty(publicUri.Fragment) ||
        !string.IsNullOrEmpty(publicUri.UserInfo))
        throw new InvalidOperationException("PublicOrigin debe ser un origen HTTPS concreto, sin ruta ni credenciales.");
    app.Use((context, next) =>
    {
        context.Request.Scheme = publicUri.Scheme;
        context.Request.Host = HostString.FromUriComponent(publicUri);
        return next(context);
    });
}
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Veterinaria API v1"));
// Render termina HTTPS en su proxy; el contenedor escucha HTTP en su red interna.
if (app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseCors("Frontend");
app.MapControllers();
app.Run();
