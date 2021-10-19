using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class MarcarVerEnTablaModel
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Nombre de la Tabla")]
        public string NombreTabla { get; set; }

        [Required]
        [DisplayName("Filtro por Unidad Técnica")]
        public int UnidadTecnicaId { get; set; }

        [Required]
        [DisplayName("Fecha desde")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaDesde { get; set; }

        [Required]
        [DisplayName("Fecha hasta")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaHasta { get; set; }

        [DisplayName("Documento de Ingreso")]
        public int[] DocumentoDeIngreso { get; set; }

        [Required]
        [DisplayName("Estado")]
        public int[] EstadoId { get; set; }

        [DisplayName("Etapa")]
        public int[] Etapa { get; set; }

        [DisplayName("Materia")]
        public string Materia { get; set; }

        [DisplayName("Etiqueta")]
        public int[] Etiqueta { get; set; }

        public bool IsFromTabla { get; set; }
    }

    public class MUltiSelectModel : MarcarVerEnTablaModel
    {
        public List<string> Ids { get; set; }
    }

    public class SelectTablaModel
    {
        public List<string> Ids { get; set; }
        public int TablaId { get; set; }
        public string DocIngreso { get; set; }
        public bool IsChecked { get; set; }
    }


    public class TablaSesionModel
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string NombreTabla { get; set; }
        public string UnidadTecnica { get; set; }
        public string CreadoPor { get; set; }
    }

    public class TablaSesionFilterModel
    {
        public int UnidadTecnicaId { get; set; }
        public int UsuarioCreadorId { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
    }
}