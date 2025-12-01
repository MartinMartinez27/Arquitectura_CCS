using Arquitectura_CCS.Common.Enums;

namespace Arquitectura_CCS.Common.Models
{
    public class Rule
    {
        public string RuleId { get; set; } = Guid.NewGuid().ToString();
        public string? VehicleId { get; set; } // Null para reglas globales
        public string Name { get; set; } = string.Empty;
        public RuleType RuleType { get; set; }

        // Condiciones (serializadas como JSON)
        public string Conditions { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navegación
        public List<RuleAction> Actions { get; set; } = new();
        public Vehicle? Vehicle { get; set; }
    }
}
