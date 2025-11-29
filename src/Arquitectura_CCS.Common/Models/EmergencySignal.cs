using Arquitectura_CCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arquitectura_CCS.Common.Models
{
    public class EmergencySignal
    {
        public string EmergencyId { get; set; } = Guid.NewGuid().ToString();
        public string VehicleId { get; set; } = string.Empty;
        public EmergencyType EmergencyType { get; set; }
        public string Source { get; set; } = string.Empty; // "panic_button", "mobile_app", "sensor"

        // Ubicación al momento de la emergencia
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string Description { get; set; } = string.Empty;
        public string? AdditionalData { get; set; } // JSON con datos adicionales
        public bool IsResolved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // Navegación
        public virtual Vehicle? Vehicle { get; set; }
    }
}
