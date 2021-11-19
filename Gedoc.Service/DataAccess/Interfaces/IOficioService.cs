using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Service.Sharepoint;

namespace Gedoc.Service.DataAccess.Interfaces
{
    public interface IOficioService
    {
        DatosAjax<List<PlantillaOficioDto>> GetPlantillaOficioAll(bool resumen = true);
        PlantillaOficioDto GetPlantillaOficioById(int id);
        ResultadoOperacion GetPlantillaConDatos(int plantillaId, List<int> reqIds, int? mainReqId);

        ResultadoOperacion GetContenidoOficioActualizado(int oficioId, string contenido, List<int> reqIds,
            int? mainReqId);
        ResultadoOperacion SavePlantillaOficio(PlantillaOficioDto datos, bool updateSoloActivo);
        List<CampoSeleccionableDto> GetCamposSeleccionablePorGrupos(int? tipoPlantilla);
        List<CampoSeleccionableDto> GetCamposSeleccionableByPadre(int? padreId, int? tipoPlantilla);
        ResultadoOperacion MarcaDeletePlantilla(int plantillaId, UsuarioActualDto usuario);
        ResultadoOperacion DeletePlantilla(int id);
        RequerimientoDto GetDatosRequerimientoMainOficio(int oficioId, List<int> reqsId, bool releer);

        #region Oficio

        DatosAjax<List<OficioDto>> GetDatosBandejaOficio(ParametrosGrillaDto<int> param, int idUsuario);
        DatosAjax<List<OficioDto>> GetOficoAll();
        OficioDto GetOficoById(int id, bool procesaEncabezado = false, bool tipoWord = false);
        DatosAjax<List<OficioObservacionDto>> GetObservacionesOficio(int oficioId);
        void SeparaEncabezadoPiePagina(OficioDto oficio);
        ResultadoOperacion SaveOficio(OficioDto oficio, byte[] file);
        ResultadoOperacion UpdateOficiosPendienteFirma(UsuarioActualDto datosUsuario);
        DatosArchivo GetArchivo(int oficioId);
        byte[] GetOficioPdfFromHtmlById(int oficioId, string baseUrl, bool tipoWord);
        byte[] GetPdfFromHtml(string contenido, string baseUrl);
        DatosAjax<List<LogSistemaDto>> HistorialOficio(int oficioId);

        #endregion
    }
}
