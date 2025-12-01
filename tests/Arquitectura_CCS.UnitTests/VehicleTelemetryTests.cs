using Arquitectura_CCS.Common.Models;
using Arquitectura_CCS.Common.Enums;

namespace Arquitectura_CCS.UnitTests.Models;

public class VehicleTelemetryTests
{
    [Fact]
    public void VehicleTelemetry_ShouldCreateWithDefaultValues()
    {
        // Arrange & Act
        var telemetry = new VehicleTelemetry();

        // Assert
        Assert.NotNull(telemetry.TelemetryId);
        Assert.NotEmpty(telemetry.TelemetryId);
        Assert.Equal(DateTime.UtcNow.Date, telemetry.Timestamp.Date);
    }

    [Fact]
    public void VehicleTelemetry_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var vehicleId = "TEST001";
        var latitude = 4.710989;
        var longitude = -74.072092;

        // Act
        var telemetry = new VehicleTelemetry
        {
            VehicleId = vehicleId,
            VehicleType = VehicleType.Truck,
            Latitude = latitude,
            Longitude = longitude,
            Speed = 65.5,
            IsMoving = true,
            EngineOn = true,
            FuelLevel = 75.0
        };

        // Assert
        Assert.Equal(vehicleId, telemetry.VehicleId);
        Assert.Equal(VehicleType.Truck, telemetry.VehicleType);
        Assert.Equal(latitude, telemetry.Latitude);
        Assert.Equal(longitude, telemetry.Longitude);
        Assert.Equal(65.5, telemetry.Speed);
        Assert.True(telemetry.IsMoving);
        Assert.True(telemetry.EngineOn);
        Assert.Equal(75.0, telemetry.FuelLevel);
    }

    [Fact]
    public void VehicleTelemetry_WithCargoData_ShouldHandleNullValues()
    {
        // Arrange & Act
        var telemetry = new VehicleTelemetry
        {
            VehicleId = "TRUCK001",
            CargoTemperature = null,
            CargoStatus = null,
            IsPlannedStop = null
        };

        // Assert
        Assert.Null(telemetry.CargoTemperature);
        Assert.Null(telemetry.CargoStatus);
        Assert.Null(telemetry.IsPlannedStop);
    }
}