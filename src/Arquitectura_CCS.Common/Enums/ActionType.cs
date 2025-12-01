using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arquitectura_CCS.Common.Enums
{
    public enum ActionType
    {
        None = 0,
        NotifyOwner = 1,        // Notificar al propietario
        NotifyAuthorities = 2,  // Notificar autoridades
        AlertRescue = 3,        // Alertar organismos de socorro
        LogEvent = 4,           // Registrar en log
        CallDriver = 5          // Llamar al conductor
    }
}
