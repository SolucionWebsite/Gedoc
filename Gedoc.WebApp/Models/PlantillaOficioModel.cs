using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class PlantillaOficioModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
        public int? TipoTramiteId { get; set; }
        public int? TipoPlantillaId { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int? UsuarioCreacionId { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int? UsuarioModificacionId { get; set; }
        public DateTime? EliminacionFecha { get; set; }
        public int? UsuarioEliminacionId { get; set; }
    }
}