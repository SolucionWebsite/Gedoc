using System;
using System.Collections.Generic;
using System.Configuration;
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
using Gedoc.Helpers.Enum;
using Gedoc.WebReport.Logging;
using Gedoc.WebReport.Models;

namespace Gedoc.WebReport
{
    public partial class VistaReporte : System.Web.UI.Page
    {

        public ResultadoOperacion ResultadoOperacion = new ResultadoOperacion();
        public List<Reporte> Reportes = new List<Reporte>();
        private GedocEntities db = new GedocEntities();
        private WsDespachosSoapClient wss = new WsDespachosSoapClient();
        private int idreporte = 0;
        private string reporte = "";


        protected override void OnPreRender(EventArgs e)
        {
            try
            {
                base.OnPreRender(e);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            idreporte = Convert.ToInt32(Page.Request["id"]);
            reporte = Page.Request["r"] ?? Page.Request.QueryString["r"];
            var ut = Page.Request["ut"] ?? Page.Request.QueryString["ut"];
            var sesion = Page.Request["sesion"] ?? Page.Request.QueryString["sesion"];

            if (!string.IsNullOrEmpty(ut) && !string.IsNullOrEmpty(sesion) && reporte == "Reporte_Tabla_de_Comision.rdl" && !Page.IsPostBack)
            {
                if (string.IsNullOrWhiteSpace(hfUnidadTecnica.Value) || string.IsNullOrWhiteSpace(hfSesionTabla.Value))
                {
                    hfUnidadTecnica.Value = ut;
                    hfSesionTabla.Value = sesion;
                }

                ResultadoOperacion.Codigo = 3;
                ResultadoOperacion.Texto = "Mostrando reporte de Tabla";
                //btnVerReporte_Click(null, null);
            }

            if (Page.IsPostBack)
            {
                var controlName = Page.Request["__EVENTTARGET"];
                Page.ClientScript.RegisterClientScriptBlock(GetType(), "IsPostBack", "var isPostBack = true; var tipoControl='" + controlName + "';", true);
            }

            if (Page.IsPostBack) return;

            GetReportes(Convert.ToInt32(idreporte));

        }


        void Crear(int idreporte)
        {
            try
            {

                var rsRuta = ConfigurationManager.AppSettings["RutaRDL"];
                var regreport = db.Reporte.SingleOrDefault(q => q.Id == idreporte);
                rpReporte.ProcessingMode = ProcessingMode.Local;
                rpReporte.LocalReport.ReportPath = rsRuta + regreport.NombreReporte;



                //Trae los Parametros de los reportes
                List<string> param = new List<string>();
                foreach (ReportParameterInfo info in rpReporte.LocalReport.GetParameters())
                {
                    param.Add(string.Format("{0}", info.Name));
                }

                HabilitarParametros(param.ToArray());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }


        }


        #region RenderReport
        void RenderReport()
        {
            try
            {

                rpReporte.Visible = true;


                #region Inicializa Reporte
                List<ReportParameter> paramList = new List<ReportParameter>();

                var regreport = db.Reporte.SingleOrDefault(q => q.Id == idreporte);
                this.reporte = regreport.NombreReporte;
                var rsRuta = ConfigurationManager.AppSettings["RutaRDL"];
                rpReporte.ProcessingMode = ProcessingMode.Local;
                rpReporte.LocalReport.ReportPath = rsRuta + regreport.NombreReporte;

                #endregion

                #region Reporte_Requerimientos.rdl
                if (this.reporte == "Reporte_Requerimientos.rdl")
                {
                    string[] tipomon = hfTipoMon.Value.Split(',');


                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));
                    paramList.Add(new ReportParameter("NombreMn", tbNombreMn.Text));

                    //Valor de UT
                    var uts = GetUtTitulosFromIds(false);
                    var multiUT = new ReportParameter("UnidadTecnica");
                    multiUT.Values.AddRange(uts);
                    paramList.Add(multiUT);

                    //Valor Profesional UT
                    var profesional = GetProfesionalNombresFromIds(false);
                    var multiprof = new ReportParameter("Profesional");
                    multiprof.Values.AddRange(profesional);
                    paramList.Add(multiprof);

                    //MultiValores Estado
                    var estados = GetEstadoTitulosFromIds(true);
                    paramList.Add(new ReportParameter("Estado", string.Join(",", estados)));
                    //var multiestado = new ReportParameter("Estado");
                    //multiestado.Values.AddRange(estados);
                    //paramList.Add(multiestado);
                    estados = GetEstadoTitulosFromIds(false);

                    //MultiValores Categoria MN
                    var catMn = GetCategoriaTitulosFromIds();
                    var multiCategMn = new ReportParameter("CategoriaMn");
                    multiCategMn.Values.AddRange(catMn);
                    paramList.Add(multiCategMn);
                    catMn = catMn[0] == "[TODOS]" ? catMn : hfTipoMon.Value.Split(','); // GetCategoriaTitulosFromIds();

                    rpReporte.LocalReport.SetParameters(paramList);
                    rpReporte.LocalReport.DataSources.Clear();
                    var result = wss.GetRequerimientosFechaUtProf(txtFechaDesde.Value, txtFechaHasta.Value,
                        string.Join(",", uts), string.Join(",", profesional),
                        string.Join(",", estados), string.Join(",", catMn), tbNombreMn.Text);
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dsRequerimientos", result));
                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                #region Reporte_Requerimientos_GrupoAuditoria.rdl
                if (this.reporte == "Reporte_Requerimientos_GrupoAuditoria.rdl")
                {

                    string[] ut = CrearArreglo(tbUnidadTecnica.Text, "M-Unidades técnicas");

                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));


                    //MultiValores UT
                    var multiUT = new ReportParameter("UnidadTecnica");
                    multiUT.Values.AddRange(ut);
                    paramList.Add(multiUT);

                    //MultiValores Estado
                    var multiestado = new ReportParameter("Estado");
                    multiestado.Values.AddRange(tbEstado.Text.Split(','));
                    paramList.Add(multiestado);

                    rpReporte.LocalReport.SetParameters(paramList);
                    rpReporte.LocalReport.DataSources.Clear();
                    var result = wss.GetRequerimientosFechaUtGrupo(txtFechaDesde.Value, txtFechaHasta.Value, tbUnidadTecnica.Text, tbEstado.Text, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dsRequerimientos", result));

                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                #region Reporte_Despachos.rdl
                if (this.reporte == "Reporte_Despachos.rdl")
                {
                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));
                    paramList.Add(new ReportParameter("NombreMn", tbNombreMn.Text));

                    //MultiValores Categoria MN
                    var catMn = (hfTipoMon.Value ?? "").Split(','); // GetCategoriaTitulosFromIds(true);
                    if (catMn.Contains("0") || catMn.Contains("[TODOS]"))
                    {
                        catMn = new String[] { "[TODOS]" };
                    }
                    var multiCategMn = new ReportParameter("CategoriaMn");
                    multiCategMn.Values.AddRange(catMn);
                    paramList.Add(multiCategMn);
                    // catMn = GetCategoriaTitulosFromIds();

                    rpReporte.LocalReport.SetParameters(paramList);
                    rpReporte.LocalReport.DataSources.Clear();
                    var result = wss.GetByFecha(txtFechaDesde.Value, txtFechaHasta.Value,
                        string.Join(",", catMn), tbNombreMn.Text);
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dsDespacho", result));
                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                #region Reporte_Requerimientos_EntregasDiarias.rdl
                if (this.reporte == "Reporte_Requerimientos_EntregasDiarias.rdl")
                {
                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));

