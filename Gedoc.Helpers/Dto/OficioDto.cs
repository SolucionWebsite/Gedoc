using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Enum;

namespace Gedoc.Helpers.Dto
{
    public class OficioDto
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
        public string NombreArchivo { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public int? UsuarioCreacionId { get; set; }
        public System.DateTime FechaModificacion { get; set; }
        public int? UsuarioModificacionId { get; set; }
        public DateTime? EliminacionFecha { get; set; }
        public int? UsuarioEliminacionId { get; set; }
        public bool Eliminado { get; set; }
        public int? RequerimientoPrincipalId { get; set; }
        public int? UnidadTecnicaId { get; set; }
        public int? PlantillaId { get; set; }
        public int? TipoPlantillaId { get; set; }
        public string PlantillaOficioNombre { get; set; }
        public string Contenido { get; set; }
        public string Observaciones { get; set; }
        public FlujoIngreso Flujo { get; set; }
        public List<GenericoDto> Requerimiento { get; set; }
        public RequerimientoDto RequerimientoMain { get; set; }
        public string Accion { get; set; }
        public UsuarioActualDto DatosUsuarioActual { get; set; }
        public string Encabezado { get; set; }
        public string Pie { get; set; }
        public bool NuevaObservacion { get; set; }
        public string BaseUrl { get; set; }
        public string CodigoDocFirmado { get; set; }
        public bool Urgente { get; set; }
        public int? ProfesionalId { get; set; }
        public string NombreDocumento { get; set; }
        public string ProfesionalNombre { get; set; }
        public string UnidadTecnicaNombre { get; set; }
        public int? UltimoUsuarioFlujoId { get; set; }
        public DatosArchivo datosArchivo { get; set; }
    }
}
