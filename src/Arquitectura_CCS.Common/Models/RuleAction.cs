using Arquitectura_CCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arquitectura_CCS.Common.Models
{
    public class RuleAction
    {
        public string ActionId { get; set; } = Guid.NewGuid().ToString();
        public string RuleId { get; set; } = string.Empty;
        public ActionType ActionType { get; set; }

        // Configuración de la acción
        public string Target { get; set; } = string.Empty; // email, phone, webhook, etc.
        public string MessageTemplate { get; set; } = string.Empty;
        public string? Parameters { get; set; } // JSON con parámetros adicionales

        public int DelaySeconds { get; set; } = 0; // Retardo en ejecución
        public bool IsEnabled { get; set; } = true;

        // Navegación
        public Rule Rule { get; set; } = null!;
    }
}
