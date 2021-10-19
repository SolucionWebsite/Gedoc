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
    public interface IDespachoIniciativaRepositorio
    {
        DatosAjax<List<DespachoIniciativaDto>> GetDatosBandejaEntrada(int idBandeja, int skip, int take,
            SortParam sort, string filterText, DateTime fechaDesde);

        DespachoIniciativaDto GetById(int id);
        ResultadoOperacion GetDespachoDetalle(DespachoIniciativaDto despachoDto);

        ResultadoOperacion NewDespachoInic(DespachoIniciativaDto datos, ProcesaArchivo procesaArchivo);
        ResultadoOperacion UpdateDatosArchivo(int id, string nombreArchivo, string urlArchivo);
        ResultadoOperacion Update(DespachoIniciativaDto datos);
        ResultadoOperacion MarcaEliminado(int despachoId, int usuarioId);
        ResultadoOperacion EliminarDespInic(int despachoId);
        ResultadoOperacion UpdateCierre(DespachoIniciativaDto datos);
    }
}
