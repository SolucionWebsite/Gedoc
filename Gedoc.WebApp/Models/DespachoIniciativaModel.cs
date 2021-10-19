using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;

namespace Gedoc.WebApp.Models
{
    public class DespachoIniciativaModel
    {
        public int Id { get; set; }

        #region Documento
        public int EstadoId { get; set; }
        public string EstadoDespachoTitulo { get; set; }
        public string NumeroDespacho { get; set; }
        public string FolioDespacho { get; set; }
        public DateTime? FechaEmisionOficio { get; set; }
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
        public List<GenericoDto> DestinatarioCopiaData { get; set; }

        #endregion

        #region Despacho
        public string AntecedenteAcuerdo { get; set; }
        public string Materia { get; set; }

        #endregion

        #region Proyecto
        public string NombreProyectoPrograma { get; set; }
        public int? CasoId { get; set; }
        public string CasoTitulo { get; set; }
        public string EtiquetaTitulos { get; set; }
        public List<string> Etiqueta { get; set; }

        #endregion

        #region Monumento Nacional
        public int MonumentoNacionalId { get; set; }
        public List<string> MonumentoNacionalCategoriaMonumentoNac { get; set; }
        public string MonumentoNacionalCategoriaMonumentoNacCodigo { get; set; }
        public string MonumentoNacionalCodigoMonumentoNac { get; set; }
        public string MonumentoNacionalDenominacionOficial { get; set; }
        public string MonumentoNacionalOtrasDenominaciones { get; set; }
        public string MonumentoNacionalNombreUsoActual { get; set; }
        public string MonumentoNacionalDireccionMonumentoNac { get; set; }
        public string MonumentoNacionalReferenciaLocalidad { get; set; }
        public List<string> MonumentoNacionalRegion { get; set; }
        public List<string> MonumentoNacionalProvincia { get; set; }
        public List<string> MonumentoNacionalComuna { get; set; }
        public string MonumentoNacionalRolSii { get; set; }

        #endregion

        #region Adjunto
        public Nullable<bool> AdjuntaDocumentacion { get; set; }
        public int? CantidadAdjuntos { get; set; }
        public List<string> TipoAdjunto { get; set; }
        [AllowHtml]
        public string ObservacionesAdjuntos { get; set; }
        public List<string> Soporte { get; set; }

        #endregion

        #region General
        public string ProyectoActividad { get; set; }
        public int? UtAsignadaId { get; set; }
        public int? ProfesionalId { get; set; }
        #endregion

        #region Cierre
        public string ProveedorDespachoCod { get; set; }
        public string NumeroGuia { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public string MedioVerificacionCod { get; set; }
        [AllowHtml]
        public string ObservacionesMedioVerif { get; set; }
        [AllowHtml]
        public string ObservacionesDespacho { get; set; }
        #endregion

        public string UrlArchivo { get; set; }
        public bool EnviarNotificacion { get; set; }
        public FlujoIngreso Flujo { get; set; }

    }
}