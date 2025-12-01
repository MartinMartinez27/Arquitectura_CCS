using Arquitectura_CCS.Common.Enums;
using Arquitectura_CCS.Common.Models;
using Arquitectura_CCS.RulesEngine.Rules;
using Arquitectura_CCS.RulesEngine.Rules.ConcreteRules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Arquitectura_CCS.UnitTests.RulesEngine
{
    public class SpeedLimitRuleTests
    {
        private readonly SpeedLimitRule _rule;
        private readonly Mock<ILogger<SpeedLimitRule>> _loggerSpeedLimitMock;
        private readonly Mock<ILogger<BaseRule>> _loggerBaseRuleMock;
        private readonly ServiceProvider _serviceProvider;

        public SpeedLimitRuleTests()
        {
            _loggerSpeedLimitMock = new Mock<ILogger<SpeedLimitRule>>();
            _loggerBaseRuleMock = new Mock<ILogger<BaseRule>>();

            // Registrar loggers en ServiceCollection
            var services = new ServiceCollection();
            services.AddScoped(_ => _loggerSpeedLimitMock.Object);
            services.AddScoped(_ => _loggerBaseRuleMock.Object);

            _serviceProvider = services.BuildServiceProvider();

            _rule = new SpeedLimitRule();
        }

        [Theory]
        [InlineData(VehicleType.Truck, 85.0, true)]
        [InlineData(VehicleType.Truck, 75.0, false)]
        [InlineData(VehicleType.Car, 105.0, true)]
        [InlineData(VehicleType.Car, 95.0, false)]
        [InlineData(VehicleType.Motorcycle, 65.0, true)]
        [InlineData(VehicleType.Motorcycle, 55.0, false)]
        public async Task EvaluateAsync_ShouldRespectSpeedLimits(VehicleType vehicleType, double speed, bool expectedResult)
        {
            var telemetry = new VehicleTelemetry
            {
                VehicleId = "TEST001",
                VehicleType = vehicleType,
                Speed = speed
            };

            var result = await _rule.EvaluateAsync(telemetry);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ExecuteActionsAsync_WhenSpeedExceeded_ShouldLogAppropriately()
        {
            var telemetry = new VehicleTelemetry
            {
                VehicleId = "TRUCK001",
                VehicleType = VehicleType.Truck,
                Speed = 90.0, // Above limit of 80
                Latitude = 4.710989,
                Longitude = -74.072092
            };

            // Act
            await _rule.ExecuteActionsAsync(telemetry, _serviceProvider);

            // Assert: logger de SpeedLimitRule recibió advertencia
            _loggerSpeedLimitMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Speed limit exceeded")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
