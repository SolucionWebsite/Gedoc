using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class PlantillaOficioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
        public int? TipoTramiteId { get; set; }
        public int? TipoPlantillaId { get; set; }
        public string TipoTramiteTitulo { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int? UsuarioCreacionId { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int? UsuarioModificacionId { get; set; }
        public DateTime? EliminacionFecha { get; set; }
        public int? UsuarioEliminacionId { get; set; }
        public bool TipoWord { get; set; }
        public string UsuarioActual { get; set; }
        public int UsuarioActualId { get; set; }
    }
}
