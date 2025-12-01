using Arquitectura_CCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arquitectura_CCS.Common.Models
{
    public class EmergencySignalRequest
    {
        public string VehicleId { get; set; } = string.Empty;
        public EmergencyType EmergencyType { get; set; }
        public string Source { get; set; } = "mobile_app"; // "panic_button", "mobile_app", "sensor"
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? AdditionalData { get; set; }
    }
}
