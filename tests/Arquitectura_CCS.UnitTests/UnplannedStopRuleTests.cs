using Arquitectura_CCS.Common.Models;
using Arquitectura_CCS.RulesEngine.Rules.ConcreteRules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Arquitectura_CCS.UnitTests.RulesEngine
{
    public class UnplannedStopRuleTests
    {
        private readonly UnplannedStopRule _rule;
        private readonly Mock<ILogger<UnplannedStopRule>> _loggerMock;
        private readonly ServiceProvider _serviceProvider;

        public UnplannedStopRuleTests()
        {
            _loggerMock = new Mock<ILogger<UnplannedStopRule>>();

            // Configurar ServiceCollection real
            var services = new ServiceCollection();
            services.AddScoped(_ => _loggerMock.Object);

            _serviceProvider = services.BuildServiceProvider();

            _rule = new UnplannedStopRule();
        }

        [Theory]
        [InlineData(true, true, null, false)]  // Vehicle is moving -> no alert
        [InlineData(false, true, false, true)] // Engine on, not moving, unplanned -> alert
        [InlineData(false, false, false, false)] // Engine off -> no alert
        [InlineData(false, true, true, false)] // Planned stop -> no alert
        public async Task EvaluateAsync_ShouldDetectUnplannedStop(bool isMoving, bool engineOn, bool? isPlannedStop, bool expectedResult)
        {
            var telemetry = new VehicleTelemetry
            {
                VehicleId = "TEST001",
                IsMoving = isMoving,
                EngineOn = engineOn,
                IsPlannedStop = isPlannedStop
            };

            var result = await _rule.EvaluateAsync(telemetry);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ExecuteActionsAsync_ShouldLogWarning()
        {
            // Arrange
            var telemetry = new VehicleTelemetry
            {
                VehicleId = "TEST001",
                Latitude = 4.710989,
                Longitude = -74.072092
            };

            // Act
            await _rule.ExecuteActionsAsync(telemetry, _serviceProvider);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Unplanned stop detected")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
