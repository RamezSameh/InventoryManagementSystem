using FluentValidation;
using InventorySystem.API.Middleware;
using InventorySystem.Application.Common.Behaviors;
using InventorySystem.Application.Features.Products.Commands;
using InventorySystem.Infrastructure;
using InventorySystem.Infrastructure.Persistence;
using MediatR;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

EnsureJwtKey(builder);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddControllers().AddJsonOptions(o =>
{
    // The Angular frontend binds camelCase properties; serialize everything that way.
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    In = ParameterLocation.Header,
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer"
}));

var allowedOrigins = builder.Configuration.GetValue<string>("Cors:AllowedOrigins")
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
allowedOrigins = allowedOrigins is { Length: > 0 } ? allowedOrigins : new[] { "http://localhost:4200" };
builder.Services.AddCors(o => o.AddPolicy("AppCors", p => p
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

// The default admin account is seeded in every environment so login always works.
// Demo data below is for local development only — never in production.
await DbSeeder.EnsureAdminAsync(app.Services);
if (app.Environment.IsDevelopment())
{
    await DbSeeder.SeedDemoDataAsync(app.Services);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("AppCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static void EnsureJwtKey(WebApplicationBuilder builder)
{
    const string committedDevKey = "InventorySystem-Super-Secret-Key-Change-In-Production-2026";
    const string placeholder = "SET-JWT-KEY-VIA-ENVIRONMENT";

    var key = builder.Configuration["Jwt:Key"];
    if (!string.IsNullOrWhiteSpace(key) && key != committedDevKey && key != placeholder)
    {
        if (key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters long.");
        return;
    }

    if (builder.Environment.IsDevelopment())
    {
        Log.Warning("Jwt:Key is not configured — using an insecure development-only key. Set the Jwt__Key environment variable (or user secrets) for anything real.");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "dev-only-insecure-key-not-for-production-0123456789"
        });
        return;
    }

    throw new InvalidOperationException(
        "Jwt:Key is missing or still the default placeholder. Set the Jwt__Key environment variable to a random 256-bit value before starting in production.");
}

// Required so WebApplicationFactory<Program> can bootstrap the API in integration tests.
public partial class Program { }
