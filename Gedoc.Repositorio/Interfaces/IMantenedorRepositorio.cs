using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Interfaces
{
    public interface IMantenedorRepositorio
    {
        List<GenericoDto> GetGenericoMantenedor(Mantenedor tipoMantenedor, string extra, string extra2);
        List<GenericoDto> GetTipoDocumentoAll();
        UsuarioDto GetResponsableUt(int utId);
        DatosAjax<List<UsuarioDto>> GetUsuariosUt(int utId);
        List<UnidadTecnicaDto> GetUnidadTecnicaAll();
        UnidadTecnicaDto GetUnidadTecnicaById(int id);
        UnidadTecnicaDto GetUnidadTecnicaByUtTramiteId(int id);
        List<UnidadTecnicaDto> GetUnidadTecnicaByUsuario(int usuarioId, bool esEncargado);
        ResultadoOperacion SaveUnidadTecnica(UnidadTecnicaDto ut, bool updateSoloActivo);
        ResultadoOperacion SaveUsuarioUnidadTecnica(int utId, int usuarioId);
        void DeleteUnidadTecnica(int utId);
        void DeleteUsuarioUnidadTecnica(int utId, int usuarioId);
        GenericoDto GetSesionTablaById(int id);
        List<PrioridadDto> GetPrioridadAll();
        PrioridadDto GetPrioridadById(string cod);
        UsuarioDto GetUsuarioById(int id);
        OficioDto GetOficioById(int id);
        bool IsAdminUser(int id);
        UsuarioDto GetUsuarioByUserNamePassword(string userName, string passw);
        UsuarioDto GetUsuarioByUserName(string userName);
        List<CalendarioBitacoraDto> GetCalendarioBitacoraByTipo(string tipoBitacoraId, int anno);
        CalendarioBitacoraDto GetCalendarioBitacoraById(int calendarioId);
        List<int> GetCalendarioBitacoraAnnos(string tipoBitacoraId);
        ResultadoOperacion SaveCalendarioBitacora(CalendarioBitacoraDto calendario);
        void DeleteCalendarioBitacora(int calendarioId);
        GenericoDto GetTipoBitacoraById(string cod);
        List<GenericoDto> GetRemitenteResumenAll();
        List<GenericoDto> GetRemitenteResumeByIds(List<int> ids);
        List<RemitenteDto> GetRemitentesByIds(List<int> ids);
        DatosAjax<List<GenericoDto>> GetRemitenteResumenPaging(int skip, int take, string filtro);
        RemitenteDto GetRemitenteById(int id);
        RemitenteDto GetRemitenteByRut(string rut);
        RemitenteDto GetRemitenteByNombre(string nombre);
        List<RemitenteDto> GetRemitenteByFilter(string filter);
        ResultadoOperacion SaveRemitente(RemitenteDto datos);
        ResultadoOperacion DesactivaRemitente(int id, UsuarioActualDto usuario);
        List<AccionBandejaDto> GetDatosAccionesBandeja();
        ConfigBandejaDto GetConfigBandeja(int id, bool resumido, bool byIdBandeja = false, int[] roles = null, int? bandejaMainId = null);
        List<GenericoDto> GetDatosBandejaResumen(int idBandeja);
        List<GenericoDto> GetDatosBandejaResumenUt(int idBandeja);
        List<GenericoDto> GetDatosBandejaResumenEstado(int idBandeja);
        ResultadoOperacion SaveLogSistema(LogSistemaDto log);
        LogWssIntegracionDto GetLastLogWssBySolicitudId(int idSolicitud, string operacion = "", string resultado = "OK");
        ResultadoOperacion SaveLogWss(LogWssIntegracionDto log);
        ResultadoOperacion SaveLogSistemaMulti(List<LogSistemaDto> logList);
        List<GenericoDto> GetRegionesByCodigos(List<string> codigos);
        List<GenericoDto> GetProvinciasByCodigos(List<string> codigos);
        List<GenericoDto> GetComunasByCodigos(List<string> codigos);
        List<string> GetNumeroDespachoResumenAll();
        List<string> GetNumeroDespachoResumeByIds(List<string> ids);
        DatosAjax<List<string>> GetNumeroDespachoResumenPaging(int skip, int take, string filtro);
        DateTime GetFechaDiasHabiles(DateTime fecha, int plazo);
        NotificacionEmailDto GetNotificacionByCodigo(string codigo);
        NotificacionEmailDto GetNotificacionById(int id);
        List<NotificacionEmailDto> GetNotificacionAll();
        ResultadoOperacion SaveNotificacion(NotificacionEmailDto notifDto, bool updateSoloActivo);
        List<PlantillaOficioDto> GetPlantillaOficioAll(bool resumen = true);
        PlantillaOficioDto GetPlantillaOficioById(int id);
        ResultadoOperacion NewPlantillaOficio(PlantillaOficioDto datos);
        ResultadoOperacion UpdatePlantillaOficio(PlantillaOficioDto datos, bool updateSoloActivo);
        List<CampoSeleccionableDto> GetCamposSeleccionablePorGrupos(List<string> tiposCampo);
        List<CampoSeleccionableDto> GetCamposSeleccionable();
        List<CampoSeleccionableDto> GetCamposSeleccionableByPadre(int? padreId, List<string> tiposCampo);
        void MarcaDeletePlantilla(int id, int usuarioId);
        void DeletePlantilla(int id);
        List<ReservaCorrelativoDto> GetReservaCorrelativoAll();
        void ReservarCorrelativoOficio(ReservaCorrelativoDto datos);
        List<GenericoDto> GetTipoTramiteGenericoAll();
        List<TipoTramiteDto> GetTipoTramiteAll(bool incluirInactivos);
        TipoTramiteDto GetTipoTramiteByCodigo(string codigo);
        TipoTramiteDto GetTipoTramiteById(int id);
        ResultadoOperacion SaveTipoTramite(TipoTramiteDto tipoTr, bool updateSoloActivo);
        void DeleteTipoTramite(int id);

        #region Listas
        List<ListaDto> GetListaAll();
        ListaDto GetListaById(int id);
        ResultadoOperacion SaveLista(ListaDto lista);
        void DeleteLista(int id);
        List<ListaValorDto> GetListaValorAllByListaId(int listaId);
        ListaValorDto GetListaValorById(int listaId, string codigo);
        ResultadoOperacion SaveListaValor(ListaValorDto listaValor);
        void DeleteListaValor(int listaId, string codigo);
        #endregion

        #region Casos
        List<CasoDto> GetCasoAll();
        CasoDto GetCasoById(int id);
        ResultadoOperacion NewCaso(CasoDto caso);
        ResultadoOperacion UpdateCaso(CasoDto caso);
        List<RequerimientoDto> GetRequerimientosNoAsignadoCaso(CasoFilterDto filtros);
        List<RequerimientoDto> GetRequerimientosByCasoId(int casoId, CasoFilterDto filtros);
        void AgregaRequeriminetosCaso(int casoId, List<int> reqs);
        void EliminaRequerimientosCaso(List<int> reqs);
        ResultadoOperacion EliminaCaso(int casoId);

        #endregion

        List<ReporteDto> GetReporteAll();

        ReporteDto GetReporteById(int id);
    }
}
