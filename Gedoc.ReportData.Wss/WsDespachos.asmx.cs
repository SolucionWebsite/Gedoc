using System;
using System.Collections.Generic;
using System.Web.Services;
using Gedoc.ReportData.Wss.Data;
using Gedoc.ReportData.Wss.Logging;

namespace Gedoc.ReportData.Wss
{
    /// <summary>
    /// Summary description for WsDespachos
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WsDespachos : System.Web.Services.WebService
    {

        [WebMethod]
        public List<DespachoBL> GetByFecha(string fechadesde, string fechahasta, string categoriaMn, string nombreMn)
        {
            DateTime fechad = Convert.ToDateTime(fechadesde);
            DateTime fechah = Convert.ToDateTime(fechahasta);

            var result = DataUtil.GetByFecha(fechad, fechah, categoriaMn, nombreMn);
            return result;

        }

        [WebMethod]
        public List<RequerimientoBL> RequerimientoGetByFecha(string fechadesde, string fechahasta, string etiqueta)
        {
            DateTime fechad = Convert.ToDateTime(fechadesde);
            DateTime fechah = Convert.ToDateTime(fechahasta);


            var result = DataUtil.RequerimientosGetByFecha(fechad, fechah, etiqueta);
            return result;
        }


        [WebMethod]
        public List<RequerimientoBL> GetBySesion(int id)
        {

            var result = DataUtil.GetBySesion(id);
            return result;

        }

        [WebMethod]
        public List<SessionBL> GetUTByUser(string name)
        {
            return DataUtil.GetByUTByUser(name);
        }

        [WebMethod]
        public List<SessionBL> GetUTAll()
        {
            return DataUtil.GetUTAll();
        }


        [WebMethod]
        public List<SessionBL> GetSessionByUT(int idut)
        {
            return DataUtil.GetSessionByUT(idut);
        }

        [WebMethod]
        public List<RequerimientoBL> GetRequerimientosFechaUtProf(string fechadesde, string fechahasta, string ut, string profesional, string estado,
            string categoriaMn, string nombreMn)
        {
            DateTime fechad = Convert.ToDateTime(fechadesde);
            DateTime fechah = Convert.ToDateTime(fechahasta);
            var result = DataUtil.RequerimientosGetByFechaUTProf(fechad, fechah, ut, profesional, estado, categoriaMn, nombreMn);
            return result;
        }

        [WebMethod]
        public List<RequerimientoBL> GetRequerimientosContables(string fechadesde, string fechahasta)
        {
            DateTime fechad = Convert.ToDateTime(fechadesde);
            DateTime fechah = Convert.ToDateTime(fechahasta);
            var result = DataUtil.RequerimientosContables(fechad, fechah);
            return result;
        }

        [WebMethod]
        public List<RequerimientoBL> GetRequerimientosUtCopia(string fechadesde, string fechahasta, string UtsCopia)
        {
            DateTime fechad = Convert.ToDateTime(fechadesde);
            DateTime fechah = Convert.ToDateTime(fechahasta);
            var result = DataUtil.RequerimientosUtCopia(fechad, fechah, UtsCopia);
            return result;
        }


        [WebMethod]
        public List<RequerimientoBL> GetRequerimientoReporteGenerico(string fechad,string fechah, string ut,string tipo)
        {
            var result = DataUtil.GetRequerimientoReporteGenerico(fechad, fechah, ut, tipo);
            return result;
        }

        [WebMethod]
        public List<RequerimientoBL> GetRequerimientosFechaUtGrupo(string fechadesde, string fechahasta, string ut, string estado,
            string prioridad, string nombreproyectoprograma, string instRemitente, string etiqueta, string region, string comuna,
            string mnInvolucrado, string profesionalUt, string utCopia, string requiereRespuesta, string motivoCierre, string requiereTimbraje,
            string requiereAcuerdo, string despachoInicSiNo, string despachoSiNo, string instDestinatario, string tipoInstRemitente,
            string tipoInstDestinatario, string semaforo, string fechaResolucionDesde, string fechaResolucionHasta,
            string relacionDesdeHasta)
        {
            DateTime fechad = string.IsNullOrEmpty(fechadesde) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(fechadesde);
            DateTime fechah = string.IsNullOrEmpty(fechahasta) ? new DateTime(2099, 1, 1) : Convert.ToDateTime(fechahasta);
            DateTime fechaResD = string.IsNullOrEmpty(fechaResolucionDesde) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(fechaResolucionDesde);
            DateTime fechaResH = string.IsNullOrEmpty(fechaResolucionHasta) ? new DateTime(2099, 1, 1) : Convert.ToDateTime(fechaResolucionHasta);
            var result = DataUtil.RequerimientosGetByFechaUtGrupo(fechad, fechah, ut, estado,
                prioridad, nombreproyectoprograma, instRemitente, etiqueta, region, comuna,
                mnInvolucrado, profesionalUt, utCopia, requiereRespuesta, motivoCierre, requiereTimbraje,
                requiereAcuerdo, despachoInicSiNo, despachoSiNo, instDestinatario, tipoInstRemitente,
                tipoInstDestinatario, semaforo, fechaResD, fechaResH, (relacionDesdeHasta ?? "").Split(';'));
            return result;
        }

        [WebMethod]
        public List<ResumenGrupo> GetResumenGrupo(string fechadesde, string fechahasta, string ut, string estado,
            string prioridad, string nombreproyectoprograma, string instRemitente, string etiqueta, string region, string comuna,
            string mnInvolucrado, string profesionalUt, string utCopia, string requiereRespuesta, string motivoCierre, string requiereTimbraje,
            string requiereAcuerdo, string despachoInicSiNo, string despachoSiNo, string instDestinatario, string tipoInstRemitente,
            string tipoInstDestinatario, string semaforo, string fechaResolucionDesde, string fechaResolucionHasta,
            string relacionDesdeHasta)
        {
            //if (relacionDesdeHasta == null || relacionDesdeHasta.Length == 0)
            {
                return new List<ResumenGrupo>();
            }
            //DateTime fechad = string.IsNullOrEmpty(fechadesde) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(fechadesde);
            //DateTime fechah = string.IsNullOrEmpty(fechahasta) ? new DateTime(2099, 1, 1) : Convert.ToDateTime(fechahasta);
            //DateTime fechaResD = string.IsNullOrEmpty(fechaResolucionDesde) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(fechaResolucionDesde);
            //DateTime fechaResH = string.IsNullOrEmpty(fechaResolucionHasta) ? new DateTime(2099, 1, 1) : Convert.ToDateTime(fechaResolucionHasta);
            //var result = DataUtil.ResumenGrupo(fechad, fechah, ut, estado,
            //    prioridad, nombreproyectoprograma, instRemitente, etiqueta, region, comuna,
            //    mnInvolucrado, profesionalUt, utCopia, requiereRespuesta, motivoCierre, requiereTimbraje,
            //    requiereAcuerdo, despachoInicSiNo, despachoSiNo, instDestinatario, tipoInstRemitente,
            //    tipoInstDestinatario, semaforo, fechaResD, fechaResH, relacionDesdeHasta.Split(';'));
            //return result;
        }

        [WebMethod]
        public List<DespachoBL> CMNEstado(string fechadesde, string fechahasta, string categoriaMn, string nombreMn)
        {
            DateTime fechad = Convert.ToDateTime(fechadesde);
            DateTime fechah = Convert.ToDateTime(fechahasta);
            var result = DataUtil.CMNGetByFecha(fechad,fechah,  categoriaMn,  nombreMn);
            return result;
        }

        [WebMethod]
        public List<RequerimientoBL> GetProcesosMasivos(string fechadesde, string fechahasta, string ut, string profesional, string estado,
            string etapa, string etiqueta, string tipomon, string region, string accionpm)
        {
            DateTime fechad = string.IsNullOrEmpty(fechadesde) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(fechadesde);
            DateTime fechah = string.IsNullOrEmpty(fechahasta) ? new DateTime(2099, 1, 1) : Convert.ToDateTime(fechahasta);

            var result = new List<RequerimientoBL>();
            result = DataUtil.GetProcesosMasivos(fechad, fechah, ut ?? "", profesional ?? "", estado ?? "",
             etapa ?? "",  etiqueta ?? "",  tipomon ?? "",  region ?? "",  accionpm ?? "");

            
            return result;
        }

        [WebMethod]
        public List<DatosSearch> GetDatosSearch(string tipoSearch)
        {
            var result = DataUtil.GetDatosSearch(tipoSearch);
            return result;

        }

        [WebMethod]
        public List<RequerimientoBL> GetRequerimientosTramites(string fechadesde, string fechahasta,
            string tipotramite, string canalllegada, string numerocaso, string nobrecaso,
            string documentoingreso, string estado, string etapa, string remitente,
            string institucionremitente, string categoriamonumento, string denominacionoficial, string direccionmonumento, string region,
            string provincia, string comuna, string materia, string etiqueta, string unidadtecnica, string profesionalut)
        {
            DateTime fechad = string.IsNullOrEmpty(fechadesde) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(fechadesde);
            DateTime fechah = string.IsNullOrEmpty(fechahasta) ? new DateTime(2099, 1, 1) : Convert.ToDateTime(fechahasta);
            var result = DataUtil.RequerimientosTramites(fechad, fechah,
            tipotramite, canalllegada, numerocaso, nobrecaso,
            documentoingreso, estado, etapa, remitente,
            institucionremitente, categoriamonumento, denominacionoficial, direccionmonumento, region,
            provincia, comuna, materia, etiqueta, unidadtecnica, profesionalut);
            return result;
        }

        [WebMethod]
        public List<LogTransacciones> LogTransacciones(DateTime fechaD, DateTime fechaH)
        {
            var result = DataUtil.LogTransacciones(fechaD, fechaH);
            return result;

        }


    }
}
