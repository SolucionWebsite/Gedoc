using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Service.Sharepoint
{
    public enum ReturnSharePoint
    {
        Error = -1,
        CarpetaCreada = 1,
        CarpetaYaExiste = 2,
        ArchivoEliminado = 3,
        ArchivoCreado,
        ArchivoDescargado,
        ArchivoYaExiste,
        ArchivoNoExiste,
        AccesoDenegado,
        NoSePuedeBorrarArchivo,
        SharePointTimeOut,
        SharePointCommunicationException
    }
}
