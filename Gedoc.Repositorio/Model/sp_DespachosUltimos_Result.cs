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
    
    public partial class sp_DespachosUltimos_Result
    {
        public int Id { get; set; }
        public string NumeroDespacho { get; set; }
        public Nullable<System.DateTime> FechaEmisionOficio { get; set; }
        public Nullable<int> DestinatarioId { get; set; }
        public string RemitenteNombre { get; set; }
        public string RemitenteInstitucion { get; set; }
        public string Materia { get; set; }
        public string EstadoDespachoTitulo { get; set; }
    }
}
