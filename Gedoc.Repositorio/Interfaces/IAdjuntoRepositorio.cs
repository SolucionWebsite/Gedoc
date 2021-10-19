using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;

namespace Gedoc.Repositorio.Interfaces
{
    public interface IAdjuntoRepositorio
    {
        AdjuntoDto GetById(int id);
        DatosAjax<List<AdjuntoDto>> GetAdjuntosIngreso(int idIngreso, bool incluyeEliminados = false);

        ResultadoOperacion Save(AdjuntoDto datos);
        ResultadoOperacion New(AdjuntoDto datos, ProcesaArchivo procesaArchivo);
        ResultadoOperacion UpdateDatosArchivo(int id, string nombreArchivo, string urlArchivo);
        void MarcaAdjuntosEliminado(int[] adjIds, int usuarioId);
        List<AdjuntoDto> GetAdjuntosUsuario(DateTime fechaD, DateTime fechaH, int usuarioId);

        #region Adjuntos Oficio
        AdjuntoDto GetAdjuntoOficioById(int id);
        DatosAjax<List<AdjuntoDto>> GetAdjuntosOficio(int idOficio, bool incluyeEliminados = false);

        ResultadoOperacion SaveAdjuntoOficio(AdjuntoDto datos);
        ResultadoOperacion NewAdjuntoOficio(AdjuntoDto datos, ProcesaArchivo procesaArchivo);
        ResultadoOperacion UpdateDatosArchivoAdjuntoOficio(int id, string nombreArchivo, string urlArchivo);
        void MarcaAdjuntosOficioEliminado(int[] adjIds, int usuarioId);
        List<AdjuntoDto> GetAdjuntosOficioUsuario(DateTime fechaD, DateTime fechaH, int usuarioId);
        #endregion
    }
}
