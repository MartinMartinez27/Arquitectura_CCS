using Arquitectura_CCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arquitectura_CCS.RulesEngine
{
    public interface IRule
    {
        string RuleId { get; }
        string Name { get; }
        string Description { get; }
        int Priority { get; }
        Task<bool> EvaluateAsync(VehicleTelemetry telemetry);
        Task ExecuteActionsAsync(VehicleTelemetry telemetry, IServiceProvider serviceProvider);
    }
}
