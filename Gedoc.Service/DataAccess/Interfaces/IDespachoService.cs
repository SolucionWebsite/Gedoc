using System.Collections.Generic;
using System.IO;
using System.Web;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Service.Sharepoint;

namespace Gedoc.Service.DataAccess.Interfaces
{
    public interface IDespachoService
    {
        #region Despachos Iniciativas
        DatosAjax<List<DespachoIniciativaDto>> GetDatosBandejaEntradaInic(ParametrosGrillaDto<int> param, int idUsuario);
        DespachoIniciativaDto GetDespachoInicById(int id);
        #endregion
        #region Despacho
        DatosAjax<List<DespachoDto>> GetDespachosUltimos(int diasAtras);
        DatosAjax<List<DespachoDto>> GetDespachosIngreso(ParametrosGrillaDto<int> param);
        DespachoDto GetDespachoById(int id);
        DatosAjax<List<DespachoDto>> GetDatosVistas(ParametrosGrillaDto<int> param);
        ResultadoOperacion Save(DespachoDto despacho, IEnumerable<HttpPostedFileBase> files);
        ResultadoOperacion MarcaEliminadoDesp(int despachoId, int usuarioId);
        ResultadoOperacion EliminarDespp(int despachoId);
        ResultadoOperacion ValidaNumeroOficio(string numeroOficio);
        DatosArchivo GetArchivo(int despachoId);
        ResultadoOperacion MarcaEliminadoDespInic(int despachoId, int usuarioId);
        ResultadoOperacion EliminarDespInic(int despachoId);
        DatosArchivo GetArchivoDespInic(int despachoId);
        ResultadoOperacion SaveDespachoInic(DespachoIniciativaDto despacho, IEnumerable<HttpPostedFileBase> files);

        #endregion

    }
}
