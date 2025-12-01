using Arquitectura_CCS.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arquitectura_CCS.RulesEngine.Rules.ConcreteRules;

public class CargoTemperatureRule : BaseRule
{
    public override string RuleId => "CARGO_TEMPERATURE_001";
    public override string Name => "Temperatura de Carga";
    public override string Description => "Monitoriza la temperatura de carga en camiones";

    private readonly double _minTemperature = 15.0;
    private readonly double _maxTemperature = 25.0;

    public override async Task<bool> EvaluateAsync(VehicleTelemetry telemetry)
    {
        // Solo aplica a camiones y si tienen temperatura de carga
        return telemetry.VehicleType == Common.Enums.VehicleType.Truck &&
               telemetry.CargoTemperature.HasValue &&
               (telemetry.CargoTemperature.Value < _minTemperature ||
                telemetry.CargoTemperature.Value > _maxTemperature);
    }

    public override async Task ExecuteActionsAsync(VehicleTelemetry telemetry, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CargoTemperatureRule>>();

        var status = telemetry.CargoTemperature.Value < _minTemperature ? "BAJA" : "ALTA";

        logger.LogWarning("Cargo temperature alert for vehicle {VehicleId}: {Temperature}°C ({Status})",
            telemetry.VehicleId, telemetry.CargoTemperature, status);

        var alertMessage = new
        {
            AlertId = Guid.NewGuid().ToString(),
            RuleId = RuleId,
            VehicleId = telemetry.VehicleId,
            AlertType = "CargoTemperature",
            Severity = "Medium",
            Message = $"Temperatura de carga {status} en vehículo {telemetry.VehicleId}: {telemetry.CargoTemperature}°C",
            Temperature = telemetry.CargoTemperature,
            Status = status,
            Limits = new { Min = _minTemperature, Max = _maxTemperature },
            Location = new { telemetry.Latitude, telemetry.Longitude },
            Timestamp = DateTime.UtcNow
        };

        await PublishToKafkaAsync("alerts-topic", alertMessage, serviceProvider);
    }

}