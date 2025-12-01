using Arquitectura_CCS.Common.Models;
using Arquitectura_CCS.Common.Enums;
using Arquitectura_CCS.RulesEngine.Rules.ConcreteRules;
using Moq;
using Microsoft.Extensions.Logging;

namespace Arquitectura_CCS.UnitTests.RulesEngine;

public class SpeedLimitRuleTests
{
	private readonly SpeedLimitRule _rule;
	private readonly Mock<ILogger<SpeedLimitRule>> _loggerMock;
	private readonly Mock<IServiceProvider> _serviceProviderMock;

	public SpeedLimitRuleTests()
	{
		_loggerMock = new Mock<ILogger<SpeedLimitRule>>();
		_serviceProviderMock = new Mock<IServiceProvider>();
		_serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<SpeedLimitRule>)))
			.Returns(_loggerMock.Object);

		_rule = new SpeedLimitRule();
	}

	[Theory]
	[InlineData(VehicleType.Truck, 85.0, true)]    // Límite 80, velocidad 85 -> ALERTA
	[InlineData(VehicleType.Truck, 75.0, false)]   // Límite 80, velocidad 75 -> OK
	[InlineData(VehicleType.Car, 105.0, true)]     // Límite 100, velocidad 105 -> ALERTA
	[InlineData(VehicleType.Car, 95.0, false)]     // Límite 100, velocidad 95 -> OK
	[InlineData(VehicleType.Motorcycle, 65.0, true)] // Límite 60, velocidad 65 -> ALERTA
	[InlineData(VehicleType.Motorcycle, 55.0, false)] // Límite 60, velocidad 55 -> OK
	public async Task EvaluateAsync_ShouldRespectSpeedLimits(VehicleType vehicleType, double speed, bool expectedResult)
	{
		// Arrange
		var telemetry = new VehicleTelemetry
		{
			VehicleId = "TEST001",
			VehicleType = vehicleType,
			Speed = speed
		};

		// Act
		var result = await _rule.EvaluateAsync(telemetry);

		// Assert
		Assert.Equal(expectedResult, result);
	}

	[Fact]
	public async Task ExecuteActionsAsync_WhenSpeedExceeded_ShouldLogAppropriately()
	{
		// Arrange
		var telemetry = new VehicleTelemetry
		{
			VehicleId = "TRUCK001",
			VehicleType = VehicleType.Truck,
			Speed = 90.0,
			Latitude = 4.710989,
			Longitude = -74.072092
		};

		// Act
		await _rule.ExecuteActionsAsync(telemetry, _serviceProviderMock.Object);

		// Assert
		_loggerMock.Verify(
			x => x.Log(
				LogLevel.Warning,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Speed limit exceeded")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()),
			Times.Once);
	}
}