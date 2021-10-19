using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Gedoc.WebReport.Dtos;
using Gedoc.Repositorio.Model;
using Microsoft.Reporting.WebForms;
using Reporte = Gedoc.Repositorio.Model.Reporte;
using Gedoc.WebReport.WSGedoc;
using System.Data.SqlClient;
using Gedoc.WebReport.Controllers;
using Gedoc.WebReport.Logging;
using Gedoc.WebReport.Models;
using Gedoc.Helpers.Enum;
using Microsoft.ReportingServices.Interfaces;

namespace Gedoc.WebReport
{
    public partial class VistaReporteTramites : System.Web.UI.Page
    {
        public ResultadoOperacion ResultadoOperacion = new ResultadoOperacion();
        public List<Reporte> Reportes = new List<Reporte>();
        private GedocEntities db = new GedocEntities();
        private WsDespachosSoapClient wss = new WsDespachosSoapClient();

        protected void Page_Load(object sender, EventArgs e)
        {

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
                var idreporte = 12;
                var reporte = "";
                var estadoFiltro = "";
                var etapaFiltro = "";
                var comunaFiltro = "";
                var provFiltro = "";
                var regionFiltro = "";
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
                paramList.Add(new ReportParameter("TipoTramite", hfTipoTramite.Value));
                paramList.Add(new ReportParameter("CanalLlegada", hfCanalTramite.Value));
                paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value));
                paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value));
                paramList.Add(new ReportParameter("NumeroCaso", hfNumeroCaso.Value));
                paramList.Add(new ReportParameter("NombreCaso", hfNombreCaso.Value));
                var idCasoNombre = -1;
                if (string.IsNullOrEmpty(hfNombreCaso.Value))
                {
                    paramList.Add(new ReportParameter("NombreCasoTitulo", ""));
                } else
                {
                    Int32.TryParse(hfNombreCaso.Value, out idCasoNombre);
                    var caso = db.Caso.FirstOrDefault(c => c.Id == idCasoNombre);
                    paramList.Add(new ReportParameter("NombreCasoTitulo", caso.Titulo));
                }
                paramList.Add(new ReportParameter("DocumentoIngreso", hfDocumentoIngreso.Value.Contains("[TODOS]") ? "[TODOS]" : hfDocumentoIngreso.Value));
                var estadosEtapas = GenericoController.SeparaEstadosEtapa(hfEstadoEtapa.Value);
                if (estadosEtapas[0].Count > 0)
                {
                    var estadosArr = estadosEtapas[0].ToArray();
                    estadoFiltro = string.Join(",", estadosArr);
                    paramList.Add(new ReportParameter("Estado", estadoFiltro));
                    var dato = "";
                    for (int i = 0; i < estadosEtapas[0].Count; i++)
                    {
                        dato += estadosEtapas[0][i] + " - " + estadosEtapas[1][i];
                    }
                    paramList.Add(new ReportParameter("EstadosEtapas", dato));
                }
                else
                {
                    paramList.Add(new ReportParameter("EstadosEtapas", ""));
                    paramList.Add(new ReportParameter("Estado", ""));
                }
                // Etapa
                if (estadosEtapas[1].Count > 0)
                {
                    var etapasArr = estadosEtapas[1].ToArray();
                    etapaFiltro = string.Join(",", etapasArr);
                    paramList.Add(new ReportParameter("Etapa", etapaFiltro));
                }
                else
                {
                    paramList.Add(new ReportParameter("Etapa", ""));
                }
                paramList.Add(new ReportParameter("Remitente", hfRemitente.Value));
                paramList.Add(new ReportParameter("IntitucionRemitente", txtInstitucionRemitente.Text));
                paramList.Add(new ReportParameter("CategoriaMonumento", hfTipoMon.Value.Contains("[TODOS]") ? "[TODOS]" : hfTipoMon.Value));
                paramList.Add(new ReportParameter("CodigoMonumento", txtCodigoMonumento.Text));
                paramList.Add(new ReportParameter("DenominacionOficial", txtDenomiOfi.Text));
                paramList.Add(new ReportParameter("DireccionMonumento", txtDireccionTxt.Text));
                // Filtros de Región, provincia y Comuna
                if (string.IsNullOrEmpty(hfComuna.Value))
                {
                    paramList.Add(new ReportParameter("Region", "[TODOS]"));
                    paramList.Add(new ReportParameter("Provincia", "[TODOS]"));
                    paramList.Add(new ReportParameter("Comuna", "[TODOS]"));
                    paramList.Add(new ReportParameter("RegionProvComunas", ""));
                }
                else
                {
                    string[] comunas = hfComuna.Value.Split(',');

                    //var datosFiltrosComunas = (from lvC in db.ListaValor
                    //        join lisC in db.Lista on lvC.IdLista equals lisC.IdLista
                    //        join lvP in db.ListaValor on lisC.IdListaPadre equals lvP.IdLista
                    //        join lisP in db.Lista on lvP.IdLista equals lisP.IdLista
                    //        join lvR in db.ListaValor on lisP.IdListaPadre equals lvR.IdLista
                    //        where (lisC.IdLista == (int)Mantenedor.Comuna &&
                    //               comunas.Contains(lvC.Codigo) ) 
                    //        select new 
                    //        {
                    //            lisC, lvC, lisP, lvP, lvR
                    //        })
                    //    .ToList();

                    var datosFiltrosComunas = (from lvC in db.ListaValor
                        join lisC in db.Lista on lvC.IdLista equals lisC.IdLista
                        join lvP in db.ListaValor on lvC.CodigoPadre equals lvP.Codigo
                        join lvR in db.ListaValor on lvP.CodigoPadre equals lvR.Codigo
                        where (lvC.IdLista == (int)Mantenedor.Comuna &&
                               lvP.IdLista == (int)Mantenedor.Provincia &&
                               lvR.IdLista == (int)Mantenedor.Region &&
                               comunas.Contains(lvC.Codigo))
                        select new
                        {
                            lvC,
                            lvP,
                            lvR
                        })
                        .ToList();
                    var dato = String.Join(",", datosFiltrosComunas.Select(d => d.lvR.Titulo).ToArray());
                    regionFiltro = string.Join(",", dato);
                    paramList.Add(new ReportParameter("Region", dato));
                    dato = String.Join(",", datosFiltrosComunas.Select(d => d.lvP.Titulo).ToArray());
                    provFiltro = string.Join(",", dato);
                    paramList.Add(new ReportParameter("Provincia", dato));
                    dato = String.Join(",", datosFiltrosComunas.Select(d => d.lvC.Titulo).ToArray());
                    comunaFiltro = string.Join(",", dato);
                    paramList.Add(new ReportParameter("Comuna", dato));

                    var arrFiltroReg = datosFiltrosComunas.Select(d => d.lvR.Titulo + " - " + d.lvP.Titulo + " - " + d.lvC.Titulo).ToArray();
                    paramList.Add(new ReportParameter("RegionProvComunas", string.Join("; ", arrFiltroReg)));
                }
                paramList.Add(new ReportParameter("Materia", txtMateria.Text));
                paramList.Add(new ReportParameter("Etiqueta", hfEtiqueta.Value.Contains("[TODOS]") ? "[TODOS]" : hfEtiqueta.Value));
                paramList.Add(new ReportParameter("UnidadTecnica", hfUnidadTecnica.Value.Contains("[TODOS]") ? "[TODOS]" : hfUnidadTecnica.Value));
                paramList.Add(new ReportParameter("ProfesionalUt", hfProfUt.Value.Contains("[TODOS]") || hfProfUt.Value.Contains("Seleccione una UT") ? "[TODOS]" : hfProfUt.Value));
                // Nombre de la UT filtradas:
                //if (string.IsNullOrEmpty(hfUnidadTecnica.Value))
                //{
                //    paramList.Add(new ReportParameter("UnidadTecnicaTitulos", ""));
                //}
                //else if (hfUnidadTecnica.Value == "-1")
                //{
                //    paramList.Add(new ReportParameter("UnidadTecnicaTitulos", "[TODOS]"));
                //}
                //else
                //{
                //    string[] uts = hfUnidadTecnica.Value.Split(',');
                //    string utsTitulos = "";
                //    var uts2 = db.UnidadTecnica
                //        .Where(u => uts.Contains(u.Id.ToString())).ToList();
                //        //.All(u =>
                //        //{
                //        //    utsTitulos += u.Titulo; return true; });
                //    uts2.ForEach(u => utsTitulos += u.Titulo );
                //    paramList.Add(new ReportParameter("UnidadTecnicaTitulos", utsTitulos));
                //}
                if (hfUnidadTecnica.Value.Contains("[TODOS]"))
                {
                    paramList.Add(new ReportParameter("UnidadTecnicaTitulos", "[TODOS]"));
                }
                else
                {
                    paramList.Add(new ReportParameter("UnidadTecnicaTitulos", hfUnidadTecnica.Value));
                }

                rpReporte.LocalReport.SetParameters(paramList);


                rpReporte.LocalReport.DataSources.Clear();
                var result = wss.GetRequerimientosTramites(txtFechaDesde.Value.ToString(), txtFechaHasta.Value.ToString(),
                    hfTipoTramite.Value, hfCanalTramite.Value, hfNumeroCaso.Value, hfNombreCaso.Value,
                    hfDocumentoIngreso.Value.Contains("[TODOS]") ? "[TODOS]" : hfDocumentoIngreso.Value, 
                    estadoFiltro, etapaFiltro, hfRemitente.Value,
                    txtInstitucionRemitente.Text, 
                    (hfTipoMon.Value.Contains("[TODOS]") ? "[TODOS]" : hfTipoMon.Value),
                    txtDenomiOfi.Text, txtDireccionTxt.Text,
                    regionFiltro, provFiltro, comunaFiltro,
                    txtMateria.Text, hfEtiqueta.Value.Contains("[TODOS]") ? "[TODOS]" : hfEtiqueta.Value,
                    hfUnidadTecnica.Value.Contains("[TODOS]") ? "[TODOS]" : hfUnidadTecnica.Value, 
                    hfProfUt.Value.Contains("Seleccione una UT") ? "[TODOS]" : hfProfUt.Value);
                rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dsRequerimientosTramiteCasos", result));

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