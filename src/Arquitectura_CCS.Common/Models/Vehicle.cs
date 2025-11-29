using Arquitectura_CCS.Common.Enums;
using System.Text.Json.Serialization;

namespace Arquitectura_CCS.Common.Models
{
    public class Vehicle
    {
        public string VehicleId { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegación - IGNORAR en serialización JSON
        [JsonIgnore]
        public List<Rule> Rules { get; set; } = new();

        [JsonIgnore]
        public List<VehicleTelemetry> TelemetryHistory { get; set; } = new();
    }
}