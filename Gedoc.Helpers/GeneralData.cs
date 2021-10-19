using System.Collections.Generic;
using System.Linq.Expressions;
using Gedoc.Helpers.Dto;

namespace Gedoc.Helpers
{
    public static class GeneralData
    {
        public const string FORMATO_FECHA_CORTO = "dd/MM/yyyy";
        public const string FORMATO_FECHA_LARGO = "dd/MM/yyyy HH:mm";
        public const string FORMATO_FECHA_TEXTUAL = "dd 'de' MMMM 'de' yyyy ";
        public const string FORMATO_FECHA_CORTO_GRID = "{0:dd/MM/yyyy}";
        public const string FORMATO_FECHA_LARGO_GRID = "{0:dd/MM/yyyy HH:mm}";
        public const string FORMATO_FECHA_LARGO_seg_GRID = "{0:dd/MM/yyyy HH:mm:ss}";
        public const string TIPO_INGRESO_TRANSP = "SIAC/TRANSPARENCIA";
        public const string SEC_KEY = "EhS1q7qP0rApbKX";
        public static Dictionary<string, string> CamposControlCambio = new Dictionary<string, string>
        {
            {"ObservacionesAdjuntos", "Observaciones al Tipo de Adjunto"},
            {"ObservacionesTipoDoc", "Observaciones al Tipo de Documento"},
            {"LiberarAsignacionTemp", "Liberación de Asignación Temporal"},
            {"ObservacionesFormaLlegada", "Observaciones a la Forma de Llegada"},
            {"ObservacionesCaracter", "Observaciones de Carácter"},
            {"Redireccionado", "Redireccionado"},
            {"ObservacionesTransparencia", "Observaciones de Transparencia"},
            {"ComentarioEncargadoUt", "Comentario de Encargado Ut"},
            {"ComentarioAsignacion", "Comentario de Asignación"},
            {"UtApoyoId", "Unidad Técnica de Apoyo"},
            {"UtConocimientoId", "Unidad Técnica en Conocimiento"},
            {"RecepcionUt", "Fecha de Recepción UT"},
            {"CasoId", "Nombre de Caso"},
            {"TipoTramiteId", "Tipo de Trámite"},
            {"Materia", "Materia"},
            {"ProyectoActividad", "Proyecto o Actividad"},
            {"RequiereAcuerdo", "Requiere Acuerdo"},
            {"RequiereRespuesta", "Requiere Respuesta"},
            {"DenominacionOficial", "Denominacion Oficial"},
            {"Region", "Región"},
            {"CategoriaMonumentoNac", "Categoria de Monumento Nacional"}
        };

        public const string ARCHIVOS_PERMITIDOS =
            "Tamaño de archivo permitido: 50 Mb. Formatos permitidos: Word, Excel, Dwg (Autocad), PowerPoint, PDF, Email e imagenes Jpg.";
    }
}