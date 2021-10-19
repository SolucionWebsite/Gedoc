using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Enum
{
    public enum AspectoCampo
    {
        SoloLectura,
        Editable,
        Oculto,
        Deshabilitado, // En el Solo Lectura aparece un label en el campo, con Deshabilitado aparece el control de ingreso de datos pero deshabilitado,
        Habilitado
    }
}
