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
    
    public partial class NotificacionEmailRegistro
    {
        public int Id { get; set; }
        public int NotificacionEmailId { get; set; }
        public Nullable<int> RequerimientoId { get; set; }
        public Nullable<int> DespachoId { get; set; }
        public Nullable<int> DespachoIniciativaId { get; set; }
        public System.DateTime FechaUltimoEnvio { get; set; }
    
        public virtual NotificacionEmail NotificacionEmail { get; set; }
    }
}