using Gedoc.Repositorio.Model;
using Gedoc.WebReport.Dtos;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Gedoc.WebReport.Logging;

namespace Gedoc.WebReport
{
    public partial class VerAuditoria : System.Web.UI.Page
    {
        private GedocEntities db = new GedocEntities();
        private WSGedoc.WsDespachosSoapClient wss = new WSGedoc.WsDespachosSoapClient();

        public ResultadoOperacion ResultadoOperacion = new ResultadoOperacion();
        private string reporte = "Reporte_Requerimientos_GrupoAuditoria.rdl";
        public static Dictionary<string, string> FechasSumatorias = new Dictionary<string, string>(){
            {"1","Fecha de Ingreso"},
            {"2","Fecha de Asignación Unidad Técnica"},
            {"3","Fecha de Asignación Unidad Técnica en copia"},
            {"4","Fecha de Asignación Unidad Técnica en conocimiento"},
            {"5","Fecha de Asignación Unidad Técnica Temporal"},
            {"6","Fecha de Priorización"},
            {"7","Fecha de Resolución Estimada"},
          //  {"8","Plazo (Número de días)"},
            {"9","Fecha Asignación Profesional Temporal"},
            {"10","Fecha  Asignación Profesional"},
            {"11","Fecha  Reasignación Profesional"},
            {"12","Fecha de Recepción UT"},
            {"13","Fecha de Unidad Técnica Reasignada"},
            {"14","Fecha de Solicitud de Reasignación"},
            {"15","Liberación de Asignación Temporal"},
            {"16","Fecha de último Acuerdo Comisión"},
            {"17","Fecha de último Acuerdo Sesión"},
            {"18","Fecha de Emisión de oficio"},
            {"19","Fecha de Ingreso Histórico"},
            {"20","Fecha de Cierre de Requerimiento"}
        };

        protected void Page_Load(object sender, EventArgs e)
        {

           

        }





        protected void btnVerReporte_Click(object sender, EventArgs e)
        {
  
        }

