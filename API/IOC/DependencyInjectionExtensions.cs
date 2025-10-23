using AgendaSalud.AuthService.Application.Settings;
using AgendaSalud.AuthService.Infrastructure.Logger;
using AgendaSalud.AuthService.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace AuthService.API.IOC
{
    public static class DependencyInjectionExtensions
    {


        public static IServiceCollection AddPresentationLayerService(
                                                          this IServiceCollection services,
                                                                IConfiguration configuration)
        {
            Console.WriteLine("Adding JWT settings...");
            services.Configure<JwtSettings>(options =>
            {
                configuration.GetSection("Jwt").Bind(options);
                options.Key = Environment.GetEnvironmentVariable("Jwt__Key") ?? options.Key;
            });

            // COMENTAR TEMPORALMENTE TODOS LOS HEALTH CHECKS
            Console.WriteLine("Adding health checks...");
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("AuthService is running"));

            // COMENTAR TODO ESTO TEMPORALMENTE:
            // .AddDbContextCheck<AuthenticationDbContext>("database")
            // .AddCheck("jwt-configuration", ...)
            // .AddCheck("google-oauth", ...)
            // .AddCheck("memory", ...)

            Console.WriteLine("Adding basic services...");
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // COMENTAR CORS TEMPORALMENTE
            // Console.WriteLine("Adding CORS...");
            Console.WriteLine("Adding CORS...");
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            Console.WriteLine("Adding logger...");
            services.AddSingleton(typeof(IAppLogger<>), typeof(FileLogger<>));

            Console.WriteLine("Adding authentication...");
            Console.WriteLine("Adding authentication...");

            Console.WriteLine("Adding JWT authentication...");
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var key = Environment.GetEnvironmentVariable("Jwt__Key") ?? configuration["Jwt:Key"];

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"], // ← Igual que tu _config.Issuer
                        ValidateAudience = true,
                        ValidAudience = configuration["Jwt:Audience"], // ← Igual que tu _config.Audience
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero // ← Igual que tu generador
                    };
                })
                .AddGoogle(options =>
                    {
                        options.ClientId = Environment.GetEnvironmentVariable("Authentication__Google__ClientId")
                                          ?? configuration["Authentication:Google:ClientId"];
                        options.ClientSecret = Environment.GetEnvironmentVariable("Authentication__Google__ClientSecret")
                                              ?? configuration["Authentication:Google:ClientSecret"];
                        options.CallbackPath = configuration["Authentication:Google:CallbackPath"];
                    });



            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService API", Version = "v1" });

                // Configurar JWT en Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });



            // NO agregues políticas que requieran autenticación por defecto
            services.AddAuthorization(); // Sin políticas por defecto

            return services;
        }
    }
}