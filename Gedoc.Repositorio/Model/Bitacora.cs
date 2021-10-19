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
    
    public partial class Bitacora
    {
        public int Id { get; set; }
        public Nullable<int> RequerimientoId { get; set; }
        public Nullable<System.DateTime> Fecha { get; set; }
        public Nullable<System.DateTime> FechaSolicitudDespacho { get; set; }
        public Nullable<System.DateTime> FechaSolicitudRevision { get; set; }
        public string TipoContenido { get; set; }
        public string ObsAcuerdoComentario { get; set; }
        public int Orden { get; set; }
        public string UrlArchivo { get; set; }
        public string UltimoComentario { get; set; }
        public Nullable<int> UsuarioCreacionId { get; set; }
        public Nullable<int> UsuarioEliminacionId { get; set; }
        public Nullable<System.DateTime> EliminacionFecha { get; set; }
        public bool Eliminado { get; set; }
        public string Titulo { get; set; }
        public string NombreArchivo { get; set; }
        public Nullable<int> DespachoInicId { get; set; }
        public System.DateTime FechaModificacion { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public string TipoBitacoraCod { get; set; }
        public int TipoBitacoraListaId { get; set; }
    
        public virtual Requerimiento Requerimiento { get; set; }
        public virtual Usuario UsuarioCreacion { get; set; }
        public virtual Usuario Usuario1 { get; set; }
        public virtual ListaValor TipoBitacora { get; set; }
    }
}
