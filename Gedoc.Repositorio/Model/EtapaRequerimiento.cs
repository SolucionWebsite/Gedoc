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
    
    public partial class EtapaRequerimiento
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EtapaRequerimiento()
        {
            this.AccionesPermitidasBandejas = new HashSet<AccionesPermitidasBandejas>();
            this.TipoTramite = new HashSet<TipoTramite>();
            this.Requerimiento = new HashSet<Requerimiento>();
            this.EstadoRequerimiento = new HashSet<EstadoRequerimiento>();
        }
    
        public int Id { get; set; }
        public string Titulo { get; set; }
        public bool Activo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AccionesPermitidasBandejas> AccionesPermitidasBandejas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TipoTramite> TipoTramite { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Requerimiento> Requerimiento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EstadoRequerimiento> EstadoRequerimiento { get; set; }
    }
}
