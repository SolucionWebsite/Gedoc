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
    public interface IBitacoraRepositorio
    {
        BitacoraDto GetById(int id);
        BitacoraDto GetUltimoComentarioByTipo(string tipoBitacoraId, int reqId);
        DatosAjax<List<BitacoraDto>> GetBitacorasIngreso(int idIngreso);
        DatosAjax<List<BitacoraDto>> GetBitacorasDespachoInic(int idDesp);

        ResultadoOperacion Save(BitacoraDto datos);
        ResultadoOperacion New(BitacoraDto datos, ProcesaArchivo procesaArchivo);
        ResultadoOperacion UpdateDatosArchivo(int id, string nombreArchivo, string urlArchivo);
    }
}
