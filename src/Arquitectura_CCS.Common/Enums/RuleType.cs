using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arquitectura_CCS.Common.Enums
{
    public enum RuleType
    {
        UnplannedStop = 1,  // Detención no planeada
        SpeedLimit = 2,     // Límite de velocidad
        Geofence = 3,       // Fuera de zona permitida
        Emergency = 4,      // Emergencia
        Temperature = 5,    // Temperatura de carga
        TimeWindow = 6      // Horario no permitido
    }
}