        #region RenderReport
        void RenderReport()
        {
            try
            {

                rpReporte.Visible = true;

                #region Inicializa Reporte
                List<ReportParameter> paramList = new List<ReportParameter>();


                rpReporte.ProcessingMode = ProcessingMode.Local;

                var regreport = db.Reporte.SingleOrDefault(q => q.Id == 14);
                this.reporte = regreport.NombreReporte;
                var rsRuta = ConfigurationManager.AppSettings["RutaRDL"];
                rpReporte.ProcessingMode = ProcessingMode.Local;
                rpReporte.LocalReport.ReportPath = rsRuta + regreport.NombreReporte;

                #endregion

                #region Reporte_Requerimientos_GrupoAuditoria.rdl

                var requiereRespChb = (chbRequiereRespSi.Checked ? "Sí" : "") + (chbRequiereRespSi.Checked && chbRequiereRespNo.Checked ? "," : "") + (chbRequiereRespNo.Checked ? "No" : "");
                var requiereAcuChb = (chbRequiereAcueSi.Checked ? "Sí" : "") + (chbRequiereAcueSi.Checked && chbRequiereAcueNo.Checked ? "," : "") + (chbRequiereAcueNo.Checked ? "No" : "");
                var timbrajeChb = (chbTimbrajeSi.Checked ? "Sí" : "") + (chbTimbrajeSi.Checked && chbTimbrajeNo.Checked ? "," : "") + (chbTimbrajeNo.Checked ? "No" : "");
                var despachoChb = (chbDespachoSi.Checked ? "Sí" : "") + (chbDespachoSi.Checked && chbDespachoNo.Checked ? "," : "") + (chbDespachoNo.Checked ? "No" : "");

                paramList.Add(new ReportParameter("Username", "PAAP"));
                paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value));
                paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value));

                paramList.Add(new ReportParameter("UnidadTecnica", hfUnidadTecnica.Value));
                paramList.Add(new ReportParameter("Estado", hfEstado.Value));
                paramList.Add(new ReportParameter("Prioridad", hfPrioridad.Value));
                paramList.Add(new ReportParameter("NombreProyectoPrograma", tbNombreProyecto.Text));
                paramList.Add(new ReportParameter("InstitucionRemitente", tbInstitucionRem.Text));
                paramList.Add(new ReportParameter("Etiqueta", hfEtiqueta.Value));
                paramList.Add(new ReportParameter("Region", hfRegion.Value));
                paramList.Add(new ReportParameter("Comuna", hfComuna.Value));
                paramList.Add(new ReportParameter("MNInvolucrado", tbMonNacional.Text));
                paramList.Add(new ReportParameter("UnidadTecnicaCopia", hfUnidadTecnicaCopia.Value));
                paramList.Add(new ReportParameter("ProfesionalUt", hfProfUt.Value));
                paramList.Add(new ReportParameter("RequiereRespuesta", requiereRespChb));
                paramList.Add(new ReportParameter("RequiereAcuerdoActa", requiereAcuChb));
                paramList.Add(new ReportParameter("InstitucionDestinatario", tbInstituciónDest.Text));
                paramList.Add(new ReportParameter("TipoInstitucionRemitente", hfTipoInstRem.Value));
                paramList.Add(new ReportParameter("TipoInstitucionDestinatario", hfTipoInstDes.Value));
                paramList.Add(new ReportParameter("Semaforo", hfSemaforo.Value));
                paramList.Add(new ReportParameter("FechaResolucionDesde", dtcFechaResolEstimDesde.Value));
                paramList.Add(new ReportParameter("FechaResolucionHasta", dtcFechaResolEstimHasta.Value));
                paramList.Add(new ReportParameter("Plazo", hfPlazo.Value));
                paramList.Add(new ReportParameter("DespachoSiNo", despachoChb));
                paramList.Add(new ReportParameter("MotivoCierre", hfMotivoCierre.Value));
                paramList.Add(new ReportParameter("RequiereTimbraje", timbrajeChb));

                paramList.Add(new ReportParameter("RelacionDesdeHasta", hfRelacionDesdeHasta.Value));

                rpReporte.LocalReport.SetParameters(paramList);
                rpReporte.LocalReport.DataSources.Clear();
                var result = wss.GetRequerimientosFechaUtGrupo(txtFechaDesde.Value,txtFechaHasta.Value, 
                    hfUnidadTecnica.Value,hfEstado.Value,hfPrioridad.Value,tbPrioridadReq.Text,
                    tbInstitucionRem.Text,hfEtiqueta.Value,hfRegion.Value,hfComuna.Value,tbMonNacional.Text,
                    hfProfUt.Value,hfUnidadTecnicaCopia.Value, requiereRespChb,
                    tbMotivoCierre.Text, timbrajeChb, requiereAcuChb, despachoChb, despachoChb,
                    hfTipoInstDes.Value,hfTipoInstRem.Value,hfTipoInstDes.Value,hfSemaforo.Value,dtcFechaResolEstimDesde.Value,dtcFechaResolEstimHasta.Value,hfRelacionDesdeHasta.Value);
                rpReporte.LocalReport.DataSources.Add( new ReportDataSource("dsRequerimientos", result));
                rpReporte.LocalReport.Refresh();
                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                ResultadoOperacion.Codigo = -1;
                ResultadoOperacion.Texto = "Ha ocurrido un error al mostrar el reporte, por favor chequee el log de errores.";
                rpReporte.Visible = false;
                return;
            }

        }
        #endregion

        protected void btnVerAuditoria_Click(object sender, EventArgs e)
        {
            errorMsgReporte.Visible = false;
            ResultadoOperacion.Codigo = 0;
            hfAlertaMensaje.Value = string.Empty;
            RenderReport();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected void rpReporte_ReportError(object sender, ReportErrorEventArgs e)
        {
            //LoggingService.LogError(e.Exception);
            if (e.Exception.Message.Contains("rsReportParameterTypeMismatch"))
                errorMsgReporte.Text = "Error al generar el reporte: Parametros incorrectos. Por favor chequee el log de errores para más detalles.";
            else
                if (e.Exception.Message.Contains("Unable to connect to the remote server"))
                errorMsgReporte.Text = "Error al generar el reporte: No es posible conectar con el servidor. Por favor chequee el log de errores para más detalles.";
            else
                errorMsgReporte.Text = "Error al generar el reporte. Especifique otros filtros o disminuya el rango de fechas para reducir la cantidad de registros a procesar. Por favor chequee el log de errores para más detalles.";

            errorMsgReporte.Visible = true;
            e.Handled = true;
            //rpReporte.Visible = false;
        }

        private List<string> GetDateParameters()
        {

            List<string> param = new List<string>();
            foreach (ReportParameterInfo info in rpReporte.ServerReport.GetParameters())
            {
                param.Add(string.Format("{0}", info.Name));
            }

            return param;
        }



    }
}