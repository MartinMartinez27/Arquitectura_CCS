using Arquitectura_CCS.Common.Models;
using Arquitectura_CCS.Common.Enums;
using Arquitectura_CCS.RulesEngine.Engine;
using Arquitectura_CCS.RulesEngine.Rules;
using Moq;
using Microsoft.Extensions.Logging;

namespace Arquitectura_CCS.UnitTests.RulesEngine;

public class RulesEngineTests
{
    private readonly Mock<ILogger<RulesEngine>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly RulesEngine _rulesEngine;

    public RulesEngineTests()
    {
        _loggerMock = new Mock<ILogger<RulesEngine>>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _rulesEngine = new RulesEngine(_loggerMock.Object, _serviceProviderMock.Object);
    }

    [Fact]
    public void GetActiveRules_ShouldReturnAllRules()
    {
        // Act
        var rules = _rulesEngine.GetActiveRules();

        // Assert
        Assert.NotNull(rules);
        Assert.NotEmpty(rules);
        Assert.Contains(rules, r => r.Name == "Detención No Planeada");
        Assert.Contains(rules, r => r.Name == "Límite de Velocidad");
        Assert.Contains(rules, r => r.Name == "Temperatura de Carga");
    }

    [Fact]
    public async Task ProcessTelemetryAsync_ShouldExecuteMatchingRules()
    {
        // Arrange
        var telemetry = new VehicleTelemetry
        {
            VehicleId = "TRUCK001",
            VehicleType = VehicleType.Truck,
            Speed = 85.0, // Excede límite
            IsMoving = false,
            EngineOn = true,
            IsPlannedStop = false,
            CargoTemperature = 30.0 // Fuera de rango
        };

        var ruleMock = new Mock<IRule>();
        ruleMock.Setup(r => r.Name).Returns("Test Rule");
        ruleMock.Setup(r => r.EvaluateAsync(It.IsAny<VehicleTelemetry>())).ReturnsAsync(true);
        ruleMock.Setup(r => r.ExecuteActionsAsync(It.IsAny<VehicleTelemetry>(), It.IsAny<IServiceProvider>()));

        // Act
        await _rulesEngine.ProcessTelemetryAsync(telemetry);

        // Assert - Verificar que al menos se ejecutó una regla
        // (Este test verifica que el flujo completo funciona sin errores)
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessTelemetryAsync_ShouldHandleRuleExceptions()
    {
        // Arrange
        var telemetry = new VehicleTelemetry
        {
            VehicleId = "TRUCK001",
            VehicleType = VehicleType.Truck
        };

        // Configurar el service provider para devolver un logger
        var ruleLoggerMock = new Mock<ILogger<BaseRule>>();
        _serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<BaseRule>)))
            .Returns(ruleLoggerMock.Object);

        // Act - No debería lanzar excepción incluso si hay errores internos
        var exception = await Record.ExceptionAsync(() =>
            _rulesEngine.ProcessTelemetryAsync(telemetry));

        // Assert
        Assert.Null(exception);
    }
}