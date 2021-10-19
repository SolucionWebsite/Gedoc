using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Gedoc.WebReport.Dtos;
using Gedoc.Repositorio.Model;
using Microsoft.Reporting.WebForms;
using Reporte = Gedoc.Repositorio.Model.Reporte;
using Gedoc.WebReport.WSGedoc;
using Gedoc.WebReport.Controllers;
using Gedoc.WebReport.Logging;

namespace Gedoc.WebReport
{
    public partial class VistaReporteProcMasivos : System.Web.UI.Page
    {
        public ResultadoOperacion ResultadoOperacion = new ResultadoOperacion();
        public List<Reporte> Reportes = new List<Reporte>();
        private GedocEntities db = new GedocEntities();
        private WsDespachosSoapClient wss = new WsDespachosSoapClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
                return;

            // Tipos de Procesos Masivos
            ddlAccion.Items.Add(new System.Web.UI.WebControls.ListItem("Nuevo Despacho", "DES"));
            ddlAccion.Items.Add(new System.Web.UI.WebControls.ListItem("Asignar Unidad Técnica", "AUT"));
            ddlAccion.Items.Add(new System.Web.UI.WebControls.ListItem("Reasignar Unidad Técnica", "RUT"));
            ddlAccion.Items.Add(new System.Web.UI.WebControls.ListItem("Asignar Profesional UT", "APR"));
            ddlAccion.Items.Add(new System.Web.UI.WebControls.ListItem("Reasignar Profesional UT", "RPR"));
            ddlAccion.Items.Add(new System.Web.UI.WebControls.ListItem("Modificar Etiqueta", "ETI"));
            ddlAccion.Items.Add(new System.Web.UI.WebControls.ListItem("Abrir Ingresos", "AI"));
            ddlAccion.SelectedIndex = 0;
        }

        protected void btnVerReporte_Click(object sender, EventArgs e)
        {
            //tbal.Value = string.Empty;
            RenderReport();
        }

        #region RenderReport
        void RenderReport()
        {
            try
            {
                var idreporte = 13;
                var reporte = "";
                rpReporte.Visible = true;

                #region Inicializa Reporte
                List<ReportParameter> paramList = new List<ReportParameter>();
                rpReporte.ProcessingMode = ProcessingMode.Local;
                var regreport = db.Reporte.SingleOrDefault(q => q.Id == idreporte);
                reporte = regreport.Nombre;
                var rsRuta = ConfigurationManager.AppSettings["RutaRDL"];
                rpReporte.ProcessingMode = ProcessingMode.Local;
                rpReporte.LocalReport.ReportPath = rsRuta + regreport.NombreReporte;

                #endregion

                #region Reporte

                paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value));
                paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value));
                paramList.Add(new ReportParameter("UnidadTecnica", hfUnidadTecnica.Value));  // tbUnidadTecnica.Text)); // 
                paramList.Add(new ReportParameter("ProfesionalUt", hfProfUt.Value)); // tbProfesionalArea.Text)); // 
                paramList.Add(new ReportParameter("Estado", hfEstado.Value)); // tbEstado.Text)); // 
                paramList.Add(new ReportParameter("Etapa", hfEtapa.Value)); // tbEtapa.Text)); // 
                paramList.Add(new ReportParameter("Etiqueta", hfEtiqueta.Value)); // tbEtiqueta.Text)); // 
                paramList.Add(new ReportParameter("TipoMon", hfTipoMon.Value)); // tbCategoriaMonumento.Text)); // 
                paramList.Add(new ReportParameter("Region", hfRegion.Value)); // tbRegion.Text)); // 
                paramList.Add(new ReportParameter("AccionPm", ddlAccion.SelectedValue));
                var accion = "";
                switch (ddlAccion.SelectedValue)
                {
                    case "DES": // Crear Despacho
                        accion = "Crear Despacho";
                        break;
                    case "AUT": // Asignar UT
                        accion = "Asignar UT";
                        break;
                    case "RUT": // Reasignar UT
                        accion = "Reasignar UT";
                        break;
                    case "APR": // Asignar Profesional
                        accion = "Asignar Profesional";
                        break;
                    case "RPR": // Reasignar Profesional
                        accion = "Reasignar Profesional";
                        break;
                    case "ETI":  // Modificar Etiqueta
                        accion = "Modificar Etiqueta";
                        break;
                    case "AI":  // Abrir Ingresos
                        accion = "Abrir Ingresos";
                        break;
                }
                paramList.Add(new ReportParameter("TituloAccion", accion));

                rpReporte.LocalReport.SetParameters(paramList);


                rpReporte.LocalReport.DataSources.Clear();
                var result = wss.GetProcesosMasivos(txtFechaDesde.Value, txtFechaHasta.Value,
                    tbUnidadTecnica.Text, tbProfesionalArea.Text, tbEstado.Text,
                    tbEtapa.Text, tbEtiqueta.Text, tbCategoriaMonumento.Text, tbRegion.Text, ddlAccion.SelectedValue);
                rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dsRequerimientos", result));

                rpReporte.LocalReport.Refresh();
                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                rpReporte.Visible = false;
                return;
            }

        }
        #endregion



    }
}