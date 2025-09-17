// Program.cs - AuthService con Swagger Fix

using AgendaSalud.AuthService.Application.IOC;
using AgendaSalud.AuthService.Application.Settings;
using AgendaSalud.AuthService.Infrastructure.IOC;
using AgendaSalud.AuthService.Infrastructure.Logger;
using AgendaSalud.AuthService.Infrastructure.Persistence.Context;
using AgendaSalud.AuthService.Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

Console.WriteLine("=== DEBUGGING STARTUP ===");
Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
Console.WriteLine($"JWT Key configured: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Jwt__Key"))}");
Console.WriteLine($"DB Connection configured: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ConnectionStrings__AgendaSaludAthentication"))}");
Console.WriteLine($"Google Client ID configured: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Authentication__Google__ClientId"))}");

var builder = WebApplication.CreateBuilder(args);

try
{
    Console.WriteLine("Adding configuration...");
    builder.Configuration.AddEnvironmentVariables();

    Console.WriteLine("Adding DbContext...");
    builder.Services.AddDbContext<AuthenticationDbContext>(options =>
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__AgendaSaludAthentication")
            ?? builder.Configuration.GetConnectionString("AgendaSaludAthentication");
        Console.WriteLine($"Using connection string: {!string.IsNullOrEmpty(connectionString)}");
        options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
    });

    Console.WriteLine("Adding JWT settings...");
    builder.Services.Configure<JwtSettings>(options =>
    {
        builder.Configuration.GetSection("Jwt").Bind(options);
        options.Key = Environment.GetEnvironmentVariable("Jwt__Key") ?? options.Key;
    });

    Console.WriteLine("Adding health checks...");
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("AuthService is running"))
        .AddDbContextCheck<AuthenticationDbContext>("database")
        .AddCheck("jwt-configuration", (serviceProvider) =>
        {
            try
            {
                var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key");
                if (string.IsNullOrEmpty(jwtKey))
                {
                    return HealthCheckResult.Unhealthy("JWT Key not configured");
                }
                if (jwtKey.Length < 32)
                {
                    return HealthCheckResult.Degraded("JWT Key might be too short for security");
                }
                return HealthCheckResult.Healthy("JWT configuration is valid");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"JWT configuration check failed: {ex.Message}");
            }
        })
        .AddCheck("google-oauth", (serviceProvider) =>
        {
            try
            {
                var clientId = Environment.GetEnvironmentVariable("Authentication__Google__ClientId");
                var clientSecret = Environment.GetEnvironmentVariable("Authentication__Google__ClientSecret");

                var issues = new List<string>();
                if (string.IsNullOrEmpty(clientId)) issues.Add("ClientId");
                if (string.IsNullOrEmpty(clientSecret)) issues.Add("ClientSecret");

                if (issues.Any())
                {
                    return HealthCheckResult.Degraded($"Google OAuth partially configured. Missing: {string.Join(", ", issues)}");
                }

                return HealthCheckResult.Healthy("Google OAuth is properly configured");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Google OAuth check failed: {ex.Message}");
            }
        })
        .AddCheck("memory", () =>
        {
            var allocated = GC.GetTotalMemory(false);
            var data = new Dictionary<string, object>
            {
                ["allocated"] = allocated,
                ["gen0"] = GC.CollectionCount(0),
                ["gen1"] = GC.CollectionCount(1),
                ["gen2"] = GC.CollectionCount(2)
            };

            var status = allocated < 200_000_000 ? HealthStatus.Healthy : HealthStatus.Degraded;
            var message = $"Memory usage: {allocated / 1024 / 1024}MB";

            return new HealthCheckResult(status, message, data: data);
        });

    Console.WriteLine("Adding basic services...");
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // MEJORAR CONFIGURACIÓN DE SWAGGER
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "AgendaSalud AuthService API",
            Version = "v1",
            Description = "Servicio de Autenticación para AgendaSalud"
        });
    });

    Console.WriteLine("Adding CORS...");
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            builder => builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader());
    });

    Console.WriteLine("Adding logger...");
    builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(FileLogger<>));

    Console.WriteLine("Adding application services...");

    builder.Services.AddInfrastructureLayerService();
    Console.WriteLine($"Infrastructure services registered. Total services: {builder.Services.Count}");
    builder.Services.AddApplicationLayerService();
    Console.WriteLine($"Application services registered. Total services: {builder.Services.Count}");

    Console.WriteLine("Adding authentication...");
   /* builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = Environment.GetEnvironmentVariable("Authentication__Google__ClientId")
                ?? builder.Configuration["Authentication:Google:ClientId"];
            options.ClientSecret = Environment.GetEnvironmentVariable("Authentication__Google__ClientSecret")
                ?? builder.Configuration["Authentication:Google:ClientSecret"];
            options.CallbackPath = builder.Configuration["Authentication:Google:CallbackPath"];
        });
   */
    Console.WriteLine("Building app...");
    var app = builder.Build();

    Console.WriteLine("Configuring pipeline...");

    // CONFIGURAR URLS SEGÚN EL ENTORNO
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("Running in Development mode");

        // NO configurar URLs aquí en desarrollo - usar launchSettings.json
        // Las URLs se configuran automáticamente desde launchSettings.json

        // SWAGGER CONFIGURATION - solo en Development
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgendaSalud AuthService API v1");
            c.RoutePrefix = string.Empty; // Swagger en la raíz /
            c.DocumentTitle = "AgendaSalud AuthService API";
            c.DefaultModelsExpandDepth(-1);
        });

        Console.WriteLine("Swagger UI available at: /");
    }
    else
    {
        Console.WriteLine("Running in Production mode");
        // SOLO en producción configurar puerto para Railway
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        app.Urls.Add($"http://0.0.0.0:{port}");
        Console.WriteLine($"Production port configured: {port}");
    }

    app.UseCors("AllowAllOrigins");

    // Solo usar HTTPS redirect en producción
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

   // app.UseAuthentication();
    app.UseAuthorization();

    // HEALTH CHECK ENDPOINTS
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                service = "AgendaSalud.AuthService",
                duration = report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        status = kvp.Value.Status.ToString(),
                        duration = kvp.Value.Duration.TotalMilliseconds,
                        description = kvp.Value.Description
                    }
                )
            };
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    });

    // Endpoint simple para Railway health check
    app.MapGet("/health/live", () => Results.Ok(new
    {
        status = "alive",
        timestamp = DateTime.UtcNow,
        service = "AuthService"
    }));

    app.MapControllers();

    Console.WriteLine("Starting seeding...");
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
        // Si tienes el RoleSeeder, descomenta esta línea:
        await RoleSeeder.SeedAsync(dbContext);
        Console.WriteLine("Seeding completed successfully");
    }
    catch (Exception seedEx)
    {
        Console.WriteLine($"Seeding failed: {seedEx.Message}");
        // No fallar la app por seeding
    }




    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("AuthService starting in Development mode...");
        Console.WriteLine("Swagger UI will be available at the configured URL");
    }
    else
    {
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        Console.WriteLine($"AuthService starting in Production mode on port {port}");
    }

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"CRITICAL ERROR: {ex.GetType().Name}");
    try { Console.WriteLine($"Message: {ex.Message}"); } catch { }
    try { Console.WriteLine($"Inner: {ex.InnerException?.Message}"); } catch { }
    throw;
}