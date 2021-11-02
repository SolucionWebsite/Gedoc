using System;
using System.Collections.Generic;
using System.Web;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Service.Sharepoint;

namespace Gedoc.Service.DataAccess.Interfaces
{
    public interface IAdjuntoService
    {
        DatosAjax<List<AdjuntoDto>> GetAdjuntosIngreso(int idIngreso);
        AdjuntoDto GetAdjuntoById(int id);
        DatosArchivo GetArchivo(int adjuntoId);
        DatosArchivo GetArchivo(string url);
        ResultadoOperacion Save(AdjuntoDto adjunto, IEnumerable<HttpPostedFileBase> files);
        ResultadoOperacion MarcaAdjuntosEliminado(int[] adjIds, int usuarioId);
        DatosAjax<List<AdjuntoDto>> GetAdjuntosUsuario(DateTime fechaD, DateTime fechaH, int usuarioId);
        DatosAjax<List<AdjuntoDto>> GetAdjuntosOficio(int idOficio);
        AdjuntoDto GetAdjuntoOficioById(int id);
        DatosArchivo GetArchivoOficio(int adjuntoId);
        ResultadoOperacion SaveAdjuntoOficio(AdjuntoDto adjunto, IEnumerable<HttpPostedFileBase> files);
        ResultadoOperacion MarcaAdjuntosOficioEliminado(int[] adjIds, int usuarioId);
        DatosAjax<List<AdjuntoDto>> GetAdjuntosOficioUsuario(DateTime fechaD, DateTime fechaH, int usuarioId);

    }
}
