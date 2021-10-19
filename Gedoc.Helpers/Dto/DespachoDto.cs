using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Enum;

namespace Gedoc.Helpers.Dto
{
    public class DespachoDto
    {
        public int Id { get; set; }
        public Nullable<bool> AdjuntaDocumentacion { get; set; }
        public int? CantidadAdjuntos { get; set; }
        public string ProyectoActividad { get; set; }
        public string Comuna { get; set; }
        public int? DestinatarioId { get; set; }
        public int EstadoId { get; set; }
        public string EstadoDespachoTitulo { get; set; }
        public string Etiqueta { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public string FolioDespacho { get; set; }
        public string Materia { get; set; }
        public string MedioDespachoCod { get; set; }
        public string MedioDespachoTitulo { get; set; }
        public string MedioVerificacionCod { get; set; }
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
        public DateTime? FechaEmisionOficio { get; set; }
        public string ProveedorDespachoCod { get; set; }
        public virtual List<GenericoDto> Requerimiento { get; set; }
        //public virtual List<RequerimientoDto> Requerimientos { get; set; }
        public virtual RequerimientoDto RequerimientoMain { get; set; }
        public string UtAsignadaTitulo { get; set; }
        public string ProfesionalNombre { get; set; }
        public int? RequerimientoPrincipalId { get; set; }

        public string ProveedorDespachoTitulo { get; set; }
        #region Datos del destinatario
        public string RemitenteNombre { get; set; }
        public string RemitenteCargo { get; set; }
        public string RemitenteTelefono { get; set; }
        public string RemitenteInstitucion { get; set; }
        public string RemitenteGenero { get; set; }
        public string RemitenteDireccion { get; set; }
        public string RemitenteRut { get; set; }
        public string RemitenteTipoInstitucion { get; set; }
        public string RemitenteEmail { get; set; }
        #endregion
        public string UsuarioActual { get; set; }
        public int? UsuarioCreacionId { get; set; }
        public List<GenericoDto> TipoAdjunto { get; set; }
        public string TipoAdjuntoTitulos { get; set; }
        public List<GenericoDto> Soporte { get; set; }
        public string SoporteTitulos { get; set; }
        public List<GenericoDto> DestinatarioCopia { get; set; }
        public FlujoIngreso Flujo { get; set; }
        public bool EnviarNotificacion { get; set; }
        public bool EsProcesoMasivo { get; set; }

        public DatosArchivo DatosArchivo { get; set; }
        public bool DesdeOficio { get; set; }
        public string DestinatarioNombre { get; set; }
    }
}
