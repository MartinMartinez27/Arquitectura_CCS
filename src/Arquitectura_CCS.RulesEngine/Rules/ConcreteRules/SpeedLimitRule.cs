using Arquitectura_CCS.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arquitectura_CCS.RulesEngine.Rules.ConcreteRules;

public class SpeedLimitRule : BaseRule
{
    public override string RuleId => "SPEED_LIMIT_001";
    public override string Name => "Límite de Velocidad";
    public override string Description => "Detecta cuando un vehículo excede el límite de velocidad";

    private readonly Dictionary<int, double> _speedLimits = new()
    {
        { 1, 80.0 }, // Camiones: 80 km/h
        { 2, 100.0 }, // Carros: 100 km/h  
        { 3, 60.0 }, // Motos: 60 km/h
        { 4, 60.0 }, // Taxis: 60 km/h
        { 5, 80.0 }  // Buses: 80 km/h
    };

    public override async Task<bool> EvaluateAsync(VehicleTelemetry telemetry)
    {
        if (_speedLimits.TryGetValue((int)telemetry.VehicleType, out double limit))
        {
            return telemetry.Speed > limit;
        }
        return false;
    }

    public override async Task ExecuteActionsAsync(VehicleTelemetry telemetry, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SpeedLimitRule>>();

        var limit = _speedLimits[(int)telemetry.VehicleType];

        logger.LogWarning("Speed limit exceeded for vehicle {VehicleId}: {Speed} km/h (Limit: {Limit} km/h)",
            telemetry.VehicleId, telemetry.Speed, limit);

        var alertMessage = new
        {
            AlertId = Guid.NewGuid().ToString(),
            RuleId = RuleId,
            VehicleId = telemetry.VehicleId,
            AlertType = "SpeedLimitExceeded",
            Severity = "High",
            Message = $"Vehículo {telemetry.VehicleId} excedió límite de velocidad: {telemetry.Speed} km/h",
            Speed = telemetry.Speed,
            Limit = limit,
            Location = new { telemetry.Latitude, telemetry.Longitude },
            Timestamp = DateTime.UtcNow
        };

        await PublishToKafkaAsync("alerts-topic", alertMessage, serviceProvider);
    }
}