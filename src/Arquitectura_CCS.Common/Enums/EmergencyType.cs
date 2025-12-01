using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arquitectura_CCS.Common.Enums
{
    public enum EmergencyType
    {
        None = 0,
        PanicButton = 1,    // Botón de pánico
        Mechanical = 2,     // Problemas mecánicos
        Security = 3,       // Eventualidades de seguridad
        Accident = 4,       // Accidente
        UnplannedStop = 5,  // Detención no planeada
        Theft = 6           // Robo
    }
}
