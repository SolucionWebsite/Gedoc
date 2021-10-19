using System.Collections.Generic;
using System.Web;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Service.Sharepoint;

namespace Gedoc.Service.DataAccess.Interfaces
{
    public interface IBitacoraService
    {
        DatosAjax<List<BitacoraDto>> GetBitacorasDoc(int idDoc, char tipoDoc);
        BitacoraDto GetBitacoraById(int id) ;
        DatosAjax<BitacoraDto> GetUltimoComentarioByTipo(string tipoBitacoraId, int reqId);
        ResultadoOperacion Save(BitacoraDto bitacora, IEnumerable<HttpPostedFileBase> files);
        DatosArchivo GetArchivo(int bitacoraId);
        ResultadoOperacion EliminarBitacora(int id, int userId);
    }
}
