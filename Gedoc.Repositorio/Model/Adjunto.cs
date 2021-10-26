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
    
    public partial class Adjunto
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Adjunto()
        {
            this.TipoAdjunto = new HashSet<ListaValor>();
        }
    
        public int Id { get; set; }
        public Nullable<int> RequerimientoId { get; set; }
        public string UrlArchivo { get; set; }
        public Nullable<int> FileSize { get; set; }
        public string FilType { get; set; }
        public string CreadoPor { get; set; }
        public Nullable<System.DateTime> FechaCarga { get; set; }
        public string NombreArchivo { get; set; }
        public string OrigenAdjunto { get; set; }
        public Nullable<int> UsuarioEliminacionId { get; set; }
        public Nullable<System.DateTime> EliminacionFecha { get; set; }
        public bool Eliminado { get; set; }
        public Nullable<int> UsuarioCreacionId { get; set; }
    
        public virtual Usuario Usuario { get; set; }
        public virtual Usuario UsuarioCreacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ListaValor> TipoAdjunto { get; set; }
        public virtual Requerimiento Requerimiento { get; set; }
    }
}