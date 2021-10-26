//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gedoc.Repositorio.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Oficio
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Oficio()
        {
            this.Requerimiento = new HashSet<Requerimiento>();
            this.OficioObservacion = new HashSet<OficioObservacion>();
            this.AdjuntoOficio = new HashSet<AdjuntoOficio>();
        }
    
        public int Id { get; set; }
        public int EstadoId { get; set; }
        public int EtapaId { get; set; }
        public System.DateTime FechaUltEstado { get; set; }
        public System.DateTime FechaUltEtapa { get; set; }
        public Nullable<System.DateTime> FechaEmisionOficio { get; set; }
        public string NumeroOficio { get; set; }
        public string UrlArchivo { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public Nullable<int> UsuarioCreacionId { get; set; }
        public System.DateTime FechaModificacion { get; set; }
        public Nullable<int> UsuarioModificacionId { get; set; }
        public Nullable<System.DateTime> EliminacionFecha { get; set; }
        public Nullable<int> UsuarioEliminacionId { get; set; }
        public bool Eliminado { get; set; }
        public Nullable<int> RequerimientoPrincipalId { get; set; }
        public int PlantillaId { get; set; }
        public string Contenido { get; set; }
        public Nullable<int> TipoTramiteId { get; set; }
        public string Observaciones { get; set; }
        public string NombreArchivo { get; set; }
        public Nullable<int> UnidadTecnicaId { get; set; }
        public string CodigoDocFirmado { get; set; }
        public Nullable<bool> Urgente { get; set; }
    
        public virtual EstadoOficio EstadoOficio { get; set; }
        public virtual EtapaOficio EtapaOficio { get; set; }
        public virtual PlantillaOficio PlantillaOficio { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Requerimiento> Requerimiento { get; set; }
        public virtual Usuario UsuarioCreacion { get; set; }
        public virtual Usuario UsuarioEliminacion { get; set; }
        public virtual Usuario UsuarioEdicion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OficioObservacion> OficioObservacion { get; set; }
        public virtual UnidadTecnica UnidadTecnica { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AdjuntoOficio> AdjuntoOficio { get; set; }
    }
}