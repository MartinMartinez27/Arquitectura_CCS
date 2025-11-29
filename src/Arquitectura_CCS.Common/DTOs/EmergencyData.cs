using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arquitectura_CCS.Common.DTOs
{
    public class EmergencyData
    {
        public string EmergencyId { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public int EmergencyType { get; set; }
        public string Source { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? AdditionalData { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Priority { get; set; } = string.Empty;
    }
}
