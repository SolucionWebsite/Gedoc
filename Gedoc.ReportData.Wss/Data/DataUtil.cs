using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using Gedoc.ReportData.Wss.Linq;
using Gedoc.ReportData.Wss.Logging;
using Gedoc.ReportData.Wss.Repository;

namespace Gedoc.ReportData.Wss.Data
{

    public static class DataUtil
    {
        //private static NetworkCredential networkCredential = null;
        //private static NetworkCredential networkCredentialHist = null;
        //private static string SP_URL = ConfigurationManager.AppSettings["URLSharepoint"];
        //private static string SP_User = ConfigurationManager.AppSettings["SPUsuario"];
        //private static string SP_Pass = ConfigurationManager.AppSettings["SPClave"];
        //private static string SP_Domain = ConfigurationManager.AppSettings["SPDominio"];
        //private static string SP_URL_Hist = ConfigurationManager.AppSettings["SP.Historial.URL"];
        //private static string SP_User_Hist = ConfigurationManager.AppSettings["SP.Historial.User"];
        //private static string SP_Pass_Hist = ConfigurationManager.AppSettings["SP.Historial.Pass"];
        //private static string SP_Domain_Hist = ConfigurationManager.AppSettings["SP.Historial.Domain"];


        //public DataUtil()
        //{
        //    SP_URL = ConfigurationManager.AppSettings["SP_URL"];
        //    SP_User = ConfigurationManager.AppSettings["SP_User"];
        //    SP_Pass = ConfigurationManager.AppSettings["SP_Pass"];
        //    SP_Domain = ConfigurationManager.AppSettings["SP_Domain"];

        //}

        #region getUnidadTecnica
        private static Generico getUnidadTecnica(int id)
        {
            try
            {
                var genericRepo = new GenericRepo();
                var query = "SELECT ID as Id, Title as Titulo " +
                            " FROM UnidadesTecnica req " +
                            " WHERE ID=@idUt";
                var datos = genericRepo.ExecuteQuery<Generico>(query, parametros: new { idUt = id });
                return datos != null && datos.Count > 0 ? datos[0] : null;
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return null;
            }
        }
        #endregion


        #region getRequerimientosByDocsIngreso
        private static List<dynamic> GetRequerimientosByDocsIngresoBd(List<string> docsIng)
        {
            if (docsIng == null || docsIng.Count == 0)
                return null;

            var genericRepo = new GenericRepo();
            var query = "SELECT req.*, ca.Title as NombreCaso, ca.Fecha_x0020_Referencia_x0020_del as FechaCaso " +
                " FROM Requerimientos req " +
                "   left join casos ca " +
                "       ON req.N_x00fa_mero_x0020_de_x0020_caso = ca.Title" +
                " WHERE Documento_x0020_de_x0020_ingreso IN @docsIng";
            var datos = genericRepo.ExecuteQuery(query, parametros: new { docsIng = docsIng.ToArray() });
            return datos;
        }

        private static List<RequerimientoBL> GetRequerimientosByLogBd(string accionLogMulti, string formAccionSimple, string accionLogSimple, DateTime fechadesde, DateTime fechahasta, string ut,
                                string profesional, string estado, string etapa, string etiqueta, string tipomon, string region, int incluyeDesp)
        {
            var genericRepo = new GenericRepo();
            var data = genericRepo.ExecuteSP<RequerimientoBL>("SpRequerimientosLog",
                new
                {
                    incluyeDesp = incluyeDesp,
                    accionLogMulti = accionLogMulti,
                    accionLogSimple = accionLogSimple,
                    formAccionSimple = formAccionSimple,
                    fechadesde = fechadesde,
                    fechahasta = fechahasta,
                    ut = ut,
                    profesional = profesional,
                    estado = estado,
                    etapa = etapa,
                    etiqueta = etiqueta,
                    tipomon = tipomon,
                    region = region
                });


            return data;
        }
        #endregion

