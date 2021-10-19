using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers.Enum;

namespace Gedoc.WebApp.Models
{
    public class DespachoModel
    {
        public int Id { get; set; }
        public int EstadoId { get; set; }
        public string EstadoDespachoTitulo { get; set; }

        #region Documento
        public string NumeroDespacho { get; set; }
        public Nullable<System.DateTime> FechaEmisionOficio { get; set; }
        #endregion

        #region Destinatario
        public int? DestinatarioId { get; set; }
        public string RemitenteNombre { get; set; }
        public string RemitenteCargo { get; set; }
        public string RemitenteTelefono { get; set; }
        public string RemitenteInstitucion { get; set; }
        public string RemitenteGenero { get; set; }
        public string RemitenteDireccion { get; set; }
        public string RemitenteRut { get; set; }
        public string RemitenteTipoInstitucion { get; set; }
        public string RemitenteEmail { get; set; }
        public List<int> DestinatarioCopia { get; set; }
        #endregion

        #region Despacho
        public string Materia { get; set; }
        #endregion

        #region Adjunto
        public Nullable<bool> AdjuntaDocumentacion { get; set; }
        public int? CantidadAdjuntos { get; set; }
        [AllowHtml]
        public string ObservacionesAdjuntos { get; set; }
        public List<string> TipoAdjunto { get; set; }
        public string SoporteTitulos { get; set; }
        public List<string> Soporte { get; set; }
        #endregion

        #region Proyecto

        #endregion

        #region Monumento Nacional

        #endregion

        #region General
        public string ProyectoActividad { get; set; }
        public virtual List<int> Requerimiento { get; set; }
        public virtual RequerimientoModel RequerimientoMain { get; set; }
        public int? RequerimientoPrincipalId { get; set; }
        #endregion

        #region Cierre
        public string MedioDespachoCod { get; set; }
        public string ProveedorDespachoCod { get; set; }
        public int? ProveedorDespachoTitulo { get; set; }
        public string NumeroGuia { get; set; }
        public Nullable<System.DateTime> FechaRecepcion { get; set; }
        public string MedioVerificacionCod { get; set; }
        [AllowHtml]
        public string ObservacionesMedioVerif { get; set; }
        [AllowHtml]
        public string ObservacionesDespacho { get; set; }
        #endregion

        public string NombreArchivo { get; set; }
        public string UrlArchivo { get; set; }
        public bool EnviarNotificacion { get; set; }
        public FlujoIngreso Flujo { get; set; }
        public bool EsProcesoMasivo { get; set; }



        //public string Comuna { get; set; }
        //public string Etiqueta { get; set; }
        //public string FolioDespacho { get; set; }
        //[AllowHtml]
        //public string ObservacionesAcuerdosCom { get; set; }
        //public string Region { get; set; }
        //public string RolSii { get; set; }
    }
}