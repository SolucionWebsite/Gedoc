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
    
    public partial class OficioObservacion
    {
        public int Id { get; set; }
        public int OficioId { get; set; }
        public string Observaciones { get; set; }
        public System.DateTime Fecha { get; set; }
        public int UsuarioId { get; set; }
        public int EstadoId { get; set; }
        public int EtapaId { get; set; }
    
        public virtual EstadoOficio EstadoOficio { get; set; }
        public virtual EtapaOficio EtapaOficio { get; set; }
        public virtual Oficio Oficio { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
