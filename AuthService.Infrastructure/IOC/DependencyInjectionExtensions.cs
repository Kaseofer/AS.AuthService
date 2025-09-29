using AgendaSalud.AuthService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgendaSalud.AuthService.Infrastructure.IOC
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructureLayerService(this IServiceCollection services,
                                                                       IConfiguration configuration)
        {
            // Aquí puedes registrar tus servicios de infraestructura, como repositorios, contextos de base de datos, etc.
            // services.AddScoped<IYourRepository, YourRepositoryImplementation>();
            // Repositorios

            Console.WriteLine("Adding DbContext...");
            services.AddDbContext<AuthenticationDbContext>(options =>
            {
                var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__AgendaSaludAthentication")
                    ?? configuration.GetConnectionString("AgendaSaludAthentication");
                Console.WriteLine($"Using connection string: {!string.IsNullOrEmpty(connectionString)}");
                options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
            });

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


            return services;
        }
    }
}
