using Microsoft.Reporting.WebForms;
using System;

namespace Gedoc.WebReport
{
    public class ReportViewer_ES : IReportViewerMessages /*, IReportViewerMessages2, IReportViewerMessages3*/
    {
        public string DocumentMapButtonToolTip { get { return "Mapa del documento"; } }       

        public string ParameterAreaButtonToolTip { get { return "Mostrar u ocultar parámetros"; } }

        public string FirstPageButtonToolTip { get { return "Primera página"; } }

        public string PreviousPageButtonToolTip { get { return "Página anterior"; } }

        public string CurrentPageTextBoxToolTip { get { return "Página actual"; } }

        public string PageOf { get { return "de"; } }

        public string NextPageButtonToolTip { get { return "Siguiente página"; } }

        public string LastPageButtonToolTip { get { return "Última página"; } }

        public string BackButtonToolTip { get { return "Atrás"; } }

        public string RefreshButtonToolTip { get { return "Actualizar"; } }

        public string PrintButtonToolTip { get { return "Imprimir"; } }

        public string ExportButtonToolTip { get { return "Exportar"; } }

        public string ZoomControlToolTip { get { return "Zoom"; } }

        public string SearchTextBoxToolTip { get { return "Buscar texto"; } }

        public string FindButtonToolTip { get { return "Buscar"; } }

        public string FindNextButtonToolTip { get { return "Siguiente"; } }

        public string ZoomToPageWidth { get { return "Ancho de página"; } }

        public string ZoomToWholePage { get { return "Toda la página"; } }

        public string FindButtonText { get { return "Buscar"; } }

        public string FindNextButtonText { get { return "Siguiente"; } }

        public string ViewReportButtonText { get { return "Ver informe"; } }

        public string ProgressText { get { return "El informe se está generando"; } }

        public string TextNotFound { get { return "Texto no encontrado"; } }

        public string NoMoreMatches { get { return "No se encuentran más coincidencias"; } }

        public string ChangeCredentialsText { get { return "Cambiar Credenciales"; } }

        public string NullCheckBoxText { get { return "Nulo"; } }

        public string NullValueText { get { return "Nulo"; } }

        public string TrueValueText { get { return "Verdadero"; } }

        public string FalseValueText { get { return "Falso"; } }

        public string SelectAValue { get { return "<Seleccione un valor>"; } }

        public string UserNamePrompt { get { return "Usuario"; } }

        public string PasswordPrompt { get { return "Contraseña"; } }

        public string SelectAll { get { return "(Seleccionar Todo)"; } }

        public string TodayIs { get { return "Hoy es"; } }

        public string ExportFormatsToolTip { get { return "Formatos de exportación"; } }

        public string ExportButtonText { get { return "Exportar"; } }

        public string SelectFormat { get { return "Seleccionar un formato"; } }

        public string DocumentMap { get { return "Mapa"; } }

        public string InvalidPageNumber { get { return "Página no válida"; } }

        public string ChangeCredentialsToolTip { get { return "Cambiar Credenciales"; } }
        
    }
}