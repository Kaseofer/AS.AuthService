using AgendaSalud.AuthService.Application.IOC;
using AgendaSalud.AuthService.Infrastructure.IOC;
using AuthService.API.IOC;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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

    // Configurar URLs ANTES de build (solo para producción)
    if (!builder.Environment.IsDevelopment())
    {
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        Console.WriteLine($"Production: Configured for port {port}");
    }

    Console.WriteLine("Registering services...");

    builder.Services.AddInfrastructureLayerService(builder.Configuration);
    builder.Services.AddApplicationLayerService();
    builder.Services.AddPresentationLayerService(builder.Configuration);

    Console.WriteLine("Building app...");

    var app = builder.Build();


    app.Use(async (context, next) =>
    {
        Console.WriteLine($"🌐 REQUEST: {context.Request.Method} {context.Request.Path}");
        try
        {
            await next();
            Console.WriteLine($"✅ RESPONSE: {context.Response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ MIDDLEWARE EXCEPTION: {ex.GetType().Name} - {ex.Message}");
            throw;
        }
    });


    Console.WriteLine("Configuring pipeline...");

    // SWAGGER CONFIGURATION (solo en Development)
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("Configuring Swagger for Development...");
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "swagger";
        });
    }

    // MIDDLEWARE PIPELINE (orden importante)
    app.UseCors("AllowAllOrigins");

    // HTTPS redirect comentado para Railway
    // app.UseHttpsRedirection();

    app.UseAuthentication();
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
                environment = app.Environment.EnvironmentName,
                duration = report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        status = kvp.Value.Status.ToString(),
                        duration = kvp.Value.Duration.TotalMilliseconds,
                        description = kvp.Value.Description,
                        data = kvp.Value.Data
                    }
                )
            };
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    });

    // Endpoint simple para Railway health check
    app.MapGet("/health/live", () => Results.Ok(new
    {
        status = "alive",
        timestamp = DateTime.UtcNow,
        service = "AuthService",
        environment = app.Environment.EnvironmentName
    }));

    // CONTROLLERS
    app.MapControllers();

    // SEEDING (si lo tienes)
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("Running development seeding...");
        // Tu código de seeding aquí si lo tienes
    }

    Console.WriteLine($"🚀 Starting AuthService in {app.Environment.EnvironmentName} mode...");

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("📖 Swagger available at: /swagger");
        Console.WriteLine("❤️ Health checks available at: /health");
    }

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("❌ CRITICAL ERROR DURING STARTUP:");
    Console.WriteLine($"Type: {ex.GetType().Name}");
    Console.WriteLine($"Message: {ex.Message}");

    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }

    Console.WriteLine("Stack Trace:");
    Console.WriteLine(ex.StackTrace);

    throw;
}