using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;
using Gedoc.Helpers.Dto;
using Gedoc.Interop.Wss.Data;
using Gedoc.Interop.Wss.Helpers;
using Gedoc.Service.DataAccess;
using DateTime = System.DateTime;

namespace Gedoc.Interop.Wss.Services
{
    public interface IIngresoSrv
    {
        DatosIngresoNuevo GetDatosIngreso(string numeroIng);
        ResultadoNuevoIngreso CreaIngreso(DatosNuevoIngreso datos);
        DatosAdjunto GetAdjuntoDespacho(string numero);
        ResultadoEstadoSolicitud GetEstadoSolicitud(int idSolicitud);
        ResultadoOperacion InsertaLog(LogWssIntegracionDto log);

    }
}