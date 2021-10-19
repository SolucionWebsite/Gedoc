using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;

namespace Gedoc.WebApp.Models
{
    public class BusquedaAvanzadaModel
    {
        public string TipoBusquedaBusqAv { get; set; }
        public string TipoBitacoraBusqAv { get; set; }
        public DateTime? FechaIngresoBusqAv { get; set; }
        public int? DocumentoIngresoIdBusqAv { get; set; }
        public DateTime? FechaDocumentoIngresoBusqAv { get; set; }
        public int? RequerimientoAnteriorIdBusqAv { get; set; }
        public int? RemitenteIdBusqAv { get; set; }
        public string InstitucionRemitenteBusqAv { get; set; }
        public string CargoProfesionRemitenteBusqAv { get; set; }
        public string NumeroOficioCMNBusqAv { get; set; }
        public DateTime? FechaEmisionOficioCMNBusqAv { get; set; }
        public string MateriaBusqAv { get; set; }
        public string RegionBusqAv { get; set; }
        public string ComunaBusqAv { get; set; }
        public string EtiquetaBusqAv { get; set; }
        public string DenominacionOficialBusqAv { get; set; }
        public string CategoriaMonumentoNacionalBusqAv { get; set; }
        public string RolSIIPropiedadBusqAv { get; set; }
        public DateTime? FechaBitacoraBusqAv { get; set; }
        public int? UnidadTecnicaAsignadaBusqAv { get; set; }
        public int? ProfesionalUTAsignadoBusqAv { get; set; }
        public int? CreadorBitacoraBusqAv { get; set; }
        public string FormaLlegadaBusqAv { get; set; }
        public int? EstadoBusqAv { get; set; }
        public int? EstadoDespachoBusqAv { get; set; }
        public string ObservacionAcuerdoComentarioBusqAv { get; set; }
    }
}