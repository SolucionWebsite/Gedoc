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
    
    public partial class sp_DatosBandejaEntrada_Result
    {
        public int Id { get; set; }
        public string DocumentoIngreso { get; set; }
        public System.DateTime FechaIngreso { get; set; }
        public Nullable<int> RemitenteId { get; set; }
        public string RemitenteNombre { get; set; }
        public string RemitenteInstitucion { get; set; }
        public string Materia { get; set; }
        public Nullable<System.DateTime> AsignacionUt { get; set; }
        public Nullable<System.DateTime> Resolucion { get; set; }
        public Nullable<int> CantOficiosCmn { get; set; }
        public int EstadoId { get; set; }
        public string EstadoTitulo { get; set; }
        public int EtapaId { get; set; }
        public string EtapaTitulo { get; set; }
        public Nullable<int> UtAsignadaId { get; set; }
        public string UtAsignadaTitulo { get; set; }
        public Nullable<int> ProfesionalId { get; set; }
        public Nullable<int> ProfesionalNombre { get; set; }
        public string Acciones { get; set; }
    }
}
