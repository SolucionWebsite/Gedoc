using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class VistaGeneroDto
    {
        [Description("Hidden_Id")] public int Id { get; set; }
        [Description("Hidden_Anno")] public string Anno { get; set; }
        [Description("Hidden_Mes")] public string Mes { get; set; }
        [Description("Año y Mes")] public string AnnoMes { get; set; }
        [Description("Género")] public string RemitenteGenero { get; set; }
        [Description("Documento Ingreso")] public string DocumentoIngreso { get; set; }
        [Description("Fecha Ingreso")] public DateTime FechaIngreso { get; set; }
        [Description("Etiqueta")] public string EtiquetaTitulos { get; set; }
        [Description("Estado")] public string EstadoTitulo { get; set; }
        [Description("Número Ingreso")] public int NumeroIngreso { get; set; }
        [Description("Cetagoría MN")] public string CategoriaMonumentoNacTitulo { get; set; }
        [Description("Remitente")] public string RemitenteNombre { get; set; }
        [Description("Institución Remitente")] public string RemitenteInstitucion { get; set; }
        [Description("Materia")] public string Materia { get; set; }
        [Description("Unidad Técnica")] public string UtAsignadaTitulo { get; set; }
        [Description("Profesional en área")] public string ProfesionalNombre { get; set; }
        [Description("Fecha Cierre")] public DateTime? Cierre { get; set; }
        [Description("Motivo Cierre")] public string MotivoCierreTitulo { get; set; }
        [Description("Comentario Cierre")] public string ComentarioCierre { get; set; }
        [Description("Cerrado Por")] public string CerradoPor { get; set; }
        [Description("Tipo de Ingreso")] public string TipoIngreso { get; set; }
        [Description("Número de Caso")] public int? CasoId { get; set; }
        [Description("Tipo de Trámite")] public string TipoTramiteTitulo { get; set; }

        [Description("Canal de Llegada del Trámite")]
        public string CanalLlegadaTramiteTitulo { get; set; }

        [Description("Denominación Oficial")] public string MonumentoNacionalDenominacionOficial { get; set; }
        [Description("Otras denominaciones")] public string MonumentoNacionalOtrasDenominaciones { get; set; }
        [Description("Nombre o uso actual")] public string MonumentoNacionalNombreUsoActual { get; set; }

        [Description("Referecia de localidad")]
        public string MonumentoNacionalReferenciaLocalidad { get; set; }

        [Description("Región")] public string MonumentoNacionalRegionTitulo { get; set; }
        [Description("Comuna")] public string MonumentoNacionalComunaTitulo { get; set; }
        [Description("Hidden_MN")] public MonumentoNacionalDto MonumentoNacional { get; set; }
        [Description("Hidden_Total")] public int? Total { get; set; }
    }
}
