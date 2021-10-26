using System;
using System.Collections.Generic;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;

namespace Gedoc.Service.DataAccess.Interfaces
{
    public interface IRequerimientoService
    {
        ResultadoOperacion Save(RequerimientoDto ingreso);
        ResultadoOperacion MarcaEliminado(int reqId, UsuarioActualDto usuario);

        ResultadoOperacion SaveForzarPrioridad(ParametrosGrillaDto<int> param, UsuarioActualDto usuario, DateTime? nuevaFecha,
            int? idRequerimiento);
        ResultadoOperacion EnviarNotificacionEmail(RequerimientoDto ingreso);
        UsuarioDto GetResponsableUt(int utId);
        DatosAjax<List<RequerimientoDto>> GetIngresosUltimos(int diasAtras);
        //DatosAjax<List<RequerimientoDto>> GetByUnidadTecnica(int idUT, int? idProfesional);
        List<RequerimientoDto> GetDatosCierreMultiple(int idUT, int idProfesional, DateTime fechaDesde,
            DateTime fechaHasta);
        DatosAjax<List<UsuarioDto>> GetProfesionales(int idUT, int? idProfesional);
        /// <summary>
        /// Devuelve los datos e los ingresos de una bandeja.
        /// </summary>
        /// <param name="param">Opciones para obtener los datos</param>
        /// <param name="idUsuario"></param>
        /// <returns></returns>
        DatosAjax<List<RequerimientoDto>> GetDatosBandejaEntrada(ParametrosGrillaDto<int> param, int idUsuario, bool? soloTramite);

        DatosAjax<List<GroupResult>> GetDatosBandejaPriorizados(ParametrosGrillaDto<int> param, int idUsuario);
        RequerimientoDto GetById(int id);
        RequerimientoDto GetByDocumentoIngreso(string docIngreso);
        RequerimientoDto GetResumenByDocumentoIngreso(string docIngreso);
        RequerimientoDto GetBySolicitudId(int idSolic);
        DatosAjax<RequerimientoDto> GetFichaById(int id, bool fullDatos = false);
        DatosAjax<List<GenericoDto>> GetRequerimientoResumenByIds(List<int> ids, bool cerrado);
        DatosAjax<List<RequerimientoDto>> GetResumenAll(bool soloCerrados);
        DatosAjax<List<RequerimientoDto>> GetDatosVistas(ParametrosGrillaDto<int> param);
        DatosAjax<List<GroupResult>> GetDatosVistaGenero(ParametrosGrillaDto<int> param);

        List<VistaGeneroDto> GetDatosVistaGeneroSinGrupos(string filterText, string filtroSql, object[] filtroSqlParams);

        DatosAjax<List<RequerimientoDto>> GetDatosBusquedaArchivar(ParametrosGrillaDto<int> param,
            DateTime fechaDesde, DateTime fechaHasta, int unidadTecnicaId, int tipoBusqueda);
        List<LogSistemaDto> GetLogRequerimiento(int idReq);
        DatosAjax<List<LogBitacoraDto>> GetLogBitacoraRequerimiento(int idReq);
        ResultadoOperacion ActualizarMnFromRegmon(List<int> reqIds, List<int> casoIds, MonumentoNacionalDto datosMn);

        ResultadoOperacion CierreMultiple(List<int> reqIds, int motivoCierre, bool cerrar, string comentarioCierre,
            UsuarioActualDto usuario);
    }
}
