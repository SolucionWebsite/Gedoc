using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;

namespace Gedoc.Service.DataAccess.Interfaces
{
    public interface IMantenedorService
    {
        DatosAjax<List<GenericoDto>> GetGenericoMatenedor(Mantenedor tipoMantenedor, string extra = "", string extra2 = "");
        DatosAjax<List<GenericoDto>> GetTipoDocumentoAll();
        DatosAjax<List<UnidadTecnicaDto>> GetUnidadTecnicaAll();
        UnidadTecnicaDto GetUnidadTecnicaById(int id);
        UnidadTecnicaDto GetUnidadTecnicaByUtTramiteId(int id);
        List<UnidadTecnicaDto> GetUnidadTecnicaByUsuario(int usuarioId, bool esEncargado);
        DatosAjax<List<UsuarioDto>> GetUsuariosUt(int utId);
        ResultadoOperacion SaveUnidadTecnica(UnidadTecnicaDto ut, bool updateSoloActivo);
        ResultadoOperacion DeleteUnidadTecnica(int utId);
        ResultadoOperacion DeleteUsuarioUnidadTecnica(int utId, int usuarioId);
        ResultadoOperacion SaveUsuarioUnidadTecnica(int utId, int usuarioId);
        GenericoDto GetSesionTablaById(int id);
        DatosAjax<List<PrioridadDto>> GetPrioridadAll();
        PrioridadDto GetPrioridadById(string cod);
        DatosAjax<List<CalendarioBitacoraDto>> GetCalendarioBitacoraByTipo(string tipoBitacoraId, int anno);
        CalendarioBitacoraDto GetCalendarioBitacoraById(int calendarioId);
        DatosAjax<List<GenericoDto>> GetCalendarioBitacoraAnnos(string tipoBitacoraId);
        ResultadoOperacion SaveCalendarioBitacora(CalendarioBitacoraDto calendario);
        ResultadoOperacion DeleteCalendarioBitacora(int calendarioId);
        ResultadoOperacion SaveLogSistema(LogSistemaDto log);
        ResultadoOperacion SaveLogSistemaMulti(List<LogSistemaDto> logList);
        LogWssIntegracionDto GetLastLogWssBySolicitudId(int idSolicitud, string operacion = "", string resultado = "OK");
        ResultadoOperacion SaveLogWss(LogWssIntegracionDto log);
        DatosAjax<List<GenericoDto>> GetRemitenteResumenAll();
        DatosAjax<List<GenericoDto>> GetRemitenteResumenByIds(List<int> ids);
        DatosAjax<List<GenericoDto>> GetRemitenteResumenPaging(ParametrosGrillaDto<int> param);
        DatosAjax<RemitenteDto> GetRemitenteById(int id);
        RemitenteDto GetRemitenteByRut(string rut);
        RemitenteDto GetRemitenteByNombre(string nombre);
        List<RemitenteDto> GetRemitenteByFilter(string filter);
        ResultadoOperacion SaveRemitente(RemitenteDto datos);
        ResultadoOperacion DesactivaRemitente(int id, UsuarioActualDto usuario);

        DatosAjax<List<string>> GetNumeroDespachoResumenAll();
        DatosAjax<List<dynamic>> GetNumeroDespachoResumenByIds(List<string> ids);
        DatosAjax<List<string>> GetNumeroDespachoResumenPaging(ParametrosGrillaDto<int> param);

        /// <summary>
        /// Devuelve los datos de todas las acciones configuradas en el sistema.
        /// </summary>
        /// <returns></returns>
        List<AccionBandejaDto> GetDatosAccionesBandeja();

        ConfigBandejaDto GetBandejaById(int id, bool byIdBandeja = false);

        /// <summary>
        /// Devuelve las acciones permitidas en la bandeja especificada.
        /// </summary>
        /// <param name="idBandeja">Id de bandeja</param>
        /// <returns>Configuración de la bandeja con las acciones permitidas</returns>
        ConfigBandejaDto GetAccionesBandeja(int idBandeja, int[] roles, bool byIdBandeja = true, int? bandejaMainId = null);

        List<GenericoDto> GetDatosBandejaResumen(int idBandeja);
        List<GenericoDto> GetDatosBandejaResumenUt(int idBandeja);
        List<GenericoDto> GetDatosBandejaResumenEstado(int idBandeja);

        List<GenericoDto> GetRegionesByCodigos(List<string> codigos);
        List<GenericoDto> GetProvinciasByCodigos(List<string> codigos);
        List<GenericoDto> GetComunasByCodigos(List<string> codigos);
        DateTime GetFechaDiasHabiles(DateTime fecha, int plazo);
        NotificacionEmailDto GetNotificacionById(int id);
        DatosAjax<List<NotificacionEmailDto>> GetNotificacionAll();
        ResultadoOperacion SaveNotificacion(NotificacionEmailDto notif, bool updateSoloActivo);
        DatosAjax<List<ReservaCorrelativoDto>> GetReservaCorrelativoAll();
        ResultadoOperacion ReservarCorrelativoOficio(int usuarioId, string observaciones);
        DatosAjax<List<GenericoDto>> GetTipoTramiteGenericoAll();
        DatosAjax<List<TipoTramiteDto>> GetTipoTramiteAll(bool incluirInactivos);
        TipoTramiteDto GetTipoTramiteByCodigo(string codigo);
        TipoTramiteDto GetTipoTramiteById(int id);
        ResultadoOperacion SaveTipoTramite(TipoTramiteDto tipoTr, bool updateSoloActivo);
        ResultadoOperacion DeleteTipoTramite(int tipoTrId);
        UsuarioDto GetUsuarioByUserNamePassword(string userName, string passw);
        UsuarioDto GetUsuarioByUserName(string userName);

        #region Listas
        DatosAjax<List<ListaDto>> GetListaAll();
        ListaDto GetListaById(int id);
        ResultadoOperacion SaveLista(ListaDto lista);
        ResultadoOperacion DeleteLista(int id);
        DatosAjax<List<ListaValorDto>> GetListaValorAllByListaId(int listaId);
        ListaValorDto GetListaValorById(int listaId, string codigo);
        ResultadoOperacion SaveListaValor(ListaValorDto listaValor);
        ResultadoOperacion DeleteListaValor(int id, string codigo);
        List<CasoDto> GetCasoAll();
        CasoDto GetCasoById(int id);
        ResultadoOperacion SaveCaso(CasoDto caso);
        List<RequerimientoDto> GetRequerimientosNoAsignadoCaso(CasoFilterDto filtros);
        List<RequerimientoDto> GetRequerimientosByCasoId(int casoId, CasoFilterDto filtros);
        ResultadoOperacion AgregaRequeriminetosCaso(int casoId, List<int> reqs);
        ResultadoOperacion EliminaRequerimientosCaso(List<int> reqs);
        ResultadoOperacion EliminaCaso(int casoId);

        #endregion

        List<ReporteDto> GetReporteAll();
        ReporteDto GetReporteById(int id);
    }
}
