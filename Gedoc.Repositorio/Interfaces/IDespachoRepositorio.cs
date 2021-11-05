using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;

namespace Gedoc.Repositorio.Interfaces
{
    public delegate ResultadoOperacion ProcesaArchivo(DatosArchivo datosArchivo, bool subirARepoGedoc, bool eliminar);
    public delegate ResultadoOperacion CreaNuevoDespacho(DespachoDto despacho, DespachoIniciativaDto despachoInic);
    public delegate ResultadoOperacion FirmaDigitalOficio(DatosArchivo datosArchivo, bool eliminar);

    public interface IDespachoRepositorio
    {
        #region Despacho
        DespachoDto GetById(int id);
        ResultadoOperacion GetDespachoDetalle(DespachoDto despachoDto);
        DatosAjax<List<DespachoDto>> GetDatosUltimos(int diasAtras);
        DatosAjax<List<DespachoDto>> GetDespachosIngreso(int idIngreso, int skip, int take,
            SortParam sort);
        DatosAjax<List<DespachoDto>> GetDatosVistaDestinatario(int skip, int take,
            SortParam sort, string filterText);

        ResultadoOperacion MarcaEliminado(int despachoId, int usuarioId);
        ResultadoOperacion EliminarDesp(int despachoId);

        ResultadoOperacion NewDespacho(DespachoDto datos, ProcesaArchivo procesaArchivo);
        ResultadoOperacion UpdateDatosArchivo(int id, string nombreArchivo, string urlArchivo);
        ResultadoOperacion Update(DespachoDto datos);
        ResultadoOperacion UpdateCierre(DespachoDto datos);
        T Obtener<T>(Expression<Func<T, bool>> predicado,
            params Expression<Func<T, object>>[] includeExpressions) where T : class;

        ResultadoOperacion Actualizar<T>(T entity, string[] fieldsModified = null,
            params Expression<Func<T, object>>[] includeExpressions)
            where T : class;
        ResultadoOperacion Eliminar<T>(Expression<Func<T, bool>> predicado) where T : class;

        #endregion

        #region Oficio

        DatosAjax<List<OficioDto>> GetDatosBandejaEntradaOficio(int idBandeja, int skip, int take, SortParam sort,
            string filterText, DateTime fechaDesde, int idUsuario, ConfigBandejaDto configBandeja);
        List<OficioDto> GetOficoAll();
        OficioDto GetOficoById(int id);
        List<OficioDto> GetOficiosPendienteFirma();
        List<OficioObservacionDto> GetObservacionesOficio(int oficioId);

        #endregion

        #region Oficio

        ResultadoOperacion NewOficio(OficioDto datos);
        ResultadoOperacion UpdateOficio(OficioDto datos);
        ResultadoOperacion UpdateEstadoOficio(OficioDto datos);

        ResultadoOperacion FirmarOficio(OficioDto datos, ProcesaArchivo procesaArchivo,
            CreaNuevoDespacho creaNuevoDespacho, FirmaDigitalOficio firmaDigitalOficio);
        ResultadoOperacion UpdateOficioFirmadoDigital(OficioDto datos, ProcesaArchivo procesaArchivo,
            CreaNuevoDespacho creaNuevoDespacho);
        bool ExistePlantillaEnOficio(int plantillaId);
        ResultadoOperacion ValidaNumeroOficio(string numeroOficio);

        #endregion

        List<RequerimientoDto> GetRequerimientosDespacho(int idDespacho);
        List<LogSistemaDto> HistorialOficio(int oficioId);

    }
}
