using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Enum
{
    public enum EstadoOficio
    {
        Borrador = 1,
        EnviadoEncargadoUT = 2,
        AprobadoEncargadoUT = 3,
        CorreccionesJefatura = 4,
        CorreccionesEncargadoUT = 5,
        Firmado = 6,
        CorreccionesVisadorGen = 7,
        AprobadoVisadorGen = 8,
        EnviadoFirma = 9,
        RechazadoFirma = 10
    }
}
