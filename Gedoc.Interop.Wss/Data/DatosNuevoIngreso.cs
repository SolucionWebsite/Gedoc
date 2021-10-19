using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Gedoc.Helpers.Enum;

namespace Gedoc.Interop.Wss.Data
{
    [DataContract]
    public class DatosNuevoIngreso
    {

        [DataMember]
        public string TipoTramite { get; set; }

        [DataMember]
        public string TipoDocumento { get; set; }

        [DataMember]
        public string ObservacionTipoDoc { get; set; }

        [DataMember]
        public DateTime FechaDocumento { get; set; }

        [DataMember]
        public bool AdjuntaDoc { get; set; }

        [DataMember]
        public int CantAdjuntos { get; set; }

        [DataMember]
        public string[] TipoAdjuntos { get; set; }

        [DataMember]
        public string[] Soporte { get; set; }

        [DataMember]
        public string ObservacionAdjuntos { get; set; }

        [DataMember]
        public string RemitenteNombre { get; set; }

        [DataMember]
        public string RemitenteRut { get; set; }

        [DataMember]
        public string RemitenteEmail { get; set; }

        [DataMember]
        public string RemitenteGenero { get; set; }

        [DataMember]
        public string RemitenteDireccion { get; set; }

        [DataMember]
        public string RemitenteTelefono { get; set; }

        [DataMember]
        public string RemitenteInstitucion { get; set; }

        [DataMember]
        public string NombreProyectoPrograma { get; set; }

        [DataMember]
        public string NombreCaso { get; set; }

        [DataMember]
        public string Materia { get; set; }

        [DataMember]
        public string[] Etiqueta { get; set; }

        [DataMember]
        public string[] CategoriaMonNac { get; set; }

        [DataMember]
        public string CodigoMonNac { get; set; }

        [DataMember]
        public string DenominacionOficMonNac { get; set; }

        [DataMember]
        public string OtrasDenominacionesMonNac { get; set; }

        [DataMember]
        public string NombreUsoActualMonNac { get; set; }

        [DataMember]
        public string DireccionMonNac { get; set; }

        [DataMember]
        public string ReferenciaLocalidadMonNac { get; set; }

        [DataMember]
        public string[] CodigoRegion { get; set; }

        [DataMember]
        public string[] CodigoProvincia { get; set; }

        [DataMember]
        public string[] CodigoComuna { get; set; }

        [DataMember]
        public string Rol { get; set; }

        [DataMember]
        public string FormaLlegada { get; set; }

        [DataMember]
        public string ObservacionesFormaLlegada { get; set; }

        [DataMember]
        public string Caracter { get; set; }

        [DataMember]
        public string ObservacionesCaracter { get; set; }

        [DataMember]
        public bool Redireccionado { get; set; }

        [DataMember]
        public string NumeroTicket { get; set; }

        //[DataMember]
        //public bool RequiereAcuerdo { get; set; }

        [DataMember]
        public string RequermientoAnterior { get; set; }

        [DataMember]
        public string RequermientoNoRegistrado { get; set; }


        [DataMember]
        public string IdSolicitud { get; set; }

        [DataMember]
        public string Clave { get; set; }

        [DataMember]
        public string CodigoUt { get; set; }

        public int IdRequerimiento;
        public string DocumentoIngreso;
        public int IdSolicitudInt;
        public int IdTipoTramite;
        public string IdTipoDoc;
        public List<string> IdTipoAdj;
        public int IdRemitente;
        public List<string> IdSoporte;
        public int? IdCaso;
        public List<string> IdCategoriaMn;
        public List<string> IdEtiqueta;
        public List<string> IdRegion;
        public List<string> IdComuna;
        public List<string> IdProvincia;
        public string IdFormaLlegada;
        public int? IdReqAnterior;
        public int Estado;
        public int Etapa;
        public string IdPrioridad;
        public int? CaracterId;
        public int? UnidadTecnica;
        public string UnidadTecnicaTitulo;
    }
}