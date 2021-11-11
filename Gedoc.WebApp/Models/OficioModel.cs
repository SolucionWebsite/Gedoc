using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;

namespace Gedoc.WebApp.Models
{
    public class OficioModel
    {
        public int Id { get; set; }
        public int EstadoId { get; set; }
        public string EstadoOficioTitulo { get; set; }
        public int EtapaId { get; set; }
        public string EtapaOficioTitulo { get; set; }
        public System.DateTime FechaUltEstado { get; set; }
        public System.DateTime FechaUltEtapa { get; set; }
        public DateTime? FechaEmisionOficio { get; set; }
        public string NumeroOficio { get; set; }
        public string UrlArchivo { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public int? UsuarioCreacionId { get; set; }
        public System.DateTime FechaModificacion { get; set; }
        public int? UsuarioModificacionId { get; set; }
        public DateTime? EliminacionFecha { get; set; }
        public int? UsuarioEliminacionId { get; set; }
        public bool Eliminado { get; set; }
        public List<int> Requerimiento { get; set; }
        //public List<GenericoDto> RequerimientosDatos { get; set; }
        public virtual RequerimientoModel RequerimientoMain { get; set; }
        public int? RequerimientoPrincipalId { get; set; }
        public int? UnidadTecnicaId { get; set; }
        public int? TipoTramiteId { get; set; }
        public string TipoTramiteTitulo { get; set; }
        public int? PlantillaId { get; set; }
        public int? TipoPlantillaId { get; set; }
        public string PlantillaOficioNombre { get; set; }
        public string Contenido { get; set; }
        public string Observaciones { get; set; }
        public bool Urgente { get; set; }

        public string UsuarioActual { get; set; }
        public int UsuarioActualId { get; set; }
        public FlujoIngreso Flujo { get; set; }
        public string Encabezado { get; set; }
        public string Pie { get; set; }
        public string Accion { get; set; }
        public string PdfBase64 { get; set; }
        public bool TipoWord { get; set; }
        public DatosArchivo Documento { get; set; }
    }
}