        #region GetBySesion
        public static List<RequerimientoBL> GetBySesion(int id)
        {
            var genericRepo = new GenericRepo();
            var data = genericRepo.ExecuteSP<RequerimientoBL>("SpRequerimientosPorsesion",
                new
                {
                    sesionId = id
                });


            return data;

            //var lstReq = new List<RequerimientoBL>();
            //using (var cn = new GEDOCSQLDataContext())
            //{
            //    var docsIng = cn.TB_SesionTablaDetalles.Where(q => q.Sesion_Id == id).Select(r => r.DocumentoIngreso).ToList();
            //    var reqs = GetRequerimientosByDocsIngresoBd(docsIng);
            //    if (reqs == null)
            //        return lstReq;

            //    var despachos = GetDespachoMultiBd(docsIng, false);
            //    var bitacoras = GetBitacoraMultiBd(docsIng, false);

            //    foreach (var req in reqs)
            //    {
            //        //var reqList = GetByRequerimiento(item.DocumentoIngreso); // getReqByID(item.Requerimento_Id);
            //        //if (reqList == null || reqList.Count == 0) return new List<RequerimientoBL>();
            //        var requerimiento = new RequerimientoBL();
            //        //var req = reqList[0];
            //        requerimiento.DocumentoIngreso = req.Documento_x0020_de_x0020_ingreso ?? "";
            //        requerimiento.FechaIngreso = req.Fecha_x0020_de_x0020_ingreso == null ? new DateTime() : Convert.ToDateTime(req.Fecha_x0020_de_x0020_ingreso);
            //        requerimiento.FechaDocumento = req.Fecha_x0020_de_x0020_documento == null ? new DateTime() : Convert.ToDateTime(req.Fecha_x0020_de_x0020_documento);
            //        requerimiento.Estado = req.Estado_x0020_del_x0020_requerimi ?? "";
            //        requerimiento.Etapa = req.Etapa ?? "";
            //        requerimiento.Materia = req.Materia ?? "";
            //        requerimiento.MonumentoNacionalinvolucrado = req.Monumento_x0020_Nacional_x0020_i ?? "";
            //        requerimiento.DirMonumentoNacionalinvolucrado = req.Direcci_x00f3_n_x0020_Monumento_ ?? "";
            //        requerimiento.NombreProgramaProyecto = req.Nombre_x0020_de_x0020_proyecto ?? "";
            //        requerimiento.ProyectoActividad = req.Descripci_x00f3_n_x0020_de_x0020 ?? "";
            //        requerimiento.NombreComuna = req.Nombre_x0020_Comuna ?? "";
            //        requerimiento.NombreRegion = LimpiaDuplicados(req.Nombre_x0020_Comuna_x003A_Nombre1 ?? "", ';');
            //        requerimiento.ObservacionesTipoDocumento = req.Observaciones_x0020_al_x0020_tip ?? "";
            //        requerimiento.ProfesionalAsignado = req.Responsable_x0020_del_x0020_requ ?? "";
            //        requerimiento.RemitenteCargoProfesión = req.Cargo_x0020_o_x0020_Profesi_x00f ?? "";
            //        requerimiento.RemitenteInstitucion = req.Remitente_x003a_Instituci_x00f3_ ?? "";
            //        requerimiento.RemitenteNombre = req.Remitente_x003A_T_x00ed_tulo ?? "";
            //        requerimiento.TipoMonumento = req.Tipo_x0020_de_x0020_monumento ?? "";
            //        requerimiento.ultima_fecha_acuerdo_comisión = req.Fecha_x0020_de_x0020_acuerdo_x00 == null ? "" : ((DateTime)req.Fecha_x0020_de_x0020_acuerdo_x00).ToString("dd/MM/yyyy hh:mm");
            //        requerimiento.UT = req.Unidad_x0020_T_x00e9_cnica_x0020 ?? "";

            //        requerimiento.TipoAdjuntos = req.Tipo_x0020_de_x0020_documento ?? "";
            //        requerimiento.ObservacionTipoAdjunto = req.Observaciones_x0020_de_x0020_adj ?? "";
            //        requerimiento.TipoDocumento = req.Tipos_x0020_de_x0020_adjuntos ?? "";

            //        requerimiento.TipoTramite = req.Tipo_x0020_de_x0020_tr_x00e1_mit ?? "";
            //        requerimiento.CanalLlegadaTramite = req.Canal_x0020_de_x0020_llegada_x00 ?? "";
            //        requerimiento.CategoriaMonNac = req.Tipo_x0020_de_x0020_monumento ?? "";
            //        requerimiento.CodigoMonNac = req.C_x00f3_digo_x0020_Monumento_x00 ?? "";
            //        requerimiento.DenominacionOf = req.Monumento_x0020_Nacional_x0020_i ?? "";
            //        requerimiento.OtrasDenominaciones = req.Otras_x0020_denominaciones ?? "";
            //        requerimiento.NombreUsoActual = req.Nombre_x0020_o_x0020_uso_x0020_a ?? "";
            //        requerimiento.DireccionMonNac = req.Direcci_x00f3_n_x0020_Monumento_ ?? "";
            //        requerimiento.ReferenciaLocalidad = req.Referencia_x0020_de_x0020_locali ?? "";
            //        requerimiento.Provincia = req.Provincia ?? "";
            //        requerimiento.Rol = req.Rol_x0020_MN ?? "";
            //        requerimiento.NumeroCaso = req.N_x00fa_mero_x0020_de_x0020_caso ?? "";
            //        requerimiento.NombreCaso = req.NombreCaso ?? "";
            //        requerimiento.FechaReferenciaCaso = req.FechaCaso == null ? "" : ((DateTime)req.FechaCaso).ToString("dd/MM/yyyy");

            //        #region Despachos del requerimiento
            //        var despachosReq = requerimiento.DocumentoIngreso != null && despachos.ContainsKey(requerimiento.DocumentoIngreso) ? despachos[requerimiento.DocumentoIngreso] : new List<DespachoBL>(0);// GetDespacho(requerimiento.DocumentoIngreso);

            //        if (despachosReq.Count > 0)
            //        { // Requerimiento con despachos
            //            var _despachos = despachosReq.Where(q => q.DocumentoIngreso == requerimiento.DocumentoIngreso).OrderByDescending(o => o.FechaEmisionOficio).FirstOrDefault();
            //            requerimiento.ultima_fecha_oficio = _despachos == null ? "" : CorregirFecha(_despachos.FechaEmisionOficio.GetValueOrDefault());
            //            requerimiento.FechaEmisionUltimoOficio = _despachos == null ? (DateTime?)null : CorregirFechaDateTime(_despachos.FechaEmisionOficio.GetValueOrDefault());
            //            requerimiento.NumeroDespacho = _despachos == null ? "" : _despachos.NumeroDespacho;
            //            requerimiento.MateriaDesp = _despachos == null ? "" : _despachos.MateriaDespacho;
            //        }
            //        else
            //        { // Requerimiento sin despachos
            //            requerimiento.ultima_fecha_oficio = "";
            //        }
            //        #endregion

            //        #region Bitacoras del requerimiento
            //        var sesionycomision = requerimiento.DocumentoIngreso != null && bitacoras.ContainsKey(requerimiento.DocumentoIngreso) ? bitacoras[requerimiento.DocumentoIngreso] : new List<BitacoraBL>(0); // GetBitacora(requerimiento.DocumentoIngreso);

            //        if (sesionycomision.Count > 0)
            //        {
            //            var _sesion = sesionycomision.Where(q => q.TipoBitacora == "Acuerdo Sesión").OrderByDescending(q => q.FechaBitacora).FirstOrDefault();
            //            var _comision = sesionycomision.Where(q => q.TipoBitacora == "Acuerdo Comisión").OrderByDescending(q => q.FechaBitacora).FirstOrDefault();
            //            var _respuesta = sesionycomision.Where(q => q.TipoBitacora == "Respuesta").OrderByDescending(q => q.FechaBitacora).FirstOrDefault();

            //            requerimiento.ultima_fecha_acuerdo_sesion = _sesion == null ? "" : CorregirFecha(_sesion.FechaBitacora);
            //            requerimiento.ultimo_acuerdo_sesion = _sesion == null ? "" : _sesion.Observacion;
            //            requerimiento.ultima_fecha_acuerdo_comisión = _comision == null ? "" : CorregirFecha(_comision.FechaBitacora);
            //            requerimiento.ultimo_acuerdo_comision = _comision == null ? "" : _comision.Observacion;
            //            requerimiento.ultima_fecha_respuesta = _respuesta == null ? "" : CorregirFecha(_respuesta.FechaBitacora);
            //            requerimiento.FechaUltimoAcuerdoSesion = _sesion == null ? (DateTime?)null : CorregirFechaDateTime(_sesion.FechaBitacora);
            //            requerimiento.FechaUltimoAcuerdoComision = _comision == null ? (DateTime?)null : CorregirFechaDateTime(_comision.FechaBitacora);
            //            requerimiento.FechaUltimaRespuesta = _respuesta == null ? (DateTime?)null : CorregirFechaDateTime(_respuesta.FechaBitacora);
            //        }
            //        else
            //        {
            //            requerimiento.ultima_fecha_acuerdo_sesion = "";
            //            requerimiento.ultimo_acuerdo_sesion = "";
            //            requerimiento.ultima_fecha_acuerdo_comisión = "";
            //            requerimiento.ultimo_acuerdo_comision = "";
            //            requerimiento.ultima_fecha_respuesta = "";
            //        }
            //        #endregion


            //        lstReq.Add(requerimiento);
            //    }

            //    return lstReq;
            //}


        }
        #endregion

        #region GetBitacoraMulti

