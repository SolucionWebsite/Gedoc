using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Service.DataAccess.Interfaces;

namespace Gedoc.Service.DataAccess
{
    public class MantenedorService: BaseService, IMantenedorService
    {
        private readonly IMantenedorRepositorio _repoMant;

        public MantenedorService(IMantenedorRepositorio repoMant)
        {
            this._repoMant = repoMant;
        }

        public DatosAjax<List<GenericoDto>> GetGenericoMatenedor(Mantenedor tipoMantenedor, string extra = "", string extra2 = "")
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<GenericoDto>>(new List<GenericoDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetGenericoMantenedor(tipoMantenedor, extra, extra2);
                resultado.Data = datos;
                resultado.Total = resultado.Data != null && resultado.Total == 0
                    ? resultado.Data.Count
                    : resultado.Total;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de " + tipoMantenedor);
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public DatosAjax<List<GenericoDto>> GetTipoDocumentoAll()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<GenericoDto>>(new List<GenericoDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetTipoDocumentoAll();
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de Tipo de Documento.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        #region Unidad Técnica
        public DatosAjax<List<UnidadTecnicaDto>> GetUnidadTecnicaAll()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<UnidadTecnicaDto>>(new List<UnidadTecnicaDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetUnidadTecnicaAll();
                resultado.Data = datos;
                resultado.Total = datos.Count;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de Unidad Técnica.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public UnidadTecnicaDto GetUnidadTecnicaById(int id)
        {
            try
            {
                return _repoMant.GetUnidadTecnicaById(id);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new UnidadTecnicaDto();
        }

        public UnidadTecnicaDto GetUnidadTecnicaByUtTramiteId(int id)
        {
            try
            {
                return _repoMant.GetUnidadTecnicaByUtTramiteId(id) ?? new UnidadTecnicaDto();
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public List<UnidadTecnicaDto> GetUnidadTecnicaByUsuario(int usuarioId, bool esEncargado)
        {
            try
            {
                return _repoMant.GetUnidadTecnicaByUsuario(usuarioId, esEncargado);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new List<UnidadTecnicaDto>();
            }
        }

        public DatosAjax<List<UsuarioDto>> GetUsuariosUt(int utId)
        {
            var resultado = new DatosAjax<List<UsuarioDto>>(new List<UsuarioDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                resultado = _repoMant.GetUsuariosUt(utId);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return resultado;
        }

        public ResultadoOperacion SaveUnidadTecnica(UnidadTecnicaDto ut, bool updateSoloActivo)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var logSistema = new LogSistemaDto()
                {
                    Flujo = "UNIDADTECNICA",
                    Fecha = DateTime.Now,
                    Origen = "UNIDADTECNICA",
                    OrigenFecha = null,
                    RequerimientoId = 0,
                    Usuario = ut.UsuarioActual ?? "<desconocido>",
                    UsuarioId = ut.UsuarioCreacionId
                };

                // Se graba en BD
                resultado = _repoMant.SaveUnidadTecnica(ut, updateSoloActivo);

                resultado.Mensaje = resultado.Codigo < 0
                    ? resultado.Mensaje
                    : (ut.Id == 0 ? "Se ha creado satisfactoriamente el registro." : "Se ha actualizado satisfactoriamente el registro.");

                logSistema.Accion = ut.Id == 0 ? "CREACION" : "EDICION";
                logSistema.OrigenId = ut.Id;
                logSistema.ExtraData = null;
                _repoMant.SaveLogSistema(logSistema);
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion SaveUsuarioUnidadTecnica(int utId, int usuarioId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {

                // Se graba en BD
                resultado = _repoMant.SaveUsuarioUnidadTecnica(utId, usuarioId);

            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion DeleteUnidadTecnica(int utId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                _repoMant.DeleteUnidadTecnica(utId);
                resultado.Codigo = 1;
                resultado.Mensaje = "Registro eliminado con éxito.";
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "eliminar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion DeleteUsuarioUnidadTecnica(int utId, int usuarioId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                _repoMant.DeleteUsuarioUnidadTecnica(utId, usuarioId);
                resultado.Codigo = 1;
                resultado.Mensaje = "Usuario eliminado de los integrantes de la UT.";
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "eliminar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }
        #endregion

        public GenericoDto GetSesionTablaById(int id)
        {
            try
            {
                return _repoMant.GetSesionTablaById(id);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new GenericoDto();
        }

        public DatosAjax<List<PrioridadDto>> GetPrioridadAll()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<PrioridadDto>>(new List<PrioridadDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetPrioridadAll();
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de Priorización.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public PrioridadDto GetPrioridadById(string cod)
        {
            return _repoMant.GetPrioridadById(cod);
        }

        #region Calendario Bitácoras
        public DatosAjax<List<CalendarioBitacoraDto>> GetCalendarioBitacoraByTipo(string tipoBitacoraId, int anno)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<CalendarioBitacoraDto>>(new List<CalendarioBitacoraDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetCalendarioBitacoraByTipo(tipoBitacoraId, anno);
                resultado.Data = datos;
                resultado.Total = datos.Count;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de Calendario Bitácora.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public CalendarioBitacoraDto GetCalendarioBitacoraById(int calendarioId)
        {
            try
            {
                return _repoMant.GetCalendarioBitacoraById(calendarioId);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public DatosAjax<List<GenericoDto>> GetCalendarioBitacoraAnnos(string tipoBitacoraId)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<GenericoDto>>(new List<GenericoDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetCalendarioBitacoraAnnos(tipoBitacoraId);
                resultado.Data = datos.Select(d => new GenericoDto { IdInt = d, Titulo = d.ToString() }).ToList();
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de Calendario Bitácora.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public ResultadoOperacion SaveCalendarioBitacora(CalendarioBitacoraDto calendario)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var logSistema = new LogSistemaDto()
                {
                    Flujo = "CALENDARIO_BITACORA",
                    Fecha = DateTime.Now,
                    Origen = "CALENDARIO_BITACORA",
                    OrigenFecha = calendario.Fecha,
                    RequerimientoId = 0,
                    Usuario = calendario.UsuarioActual ?? "<desconocido>",
                    UsuarioId = calendario.UsuarioCreacionId
                };

                // Se graba en BD
                resultado = _repoMant.SaveCalendarioBitacora(calendario);

                resultado.Mensaje = resultado.Codigo < 0
                    ? resultado.Mensaje
                    : (calendario.Id == 0 ? "Se ha creado satisfactoriamente el registro."  : "Se ha actualizado satisfactoriamente el registro.");
                
                logSistema.Accion = calendario.Id == 0 ? "CREACION" : "EDICION";
                logSistema.OrigenId = calendario.Id;
                logSistema.ExtraData = $"FECHA: {calendario.Fecha.GetValueOrDefault().ToString(GeneralData.FORMATO_FECHA_CORTO)}. TIPO BITÁCORA: {calendario.TipoBitacoraCod}";
                _repoMant.SaveLogSistema(logSistema);
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                if (resultado.Codigo > 0)
                {
                    resultado = new ResultadoOperacion(-1,
                        "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
                }
            }
            return resultado;
        }

        public ResultadoOperacion DeleteCalendarioBitacora(int calendarioId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                _repoMant.DeleteCalendarioBitacora(calendarioId);
                resultado.Codigo = 1;
                resultado.Mensaje = "Registro eliminado con éxito.";
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                if (resultado.Codigo > 0)
                {
                    resultado = new ResultadoOperacion(-1,
                        "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
                }
            }
            return resultado;
        }
        #endregion

        public ResultadoOperacion SaveLogSistema(LogSistemaDto log)
        {
            return _repoMant.SaveLogSistema(log);
        }

        public ResultadoOperacion SaveLogSistemaMulti(List<LogSistemaDto> logList)
        {
            try
            {
                return _repoMant.SaveLogSistemaMulti(logList);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new ResultadoOperacion(-1, "Error", null);
            }
        }

        public LogWssIntegracionDto GetLastLogWssBySolicitudId(int idSolicitud, string operacion = "", string resultado = "OK")
        {
            try
            {
                operacion = string.IsNullOrWhiteSpace(operacion) ? "NUEVO INGRESO" : operacion;
                return _repoMant.GetLastLogWssBySolicitudId(idSolicitud, operacion, resultado) ?? new LogWssIntegracionDto();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public ResultadoOperacion SaveLogWss(LogWssIntegracionDto log)
        {
            return _repoMant.SaveLogWss(log);
        }

        #region Remitente

        public DatosAjax<List<GenericoDto>> GetRemitenteResumenAll()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<GenericoDto>>(new List<GenericoDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetRemitenteResumenAll();
                resultado.Data = datos.ToList();
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de Remitentes.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public DatosAjax<List<GenericoDto>> GetRemitenteResumenByIds(List<int> ids)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<GenericoDto>>(new List<GenericoDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetRemitenteResumeByIds(ids);
                resultado.Data = datos.ToList();
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de Remitentes.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public DatosAjax<List<GenericoDto>> GetRemitenteResumenPaging(ParametrosGrillaDto<int> param)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<GenericoDto>>(new List<GenericoDto>(), resultadoOper);
            try
            {
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var filter = param.Filters?[0] == null
                    ? ""
                    : param.Filters[0].Value;

                var datos = _repoMant.GetRemitenteResumenPaging(skip, take, filter);
                resultado = datos;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de Remitentes.");
            }
            return resultado;
        }

        public DatosAjax<RemitenteDto> GetRemitenteById(int id)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<RemitenteDto>(new RemitenteDto(), resultadoOper);
            try
            {
                var datos = _repoMant.GetRemitenteById(id);
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public RemitenteDto GetRemitenteByRut(string rut)
        {
            try
            {
                return _repoMant.GetRemitenteByRut(rut) ?? new RemitenteDto();
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public RemitenteDto GetRemitenteByNombre(string nombre)
        {
            try
            {
                return _repoMant.GetRemitenteByNombre(nombre) ?? new RemitenteDto();
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public List<RemitenteDto> GetRemitenteByFilter(string filter)
        {
            try
            {
                return _repoMant.GetRemitenteByFilter(filter);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public ResultadoOperacion SaveRemitente(RemitenteDto datos)
        {
            try
            {
                return _repoMant.SaveRemitente(datos);
                //TODO: agregar log para registrar acción de nuevo/edición remitente
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ResultadoOperacion(-1, "Ha ocurrido un error al grabar los datos del remitente.", ex.Message);
            }
        }

        public ResultadoOperacion DesactivaRemitente(int id, UsuarioActualDto usuario)
        {
            try
            {
                return _repoMant.DesactivaRemitente(id, usuario);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ResultadoOperacion(-1, "Ha ocurrido un error al desactivar el remitente.", ex.Message);
            }
        }

        #endregion

        #region Número Despacho

        public DatosAjax<List<string>> GetNumeroDespachoResumenAll()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<string>>(new List<string>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetNumeroDespachoResumenAll();
                resultado.Data = datos.ToList();
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de NumeroDespachos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public DatosAjax<List<dynamic>> GetNumeroDespachoResumenByIds(List<string> ids)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<dynamic>>(new List<dynamic>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetNumeroDespachoResumeByIds(ids).Select(s=> new  { 
                    Id = s,
                    Title = s
                });
                resultado.Data = datos.ToList<dynamic>();
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de NumeroDespachos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public DatosAjax<List<string>> GetNumeroDespachoResumenPaging(ParametrosGrillaDto<int> param)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<string>>(new List<string>(), resultadoOper);
            try
            {
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var filter = param.Filters?[0] == null
                    ? ""
                    : param.Filters[0].Value;

                var datos = _repoMant.GetNumeroDespachoResumenPaging(skip, take, filter);
                resultado = datos;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de NumeroDespachos.");
            }
            return resultado;
        }

        #endregion

        #region Bandejas de entrada
        public List<AccionBandejaDto> GetDatosAccionesBandeja()
        {
            try
            {
                var datosAcciones = _repoMant.GetDatosAccionesBandeja();
                return datosAcciones;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<AccionBandejaDto>();
        }

        public ConfigBandejaDto GetBandejaById(int id, bool byIdBandeja = false)
        {
            try
            {
                var configBandeja = _repoMant.GetConfigBandeja(id, false, byIdBandeja, null);
                return configBandeja;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new ConfigBandejaDto()
            {
                Acciones = new List<AccionPermitidaBandejaDto>()
            };
        }

        public ConfigBandejaDto GetAccionesBandeja(int idBandeja, int[] roles, bool byIdBandeja = true, int? bandejaMainId = null)
        {
            try
            {
                var configBandeja = _repoMant.GetConfigBandeja(idBandeja, true, byIdBandeja, roles, bandejaMainId);
                return configBandeja;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new ConfigBandejaDto()
            {
                Acciones = new List<AccionPermitidaBandejaDto>()
            };
        }

        public List<GenericoDto> GetDatosBandejaResumen(int idBandeja)
        {
            try
            {
                var datos = _repoMant.GetDatosBandejaResumen(idBandeja);
                return datos;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<GenericoDto>();
        }

        public List<GenericoDto> GetDatosBandejaResumenUt(int idBandeja)
        {
            try
            {
                var datos = _repoMant.GetDatosBandejaResumenUt(idBandeja);
                return datos;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<GenericoDto>();
        }

        public List<GenericoDto> GetDatosBandejaResumenEstado(int idBandeja)
        {
            try
            {
                var datos = _repoMant.GetDatosBandejaResumenEstado(idBandeja);
                return datos;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<GenericoDto>();
        }
        #endregion

        #region Región, Provincia, Comuna
        public List<GenericoDto> GetRegionesByCodigos(List<string> codigos)
        {
            try
            {
                return _repoMant.GetRegionesByCodigos(codigos);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new List<GenericoDto>();
            }
        }

        public List<GenericoDto> GetProvinciasByCodigos(List<string> codigos)
        {
            try
            {
                return _repoMant.GetProvinciasByCodigos(codigos);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new List<GenericoDto>();
            }
        }

        public List<GenericoDto> GetComunasByCodigos(List<string> codigos)
        {
            try
            {
                return _repoMant.GetComunasByCodigos(codigos);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new List<GenericoDto>();
            }
        }
        #endregion

        #region Cálculo de días hábiles

        public DateTime GetFechaDiasHabiles(DateTime fecha, int plazo)
        {
            return _repoMant.GetFechaDiasHabiles(fecha, plazo);
        }
        #endregion

        #region Mantenedor de Notificaciones

        public NotificacionEmailDto GetNotificacionById(int id)
        {
            try
            {
                return _repoMant.GetNotificacionById(id);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public DatosAjax<List<NotificacionEmailDto>> GetNotificacionAll()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<NotificacionEmailDto>>(new List<NotificacionEmailDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetNotificacionAll();
                resultado.Data = datos;
                resultado.Total = datos?.Count ?? 0;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public ResultadoOperacion SaveNotificacion(NotificacionEmailDto notif, bool updateSoloActivo)
        {
            // Log
            var logSistema = new LogSistemaDto()
            {
                Flujo = "NOTIFICACIONEMAIL",
                Fecha = DateTime.Now,
                Origen = "NOTIFICACIONEMAIL",
                Usuario = notif.UsuarioActual ?? "<desconocido>",
                UsuarioId = notif.UsuarioCreacionId,
                RequerimientoId = 0,
                EstadoId = 0,
                EtapaId = 0,
                Accion = notif.Id == 0 ? "NUEVO" : "EDICION",
                ExtraData = notif.Codigo
            };

            var resultado =  _repoMant.SaveNotificacion(notif, updateSoloActivo);

            logSistema.OrigenId = notif.Id;
            _repoMant.SaveLogSistema(logSistema);

            return resultado;
        }
        #endregion

        #region Reserva de correlativo
        public DatosAjax<List<ReservaCorrelativoDto>> GetReservaCorrelativoAll()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<ReservaCorrelativoDto>>(new List<ReservaCorrelativoDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetReservaCorrelativoAll();
                resultado.Data = datos;
                resultado.Total = datos?.Count ?? 0;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public ResultadoOperacion ReservarCorrelativoOficio(int usuarioId, string observaciones)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al reservar el correlativo.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var datos = new ReservaCorrelativoDto
                {
                    Correlativo = 0,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacionId = usuarioId,
                    Observaciones = observaciones
                };
                _repoMant.ReservarCorrelativoOficio(datos);
                resultado.Codigo = 1;
                resultado.Mensaje = $"Se ha reservado el correlativo {datos.CorrelativoStr}.";
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return resultado;
        }
        #endregion

        #region Tipo de Trámite

        public DatosAjax<List<GenericoDto>> GetTipoTramiteGenericoAll()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<GenericoDto>>(new List<GenericoDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetTipoTramiteGenericoAll();
                resultado.Data = datos;
                resultado.Total = datos.Count;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de Tipo de Trámite.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public DatosAjax<List<TipoTramiteDto>> GetTipoTramiteAll(bool incluirInactivos)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<TipoTramiteDto>>(new List<TipoTramiteDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetTipoTramiteAll(incluirInactivos);
                resultado.Data = datos;
                resultado.Total = datos.Count;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public TipoTramiteDto GetTipoTramiteByCodigo(string codigo)
        {
            try
            {
                return _repoMant.GetTipoTramiteByCodigo(codigo) ?? new TipoTramiteDto();
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public TipoTramiteDto GetTipoTramiteById(int id)
        {
            try
            {
                return _repoMant.GetTipoTramiteById(id);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new TipoTramiteDto();
        }

        public ResultadoOperacion SaveTipoTramite(TipoTramiteDto tipoTr, bool updateSoloActivo)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            { 
                //var logSistema = new LogSistemaDto()
                //{
                //    Flujo = "TIPOTRAMITE",
                //    Fecha = DateTime.Now,
                //    Origen = "TIPOTRAMITE",
                //    OrigenFecha = null,
                //    RequerimientoId = 0,
                //    Usuario = ut.UsuarioActual ?? "<desconocido>",
                //    UsuarioId = ut.UsuarioCreacionId
                //};

                // Se graba en BD
                resultado = _repoMant.SaveTipoTramite(tipoTr, updateSoloActivo);

                resultado.Mensaje = resultado.Codigo < 0
                    ? resultado.Mensaje
                    : (tipoTr.Id == 0 ? "Se ha creado satisfactoriamente el registro." : "Se ha actualizado satisfactoriamente el registro.");

                //logSistema.Accion = ut.Id == 0 ? "CREACION" : "EDICION";
                //logSistema.OrigenId = ut.Id;
                //logSistema.ExtraData = null;
                //_repoMant.SaveLogSistema(logSistema);
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion DeleteTipoTramite(int tipoTrId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                _repoMant.DeleteTipoTramite(tipoTrId);
                resultado.Codigo = 1;
                resultado.Mensaje = "Registro eliminado con éxito.";
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "eliminar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }
        #endregion

        #region Usuarios
        public UsuarioDto GetUsuarioByUserNamePassword(string userName, string passw)
        {
            try
            {
                return _repoMant.GetUsuarioByUserNamePassword(userName, passw);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new UsuarioDto();
        }
        public UsuarioDto GetUsuarioByUserName(string userName)
        {
            try
            {
                return _repoMant.GetUsuarioByUserName(userName);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new UsuarioDto();
        }
        #endregion

        #region Listas

        public DatosAjax<List<ListaDto>> GetListaAll()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<ListaDto>>(new List<ListaDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetListaAll();
                resultado.Data = datos;
                resultado.Total = datos.Count;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public ListaDto GetListaById(int id)
        {
            try
            {
                return _repoMant.GetListaById(id);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new ListaDto();
        }

        public ResultadoOperacion SaveLista(ListaDto lista)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                //var logSistema = new LogSistemaDto()
                //{
                //    Flujo = "TIPOTRAMITE",
                //    Fecha = DateTime.Now,
                //    Origen = "TIPOTRAMITE",
                //    OrigenFecha = null,
                //    RequerimientoId = 0,
                //    Usuario = ut.UsuarioActual ?? "<desconocido>",
                //    UsuarioId = ut.UsuarioCreacionId
                //};

                // Se graba en BD
                resultado = _repoMant.SaveLista(lista);

                resultado.Mensaje = resultado.Codigo < 0
                    ? resultado.Mensaje
                    : (resultado.Codigo == 1 ? "Se ha creado con éxito la lista. <br/>Por favor, agregue los valores de la nueva lista." : "Se ha actualizado satisfactoriamente el registro.");
                resultado.Extra = lista.IdLista;

                //logSistema.Accion = ut.Id == 0 ? "CREACION" : "EDICION";
                //logSistema.OrigenId = ut.Id;
                //logSistema.ExtraData = null;
                //_repoMant.SaveLogSistema(logSistema);
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion DeleteLista(int id)
        {

            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                _repoMant.DeleteLista(id);
                resultado.Codigo = 1;
                resultado.Mensaje = "Registro eliminado con éxito.";
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "eliminar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }
        #endregion

        #region Lista Valor
        public DatosAjax<List<ListaValorDto>> GetListaValorAllByListaId(int listaId)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<ListaValorDto>>(new List<ListaValorDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetListaValorAllByListaId(listaId);
                resultado.Data = datos;
                resultado.Total = datos.Count;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public ListaValorDto GetListaValorById(int listaId, string codigo)
        {
            try
            {
                return _repoMant.GetListaValorById(listaId, codigo);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new ListaValorDto();
        }

        public ResultadoOperacion SaveListaValor(ListaValorDto listaValor)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                //var logSistema = new LogSistemaDto()
                //{
                //    Flujo = "TIPOTRAMITE",
                //    Fecha = DateTime.Now,
                //    Origen = "TIPOTRAMITE",
                //    OrigenFecha = null,
                //    RequerimientoId = 0,
                //    Usuario = ut.UsuarioActual ?? "<desconocido>",
                //    UsuarioId = ut.UsuarioCreacionId
                //};

                // Se graba en BD
                resultado = _repoMant.SaveListaValor(listaValor);

                resultado.Mensaje = resultado.Codigo < 0
                    ? resultado.Mensaje
                    : (listaValor.EsNuevo ? "Se ha creado satisfactoriamente el registro." : "Se ha actualizado satisfactoriamente el registro.");

                //logSistema.Accion = ut.Id == 0 ? "CREACION" : "EDICION";
                //logSistema.OrigenId = ut.Id;
                //logSistema.ExtraData = null;
                //_repoMant.SaveLogSistema(logSistema);
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion DeleteListaValor(int id, string codigo)
        {

            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                _repoMant.DeleteListaValor(id, codigo);
                resultado.Codigo = 1;
                resultado.Mensaje = "Registro eliminado con éxito.";
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "eliminar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        #endregion

        #region Caso
        public List<CasoDto> GetCasoAll()
        {
            try
            {
                var datos = _repoMant.GetCasoAll();
                return datos;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new List<CasoDto>();
            }
        }

        public CasoDto GetCasoById(int id)
        {
            try
            {
                return _repoMant.GetCasoById(id);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new CasoDto();
        }

        public ResultadoOperacion SaveCaso(CasoDto caso)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                // Se graba en BD
                var esNuevo = caso.Id == 0;
                caso.FechaCreacion = DateTime.Now;
                caso.FechaModificacion = DateTime.Now;
                if (caso.Id == 0)
                {
                    caso.FechaCreacion = DateTime.Now;
                    caso.FechaModificacion = DateTime.Now;
                    resultado = _repoMant.NewCaso(caso);
                }
                else
                {
                    caso.FechaModificacion = DateTime.Now;
                    resultado = _repoMant.UpdateCaso(caso);
                }

                resultado.Mensaje = resultado.Codigo < 0
                    ? resultado.Mensaje
                    : (esNuevo ? "Se ha creado satisfactoriamente el registro." : "Se ha actualizado satisfactoriamente el registro.");
                resultado.Extra = caso.Id;
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public List<RequerimientoDto> GetRequerimientosNoAsignadoCaso(CasoFilterDto filtros)
        {
            return _repoMant.GetRequerimientosNoAsignadoCaso(filtros);
        }

        public List<RequerimientoDto> GetRequerimientosByCasoId(int casoId, CasoFilterDto filtros)
        {
            return _repoMant.GetRequerimientosByCasoId(casoId, filtros);
        }

        public ResultadoOperacion AgregaRequeriminetosCaso(int casoId, List<int> reqs)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                // Se graba en BD
                _repoMant.AgregaRequeriminetosCaso(casoId, reqs);

                resultado.Codigo = 1;
                resultado.Mensaje = "Los requerimientos seleccionados fueron asignados al caso exitosamente.";
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public  ResultadoOperacion EliminaRequerimientosCaso(List<int> reqs)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                // Se graba en BD
                _repoMant.EliminaRequerimientosCaso(reqs);

                resultado.Codigo = 1;
                resultado.Mensaje = "Los ingresos seleccionados fueron eliminados del caso exitosamente.";
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion EliminaCaso(int casoId)
        {
            return _repoMant.EliminaCaso(casoId);
        }

        #endregion

        #region Reportes

        public List<ReporteDto> GetReporteAll()
        {
            return _repoMant.GetReporteAll();
        }

        public ReporteDto GetReporteById(int id)
        {
            return _repoMant.GetReporteById(id);
        }

        #endregion

    }
}
