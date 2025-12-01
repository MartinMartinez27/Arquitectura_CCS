using Arquitectura_CCS.Common.Models;
using Arquitectura_CCS.RulesEngine.Rules;
using Arquitectura_CCS.RulesEngine.Rules.ConcreteRules;
using Microsoft.Extensions.Logging;

namespace Arquitectura_CCS.RulesEngine.Engine;

public class RulesEngine
{
    private readonly ILogger<RulesEngine> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IRule> _rules;

    public RulesEngine(ILogger<RulesEngine> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _rules = InitializeRules();
    }

    private List<IRule> InitializeRules()
    {
        return new List<IRule>
        {
            new UnplannedStopRule(),
            new SpeedLimitRule(),
            new CargoTemperatureRule()
        };
    }

    public async Task ProcessTelemetryAsync(VehicleTelemetry telemetry)
    {
        _logger.LogInformation("Evaluating {RuleCount} rules for vehicle {VehicleId}", _rules.Count, telemetry.VehicleId);

        var executedRules = new List<string>();

        foreach (var rule in _rules.OrderBy(r => r.Priority))
        {
            try
            {
                _logger.LogDebug("Evaluating rule: {RuleName}", rule.Name);

                var shouldExecute = await rule.EvaluateAsync(telemetry);

                if (shouldExecute)
                {
                    _logger.LogInformation("Rule triggered: {RuleName} for vehicle {VehicleId}",
                        rule.Name, telemetry.VehicleId);

                    await rule.ExecuteActionsAsync(telemetry, _serviceProvider);
                    executedRules.Add(rule.Name);
                }
                else
                {
                    _logger.LogDebug("Rule not triggered: {RuleName}", rule.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing rule {RuleName} for vehicle {VehicleId}",
                    rule.Name, telemetry.VehicleId);
            }
        }

        if (executedRules.Any())
        {
            _logger.LogInformation("Rules executed for vehicle {VehicleId}: {Rules}",
                telemetry.VehicleId, string.Join(", ", executedRules));
        }
        else
        {
            _logger.LogDebug("No rules executed for vehicle {VehicleId}", telemetry.VehicleId);
        }
    }
}