        private static Dictionary<string, List<BitacoraBL>> GetBitacoraMultiBd(List<string> documentosIngreso, bool esHistorico)
        {
            var bitacoras = new Dictionary<string, List<BitacoraBL>>();

            if (documentosIngreso == null || documentosIngreso.Count == 0)
                return new Dictionary<string, List<BitacoraBL>>();

            try
            {
                var genericRepo = new GenericRepo();
                var query = "SELECT ID as IdBitacora, Requerimiento as DocumentoIngreso, ContentType as TipoBitacora, _x00da_ltimo_x0020_comentario as Observacion, Fecha as FechaBitacora" +
                            " FROM Bitacoras req " +
                            " WHERE Requerimiento IN @docIng";
                var datos = genericRepo.ExecuteQuery<BitacoraBL>(query, parametros: new { docIng = documentosIngreso });
                foreach (var bita in datos)
                {
                    if (!bitacoras.ContainsKey(bita.DocumentoIngreso))
                    {
                        bitacoras.Add(bita.DocumentoIngreso, new List<BitacoraBL>()
                        {
                            bita
                        });
                    }
                    else
                    {
                        bitacoras[bita.DocumentoIngreso].Add(bita);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return null;
            }

            return bitacoras;

        }
        #endregion

        #region IsDate
        public static bool IsDate(object Expression)
        {
            if (Expression != null)
            {
                if (Expression is DateTime)
                {
                    return true;
                }
                if (Expression is string)
                {
                    DateTime time1;
                    return DateTime.TryParse((string)Expression, out time1);
                }
            }
            return false;
        }

        public static DateTime ValidarFecha(string dato)
        {
            if (IsDate(dato))
            {
                return Convert.ToDateTime(dato);
            }
            return Convert.ToDateTime("01/01/1999");
        }

        #endregion

        #region RequerimientosGetByFecha
        public static List<RequerimientoBL> RequerimientosGetByFecha(DateTime fechadesde, DateTime fechahasta, string etiqueta)
        {

            var genericRepo = new GenericRepo();
            var data = genericRepo.ExecuteSP<RequerimientoBL>("SpRequerimientosPorFecha",
                new
                {
                    tipoReporte = 8,
                    fechaDesde = fechadesde,
                    fechaHasta = fechahasta,
                    ut = "",
                    profesionalUt = "",
                    estado = "",
                    etiqueta = string.IsNullOrEmpty(etiqueta) ? "[TODOS]" : etiqueta,
                    categoriaMn = "",
                    nombreMn = ""
                });


            return data;
        }


        #endregion
        
        #region GetDespachoMulti
        public static Dictionary<string, List<DespachoBL>> GetDespachoMultiBd(List<string> documentosIngreso, bool esHistorico)
        {
            var despachos = new Dictionary<string, List<DespachoBL>>();

            if (documentosIngreso == null || documentosIngreso.Count == 0)
                return new Dictionary<string, List<DespachoBL>>();

            try
            {
                var genericRepo = new GenericRepo();
                var query = "SELECT asoc.Documento_x0020_de_x0020_ingreso as DocumentoIngreso," +
                            " desp.Fecha_x0020_emisi_x00f3_n_x0020_oficio as FechaEmisionOficio," +
                            " desp.Fecha_x0020_de_x0020_recepci_x00f3_n as FechaRecepcion," +
                            " desp.Fecha_x0020_emisi_x00f3_n_x0020_oficio as FechaDespacho," +
                            " desp.Tipo_x0020_de_x0020_Instituci_x00f3_n_x0020_destinatario_x0020_nuevo as TipoInstitucionDestinario," +
                            " desp.Destinatario as Destinatario," +
                            " req.Etiqueta as Etiqueta," +
                            "  desp.Materia_x0020_de_x0020_despacho as MateriaDespacho," +
                            "  desp.Proveedor_x0020_de_x0020_despacho as ProveedorDespacho," +
                            "  desp.Instituci_x00f3_n_x0020_destinatario_x0020_nuevo as InstitucionDestinario," +
                            "  req.Responsable_x0020_del_x0020_requ as ProfesionalAsignado," +
                            " desp.Tipos_x0020_de_x0020_adjuntos as TipoAdjunto," +
                            "  desp.Destinatarios_x0020_en_x0020_copia as DestinatarioCopia," +
                            " desp.Estado_x0020_del_x0020_despacho as Estado," +
                            " desp.FileLeafRef as Nombre," +
                            " desp.Cantidad_x0020_de_x0020_adjuntos as Adjuntos," +
                            " desp.N_x00fa_mero_x0020_de_x0020_despacho as Folio," +
                            "  desp.FileLeafRef as NumeroDespacho," +
                            "  desp.Estado_x0020_del_x0020_despacho as Estado" +
                            " FROM Despachos desp" +
                            "  JOIN AsocDespachosReq asoc" +
                            "    ON desp.ID = asoc.ID_Despacho" +
                            "  JOIN Requerimientos req" +
                            "    ON req.Documento_x0020_de_x0020_ingreso = asoc.Documento_x0020_de_x0020_ingreso" +
                            "  WHERE asoc.Documento_x0020_de_x0020_ingreso IN @docIng";
                var datos = genericRepo.ExecuteQuery<DespachoBL>(query, parametros: new { docIng = documentosIngreso });
                foreach (var regdespacho in datos)
                {
                    if (!despachos.ContainsKey(regdespacho.DocumentoIngreso))
                    {
                        despachos.Add(regdespacho.DocumentoIngreso, new List<DespachoBL>());
                    }
                    despachos[regdespacho.DocumentoIngreso].Add(regdespacho);

                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return null;
            }

            return despachos;

        }
        #endregion

        #region CorregirFecha
        static string CorregirFecha(DateTime fecha)
        {
            if (fecha.ToString("dd/MM/yyyy") == "01/01/0001" || fecha.ToString("dd/MM/yyyy") == "01-01-0001")
            {
                return "";
            }
            else
            {
                return fecha.ToString("dd/MM/yyyy").Replace("-", "/");
            }

        }
        #endregion

        #region CorregirFechaDateTime
        static DateTime? CorregirFechaDateTime(DateTime fecha)
        {
            if (fecha.ToString("dd/MM/yyyy") == "01/01/0001" || fecha.ToString("dd/MM/yyyy") == "01-01-0001")
            {
                return null;
            }
            else
            {
                return fecha;
            }

        }
        #endregion

        #region RequerimientoGetByFechaUTProf
        public static List<RequerimientoBL> RequerimientosGetByFechaUTProf(DateTime fechadesde, DateTime fechahasta, string ut, string profesional, string estado,
            string categoriaMn, string nombreMn)
        {

            var genericRepo = new GenericRepo();
            var data = genericRepo.ExecuteSP<RequerimientoBL>("SpRequerimientosPorFecha",
                new
                {
                    tipoReporte = 3,
                    fechaDesde = fechadesde,
                    fechaHasta = fechahasta,
                    ut,
                    profesionalUt = profesional,
                    estado,
                    etiqueta = "",
                    categoriaMn = string.IsNullOrEmpty(categoriaMn) || categoriaMn.Contains("[TODOS]") ? "[TODOS]" : categoriaMn,
                    nombreMn = nombreMn
                });

            data.ForEach(d =>
            {
                d.NombreRegion = LimpiaDuplicados(d.NombreRegion, ';');
            });

            return data;
        }

        #endregion


        public static List<RequerimientoBL> RequerimientosContables(DateTime fechadesde, DateTime fechahasta)
        {
            var genericRepo = new GenericRepo();
            var data = genericRepo.ExecuteSP<RequerimientoBL>("SpRequerimientosPorFecha",
                new
                {
                    tipoReporte = 4,
                    fechaDesde = fechadesde,
                    fechaHasta = fechahasta,
                    ut = "",
                    profesionalUt = "",
                    estado = "",
                    etiqueta = "",
                    categoriaMn = "",
                    nombreMn = ""
                });

            return data;
        }


        public static List<RequerimientoBL> RequerimientosUtCopia(DateTime fechadesde, DateTime fechahasta, string utCopia)
        {
            var genericRepo = new GenericRepo();
            var data = genericRepo.ExecuteSP<RequerimientoBL>("SpRequerimientosPorFecha",
                new
                {
                    tipoReporte = 5,
                    fechaDesde = fechadesde,
                    fechaHasta = fechahasta,
                    ut = utCopia,
                    profesionalUt = "",
                    estado = "",
                    etiqueta = "",
                    categoriaMn = "",
                    nombreMn = ""
                });

            return data;
        }

        #region GetRequerimientoTimbraje
        public static List<RequerimientoBL> GetRequerimientoReporteGenerico(string fechadesde, string fechahasta, string ut, string tipo)
        {
            DateTime fechad = string.IsNullOrEmpty(fechadesde) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(fechadesde);
            DateTime fechah = string.IsNullOrEmpty(fechahasta) ? new DateTime(2099, 1, 1) : Convert.ToDateTime(fechahasta);

            var tipoReporte = -1;
            // Tipo 0 - Timbraje de planos
            if (tipo == "0")
            {
                tipoReporte = 7;
            }

            // Tipo 1 - Entregas Diarias
            if (tipo == "1")
            {
                tipoReporte = 6;
            }

            var genericRepo = new GenericRepo();
            var data = genericRepo.ExecuteSP<RequerimientoBL>("SpRequerimientosPorFecha",
                new
                {
                    tipoReporte = tipoReporte,
                    fechaDesde = fechad,
                    fechaHasta = fechah,
                    ut = ut,
                    profesionalUt = "",
                    estado = "",
                    etiqueta = "",
                    categoriaMn = "",
                    nombreMn = ""
                });

            return data;

        }
        #endregion

        #region GetByFecha
        public static List<DespachoBL> GetByFecha(DateTime fechadesde, DateTime fechahasta, string categoriaMn, string nombreMn)
        {
            var genericRepo = new GenericRepo();
            List<DespachoBL> data = genericRepo.ExecuteSP<DespachoBL>("SpRequerimientosPorFecha",
                new
                {
                    tipoReporte = 1,
                    fechaDesde = fechadesde,
                    fechaHasta = fechahasta,
                    ut = "",
                    profesionalUt = "",
                    estado = "",
                    etiqueta = "",
                    categoriaMn = string.IsNullOrEmpty(categoriaMn) || categoriaMn.Contains("[TODOS]") ? "[TODOS]" : categoriaMn,
                    nombreMn = nombreMn
                });


            return data;
        }

        #endregion

        #region CMNGetByFecha
        public static List<DespachoBL> CMNGetByFecha(DateTime fechadesde, DateTime fechahasta, string categoriaMn, string nombreMn)
        {
            var genericRepo = new GenericRepo();
            List<DespachoBL> data = genericRepo.ExecuteSP<DespachoBL>("SpRequerimientosPorFecha",
                new
                {
                    tipoReporte = 2,
                    fechaDesde = fechadesde,
                    fechaHasta = fechahasta,
                    ut = "",
                    profesionalUt = "",
                    estado = "",
                    etiqueta = "",
                    categoriaMn = string.IsNullOrEmpty(categoriaMn) || categoriaMn.Contains("[TODOS]") ? "[TODOS]" : categoriaMn,
                    nombreMn = nombreMn

                });

            return data;

        }

        #endregion

        #region GetSessionByUT
        public static List<SessionBL> GetSessionByUT(int idut)
        {
            try
            {
                using (var cn = new GEDOCSQLDataContext())
                {
                    var result = cn.TB_SesionTablas.Where(q => q.UnidadTecnica_Id == idut);
                    var lstSession = new List<SessionBL>();
                    var itemut = getUnidadTecnica(idut);
                    if (itemut == null) return new List<SessionBL>();

                    foreach (var item in result)
                    {
                        lstSession.Add(new SessionBL
                        {
                            idSession = item.Sesion_Id, 
                            IdUT = item.UnidadTecnica_Id.ToString(), 
                            UT = itemut.Titulo, 
                            Nombre = item.Nombre
                        });

                    }
                    return lstSession.ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return new List<SessionBL>();
            }


        }
        #endregion

        #region GetByUTByUser
        public static List<SessionBL> GetByUTByUser(string name)
        {
            try {
                List<SessionBL> unidadestecnicas = new List<SessionBL>();

                // TODO

                return unidadestecnicas;

            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return new List<SessionBL>();
            }
        }

        public static List<SessionBL> GetUTAll()
        {
            var genericRepo = new GenericRepo();
            var query = "SELECT " +
                        " 0 as idSession, " +
                        " '' as Nombre, " +
                        " ID as IdUT, " +
                        " Title as UT " +
                        " FROM  " +
                        " dbo.UnidadesTecnica " +
                        " ORDER BY Title ";
            List<SessionBL> data = genericRepo.ExecuteQuery<SessionBL>(query, null);

            return data;

        }
        #endregion

        //REPORTE DE AUDITORIA

        #region Reporte AUDITORIA

        #region FechasSumatorias
        // Importante: los códigos siguiente tienen q coincidir con los definidos en la webpart CMN_ReporteAuditoria de la aplicación
        private static readonly Dictionary<string, string> FechasSumatorias = new Dictionary<string, string>(){
            {"1","Fecha de Ingreso"},
            {"2","Fecha de Asignación Unidad Técnica"},
            {"3","Fecha de Asignación Unidad Técnica en copia"},
            {"4","Fecha de Asignación Unidad Técnica en conocimiento"},
            {"5","Fecha de Asignación Unidad Técnica Temporal"},
            {"6","Fecha de Priorización"},
            {"7","Fecha de Resolución Estimada"},
            {"8","Plazo (Número de días)"},
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
        #endregion

        #region RequerimientosGetByFechaUtGrupo
        public static List<RequerimientoBL> RequerimientosGetByFechaUtGrupo(DateTime fechadesde, DateTime fechahasta, string ut, string estado,
             string prioridad, string nombreproyectoprograma, string instRemitente, string etiqueta, string region, string comuna,
             string mnInvolucrado, string profesionalUt, string utCopia, string requiereRespuesta, string motivoCierre, string requiereTimbraje,
             string requiereAcuerdo, string despachoInicSiNo, string despachoSiNo, string instDestinatario, string tipoInstRemitente,
             string tipoInstDestinatario, string semaforo, DateTime fechaResolucionDesde, DateTime fechaResolucionHasta,
             string[] relacionDesdeHasta)
        {

            try
            {
                ut = string.IsNullOrWhiteSpace(ut) || ut.Contains("[TODOS]") ? "[TODOS]" : ut;
                estado = string.IsNullOrWhiteSpace(estado) || estado.Contains("[TODOS]") ? "[TODOS]" : estado;
                prioridad = string.IsNullOrWhiteSpace(prioridad) || prioridad.Contains("[TODOS]") ? "[TODOS]" : prioridad;
                etiqueta = string.IsNullOrWhiteSpace(etiqueta) || etiqueta.Contains("[TODOS]") ? "[TODOS]" : etiqueta;
                region = string.IsNullOrWhiteSpace(region) || region.Contains("[TODOS]") ? "[TODOS]" : region;
                comuna = string.IsNullOrWhiteSpace(comuna) || comuna.Contains("[TODOS]") ? "[TODOS]" : comuna;
                profesionalUt = string.IsNullOrWhiteSpace(profesionalUt) || profesionalUt.Contains("[TODOS]") ? "[TODOS]" : profesionalUt;
                utCopia = string.IsNullOrWhiteSpace(utCopia) || utCopia.Contains("[TODOS]") ? "[TODOS]" : utCopia;
                motivoCierre = string.IsNullOrWhiteSpace(motivoCierre) || motivoCierre.Contains("[TODOS]") ? "[TODOS]" : motivoCierre;
                tipoInstRemitente = string.IsNullOrWhiteSpace(tipoInstRemitente) || tipoInstRemitente.Contains("[TODOS]") ? "[TODOS]" : tipoInstRemitente;
                tipoInstDestinatario = string.IsNullOrWhiteSpace(tipoInstDestinatario) || tipoInstDestinatario.Contains("[TODOS]") ? "[TODOS]" : tipoInstDestinatario;


                //var arrDatos = GetTitulosFromIds("M-Unidades técnicas", ut, "Lookup");
                //var arrDatos = GetTitulosFromIds("M-Etiquetas", etiqueta, "LookupMulti");
                //var arrDatos = GetTitulosFromIds("M-Regiones y Comunas", comuna, "LookupMulti");
                //var arrDatos = GetTitulosFromIds("M-Unidades técnicas", utCopia, "LookupMulti");


                // semaforo
                // Tiene q tener la misma lógica q el semáforo de la bandeja de entrada de la aplicación
                var fechaAmarilloDesde = DateTime.Today.Add(new TimeSpan(5, 0, 0, 0));
                var fechaAmarilloHasta = fechaAmarilloDesde.Add(new TimeSpan(23, 59, 59));
                var fechaRojoDesde = new DateTime(1900, 1, 1);
                var fechaRojoHasta = fechaAmarilloDesde;
                var fechaVerdeDesde = fechaAmarilloHasta;
                var fechaVerdeHasta = new DateTime(2900, 1, 1);
                if (!string.IsNullOrEmpty(semaforo))
                {
                    var arrDatos = semaforo.ToUpper().Split(',');
                    if (!arrDatos.Contains("ROJO"))
                    {
                        /* Si no es semaforo rojo se hacen las fechas del rojo igual al amarillo o verde para ignorar los req en rojo*/
                        fechaRojoDesde = arrDatos.Contains("AMARILLO") ? fechaAmarilloDesde : fechaVerdeDesde;
                        fechaRojoHasta = arrDatos.Contains("AMARILLO") ? fechaAmarilloHasta : fechaVerdeHasta;
                    }
                    if (!arrDatos.Contains("AMARILLO"))
                    {
                        fechaAmarilloDesde = arrDatos.Contains("ROJO") ? fechaRojoDesde : fechaVerdeDesde;
                        fechaAmarilloHasta = arrDatos.Contains("ROJO") ? fechaRojoHasta : fechaVerdeHasta;
                    }
                    if (!arrDatos.Contains("VERDE"))
                    {
                        fechaVerdeDesde = arrDatos.Contains("ROJO") ? fechaRojoDesde : fechaAmarilloDesde;
                        fechaVerdeHasta = arrDatos.Contains("ROJO") ? fechaRojoHasta : fechaAmarilloHasta;
                    }
                }

                var genericRepo = new GenericRepo();
                var lstRequerimientos = genericRepo.ExecuteSP<RequerimientoBL>("SpReporteAuditoria",
                    new
                    {
                        fechadesde = fechadesde,
                        fechahasta = fechahasta,
                        unidadtecnica = ut,
                        estado = estado,
                        prioridad = prioridad,
                        nombreproyectoprograma = nombreproyectoprograma,
                        instRemitente = instRemitente,
                        etiqueta = etiqueta,
                        region = region,
                        comuna = comuna,
                        mnInvolucrado = mnInvolucrado,
                        profesionalut = profesionalUt,
                        utCopia = utCopia,
                        requiereRespuesta = requiereRespuesta,
                        motivoCierre = motivoCierre,
                        requiereTimbraje = requiereTimbraje,
                        requiereAcuerdo = requiereAcuerdo,
                        tipoInstRemitente = tipoInstRemitente,
                        tipoInstDestinatario = tipoInstDestinatario,
                        fechaRojoDesde = string.IsNullOrEmpty(semaforo) ? null : (DateTime?)fechaRojoDesde,
                        fechaRojoHasta = string.IsNullOrEmpty(semaforo) ? null : (DateTime?)fechaRojoHasta,
                        fechaAmarilloDesde = string.IsNullOrEmpty(semaforo) ? null : (DateTime?)fechaAmarilloDesde,
                        fechaAmarilloHasta = string.IsNullOrEmpty(semaforo) ? null : (DateTime?)fechaAmarilloHasta,
                        fechaVerdeDesde = string.IsNullOrEmpty(semaforo) ? null : (DateTime?)fechaVerdeDesde,
                        fechaVerdeHasta = string.IsNullOrEmpty(semaforo) ? null : (DateTime?)fechaVerdeHasta,
                        fechaResolucionDesde = fechaResolucionDesde.Year == 1900 ? null : (DateTime?)fechaResolucionDesde,
                        fechaResolucionHasta = fechaResolucionHasta.Year == 2099 ? null : (DateTime?)fechaResolucionHasta
                    });

                #region Datos Resumen
                var resumen = GetResumenGrupo(lstRequerimientos, relacionDesdeHasta);
                foreach (var item in resumen)
                {
                    lstRequerimientos.Add(new RequerimientoBL()
                    {
                        Desde = item.Desde,
                        Hasta = item.Hasta,
                        CantidadProcesados = item.CantidadProcesados,
                        PromedioDias = item.PromedioDias,
                        SumaDias = item.SumaDias
                    });
                }
                #endregion

                return lstRequerimientos.ToList();
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return new List<RequerimientoBL>()
                {
                    new RequerimientoBL {
                      ObservacionTipoAdjunto = ex.Message,
                      ObservacionesTipoDocumento = ex.StackTrace,
                    }
                };
            }
        }
        #endregion

        #region ResumenGrupo
        public static List<ResumenGrupo> ResumenGrupo(DateTime fechadesde, DateTime fechahasta, string ut, string estado,
            string prioridad, string nombreproyectoprograma, string instRemitente, string etiqueta, string region, string comuna,
            string mnInvolucrado, string profesionalUt, string utCopia, string requiereRespuesta, string motivoCierre, string requiereTimbraje,
            string requiereAcuerdo, string despachoInicSiNo, string despachoSiNo, string instDestinatario, string tipoInstRemitente,
            string tipoInstDestinatario, string semaforo, DateTime fechaResolucionDesde, DateTime fechaResolucionHasta,
            string[] relacionDesdeHasta)
        {
            var resultado = new List<ResumenGrupo>();
            if (relacionDesdeHasta == null || relacionDesdeHasta.Length == 0)
            {
                return new List<ResumenGrupo>();
            }
            var listaReq = RequerimientosGetByFechaUtGrupo(fechadesde, fechahasta, ut, estado,
             prioridad, nombreproyectoprograma, instRemitente, etiqueta, region, comuna,
             mnInvolucrado, profesionalUt, utCopia, requiereRespuesta, motivoCierre, requiereTimbraje,
             requiereAcuerdo, despachoInicSiNo, despachoSiNo, instDestinatario, tipoInstRemitente,
             tipoInstDestinatario, semaforo, fechaResolucionDesde, fechaResolucionHasta, relacionDesdeHasta);


            return GetResumenGrupo(listaReq, relacionDesdeHasta);
        }

        public static List<ResumenGrupo> GetResumenGrupo(List<RequerimientoBL> listaReq, string[] relacionDesdeHasta)
        {
            try
            {
                var resultado = new List<ResumenGrupo>();

                var docsIngreso = new List<string>();

                var datosReqFechas = new Dictionary<string, DatosRequerimientoFechas>();
                foreach (var req in listaReq)
                {
                    if (!docsIngreso.Contains(req.DocumentoIngreso))
                    {
                        docsIngreso.Add(req.DocumentoIngreso);

                        // Datos q se toman del Requerimiento y no del Log:
                        var datosReq = new DatosRequerimientoFechas();
                        // fecha de ingreso
                        datosReq.FechaIngreso = req.FechaIngreso;
                        // Fecha de Asignación Unidad Técnica
                        datosReq.FechaAsignacionUT = datosReq.FechaAsignacionUT ?? req.FechaAsignacionUt;
                        // Fecha de Asignación Profesional UT
                        datosReq.FechaAsignacionProfesional = string.IsNullOrEmpty(req.FechaAsignacionProfesional)
                            ? (DateTime?)null
                            : DateTime.ParseExact(req.FechaAsignacionProfesional, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        // Fecha de Resolución Estimada
                        datosReq.FechaResolucionEstimada = req.FechaResolEstimada;
                        // Fecha de último Acuerdo Comisión
                        datosReq.FechaUltimoAcuerdoComision = req.FechaUltimoAcuerdoComision;
                        // Fecha de último Acuerdo Sesión
                        datosReq.FechaUltimoAcuerdoSesion = req.FechaUltimoAcuerdoSesion;
                        // Fecha de Emisión de oficio
                        datosReq.FechaEmisionoficio = req.FechaEmisionUltimoOficio;
                        // Liberacion Ut Temporal
                        datosReq.LiberacionAsignacionTemporal = req.FechaLiberacionUtTemporal;
                        //Fecha de Cierre de Requerimiento
                        datosReq.FechaCierreRequerimiento = string.IsNullOrEmpty(req.FechaCierre) 
                            ? (DateTime?)null 
                            : DateTime.ParseExact(req.FechaCierre, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        //Fecha de Recepcion UT
                        datosReq.FechaRecepcionUT = req.FechaRecepcionUt;

                        datosReqFechas.Add(req.DocumentoIngreso, datosReq);
                    }
                }

                using (var cn = new GEDOCSQLDataContext())
                {
                    //var datosLog = cn.TB_Logs.Where(l => docsIngreso.Contains(l.DocumentoIngreso)).OrderBy(l => l.DocumentoIngreso).ThenBy(l => l.Id_Log).ToList();
                    var datosLog = new List<TB_Log>();
                    for (int i = 0; i < Math.Ceiling((double)docsIngreso.Count / 2000); i++)
                    {
                        datosLog.AddRange(cn.TB_Logs.Where(l => docsIngreso.Skip(i * 2000).Take(2000).Contains(l.DocumentoIngreso)));
                    }
                    var docAnt = "";
                    DatosRequerimientoFechas datosReq = new DatosRequerimientoFechas();
                    foreach (var datoLog in datosLog.OrderBy(l => l.DocumentoIngreso).ThenBy(l => l.Id_Log).ToList())
                    {
                        //if (docAnt == "")
                        //{
                        //    docAnt = datoLog.DocumentoIngreso;
                        //    datosReq = datosReqFechas.ContainsKey(docAnt) ? datosReqFechas[docAnt] : new DatosRequerimientoFechas();
                        //}
                        if (docAnt != datoLog.DocumentoIngreso)
                        {
                            docAnt = datoLog.DocumentoIngreso;
                            if (datosReqFechas.ContainsKey(docAnt))
                            {
                                datosReq = datosReqFechas[docAnt];
                            }
                            else
                            { // Acá nunca debe entrar, pero por si acaso
                                datosReq = new DatosRequerimientoFechas();
                            }
                        }
                        // fecha de ingreso
                        if (datoLog.Formulario.ToUpper() == "REQUERIMIENTO-NUEVO")
                        {
                            datosReq.FechaIngreso = datoLog.Fecha;
                        }
                        // Fecha de Asignación Unidad Técnica
                        if (datoLog.Formulario.ToUpper() == "ASIGNACION" && datoLog.Estado.ToUpper() == "ASIGNADO" && datoLog.Etapa.ToUpper() == "PRIORIZACIÓN")
                        {
                            datosReq.FechaAsignacionUT = datoLog.Fecha;
                        }
                        //Fecha de Asignación Unidad Técnica en copia
                        if (datoLog.Formulario.ToUpper() == "ASIGNACION" && datoLog.Accion.ToUpper() == "ASIGNA-UT-COPIA")
                        {
                            datosReq.FechaAsignacionUTCopia = datoLog.Fecha;
                        }
                        //Fecha de Asignación Unidad Técnica en conocimiento
                        if (datoLog.Formulario.ToUpper() == "ASIGNACION" && datoLog.Accion.ToUpper() == "ASIGNA-UT-CONOCIMIENTO")
                        {
                            datosReq.FechaAsignacionUTConocimiento = datoLog.Fecha;
                        }
                        //Fecha de Asignación Unidad Técnica en Asignación Temporal
                        if (datoLog.Formulario.ToUpper() == "ASIGNACION" && datoLog.Accion.ToUpper() == "ASIGNA-UT-TEMPORAL")
                        {
                            datosReq.FechaAsignacionUTTemporal = datoLog.Fecha;
                        }
                        //Fecha de Priorización
                        if (datoLog.Formulario.ToUpper() == "REQUERIMIENTOPRIORIZACION" && datoLog.Estado.ToUpper() == "ASIGNADO" && datoLog.Etapa.ToUpper() == "UNIDAD TÉCNICA")
                        {
                            datosReq.FechaPriorizacion = datoLog.Fecha;
                        }

                        //Fecha Asignación Profesional Temporal
                        if (datoLog.Formulario.ToUpper() == "ASIGNACIONPROFESIONALUT" && datoLog.Accion.ToUpper() == "ASIGNA-PROFESIONAL-UT")
                        {
                            datosReq.FechaAsignacionProfesionalTemporal = datoLog.Fecha;
                        }
                        //Fecha  Asignación Profesional
                        if (datoLog.Formulario.ToUpper() == "ASIGNACIONPROFESIONALUT" && datoLog.Accion.ToUpper() == "ASIGNA-PROFESIONAL")
                        {
                            datosReq.FechaAsignacionProfesional = datoLog.Fecha;
                        }
                        //Fecha  Reasignación Profesional
                        if (datoLog.Formulario.ToUpper() == "REASIGNACIONPROFESIONALUT")
                        {
                            datosReq.FechaReasignacionProfesional = datoLog.Fecha;
                        }
                        //TODO: Fecha de Recepción UT

                        //Fecha de Unidad Técnica Reasignada
                        if (datoLog.Formulario.ToUpper() == "ASIGNACION" && datoLog.Accion.ToUpper() == "REASIGNACION-UT")
                        {
                            datosReq.FechaUTReasignada = datoLog.Fecha;
                        }
                        //Fecha de Solicitud de Reasignación
                        if (datoLog.Formulario.ToUpper() == "REQUERIMIENTOREASIGNACION" && datoLog.Estado.ToUpper() == "ASIGNADO" /*&& datoLog.Etapa.ToUpper() == "ASIGNACIÓN"*/)
                        {
                            datosReq.FechaSolicitudReasignacion = datoLog.Fecha;
                        }
                        //Fecha de Liberación de Asignación Temporal
                        if (datoLog.Formulario.ToUpper() == "EDICIONREQUERIMIENTOCAMPOS" && datoLog.Accion.ToUpper() == "LIBERA-ASIGNACION-TEMPORAL")
                        {
                            datosReq.LiberacionAsignacionTemporal = datoLog.Fecha;
                        }
                        //Fecha de Ingreso Histórico
                        if (datoLog.Formulario.ToUpper() == "REQUERIMIENTOHISTORICO-NUEVO")
                        {
                            datosReq.FechaIngresoHistorico = datoLog.Fecha;
                        }
                        //Fecha de Cierre de Requerimiento
                        if (datoLog.Formulario.ToUpper() == "REQUERIMIENTOCIERRE" && datoLog.Accion.ToUpper() == "CIERRE")
                        {
                            datosReq.FechaCierreRequerimiento = datoLog.Fecha;
                        }

                    }

                }

                resultado = ProcesaDatosEntreFechas(datosReqFechas.Values.ToList(), relacionDesdeHasta);

                return resultado;

            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return new List<ResumenGrupo>()
                {
                    new ResumenGrupo {
                      Desde = "<ERROR>",
                      Hasta = ex.Message
                    }
                };
            }
        }
        #endregion

        #region ProcesaDatosEntreFechas
        private static List<ResumenGrupo> ProcesaDatosEntreFechas(List<DatosRequerimientoFechas> datosReqFechas,
            string[] relacionDesdeHasta)
        {
            // cada elemento de relacionDesdeHasta tiene q venir en formato "X,Y", por ejemplo {"1,5", "1,6", "3,8"}
            var resultado = InicializaResumenFromRelaciones(relacionDesdeHasta);

            foreach (var datosReq in datosReqFechas)
            {
                for (var i = 0; i < relacionDesdeHasta.Count(); i++)
                {
                    var relacionDH = relacionDesdeHasta[i].Split(',');
                    if (relacionDH.Length != 2)
                        continue;
                    var fechaDesde = GetFechaReqById(datosReq, relacionDH[0]);
                    var fechaHasta = GetFechaReqById(datosReq, relacionDH[1]);
                    if (fechaDesde != null && fechaHasta != null /*&& DateTime.Compare(fechaDesde.Value.Date, fechaHasta.Value.Date) <= 0*/)
                    {
                        var dias = GetDiasHabilesEntreFechas(fechaDesde.Value.Date, fechaHasta.Value.Date);
                        var nombreFechaD = GetNombreFechaById(relacionDH[0]);
                        var nombreFechaH = GetNombreFechaById(relacionDH[1]);
                        var encontrado = false;
                        foreach (var elem in resultado)
                        {
                            if (elem.Desde == nombreFechaD && elem.Hasta == nombreFechaH)
                            {
                                encontrado = true;
                                elem.SumaDias += dias;
                                elem.CantidadProcesados++;
                                elem.PromedioDias = elem.SumaDias / (double)elem.CantidadProcesados;
                                break;
                            }
                        }
                        if (!encontrado)
                        {
                            resultado.Add(new ResumenGrupo()
                            {
                                CantidadProcesados = 1,
                                Desde = nombreFechaD,
                                Hasta = nombreFechaH,
                                PromedioDias = dias,
                                SumaDias = dias
                            });
                        }
                    }
                }
            }

            return resultado;
        }

        private static int GetDiasHabilesEntreFechas(DateTime fechaDesde, DateTime fechaHasta)
        {
            if (DateTime.Compare(fechaDesde.Date, fechaHasta.Date) >= 0)
            {
                return 0;
            }
            var genericRepo = new GenericRepo();
            var data = genericRepo.ExecuteSP<int>("SpDiasHabilesEntreFechas",
                new
                {
                    fechaDesde = fechaDesde.Date,
                    fechaHasta = fechaHasta.Date
                });

            return data != null && data.Count > 0 ? data[0] : (fechaHasta.Date - fechaDesde.Date).Days;
        }
        #endregion

        #region InicializaResumenFromRelaciones
        private static List<ResumenGrupo> InicializaResumenFromRelaciones(string[] relacionDesdeHasta)
        {
            var datosTmp = new Dictionary<int, List<int>>();
            var resumenIni = new List<ResumenGrupo>();
            for (var i = 0; i < relacionDesdeHasta.Count(); i++)
            {
                var relacionDH = relacionDesdeHasta[i].Split(',');
                if (relacionDH.Length != 2)
                    continue;
                var key = Convert.ToInt32(relacionDH[0]);
                var value = Convert.ToInt32(relacionDH[1]);
                if (!datosTmp.ContainsKey(key))
                {
                    datosTmp.Add(key, new List<int>());
                }
                datosTmp[key].Add(value);
            }

            var datosTmpOrdenado = new Dictionary<int, List<int>>();
            for (var i = 0; i <= FechasSumatorias.Count; i++)
            {
                if (datosTmp.ContainsKey(i))
                {
                    datosTmpOrdenado.Add(i, datosTmp[i]);
                }
            }

            foreach (var key in datosTmpOrdenado.Keys)
            {
                datosTmpOrdenado[key].Sort();
                foreach (var value in datosTmpOrdenado[key])
                {
                    resumenIni.Add(new ResumenGrupo()
                    {
                        Desde = GetNombreFechaById(key.ToString()),
                        Hasta = GetNombreFechaById(value.ToString()),
                    });
                }
            }

            return resumenIni;
        }
        #endregion

        #region GetFechaReqById
        private static DateTime? GetFechaReqById(DatosRequerimientoFechas datosReq, string idFecha)
        {
            switch (idFecha)
            {
                case "1": // Fecha de Ingreso
                    return datosReq.FechaIngreso;
                case "2": // Fecha de Asignación Unidad Técnica
                    return datosReq.FechaAsignacionUT;
                case "3": // Fecha de Asignación Unidad Técnica en copia
                    return datosReq.FechaAsignacionUTCopia;
                case "4": // Fecha de Asignación Unidad Técnica en conocimiento
                    return datosReq.FechaAsignacionUTConocimiento;
                case "5": // Fecha de Asignación Unidad Técnica en Asignación Temporal
                    return datosReq.FechaAsignacionUTTemporal;
                case "6": // Fecha de Priorización
                    return datosReq.FechaPriorizacion;
                case "7": // Fecha de Resolución Estimada
                    return datosReq.FechaResolucionEstimada;
                //case "8": // Plazo (Número de días)
                //    return null;
                case "9": // Fecha Asignación Profesional Temporal
                    return datosReq.FechaAsignacionProfesionalTemporal;
                case "10": // Fecha  Asignación Profesional
                    return datosReq.FechaAsignacionProfesional;
                case "11": // Fecha  Reasignación Profesional
                    return datosReq.FechaReasignacionProfesional;
                case "12": // Fecha de Recepción UT
                    return datosReq.FechaRecepcionUT;
                case "13": // Fecha de Unidad Técnica Reasignada
                    return datosReq.FechaUTReasignada;
                case "14": // Fecha de Solicitud de Reasignación
                    return datosReq.FechaIngreso;
                case "15": // Liberación de Asignación Temporal
                    return datosReq.LiberacionAsignacionTemporal;
                case "16": // Fecha de último Acuerdo Comisión
                    return datosReq.FechaUltimoAcuerdoComision;
                case "17": // Fecha de último Acuerdo Sesión
                    return datosReq.FechaUltimoAcuerdoSesion;
                case "18": // Fecha de Emisión de oficio
                    return datosReq.FechaEmisionoficio;
                case "19": // Fecha de Ingreso Histórico
                    return datosReq.FechaIngresoHistorico;
                case "20": // Fecha de Cierre de Requerimiento
                    return datosReq.FechaCierreRequerimiento;
            }
            return null;
        }
        #endregion

        #region GetNombreFechaById
        private static string GetNombreFechaById(string idFecha)
        {
            return FechasSumatorias.ContainsKey(idFecha) ? FechasSumatorias[idFecha] : "";
        }
        #endregion

       
        #endregion

        //private static NetworkCredential GetNetworkCredential(bool esHistorico)
        //{
        //    try
        //    {
        //        if (esHistorico)
        //        {
        //            if (networkCredentialHist != null)
        //                return networkCredentialHist;

        //            networkCredentialHist = new NetworkCredential(SP_User_Hist, SP_Pass_Hist, SP_Domain_Hist);
        //            return networkCredentialHist;
        //        }

        //        if (networkCredential != null)
        //            return networkCredential;

        //        networkCredential = new NetworkCredential(SP_User, SP_Pass, SP_Domain);
        //        return networkCredential;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Execute().Error(ex);
        //        throw;
        //    }
        //}

        public static string ToDateTime(DateTime? fecha)
        {
            if (fecha.HasValue)
            {
                return fecha.Value.ToString("dd/MM/yyyy HH:mm");
            }
            else
            {
                return "";
            }
        }

        private static string LimpiaDuplicados(string texto, char separador)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return texto;
            }
            var textoArr = texto.Split(separador);
            var textoLimpio = new List<string>();
            if (textoArr.Length > 1)
            {
                for (var i = 0; i < textoArr.Length; i++)
                {
                    if (!textoLimpio.Contains(textoArr[i]))
                    {
                        textoLimpio.Add(textoArr[i]);
                    }
                }
                texto = String.Join(separador.ToString(), textoLimpio.ToArray());
            }
            return texto;
        }


        public static List<RequerimientoBL> GetProcesosMasivos(DateTime fechadesde, DateTime fechahasta, string ut, string profesional, string estado,
            string etapa, string etiqueta, string tipomon, string region, string accionpm)
        {
            var lstReq = new List<RequerimientoBL>();
            var reqsFiltro = new List<RequerimientoBL>();
            var accionLogMulti = "";
            var accionLogSimple = "";
            var formAccionSimple = "";

            switch (accionpm)
            {
                case "DES": // Crear Despacho
                    accionLogMulti = "DESPACHO";
                    accionLogSimple = "CREACIÓN";
                    formAccionSimple = "DESPACHO";
                    break;
                case "AUT": // Asignar UT
                    accionLogMulti = "ASIGNAR_UT";
                    accionLogSimple = "EDICION";
                    formAccionSimple = "ASIGNACION";
                    break;
                case "RUT": // Reasignar UT
                    accionLogMulti = "REASIGNAR_UT";
                    accionLogSimple = "REASIGNACION-UT";
                    formAccionSimple = "ASIGNACION";
                    break;
                case "APR": // Asignar Profesional
                    accionLogMulti = "ASIGNAR_PROF";
                    accionLogSimple = "ASIGNA-PROFESIONAL";
                    formAccionSimple = "ASIGNACIONPROFESIONALUT";
                    break;
                case "RPR": // Reasignar Profesional
                    accionLogMulti = "REASIGNAR_PROF";
                    accionLogSimple = "EDICION";
                    formAccionSimple = "REASIGNACIONPROFESIONALUT";
                    break;
                case "ETI":  // Modificar Etiqueta
                    accionLogMulti = "MODIF_ETIQUETA";
                    accionLogSimple = "-";
                    formAccionSimple = "-";
                    break;
                case "AI":  // Abrir Ingresos
                    accionLogMulti = "ABRIR_INGRESOS";
                    accionLogSimple = "-";
                    formAccionSimple = "-";
                    break;
                default:
                    return lstReq;
            }

            //System.Linq.IQueryable<TB_Log> logBd = null;
            // using (var cn = new GEDOCSQLDataContext())
            {

                reqsFiltro = GetRequerimientosByLogBd(accionLogMulti, formAccionSimple, accionLogSimple, fechadesde, fechahasta, ut,
                    profesional, estado, etapa, etiqueta, tipomon, region, accionpm == "DES" ? 1 : 0);

                var despachos = new Dictionary<string, List<DespachoBL>>(); // GetDespachoMulti(docsIng, false);
                var bitacoras = new Dictionary<string, List<BitacoraBL>>(); // GetBitacoraMulti(docsIng, false);

                foreach (var req in reqsFiltro)
                {
                    req.NumeroDespacho = req.NumeroDespacho != null && req.NumeroDespacho.Contains(".")
                        ? req.NumeroDespacho.Substring(0, req.NumeroDespacho.IndexOf("."))
                        : "";
                    req.ModalidadProceso = req.Formulario.StartsWith("PROCESOMASIVO_") ? "Masivo" : "Individual";
                    req.TipoReporte = accionpm;
                    if (accionpm == "APR")
                    { // Es Asignacion de Profesional
                        req.ProfesionalUtAnterior = "";
                        req.FechaAsignacionProfAnterior = "";
                    }
                    if (accionpm == "RPR")
                    { // Es reasignación profesional
                        req.ProfesionalUtAnterior = string.IsNullOrWhiteSpace(req.extraData) || req.extraData.Length <= 9 || req.extraData.IndexOf("FECHA_ANT:") < 0
                            ? ""
                            : req.extraData.Substring(9, req.extraData.IndexOf("FECHA_ANT:") - 9);
                        req.FechaAsignacionProfAnterior = string.IsNullOrWhiteSpace(req.extraData) || req.extraData.IndexOf("FECHA_ANT:") < 0
                            ? ""
                            : req.extraData.Substring(req.extraData.IndexOf("FECHA_ANT:"), req.extraData.Length - req.extraData.IndexOf("FECHA_ANT:")).Replace("FECHA_ANT:", "");
                    }
                    if (accionpm == "AI")
                    { // Es Abrir Ingresos
                        var extraDataArr = string.IsNullOrWhiteSpace(req.extraData) ? new string[] { "", "", "", "" } : req.extraData.Split('|');
                        req.FechaCierreAnt = extraDataArr[0];
                        req.CerradoPorAnt = extraDataArr.Length >= 2 ? extraDataArr[1] : "";
                        req.MotivoCierreAnt = extraDataArr.Length >= 3 ? extraDataArr[2] : "";
                        req.ComentarioCierreAnt = extraDataArr.Length >= 4 ? extraDataArr[3] : "";
                    }
                    if (accionpm == "RUT")
                    { // Es Reasignación UT
                        var extraDataArr = string.IsNullOrWhiteSpace(req.extraData) ? new string[] { "", "" } : req.extraData.Split('|');
                        req.FechaAsignacionUtAnterior = extraDataArr[0];
                        req.UnidadTecnicaAnterior = extraDataArr.Length >= 2 ? extraDataArr[1] : "";
                    }
                    if (accionpm == "DES")
                    { // Es Despacho
                        //requerimiento.NumeroDespacho = req.N_x00fa_mero_x0020_de_x0020_despacho ?? "";
                        req.FechaEmisionUltimoOficioStr = req.FechaEmisionUltimoOficio == null ? "" : ((DateTime)req.FechaEmisionUltimoOficio).ToString("dd/MM/yyyy");
                        //requerimiento.DestinatarioDesp = req.Destinatario_x003a_Nombre ?? "";
                        //requerimiento.InstDestinatarioDesp = req.Destinatario_x003a_Instituci_x00f3_n ?? "";
                        //requerimiento.MateriaDesp = req.Materia_x0020_de_x0020_despacho ?? "";
                    }
                    if (accionpm == "ETI")
                    { // Es Reasignación UT
                        req.EtiquetaAnt = req.extraData ?? "";
                    }

                    #region Despachos del requerimiento
                    var despachosReq = req.DocumentoIngreso != null && despachos.ContainsKey(req.DocumentoIngreso) ? despachos[req.DocumentoIngreso] : new List<DespachoBL>(0);// GetDespacho(requerimiento.DocumentoIngreso);

                    if (despachosReq.Count > 0)
                    { // Requerimiento con despachos
                        var _despachos = despachosReq.Where(q => q.DocumentoIngreso == req.DocumentoIngreso).OrderByDescending(o => o.FechaEmisionOficio).FirstOrDefault();
                        req.ultima_fecha_oficio = _despachos == null ? "" : CorregirFecha(_despachos.FechaEmisionOficio.GetValueOrDefault());
                        req.FechaEmisionUltimoOficio = _despachos == null ? (DateTime?)null : CorregirFechaDateTime(_despachos.FechaEmisionOficio.GetValueOrDefault());
                        req.NumeroDespacho = _despachos == null ? "" : _despachos.NumeroDespacho;
                        req.MateriaDesp = _despachos == null ? "" : _despachos.MateriaDespacho;
                    }
                    else
                    { // Requerimiento sin despachos
                        req.ultima_fecha_oficio = "";
                    }
                    #endregion

                    #region Bitacoras del requerimiento
                    var sesionycomision = req.DocumentoIngreso != null && bitacoras.ContainsKey(req.DocumentoIngreso) ? bitacoras[req.DocumentoIngreso] : new List<BitacoraBL>(0); // GetBitacora(requerimiento.DocumentoIngreso);

                    if (sesionycomision.Count > 0)
                    {
                        var _sesion = sesionycomision.Where(q => q.TipoBitacora == "Acuerdo Sesión").OrderByDescending(q => q.FechaBitacora).FirstOrDefault();
                        var _comision = sesionycomision.Where(q => q.TipoBitacora == "Acuerdo Comisión").OrderByDescending(q => q.FechaBitacora).FirstOrDefault();
                        var _respuesta = sesionycomision.Where(q => q.TipoBitacora == "Respuesta").OrderByDescending(q => q.FechaBitacora).FirstOrDefault();

                        req.ultima_fecha_acuerdo_sesion = _sesion == null ? "" : CorregirFecha(_sesion.FechaBitacora);
                        req.ultimo_acuerdo_sesion = _sesion == null ? "" : _sesion.Observacion;
                        req.ultima_fecha_acuerdo_comisión = _comision == null ? "" : CorregirFecha(_comision.FechaBitacora);
                        req.ultimo_acuerdo_comision = _comision == null ? "" : _comision.Observacion;
                        req.ultima_fecha_respuesta = _respuesta == null ? "" : CorregirFecha(_respuesta.FechaBitacora);
                        req.FechaUltimoAcuerdoSesion = _sesion == null ? (DateTime?)null : CorregirFechaDateTime(_sesion.FechaBitacora);
                        req.FechaUltimoAcuerdoComision = _comision == null ? (DateTime?)null : CorregirFechaDateTime(_comision.FechaBitacora);
                        req.FechaUltimaRespuesta = _respuesta == null ? (DateTime?)null : CorregirFechaDateTime(_respuesta.FechaBitacora);
                    }
                    else
                    {
                        req.ultima_fecha_acuerdo_sesion = "";
                        req.ultimo_acuerdo_sesion = "";
                        req.ultima_fecha_acuerdo_comisión = "";
                        req.ultimo_acuerdo_comision = "";
                        req.ultima_fecha_respuesta = "";
                    }
                    #endregion


                    //lstReq.Add(requerimiento);
                }
            }

            return reqsFiltro;
        }


        public static List<DatosSearch> GetDatosSearch(string tipoSearch)
        {

            var genericRepo = new GenericRepo();
            var data = genericRepo.ExecuteSP<DatosSearch>("DatosSearch",
                new
                {
                    tipoBusqueda = tipoSearch
                });


            return data;

        }

        #region RequerimientosTramites
        public static List<RequerimientoBL> RequerimientosTramites(DateTime fechadesde, DateTime fechahasta, 
            string tipotramite, string canalllegada, string numerocaso, string nobrecaso,
            string documentoingreso, string estado, string etapa, string remitente,
            string institucionremitente, string categoriamonumento, string denominacionoficial, string direccionmonumento, string region,
            string provincia, string comuna, string materia, string etiqueta, string unidadtecnica, string profesionalut)
        {

            var genericRepo = new GenericRepo();
            var data = genericRepo.ExecuteSP<RequerimientoBL>("SP_ReporteTramiteCaso",
                new
                {
                    tipotramite = tipotramite,
                    canalllegada = canalllegada,
                    numerocaso = numerocaso,
                    nobrecaso = nobrecaso,
                    documentoingreso = documentoingreso,
                    fechadesde = fechadesde,
                    fechahasta = fechahasta,
                    estado = estado,
                    etapa = etapa,
                    remitente = remitente,
                    institucionremitente = institucionremitente,
                    categoriamonumento = categoriamonumento,
                    denominacionoficial = denominacionoficial,
                    direccionmonumento = direccionmonumento,
                    region = region,
                    provincia = provincia,
                    comuna = comuna,
                    materia = materia,
                    etiqueta = etiqueta,
                    unidadtecnica = unidadtecnica,
                    profesionalut = profesionalut
                });


            return data;
        }


        #endregion

        public static List<LogTransacciones> LogTransacciones(DateTime fechaD, DateTime fechaH)
        {
            fechaD = fechaD.Date;
            fechaH = fechaH.Date.Add(new TimeSpan(23, 59, 59));
            var genericRepo = new GenericRepo();
            var query = "SELECT * " +
                        " FROM dbo.LogTransacciones " +
                        " WHERE Fecha >= @fechaD AND Fecha <= @fechaH" +
                        " ORDER BY DocumentoIngreso Desc ";
            List<LogTransacciones> data = genericRepo.ExecuteQuery<LogTransacciones>(query, new { fechaD = fechaD, fechaH = fechaH });

            return data;
        }

    }
}