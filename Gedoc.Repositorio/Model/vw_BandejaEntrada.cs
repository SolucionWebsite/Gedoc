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
    
    public partial class vw_BandejaEntrada
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
        public Nullable<int> UtTemporalId { get; set; }
        public string UtTemporalTitulo { get; set; }
        public Nullable<int> UtTransparenciaId { get; set; }
        public string UtTransparenciaTitulo { get; set; }
        public Nullable<int> ProfesionalId { get; set; }
        public string ProfesionalNombre { get; set; }
        public Nullable<int> ProfesionalTempId { get; set; }
        public Nullable<int> ProfesionalTranspId { get; set; }
        public Nullable<int> ResponsableUtId { get; set; }
        public Nullable<int> ResponsableUtTempId { get; set; }
        public Nullable<int> ResponsableTranspId { get; set; }
        public string Acciones { get; set; }
        public int BandejaId { get; set; }
        public string TipoIngreso { get; set; }
        public Nullable<int> DiasResolucion { get; set; }
        public Nullable<System.DateTime> Liberacion { get; set; }
        public Nullable<bool> LiberarAsignacionTemp { get; set; }
        public Nullable<bool> ForzarPrioridad { get; set; }
        public Nullable<System.DateTime> ForzarPrioridadFecha { get; set; }
        public string ForzarPrioridadMotivo { get; set; }
    }
}
