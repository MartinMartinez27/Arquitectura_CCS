using Arquitectura_CCS.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arquitectura_CCS.RulesEngine.Rules.ConcreteRules;

public class UnplannedStopRule : BaseRule
{
    public override string RuleId => "UNPLANNED_STOP_001";
    public override string Name => "Detención No Planeada";
    public override string Description => "Detecta cuando un vehículo se detiene inesperadamente";

    public override async Task<bool> EvaluateAsync(VehicleTelemetry telemetry)
    {
        return !telemetry.IsMoving &&
               telemetry.EngineOn &&
               !(telemetry.IsPlannedStop ?? false);
    }

    public override async Task ExecuteActionsAsync(VehicleTelemetry telemetry, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<UnplannedStopRule>>();

        logger.LogWarning("Unplanned stop detected for vehicle {VehicleId} at {Location}",
            telemetry.VehicleId, $"{telemetry.Latitude}, {telemetry.Longitude}");

        await Task.CompletedTask;
    }
}