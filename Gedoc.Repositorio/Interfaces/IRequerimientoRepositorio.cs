using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Interfaces
{
    public interface IRequerimientoRepositorio
    {
        GedocEntities GetDbContext();
        ResultadoOperacion New(RequerimientoDto datos);
        ResultadoOperacion UpdateIngresoCentral(RequerimientoDto datos);
        ResultadoOperacion UpdateAsignacionUt(RequerimientoDto datos);
        ResultadoOperacion UpdateReasignacionUt(RequerimientoDto datos);
        ResultadoOperacion UpdatePriorizacion(RequerimientoDto datos);
        ResultadoOperacion UpdateAsignarProfesional(RequerimientoDto datos);
        ResultadoOperacion UpdateReasignarProfesional(RequerimientoDto datos);
        ResultadoOperacion UpdateEditarRequerimiento(RequerimientoDto datos);
        ResultadoOperacion UpdateEditarCamposUt(RequerimientoDto datos);
        ResultadoOperacion UpdateCierre(RequerimientoDto datos);

        List<RequerimientoDto> GetDatosCierreMultiple(int idUT, int idProfesional, DateTime fechaDesde,
            DateTime fechaHasta);
        ResultadoOperacion CierreMultiple(RequerimientoDto datos, UsuarioActualDto usuario);
        List<RequerimientoDto> UpdateForzarPrioridad(List<int> idReqs, DateTime? nuevaFecha);
        RequerimientoDto MarcaEliminado(int reqId, int usuarioId);
        RequerimientoDto UpdateByBitacora(string tipoBitCod, int reqId);
        ResultadoOperacion GetRequirimientoDetalle(RequerimientoDto ingresoDto);
        DatosAjax<List<RequerimientoDto>> GetDatosUltimos(int diasAtras);
        //DatosAjax<List<RequerimientoDto>> GetByUnidadTecnica(int idUT, int? idProfesional);
        DatosAjax<List<UsuarioDto>> GetProfesionales(int idUT, int? idProfesional);
        RequerimientoDto GetById(int id);
        RequerimientoDto GetByDocumentoIngreso(string docingreso);
        RequerimientoDto GetResumenByDocumentoIngreso(string docingreso);
        RequerimientoDto GetBySolicitudId(int idSolic);
        RequerimientoDto GetFichaById(int id, bool fullDatos = false);
        vw_FichaIngreso GetFichaFullById(int id, bool fullDatos = false);
        List<RequerimientoDto> GetResumenAll(bool soloCerrados);
        string GetFechasEmisionOficioReq(int idReq);
        List<GenericoDto> GetRequerimientoResumenByIds(List<int> ids, bool soloCerrados);
        List<RequerimientoDto> GetRequerimientoByIds(List<int> ids, bool soloCerrados);
        DatosAjax<List<RequerimientoDto>> GetDatosBandejaEntrada(int idBandeja, int codigoBandeja, int skip, int take,
            SortParam sort, string filterText, string filtroSql, object[] filtroSqlParams, DateTime fechaDesde, int idUsuario, int? DocumentoIngreso, DateTime? FechaHasta, 
            int? UnidadTecnica, int? Estado, bool? soloTramite);
        //DatosAjax<List<RequerimientoDto>> GetDatosBandejaEntradaHistorico(int idBandeja, int skip, int take,
        //    SortParam sort, string filterText, DateTime fechaDesde, int? DocumentoIngreso, DateTime? FechaHasta, int? UnidadTecnica, int? Estado);
        DatosAjax<List<GroupResult>> GetDatosBandejaPriorizados(int idBandeja, int codigoBandeja, int skip, int take,
            SortParam sort, string filterText, string filtroSql, object[] filtroSqlParams, DateTime fechaDesde, int idUsuario, int? DocumentoIngreso,
            DateTime? FechaHasta,
            int? UnidadTecnica, int? Estado, string tipoOper);
        DatosAjax<List<RequerimientoDto>> GetDatosVistaEtiqueta(int skip, int take,
            SortParam sort, string filterText);

        DatosAjax<List<RequerimientoDto>> GetDatosVistaRemitente(int skip, int take,
            SortParam sort, string filterText);

        DatosAjax<List<GroupResult>> GetDatosVistaGenero(int skip, int take,
            SortParam sort, string filterText, string filtroSql, object[] filtroSqlParams);

        List<VistaGeneroDto>
            GetDatosVistaGeneroSinGrupos(string filterText, string filtroSql, object[] filtroSqlParams);

        DatosAjax<List<RequerimientoDto>> GetDatosBusquedaArchivar(int skip, int take, SortParam sort,
            string filterText,
            DateTime fechaDesde, DateTime fechaHasta, int unidadTecnicaId, int tipoBusqueda);

        List<LogSistemaDto> GetLogRequerimiento(int idReq);
        List<LogBitacoraDto> GetLogBitacoraRequerimiento(int idReq);
        ResultadoOperacion ActualizarMnReq(List<int> reqIds, List<int> casoIds, MonumentoNacionalDto datosMn);
        // List<RequerimientoDto> GetRequerimientosDespacho(int idDespacho);
    }
}
