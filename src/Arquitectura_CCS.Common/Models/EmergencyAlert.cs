namespace Arquitectura_CCS.Common.Models
{
    public class EmergencyAlert
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString();
        public string VehicleId { get; set; }
        public string AlertType { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Severity { get; set; } = 1;
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
    }
}
