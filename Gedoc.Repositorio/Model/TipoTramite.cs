//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gedoc.Repositorio.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class TipoTramite
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TipoTramite()
        {
            this.Requerimiento = new HashSet<Requerimiento>();
            this.PlantillaOficio = new HashSet<PlantillaOficio>();
        }
    
        public int Id { get; set; }
        public string Titulo { get; set; }
        public bool Activo { get; set; }
        public string Codigo { get; set; }
        public Nullable<int> UnidadTecnicaId { get; set; }
        public Nullable<int> EstadoId { get; set; }
        public Nullable<int> EtapaId { get; set; }
        public string PrioridadCod { get; set; }
        public int PrioridadListaId { get; set; }
    
        public virtual EstadoRequerimiento EstadoRequerimiento { get; set; }
        public virtual EtapaRequerimiento EtapaRequerimiento { get; set; }
        public virtual UnidadTecnica UnidadTecnica { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Requerimiento> Requerimiento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PlantillaOficio> PlantillaOficio { get; set; }
        public virtual ListaValor Prioridad { get; set; }
    }
}