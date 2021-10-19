using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class CasoModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int? CreadoPor { get; set; }
        public int? ModificadoPor { get; set; }
        public int CantidadReq { get; set; }
        public string UsuarioCreacionNombresApellidos { get; set; }
        public string UsuarioModificacionNombresApellidos { get; set; }

    }

    public class ComboboxItemModel
    {
        public int Value { get; set; }
        public string Text { get; set; }
        
    }
}