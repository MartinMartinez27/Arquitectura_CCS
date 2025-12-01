using Arquitectura_CCS.Common;
using Arquitectura_CCS.Common.Models;
using Arquitectura_CCS.Common.Enums;
using Microsoft.EntityFrameworkCore;
namespace Arquitectura_CCS.UnitTests.Data;

public class CCSDbContextTests
{
    private DbContextOptions<CCSDbContext> _dbContextOptions;

    public CCSDbContextTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<CCSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Base de datos única por test
            .Options;
    }

    [Fact]
    public async Task CanInsertAndRetrieveVehicle()
    {
        // Arrange
        using var context = new CCSDbContext(_dbContextOptions);
        var vehicle = new Vehicle
        {
            VehicleId = "TEST001",
            LicensePlate = "ABC123",
            VehicleType = VehicleType.Truck,
            OwnerId = "OWNER001",
            Model = "FH16",
            Brand = "Volvo",
            Year = 2023
        };

        // Act
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        // Assert
        var retrievedVehicle = await context.Vehicles.FindAsync("TEST001");
        Assert.NotNull(retrievedVehicle);
        Assert.Equal("ABC123", retrievedVehicle.LicensePlate);
        Assert.Equal(VehicleType.Truck, retrievedVehicle.VehicleType);
    }

    [Fact]
    public async Task CanInsertTelemetryWithVehicle()
    {
        // Arrange
        using var context = new CCSDbContext(_dbContextOptions);

        var vehicle = new Vehicle
        {
            VehicleId = "TEST002",
            LicensePlate = "DEF456",
            VehicleType = VehicleType.Car,
            OwnerId = "OWNER002",
            Model = "Corolla",
            Brand = "Toyota",
            Year = 2022
        };

        var telemetry = new VehicleTelemetry
        {
            TelemetryId = Guid.NewGuid().ToString(),
            VehicleId = "TEST002",
            VehicleType = VehicleType.Car,
            Latitude = 4.710989,
            Longitude = -74.072092,
            Speed = 65.5,
            IsMoving = true,
            EngineOn = true,
            FuelLevel = 75.0
        };

        // Act
        context.Vehicles.Add(vehicle);
        context.VehicleTelemetries.Add(telemetry);
        await context.SaveChangesAsync();

        // Assert
        var telemetryWithVehicle = await context.VehicleTelemetries
            .Include(t => t.Vehicle)
            .FirstOrDefaultAsync(t => t.TelemetryId == telemetry.TelemetryId);

        Assert.NotNull(telemetryWithVehicle);
        Assert.NotNull(telemetryWithVehicle.Vehicle);
        Assert.Equal("DEF456", telemetryWithVehicle.Vehicle.LicensePlate);
    }

    [Fact]
    public async Task VehicleTelemetry_ShouldHaveRequiredProperties()
    {
        // Arrange
        using var context = new CCSDbContext(_dbContextOptions);
        var telemetry = new VehicleTelemetry
        {
            TelemetryId = Guid.NewGuid().ToString(),
            VehicleId = "TEST003",
            VehicleType = VehicleType.Motorcycle,
            Latitude = 4.710989,
            Longitude = -74.072092,
            Speed = 45.0,
            IsMoving = true,
            EngineOn = true,
            FuelLevel = 80.0
        };

        // Act
        context.VehicleTelemetries.Add(telemetry);
        await context.SaveChangesAsync();

        // Assert
        var savedTelemetry = await context.VehicleTelemetries.FindAsync(telemetry.TelemetryId);
        Assert.NotNull(savedTelemetry);
        Assert.NotEmpty(savedTelemetry.VehicleId);
        Assert.True(savedTelemetry.Latitude != 0);
        Assert.True(savedTelemetry.Longitude != 0);
        Assert.True(savedTelemetry.Timestamp > DateTime.MinValue);
    }
}