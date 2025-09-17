using AgendaSalud.AuthService.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace AgendaSalud.AuthService.Infrastructure.IOC
{
    public static class AddInfrastructureServices
    {
        public static IServiceCollection AddInfrastructureLayerService(this IServiceCollection services)
        {
            // Aquí puedes registrar tus servicios de infraestructura, como repositorios, contextos de base de datos, etc.
            // services.AddScoped<IYourRepository, YourRepositoryImplementation>();
            // Repositorios

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


            return services;
        }
    }
}
