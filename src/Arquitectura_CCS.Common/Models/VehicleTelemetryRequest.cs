using Arquitectura_CCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arquitectura_CCS.Common.Models
{
    public class VehicleTelemetryRequest
    {
        public string VehicleId { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public double Direction { get; set; }
        public bool IsMoving { get; set; }
        public bool EngineOn { get; set; }
        public double FuelLevel { get; set; }
        public double? CargoTemperature { get; set; }
        public string? CargoStatus { get; set; }
        public bool? IsPlannedStop { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
