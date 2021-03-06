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
    
    public partial class Remitente
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Remitente()
        {
            this.Despacho = new HashSet<Despacho>();
            this.Despacho1 = new HashSet<Despacho>();
            this.Requerimiento = new HashSet<Requerimiento>();
            this.DespachoIniciativa = new HashSet<DespachoIniciativa>();
            this.DespachoIniciativa1 = new HashSet<DespachoIniciativa>();
        }
    
        public int Id { get; set; }
        public string Cargo { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Genero { get; set; }
        public string Institucion { get; set; }
        public string Nombre { get; set; }
        public string Rut { get; set; }
        public string Telefono { get; set; }
        public bool Activo { get; set; }
        public string TipoInstitucion { get; set; }
        public Nullable<int> UsuarioCreacionId { get; set; }
        public Nullable<System.DateTime> FechaCreacion { get; set; }
        public Nullable<int> UsuarioModificacionId { get; set; }
        public Nullable<System.DateTime> FechaModificacion { get; set; }
        public Nullable<int> UsuarioEliminacionId { get; set; }
        public Nullable<System.DateTime> FechaEliminacion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Despacho> Despacho { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Despacho> Despacho1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Requerimiento> Requerimiento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DespachoIniciativa> DespachoIniciativa { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DespachoIniciativa> DespachoIniciativa1 { get; set; }
        public virtual Usuario UsuarioCreacion { get; set; }
        public virtual Usuario UsuarioModificacion { get; set; }
        public virtual Usuario UsuarioEliminacion { get; set; }
    }
}