                    //MultiValores UT
                    var uts = GetUtTitulosFromIds(titulos: true);
                    var multiUT = new ReportParameter("UnidadTecnica");
                    multiUT.Values.AddRange(uts);
                    paramList.Add(multiUT);

                    uts = GetUtTitulosFromIds();
                    rpReporte.LocalReport.SetParameters(paramList);
                    rpReporte.LocalReport.DataSources.Clear();
                    var result = wss.GetRequerimientoReporteGenerico(txtFechaDesde.Value, txtFechaHasta.Value,
                        string.Join(",", uts), "1");
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dsReq", result));
                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                #region Reporte_Requerimientos_IngresosDiarios.rdl
                if (this.reporte == "Reporte_Requerimientos_IngresosDiarios.rdl")
                {
                    //string[] ut = CrearArreglo(hfUnidadTecnica.Value, "M-Unidades técnicas");

                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));


                    rpReporte.LocalReport.SetParameters(paramList);
                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                #region Reporte_Requerimientos_DocumentosContablesDiario.rdl
                if (this.reporte == "Reporte_Requerimientos_DocumentosContablesDiario.rdl")
                {
                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));
                    rpReporte.LocalReport.SetParameters(paramList);

                    rpReporte.LocalReport.DataSources.Clear();
                    var rescon = wss.GetRequerimientosContables(txtFechaDesde.Value, txtFechaHasta.Value);
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("Lista_Requerimientos", rescon));
                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                #region Reporte_Requerimientos_EnCopia.rdl
                if (this.reporte == "Reporte_Requerimientos_EnCopia.rdl")
                {
                    string[] ut = CrearArreglo(tbUnidadTecnica.Text, "M-Unidades técnicas");

                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));

                    //MultiValores UT

                    var uts = GetUtTitulosFromIds(true, true);
                    var multiUT = new ReportParameter("UnidadTecnicaCopia");
                    multiUT.Values.AddRange(uts);
                    paramList.Add(multiUT);

                    uts = GetUtTitulosFromIds(true);
                    rpReporte.LocalReport.SetParameters(paramList);
                    rpReporte.LocalReport.DataSources.Clear();
                    var rescop = wss.GetRequerimientosUtCopia(txtFechaDesde.Value, txtFechaHasta.Value,
                        string.Join(",", uts));
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("Lista_Requerimientos", rescop));
                    rpReporte.LocalReport.Refresh();
                }
                #endregion

                #region Reporte_Requerimientos_TimbrajedePlanos.rdl
                if (this.reporte == "Reporte_Requerimientos_TimbrajedePlanos.rdl")
                {
                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));

                    //MultiValores UT
                    var uts = GetUtTitulosFromIds();
                    var multiUT = new ReportParameter("UnidadTecnica");
                    multiUT.Values.AddRange(uts);
                    paramList.Add(multiUT);

                    rpReporte.LocalReport.SetParameters(paramList);

                    rpReporte.LocalReport.DataSources.Clear();
                    var result = wss.GetRequerimientoReporteGenerico(txtFechaDesde.Value, txtFechaHasta.Value,
                        string.Join(",", uts), "0");
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dsReq", result));
                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                #region Reporte_Requerimientos_Etiquetas.rdl
                if (this.reporte == "Reporte_Requerimientos_Etiquetas.rdl")
                {
                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));

                    //MultiValores Etiqueta
                    var etiq = GetEtiquetaTitulosFromIds(true);
                    var multiEtiq = new ReportParameter("Etiqueta");
                    multiEtiq.Values.AddRange(etiq);
                    paramList.Add(multiEtiq);
                    etiq = GetEtiquetaTitulosFromIds();

                    rpReporte.LocalReport.SetParameters(paramList);

                    rpReporte.LocalReport.DataSources.Clear();
                    var result = wss.RequerimientoGetByFecha(txtFechaDesde.Value, txtFechaHasta.Value,
                        string.Join(",", etiq));
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dtRequerimientos", result));
                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                #region Reporte_Graficos_Estadisticas.rdl
                if (this.reporte == "Reporte_Graficos_Estadisticas.rdl")
                {

                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));

                    //MultiValores UT - UnidadTecnica
                    var ut = GetUtTitulosFromIds(true);
                    var multiUT = new ReportParameter("UT");
                    multiUT.Values.AddRange(ut);
                    paramList.Add(multiUT);
                    ut = GetUtTitulosFromIds(false);

                    //MultiValores Estado
                    var estados = GetEstadoTitulosFromIds(true);
                    var multiestado = new ReportParameter("Estado");
                    multiestado.Values.AddRange(estados);
                    paramList.Add(multiestado);
                    estados = GetEstadoTitulosFromIds();

                    rpReporte.LocalReport.SetParameters(paramList);
                    rpReporte.LocalReport.DataSources.Clear();
                    var resgaf = wss.GetRequerimientosFechaUtProf(txtFechaDesde.Value, txtFechaHasta.Value,
                        string.Join(",", ut), "[TODOS]", string.Join(",", estados),
                        "[TODOS]", "");
                    var resut = wss.GetUTAll();
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("Requerimientos", resgaf));
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dsUnidadTecnica", resut));
                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                #region Reporte_Log_Transaciones.rdl
                if (this.reporte == "Reporte_Log_Transacciones.rdl")
                {

                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));
                    rpReporte.LocalReport.SetParameters(paramList);

                    //var sqlstr = "SELECT * FROM dbo.LogTransacciones order by DocumentoIngreso Desc";

                    //var cmd = new SqlCommand();
                    //var cnnstring = ConfigurationManager.ConnectionStrings["GEDOC"].ConnectionString.ToString();
                    //var cn = new SqlConnection(cnnstring);
                    //cn.Open();
                    //cmd.Connection = cn;
                    //cmd.CommandText = sqlstr;
                    //var result = cmd.ExecuteReader(); 

                    DateTime fechad = Convert.ToDateTime(txtFechaDesde.Value);
                    DateTime fechah = Convert.ToDateTime(txtFechaHasta.Value);
                    var result = wss.LogTransacciones(fechad, fechah);

                    rpReporte.LocalReport.DataSources.Clear();
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dsLog", result));
                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                #region Reporte_Despachos_IniciativasCMN.rdl
                if (this.reporte == "Reporte_Despachos_IniciativasCMN.rdl")
                {

                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));
                    paramList.Add(new ReportParameter("NombreMn", tbNombreMn.Text));

                    //MultiValores Categoria MN
                    var catMn = GetCategoriaTitulosFromIds(true);
                    var multiCategMn = new ReportParameter("CategoriaMn");
                    multiCategMn.Values.AddRange(catMn);
                    paramList.Add(multiCategMn);
                    catMn = GetCategoriaTitulosFromIds();

                    rpReporte.LocalReport.SetParameters(paramList);

                    rpReporte.LocalReport.DataSources.Clear();
                    var result = wss.CMNEstado(txtFechaDesde.Value, txtFechaHasta.Value,
                        string.Join(",", catMn), tbNombreMn.Text);
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("dsDespacho", result));
                    rpReporte.LocalReport.Refresh();


                }
                #endregion

                //#region Reporte_Requerimientos_Respondidos.rdl
                //if (this.reporte == "Reporte_Requerimientos_Respondidos.rdl")
                //{
                //    string[] ut = CrearArreglo(tbUnidadTecnica.Text, "M-Unidades técnicas");
                //    //MultiValores UT - UnidadTecnica
                //    var multiUT = new ReportParameter("UnidadTecnica");
                //    multiUT.Values.AddRange(ut);
                //    paramList.Add(multiUT);

                //    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                //    paramList.Add(new ReportParameter("Fecha_Desde", txtFechaDesde.Value.ToString()));
                //    paramList.Add(new ReportParameter("Fecha_Hasta", txtFechaHasta.Value.ToString()));
                //    rpReporte.LocalReport.SetParameters(paramList);
                //    rpReporte.LocalReport.Refresh();

                //}
                //#endregion

                #region Reporte_Tabla_de_Comision.rdl
                if (this.reporte == "Reporte_Tabla_de_Comision.rdl")
                {
                    int idSesion = 0;
                    Int32.TryParse(hfSesionTabla.Value, out idSesion);
                    var tituloSesion = GetSesionTablaTituloFromId(idSesion);
                    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                    paramList.Add(new ReportParameter("Sesion", tituloSesion));
                    paramList.Add(new ReportParameter("NombreSesion", tituloSesion));

                    //MultiValores UT
                    var uts = GetUtTitulosFromIds();
                    var multiUT = new ReportParameter("UnidadTecnica");
                    multiUT.Values.AddRange(uts);
                    paramList.Add(multiUT);

                    rpReporte.LocalReport.SetParameters(paramList);
                    rpReporte.LocalReport.DataSources.Clear();
                    var rescon = wss.GetBySesion(idSesion);
                    rpReporte.LocalReport.DataSources.Add(new ReportDataSource("Lista_Requerimientos_No_Cerrados", rescon));
                    rpReporte.LocalReport.Refresh();

                }
                #endregion

                //#region Reporte_Tabla_de_Sesion.rdl
                //if (this.reporte == "Reporte_Tabla_de_Sesion.rdl")
                //{
                //    string[] ut = CrearArreglo(tbUnidadTecnica.Text, "M-Unidades técnicas");
                //    //MultiValores UT - UnidadTecnica
                //    var multiUT = new ReportParameter("UnidadTecnica");
                //    multiUT.Values.AddRange(ut);
                //    paramList.Add(multiUT);

                //    paramList.Add(new ReportParameter("Username", "Paulo Araneda"));
                //    paramList.Add(new ReportParameter("Sesion", tbSesion.Text));
                //    paramList.Add(new ReportParameter("NombreSesion", tbSesion.Text));



                //    rpReporte.LocalReport.SetParameters(paramList);
                //    rpReporte.LocalReport.Refresh();

                //}
                //#endregion


            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                rpReporte.Visible = false;
                divError.Visible = true;
                hfErrorDetail.Value = ex.Message;
                return;
            }

        }
        #endregion

        private string[] GetUtTitulosFromIds(bool enCopia = false, bool titulos = false)
        {
            var valor = enCopia ? hfUnidadTecnicaCopia.Value : hfUnidadTecnica.Value;
            var ut = valor.Split(',');
            if (("," + valor + ",").Contains(",0,") || string.IsNullOrWhiteSpace(valor))
            {
                if (!titulos)
                    ut = new[] { "[TODOS]" }; // result.Select(r => r.Titulo).ToArray();
                else
                    ut = db.UnidadTecnica.Where(q => q.Activo == true).Select(q => q.Titulo).ToArray();
            }
            else
            {
                var result = db.UnidadTecnica.Where(q => q.Activo == true & ut.Contains(q.Id.ToString())).ToList();
                ut = result.Select(q => q.Titulo).ToArray();
            }

            return ut;
        }

        private int GetUtIdFromTitulo(string titulo)
        {
            var result = db.UnidadTecnica.FirstOrDefault(q => q.Titulo == titulo);
            var ut = result?.Id ?? 0;

            return ut;
        }

        private int GetSesionTablaIdFromTitulo(string titulo)
        {
            var result = db.SesionTabla.FirstOrDefault(q => q.Nombre == titulo);
            var ut = result?.Id ?? 0;

            return ut;
        }

        private string GetSesionTablaTituloFromId(int id)
        {
            var result = db.SesionTabla.FirstOrDefault(q => q.Id == id);
            var ut = result?.Nombre ?? "";

            return ut;
        }

        private string[] GetEstadoTitulosFromIds(bool titulos = false)
        {
            var estados = hfEstado.Value.Split(',');
            if (("," + hfEstado.Value + ",").Contains(",0,") || string.IsNullOrWhiteSpace(hfEstado.Value))
            {
                if (!titulos)
                    estados = new[] { "[TODOS]" };
                else
                    estados = db.EstadoRequerimiento.Where(q => q.Activo == true).Select(r => r.Titulo).ToArray();
            }
            else
            {
                var result = db.EstadoRequerimiento.Where(q => q.Activo == true & estados.Contains(q.Id.ToString())).ToList();
                estados = result.Select(r => r.Titulo).ToArray();
            }

            return estados;
        }

        private string[] GetProfesionalNombresFromIds(bool titulo = false)
        {
            var profesional = hfProfesional.Value.Split(',');
            if (("," + hfProfesional.Value + ",").Contains(",0,") || string.IsNullOrWhiteSpace(hfProfesional.Value))
            {
                if (!titulo)
                    profesional = new[] { "[TODOS]" };
                else
                    profesional = db.Usuario.Where(q => q.Activo == true).Select(r => r.NombresApellidos).ToArray();
            }
            else
            {
                var result = db.Usuario.Where(q => q.Activo == true & profesional.Contains(q.Id.ToString())).ToList();
                profesional = result.Select(r => r.NombresApellidos).ToArray();
            }

            return profesional;
        }

        private string[] GetCategoriaTitulosFromIds(bool titulo = false)
        {
            var categoria = hfTipoMon.Value.Split(',');
            if (("," + hfTipoMon.Value + ",").Contains(",0,") || string.IsNullOrWhiteSpace(hfTipoMon.Value))
            {
                if(!titulo)
                    categoria = new[] { "[TODOS]" };
                else 
                    categoria = db.ListaValor.Where(q => q.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo && q.IdLista == (int)Mantenedor.CategoriaMn)
                        .Select(r => r.Titulo).ToArray();
            }
            else
            {
                var result = db.ListaValor
                    .Where(q => q.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo && q.IdLista == (int)Mantenedor.CategoriaMn && categoria.Contains(q.Codigo.ToString()) ).ToList();
                categoria = result.Select(r => r.Titulo).ToArray();
            }

            return categoria;
        }

        private string[] GetEtiquetaTitulosFromIds(bool titulo = false)
        {
            var etiqueta = hfEtiqueta.Value.Split(',');
            if (("," + hfEtiqueta.Value + ",").Contains(",0,") || string.IsNullOrWhiteSpace(hfEtiqueta.Value))
            {
                if(!titulo)
                    etiqueta = new[] { "[TODOS]" };
                else
                    etiqueta = db.ListaValor.Where(q => q.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo && q.IdLista == 10).Select(r => r.Titulo).ToArray();
            }
            else
            {
                var result = db.ListaValor.Where(q => q.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo && q.IdLista == 10 & etiqueta.Contains(q.Codigo.ToString())).ToList();
                etiqueta = result.Select(r => r.Titulo).ToArray();
            }

            return etiqueta;
        }

        #region CrearArreglo
        string[] CrearArreglo(string elemento, string lista)
        {
            if (elemento == "0" || elemento.Contains(",0,") ||
                elemento.StartsWith("0,") || elemento.EndsWith(",0"))
            {
                return new string[] { "[TODOS]" };
            }
            var elementos = elemento.Split(',');
            List<string> registro = new List<string>();
            //string registro = string.Empty;
            int i = 1;
            foreach (string item in elementos)
            {

                //var result = DataUtils.GetDatosRegistroById(Convert.ToInt32(item), lista)[0];
                //var result =  {  };
                //if (result != null)
                //{
                //    registro.Add(result[0].ToString());
                //}

                i++;
            }


            //registro = registro.Substring(0, registro.Length - 1);

            return registro.ToArray();
        }
        #endregion

        #region HabilitarParametros
        void HabilitarParametros(string[] datos)
        {

            foreach (string item in datos)
            {
                switch (item.ToUpper())
                {
                    case "FECHA_DESDE":
                        txtFechaDesde.Visible = true;
                        tdFechaDesde.Visible = true;
                        tdfFechaDesde.Visible = true;
                        break;
                    case "FECHA_HASTA":
                        txtFechaHasta.Visible = true;
                        tdfFechaHasta.Visible = true;
                        tdFechaHasta.Visible = true;
                        txtMensajeFecha.Visible = true;
                        break;
                    case "UT":
                    case "UNIDADTECNICA":
                        tbUnidadTecnica.Visible = true;
                        tdUnidad.Visible = true;
                        tdfUnidad.Visible = true;
                        break;
                    case "UNIDADTECNICACOPIA":
                        tbUnidadTecnicaCopia.Visible = true;
                        tdUnidadCopia.Visible = true;
                        tdfUnidadCopia.Visible = true;
                        break;
                    case "PROFESIONAL":
                        tbProfesionalArea.Visible = true;
                        tdProfesional.Visible = true;
                        tdfProfesional.Visible = true;
                        break;
                    case "ESTADO":
                        tbEstado.Visible = true;
                        tdEstado.Visible = true;
                        tdfEstado.Visible = true;
                        break;
                    case "ETAPA":
                        tbEtapa.Visible = true;
                        tdEtapa.Visible = true;
                        tdfEstado.Visible = true;
                        break;
                    case "ETIQUETA":
                        tbEtiqueta.Visible = true;
                        tdEtiqueta.Visible = true;
                        tdfEtiqueta.Visible = true;
                        break;
                    case "REMITENTE":
                        tbEtiqueta.Visible = true;
                        tdRemitente.Visible = true;
                        tdfRemitente.Visible = true;
                        break;
                    case "DESTINATARIO":
                        tbDestinatario.Visible = true;
                        tdDestinatario.Visible = true;
                        tdfDestinatario.Visible = true;
                        break;
                    case "REQUERIMIENTO":
                        tbRequerimiento.Visible = true;
                        tdReq.Visible = true;
                        tdfReq.Visible = true;
                        break;
                    case "SESION":
                        tbSesion.Visible = true;
                        tdSesion.Visible = true;
                        tdfSesion.Visible = true;
                        break;
                    case "NOMBREMN":
                    case "CATEGORIAMN":
                        rowTitulosFiltrosMn.Visible = true;
                        rowFiltrosMn.Visible = true;
                        break;
                }
            }


        }
        #endregion

        #region GetReportes
        void GetReportes(int idreporte)
        {
            try
            {
                //string strUrl = SPContext.Current.Web.Url;
                //string tituloReporteActual = "";
                ////SPSecurity.RunWithElevatedPrivileges((SPSecurity.CodeToRunElevated)(() =>
                //{
                //using (SPSite site = new SPSite(strUrl))
                //{
                //    using (SPWeb web = site.OpenWeb())
                //    {
                //        SPList list = web.Lists["Informe"];
                //        SPQuery myquery = new SPQuery();

                //        string query = "<Query>"
                //                       + "<OrderBy>"
                //                       + "<FieldRef Name='Title'  Ascending='True' />"
                //                       + "<FieldRef Name='FileLeafRef'  Ascending='True' />"
                //                       + "</OrderBy>"
                //                       + "</Query>";

                //        myquery.Query = query;

                //SPListItemCollection items = list.GetItems(myquery);

                //var isEjecutivaIngreso = DataUtils.IsAdminReportes(SPContext.Current.Web.CurrentUser.LoginName);
                //var isadminReporte = DataUtils.IsAdminGEDOC(SPContext.Current.Web.CurrentUser.LoginName);

                if (idreporte == 12 || idreporte == 13 || idreporte == 14)
                {
                    lbTituloReporte.Text = "Código de reporte incorrecto";
                    Deshabilitar();
                    return;
                }

                var isEjecutivaIngreso = true;
                var isadminReporte = true;
                var lstResporte = db.Reporte;
                foreach (var item in lstResporte)
                {
                    if (item.NombreReporte != "Reporte_Requerimientos_GrupoAuditoria.rdl" &&
                    // item.File.Name.ToLower() != "reporte_procesosmasivos.rdl" &&
                    (item.NombreReporte != "Reporte_Requerimientos_DocumentosContablesDiario.rdl" ||
                     (item.NombreReporte == "Reporte_Requerimientos_DocumentosContablesDiario.rdl" && isEjecutivaIngreso)) &&
                    (item.NombreReporte != "Reporte_Log_Transacciones.rdl" ||
                     (item.NombreReporte == "Reporte_Log_Transacciones.rdl" && isadminReporte)))
                    {
                        Reportes.Add(new Reporte
                        {
                            Id = item.Id,
                            Nombre = item.Nombre,
                            NombreReporte = item.NombreReporte
                        });
                        //tituloReporteActual = !string.IsNullOrEmpty(reporte) && item.File.Name == reporte ? item.Title : tituloReporteActual;

                    }

                }

                #region Ir Reporte Directamente
                if (idreporte > 0)
                {

                    var regreport = db.Reporte.SingleOrDefault(q => q.Id == idreporte);

                    rpReporte.Visible = false;
                    //btnVerReporte.Visible = true;
                    btnVer.Visible = true;
                    pnDocumento.Visible = true;
                    lbTituloReporte.Text = regreport.Nombre;
                    hfReporte.Value = regreport.NombreReporte;
                    Deshabilitar();
                    Crear(idreporte);
                }
                //pnReportes.Visible = string.IsNullOrEmpty(reporte);
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

        }
        #endregion

        #region Deshabilitar
        void Deshabilitar()
        {
            #region Campos
            txtFechaDesde.Visible = false;
            tdFechaDesde.Visible = false;
            tdfFechaDesde.Visible = false;
            txtFechaHasta.Visible = false;
            tdfFechaHasta.Visible = false;
            tdFechaHasta.Visible = false;
            tbUnidadTecnica.Visible = false;
            tdUnidad.Visible = false;
            tdfUnidad.Visible = false;
            tbUnidadTecnicaCopia.Visible = false;
            tdUnidadCopia.Visible = false;
            tdfUnidadCopia.Visible = false;
            tbProfesionalArea.Visible = false;
            tdProfesional.Visible = false;
            tdfProfesional.Visible = false;
            tbEstado.Visible = false;
            tdEstado.Visible = false;
            tdfEstado.Visible = false;
            tbEtapa.Visible = false;
            tdEtapa.Visible = false;
            tdfEstado.Visible = false;
            tbEtiqueta.Visible = false;
            tdEtiqueta.Visible = false;
            tdfEtiqueta.Visible = false;
            tbEtiqueta.Visible = false;
            tdRemitente.Visible = false;
            tdfRemitente.Visible = false;
            tbDestinatario.Visible = false;
            tdDestinatario.Visible = false;
            tdfDestinatario.Visible = false;
            tbSesion.Visible = false;
            tdSesion.Visible = false;
            tdfSesion.Visible = false;
            rowTitulosFiltrosMn.Visible = false;
            rowFiltrosMn.Visible = false;
            #endregion
        }
        #endregion



        bool FechaErronea(DateTime fecha)
        {
            var fechatext = fecha.ToString();
            if (fechatext == "01/01/01")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        protected void btnVerReporte_Click(object sender, EventArgs e)
        {
            //tbal.Value = string.Empty;
            RenderReport();
        }

        protected void rpReporte_ReportError(object sender, ReportErrorEventArgs e)
        {

            e.Handled = true;
            //rpReporte.Visible = false;
            //LoggingService.LogError(e.Exception);
        }

        protected void btnVer_Click(object sender, EventArgs e)
        {
            RenderReport();

        }
    }
}