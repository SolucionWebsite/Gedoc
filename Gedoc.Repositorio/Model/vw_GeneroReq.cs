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
    
    public partial class vw_GeneroReq
    {
        public int Id { get; set; }
        public string DocumentoIngreso { get; set; }
        public System.DateTime FechaIngreso { get; set; }
        public Nullable<int> Anno { get; set; }
        public Nullable<int> Mes { get; set; }
        public string AnnoMes { get; set; }
        public string EtiquetaTitulos { get; set; }
        public int NumeroIngreso { get; set; }
        public string CategoriaMonumentoNacTitulo { get; set; }
        public string RemitenteGenero { get; set; }
        public string RemitenteNombre { get; set; }
        public string RemitenteInstitucion { get; set; }
        public Nullable<int> UtAsignadaId { get; set; }
        public string UtAsignadaTitulo { get; set; }
        public Nullable<int> ProfesionalId { get; set; }
        public string ProfesionalNombre { get; set; }
        public Nullable<System.DateTime> Cierre { get; set; }
        public Nullable<int> MotivoCierreId { get; set; }
        public string MotivoCierreTitulo { get; set; }
        public string ComentarioCierre { get; set; }
        public Nullable<int> CerradoPorId { get; set; }
        public string CerradoPor { get; set; }
        public string TipoIngreso { get; set; }
        public Nullable<int> CasoId { get; set; }
        public string CasoTitulo { get; set; }
        public Nullable<int> TipoTramiteId { get; set; }
        public string TipoTramiteTitulo { get; set; }
        public string CanalLlegadaTramiteCod { get; set; }
        public string CanalLlegadaTramiteTitulo { get; set; }
        public string MonumentoNacionalDenominacionOficial { get; set; }
        public string MonumentoNacionalOtrasDenominaciones { get; set; }
        public string MonumentoNacionalNombreUsoActual { get; set; }
        public string MonumentoNacionalDireccionMonumentoNac { get; set; }
        public string MonumentoNacionalReferenciaLocalidad { get; set; }
        public string MonumentoNacionalRegionTitulo { get; set; }
        public string MonumentoNacionalComunaTitulo { get; set; }
        public int EstadoId { get; set; }
        public string EstadoTitulo { get; set; }
        public string Materia { get; set; }
    }
}
