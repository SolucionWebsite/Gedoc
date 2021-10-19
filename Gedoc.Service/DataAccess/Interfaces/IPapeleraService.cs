using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Service.DataAccess.Interfaces
{
    public interface IPapeleraService
    {
        DatosAjax<List<PapeleraItemDto>> GetDataPapelera(ParametrosGrillaDto<int> param, int userId);
        ResultadoOperacion RestoreTrashItems(List<PapeleraItemDto> items);
        ResultadoOperacion DeleteTrashItems(List<PapeleraItemDto> items);        
        ResultadoOperacion EmptyTrash(int userId);
    }
}
