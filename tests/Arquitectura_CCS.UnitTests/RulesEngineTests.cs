using Arquitectura_CCS.Common.Enums;
using Arquitectura_CCS.Common.Models;
using Arquitectura_CCS.RulesEngine;
using Microsoft.Extensions.Logging;
using Moq;
using RulesEngineClass = Arquitectura_CCS.RulesEngine.Engine.RulesEngine;

namespace Arquitectura_CCS.UnitTests.RulesEngineTests;

public class RulesEngineTests
{
    private readonly Mock<ILogger<RulesEngineClass>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly RulesEngineClass _rulesEngine;

    public RulesEngineTests()
    {
        _loggerMock = new Mock<ILogger<RulesEngineClass>>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _rulesEngine = new RulesEngineClass(_loggerMock.Object, _serviceProviderMock.Object);
    }

    [Fact]
    public void GetActiveRules_ShouldReturnPredefinedConcreteRules()
    {
        var rules = _rulesEngine.GetActiveRules();

        Assert.NotNull(rules);
        Assert.Equal(3, rules.Count);
    }

    [Fact]
    public async Task ProcessTelemetryAsync_ShouldNotThrow()
    {
        var telemetry = new VehicleTelemetry
        {
            VehicleId = "T1",
            VehicleType = VehicleType.Truck
        };

        var ex = await Record.ExceptionAsync(() => _rulesEngine.ProcessTelemetryAsync(telemetry));

        Assert.Null(ex);
    }

    [Fact]
    public async Task ProcessTelemetryAsync_WhenRuleIsTriggered_ShouldExecuteActionsAndLog()
    {
        // Arrange
        var telemetry = new VehicleTelemetry
        {
            VehicleId = "T1",
            VehicleType = VehicleType.Truck,
            Speed = 100.0
        };

        // Act
        await _rulesEngine.ProcessTelemetryAsync(telemetry);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Rule triggered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}
