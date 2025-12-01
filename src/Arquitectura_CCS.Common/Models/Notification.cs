using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arquitectura_CCS.Common.Models
{
    public class Notification
    {
        public string NotificationId { get; set; } = Guid.NewGuid().ToString();
        public string VehicleId { get; set; } = string.Empty;
        public string RuleId { get; set; } = string.Empty;
        public string ActionId { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty; // "email", "sms", "push", "webhook"
        public string Recipient { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public bool IsSent { get; set; } = false;
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
    }
}
