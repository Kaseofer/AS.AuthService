using AgendaSalud.AuthService.Application.IOC;
using AgendaSalud.AuthService.Application.Settings;
using AgendaSalud.AuthService.Infrastructure.IOC;
using AgendaSalud.AuthService.Infrastructure.Logger;
using AgendaSalud.AuthService.Infrastructure.Persistence.Context;
using AgendaSalud.AuthService.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AuthenticationDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? builder.Configuration.GetConnectionString("AgendaSaludAthentication");

    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention();
});

//
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<JwtSettings>(options =>
{
    builder.Configuration.GetSection("Jwt").Bind(options);

    // Sobrescribir con variables de entorno si existen
    options.Key = Environment.GetEnvironmentVariable("JWT_KEY") ?? options.Key;
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        options.AddPolicy("AllowAllOrigins",
            builder => builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader());
    });
});


builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(FileLogger<>));

// Add Application Layer Services
builder.Services.AddInfrastructureLayerService();
builder.Services.AddApplicationLayerService();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
            ?? builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
            ?? builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = builder.Configuration["Authentication:Google:CallbackPath"];
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Seed Roles
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
    await RoleSeeder.SeedAsync(dbContext);
}

app.Run();
