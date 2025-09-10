using AgendaSalud.AuthService.Application.Interfaces;
using AgendaSalud.AuthService.Application.Services;

using Microsoft.Extensions.DependencyInjection;

namespace AgendaSalud.AuthService.Application.IOC
{
    public static class AddApplicationServices
    {
        public static IServiceCollection AddApplicationLayerService(this IServiceCollection services)
        {
            // Aquí puedes registrar tus servicios de infraestructura, como repositorios, contextos de base de datos, etc.
            // services.AddScoped<IYourRepository, YourRepositoryImplementation>();
            // Repositorios

            // Pacientes


            services.AddSingleton<IJwtGenerator, JwtGenerator>();

            services.AddScoped<IAuthenticationService, AuthenticationService>();




            return services;
        }
    }
}
