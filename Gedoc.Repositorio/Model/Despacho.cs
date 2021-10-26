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
    
    public partial class Despacho
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Despacho()
        {
            this.DestinatarioCopia = new HashSet<Remitente>();
            this.Requerimiento = new HashSet<Requerimiento>();
            this.Soporte = new HashSet<ListaValor>();
            this.TipoAdjunto = new HashSet<ListaValor>();
        }
    
        public int Id { get; set; }
        public Nullable<bool> AdjuntaDocumentacion { get; set; }
        public Nullable<int> CantidadAdjuntos { get; set; }
        public string ProyectoActividad { get; set; }
        public string Comuna { get; set; }
        public Nullable<int> DestinatarioId { get; set; }
        public int EstadoId { get; set; }
        public string Etiqueta { get; set; }
        public Nullable<System.DateTime> FechaRecepcion { get; set; }
        public Nullable<System.DateTime> FechaEmisionOficio { get; set; }
        public string FolioDespacho { get; set; }
        public string Materia { get; set; }
        public string NombreArchivo { get; set; }
        public string NumeroDespacho { get; set; }
        public string NumeroGuia { get; set; }
        public string ObservacionesAdjuntos { get; set; }
        public string ObservacionesDespacho { get; set; }
        public string ObservacionesMedioVerif { get; set; }
        public string ObservacionesAcuerdosCom { get; set; }
        public string Region { get; set; }
        public string RolSii { get; set; }
        public string UrlArchivo { get; set; }
        public Nullable<int> UsuarioCreacionId { get; set; }
        public Nullable<int> UsuarioEliminacionId { get; set; }
        public Nullable<System.DateTime> EliminacionFecha { get; set; }
        public bool Eliminado { get; set; }
        public Nullable<int> RequerimientoPrincipalId { get; set; }
        public System.DateTime FechaModificacion { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public string MedioDespachoCod { get; set; }
        public string ProveedorDespachoCod { get; set; }
        public int MedioDespachoListaId { get; set; }
        public int ProveedorDespachoListaId { get; set; }
        public string MedioVerificacionCod { get; set; }
        public int MedioVerificacionListaId { get; set; }
    
        public virtual Remitente Remitente { get; set; }
        public virtual EstadoDespacho EstadoDespacho { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Remitente> DestinatarioCopia { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Requerimiento> Requerimiento { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual Usuario Usuario1 { get; set; }
        public virtual Usuario Usuario2 { get; set; }
        public virtual Requerimiento RequerimientoPrincipal { get; set; }
        public virtual ListaValor FormaLlegada { get; set; }
        public virtual ListaValor ProveedorDespacho { get; set; }
        public virtual ListaValor MedioVerificacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ListaValor> Soporte { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ListaValor> TipoAdjunto { get; set; }
    }
}