using Arquitectura_CCS.Common.Models;
using Arquitectura_CCS.Common.Enums;
using Arquitectura_CCS.RulesEngine.Rules.ConcreteRules;
using Moq;
using Microsoft.Extensions.Logging;

namespace Arquitectura_CCS.UnitTests.RulesEngine;

public class UnplannedStopRuleTests
{
    private readonly UnplannedStopRule _rule;
    private readonly Mock<ILogger<UnplannedStopRule>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public UnplannedStopRuleTests()
    {
        _loggerMock = new Mock<ILogger<UnplannedStopRule>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<UnplannedStopRule>)))
            .Returns(_loggerMock.Object);

        _rule = new UnplannedStopRule();
    }

    [Fact]
    public async Task EvaluateAsync_WhenUnplannedStop_ShouldReturnTrue()
    {
        // Arrange
        var telemetry = new VehicleTelemetry
        {
            VehicleId = "TEST001",
            IsMoving = false,
            EngineOn = true,
            IsPlannedStop = false
        };

        // Act
        var result = await _rule.EvaluateAsync(telemetry);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenMoving_ShouldReturnFalse()
    {
        // Arrange
        var telemetry = new VehicleTelemetry
        {
            VehicleId = "TEST001",
            IsMoving = true,
            EngineOn = true,
            IsPlannedStop = false
        };

        // Act
        var result = await _rule.EvaluateAsync(telemetry);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenPlannedStop_ShouldReturnFalse()
    {
        // Arrange
        var telemetry = new VehicleTelemetry
        {
            VehicleId = "TEST001",
            IsMoving = false,
            EngineOn = true,
            IsPlannedStop = true
        };

        // Act
        var result = await _rule.EvaluateAsync(telemetry);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenEngineOff_ShouldReturnFalse()
    {
        // Arrange
        var telemetry = new VehicleTelemetry
        {
            VehicleId = "TEST001",
            IsMoving = false,
            EngineOn = false,
            IsPlannedStop = false
        };

        // Act
        var result = await _rule.EvaluateAsync(telemetry);

        // Assert
        Assert.False(result);
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
        await _rule.ExecuteActionsAsync(telemetry, _serviceProviderMock.Object);

        // Assert
        // Verificar que se llamó al logger
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unplanned stop detected")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}