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
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Veterinaria API v1"));
// Render termina HTTPS en su proxy; el contenedor escucha HTTP en su red interna.
if (app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseCors("Frontend");
app.MapControllers();
app.Run();
