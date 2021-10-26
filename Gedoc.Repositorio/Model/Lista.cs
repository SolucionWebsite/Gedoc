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
    
    public partial class Lista
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Lista()
        {
            this.ListasHijas = new HashSet<Lista>();
            this.ListaValor = new HashSet<ListaValor>();
        }
    
        public int IdLista { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int IdEstadoRegistro { get; set; }
        public Nullable<int> IdListaPadre { get; set; }
    
        public virtual EstadoRegistro EstadoRegistro { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Lista> ListasHijas { get; set; }
        public virtual Lista ListaPadre { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ListaValor> ListaValor { get; set; }
    }
}