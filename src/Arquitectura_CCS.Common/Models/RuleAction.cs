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
        public ActionType ActionType { get; set; } = ActionType.None;
        public string Target { get; set; } = string.Empty; 
        public string MessageTemplate { get; set; } = string.Empty;
        public string? Parameters { get; set; } 

        public int DelaySeconds { get; set; } = 0;
        public bool IsEnabled { get; set; } = true;
        public Rule Rule { get; set; } = new Rule();
    }
}
