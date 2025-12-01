using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.InMemory;
namespace Arquitectura_CCS.Common
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCommonServices(
            this IServiceCollection services,
            IConfiguration configuration,
            bool useInMemory = false)
        {
            if (useInMemory)
            {
                // Para pruebas unitarias
                services.AddDbContext<CCSDbContext>(options =>
                    options.UseInMemoryDatabase("TestDatabase"));
            }
            else
            {
                // Para producción
                services.AddDbContext<CCSDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            }

            return services;
        }
    }
}
