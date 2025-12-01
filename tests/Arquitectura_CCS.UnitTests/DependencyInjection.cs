using Arquitectura_CCS.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;
using Arquitectura_CCS.Common.Models;

namespace Arquitectura_CCS.UnitTests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddCommonServices_ShouldRegisterDbContext_WithInMemoryDatabase()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddCommonServices(configuration: null!, useInMemory: true);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var context = serviceProvider.GetService<CCSDbContext>();
            Assert.NotNull(context);

            // Podemos probar que funciona realmente agregando un objeto
            var vehicle = new Vehicle{ VehicleId = "TEST01" };
            context.Vehicles.Add(vehicle);
            context.SaveChanges();

            var retrieved = context.Vehicles.Find("TEST01");
            Assert.NotNull(retrieved);
            Assert.Equal("TEST01", retrieved.VehicleId);
        }
    }
}
