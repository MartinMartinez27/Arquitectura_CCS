using Arquitectura_CCS.Common.Enums;
using System.Text.Json.Serialization;

namespace Arquitectura_CCS.Common.Models
{
    public class VehicleTelemetry
    {
        public string TelemetryId { get; set; } = Guid.NewGuid().ToString();
        public string VehicleId { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }

        // Ubicación
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; } // km/h
        public double Direction { get; set; } // grados

        // Estado del vehículo
        public bool IsMoving { get; set; }
        public bool EngineOn { get; set; }
        public double FuelLevel { get; set; } // porcentaje

        // Para camiones específicamente
        public double? CargoTemperature { get; set; }
        public string? CargoStatus { get; set; }
        public bool? IsPlannedStop { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navegación - IGNORAR completamente en JSON
        [JsonIgnore]
        public virtual Vehicle? Vehicle { get; set; }
    }
}