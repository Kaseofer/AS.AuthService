using AgendaSalud.AuthService.Application.IOC;
using AgendaSalud.AuthService.Application.Settings;
using AgendaSalud.AuthService.Infrastructure.IOC;
using AgendaSalud.AuthService.Infrastructure.Logger;
using AgendaSalud.AuthService.Infrastructure.Persistence.Context;
using AgendaSalud.AuthService.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("=== DEBUGGING STARTUP ===");
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
        // CORREGIR: Usar Jwt__Key en lugar de JWT_KEY
        options.Key = Environment.GetEnvironmentVariable("Jwt__Key") ?? options.Key;
    });

    Console.WriteLine("Adding basic services...");
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

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
    builder.Services.AddApplicationLayerService();

    Console.WriteLine("Adding authentication...");
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = Environment.GetEnvironmentVariable("Authentication__Google__ClientId")
                ?? builder.Configuration["Authentication:Google:ClientId"];
            options.ClientSecret = Environment.GetEnvironmentVariable("Authentication__Google__ClientSecret")
                ?? builder.Configuration["Authentication:Google:ClientSecret"];
            options.CallbackPath = builder.Configuration["Authentication:Google:CallbackPath"];
        });

    Console.WriteLine("Building app...");
    var app = builder.Build();

    Console.WriteLine("Configuring pipeline...");


    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAllOrigins");

    // Comentar HTTPS redirect para Railway
    // app.UseHttpsRedirection();

    app.UseAuthentication();  // AGREGAR ESTO
    app.UseAuthorization();
    app.MapControllers();

    // Configurar puerto para Railway
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Add($"http://0.0.0.0:{port}");

    Console.WriteLine("Starting seeding...");
    // ... resto del código de seeding

    Console.WriteLine($"Starting application on port {port}...");

    app.Run();
}
catch(Exception ex) {
    Console.WriteLine($"CRITICAL ERROR: {ex.GetType().Name}");
    try { Console.WriteLine($"Message: {ex.Message}"); } catch { }
    try { Console.WriteLine($"Inner: {ex.InnerException?.Message}"); } catch { }
    throw;
}
