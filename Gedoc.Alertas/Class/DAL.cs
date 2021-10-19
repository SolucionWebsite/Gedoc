using Gedoc.Helpers.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Alertas.Class
{
    class DAL
    {
        public static readonly String CONNECTION_STRING = System.Configuration.ConfigurationManager.AppSettings["StringConnection"];

        public List<RequerimientoDto> GetRequerimientosPrioridadForzada()
        {
            List<RequerimientoDto> ingresos = new List<RequerimientoDto>();

            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = new SqlCommand(string.Format("SELECT * FROM vw_FichaIngreso WHERE ForzarPrioridad = 1"), con))
                {

                    SqlDataReader dr = null;
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    try
                    {
                        dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    }
                    catch (SqlException sex)
                    {
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                    }

                    if (dr != null)
                    {
                        while (dr.Read())
                        {
                            ingresos.Add(new RequerimientoDto()
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                DocumentoIngreso = dr["DocumentoIngreso"].ToString(),
                                FechaIngreso = Convert.ToDateTime(dr["FechaIngreso"]),
                                FechaDocumento = Convert.ToDateTime(dr["FechaDocumento"]),
                                EstadoId = Convert.ToInt32(dr["EstadoId"]),
                                EstadoTitulo = dr["EstadoTitulo"] == DBNull.Value ? null : dr["EstadoTitulo"].ToString(),
                                EtapaId = Convert.ToInt32(dr["EtapaId"]),
                                EtapaTitulo = dr["EtapaTitulo"].ToString(),
                                TipoTramiteId = dr["TipoTramiteId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["TipoTramiteId"]),
                                TipoTramiteTitulo = dr["TipoTramiteTitulo"].ToString(),
                                CanalLlegadaTramiteCod = dr["CanalLlegadaTramiteCod"].ToString(),
                                CanalLlegadaTramiteTitulo = dr["CanalLlegadaTramiteTitulo"].ToString(),
                                TipoDocumentoCod = dr["TipoDocumentoId"].ToString(),
                                TipoDocumentoTitulo = dr["TipoDocumentoTitulo"].ToString(),
                                TipoIngreso = dr["TipoIngreso"].ToString(),
                                ObservacionesTipoDoc = dr["ObservacionesTipoDoc"].ToString(),
                                FechasEmisionOficio = dr["FechasEmisionOficio"].ToString(),
                                AdjuntaDocumentacion = Convert.ToBoolean(dr["AdjuntaDocumentacion"]),
                                CantidadAdjuntos = dr["CantidadAdjuntos"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["CantidadAdjuntos"]),
                                TipoAdjuntoTitulos = dr["TipoAdjuntoTitulos"].ToString(),
                                SoporteTitulos = dr["SoporteTitulos"].ToString(),
                                ObservacionesAdjuntos = dr["ObservacionesAdjuntos"].ToString(),
                                RemitenteId = Convert.ToInt32(dr["RemitenteId"]),
                                RemitenteNombre = dr["RemitenteNombre"].ToString(),
                                RemitenteRut = dr["RemitenteRut"].ToString(),
                                RemitenteGenero = dr["RemitenteGenero"].ToString(),
                                RemitenteCargo = dr["RemitenteCargo"].ToString(),
                                RemitenteInstitucion = dr["RemitenteInstitucion"].ToString(),
                                RemitenteTipoInstitucion = dr["RemitenteTipoInstitucion"].ToString(),
                                RemitenteDireccion = dr["RemitenteDireccion"].ToString(),
                                RemitenteEmail = dr["RemitenteEmail"].ToString(),
                                RemitenteTelefono = dr["RemitenteTelefono"].ToString(),
                                NombreProyectoPrograma = dr["NombreProyectoPrograma"].ToString(),
                                ProyectoActividad = dr["ProyectoActividad"].ToString(),
                                CasoId = dr["CasoId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["CasoId"]),
                                CasoTitulo = dr["CasoTitulo"].ToString(),
                                Materia = dr["Materia"].ToString(),
                                EtiquetaTitulos = dr["EtiquetaTitulos"].ToString(),
                                MonumentoNacional = dr["MonumentoNacionalId"] == DBNull.Value ? null : (new MonumentoNacionalDto()
                                {
                                    Id = Convert.ToInt32(dr["MonumentoNacionalId"]),
                                    CategoriaMonumentoNacCod = dr["MonumentoNacionalCategoriaMonumentoNacCod"].ToString(),
                                    CodigoMonumentoNac = dr["MonumentoNacionalCodigoMonumentoNac"].ToString(),
                                    DenominacionOficial = dr["MonumentoNacionalDenominacionOficial"].ToString(),
                                    OtrasDenominaciones = dr["MonumentoNacionalOtrasDenominaciones"].ToString(),
                                    NombreUsoActual = dr["MonumentoNacionalNombreUsoActual"].ToString(),
                                    DireccionMonumentoNac = dr["MonumentoNacionalDireccionMonumentoNac"].ToString(),
                                    ReferenciaLocalidad = dr["MonumentoNacionalReferenciaLocalidad"].ToString(),
                                    RegionTitulo = dr["MonumentoNacionalRegionTitulo"].ToString(),
                                    ProvinciaTitulo = dr["MonumentoNacionalProvinciaTitulo"].ToString(),
                                    ComunaTitulo = dr["MonumentoNacionalComunaTitulo"].ToString(),
                                    RolSii = dr["MonumentoNacionalRolSii"].ToString()
                                }),
                                FormaLlegadaCod = dr["FormaLlegadaId"].ToString(),
                                FormaLlegadaTitulo = dr["FormaLlegadaTitulo"].ToString(),
                                ObservacionesFormaLlegada = dr["ObservacionesFormaLlegada"].ToString(),
                                Redireccionado = dr["Redireccionado"] == DBNull.Value ? new bool?() : Convert.ToBoolean(dr["Redireccionado"]),
                                NumeroTicket = dr["NumeroTicket"].ToString(),
                                RequiereAcuerdo = dr["RequiereAcuerdo"] == DBNull.Value ? new bool?() : Convert.ToBoolean(dr["RequiereAcuerdo"]),
                                RequerimientoAnteriorId = dr["RequerimientoAnteriorId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["RequerimientoAnteriorId"]),
                                RequerimientoAnteriorDocIng = dr["RequerimientoAnteriorDocIng"].ToString(),
                                RequerimientoNoRegistrado = dr["RequerimientoNoRegistrado"].ToString(),
                                CaracterId = dr["CaracterId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["CaracterId"]),
                                CaracterTitulo = dr["CaracterTitulo"].ToString(),
                                ObservacionesCaracter = dr["ObservacionesCaracter"].ToString(),
                                Devolucion = dr["Devolucion"] == DBNull.Value ? new DateTime?() : Convert.ToDateTime(dr["Devolucion"]),
                                AsignacionAnterior = dr["AsignacionAnterior"] == DBNull.Value ? new DateTime?() : Convert.ToDateTime(dr["AsignacionAnterior"]),
                                UtAnteriorId = dr["UtAnteriorId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["UtAnteriorId"]),
                                UtAnteriorTitulo = dr["UtAnteriorTitulo"].ToString(),
                                UtAsignadaId = dr["UtAsignadaId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["UtAsignadaId"]),
                                UtAsignadaTitulo = dr["UtAsignadaTitulo"].ToString(),
                                AsignacionUt = dr["AsignacionUt"] == DBNull.Value ? new DateTime?() : Convert.ToDateTime(dr["AsignacionUt"]),
                                UtCopiaTitulos = dr["UtCopiaTitulos"].ToString(),
                                UtConocimientoId = dr["UtConocimientoId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["UtConocimientoId"]),
                                UtConocimientoTitulo = dr["UtConocimientoTitulo"].ToString(),
                                UtTemporalId = dr["UtTemporalId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["UtTemporalId"]),
                                UtTemporalTitulo = dr["UtTemporalTitulo"].ToString(),
                                RequiereRespuesta = dr["RequiereRespuesta"] == DBNull.Value ? new bool?() : Convert.ToBoolean(dr["RequiereRespuesta"]),
                                ComentarioAsignacion = dr["ComentarioAsignacion"].ToString(),
                                UtApoyoId = dr["UtApoyoId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["UtApoyoId"]),
                                UtApoyoTitulo = dr["UtApoyoTitulo"].ToString(),
                                Plazo = dr["Plazo"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["Plazo"]),
                                PrioridadCod = dr["PrioridadId"].ToString(),
                                PrioridadTitulo = dr["PrioridadTitulo"].ToString(),
                                SolicitanteUrgenciaId = dr["SolicitanteUrgenciaId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["SolicitanteUrgenciaId"]),
                                SolicitanteUrgenciaNombre = dr["SolicitanteUrgenciaNombre"].ToString(),
                                AsignacionUtTemp = dr["AsignacionUtTemp"] == DBNull.Value ? new DateTime?() : Convert.ToDateTime(dr["AsignacionUtTemp"]),
                                ResponsableUtTempId = dr["ResponsableUtTempId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["ResponsableUtTempId"]),
                                ResponsableUtTempNombresApellidos = dr["ResponsableUtTempNombresApellidos"].ToString(),
                                RecepcionUtTemp = dr["RecepcionUtTemp"] == DBNull.Value ? new DateTime?() : Convert.ToDateTime(dr["RecepcionUtTemp"]),
                                ProfesionalTempId = dr["ProfesionalTempId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["ProfesionalTempId"]),
                                ProfesionalTempNombresApellidos = dr["ProfesionalTempNombresApellidos"].ToString(),
                                AsignacionProfesionalTemp = dr["AsignacionProfesionalTemp"] == DBNull.Value ? new DateTime?() : Convert.ToDateTime(dr["AsignacionProfesionalTemp"]),
                                Liberacion = dr["Liberacion"] == DBNull.Value ? new DateTime?() : Convert.ToDateTime(dr["Liberacion"]),
                                UtTransparenciaId = dr["UtTransparenciaId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["UtTransparenciaId"]),
                                UtTransparenciaTitulo = dr["UtTransparenciaTitulo"].ToString(),
                                ProfesionalTranspId = dr["ProfesionalTranspId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["ProfesionalTranspId"]),
                                ProfesionalTranspNombresApellidos = dr["ProfesionalTranspNombresApellidos"].ToString(),
                                ObservacionesTransparencia = dr["ObservacionesTransparencia"].ToString(),
                                ResponsableUtId = dr["ResponsableUtId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["ResponsableUtId"]),
                                ResponsableNombre = dr["ResponsableNombre"].ToString(),
                                RecepcionUt = dr["RecepcionUt"] == DBNull.Value ? new DateTime?() : Convert.ToDateTime(dr["RecepcionUt"]),
                                ProfesionalId = dr["ProfesionalId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["ProfesionalId"]),
                                ProfesionalNombre = dr["ProfesionalNombre"].ToString(),
                                AsignacionResponsable = dr["AsignacionResponsable"] == DBNull.Value ? new DateTime?() : Convert.ToDateTime(dr["AsignacionResponsable"]),
                                ComentarioEncargadoUt = dr["ComentarioEncargadoUt"].ToString(),
                                RequiereTimbrajePlano = dr["RequiereTimbrajePlano"] == DBNull.Value ? new bool?() : Convert.ToBoolean(dr["RequiereTimbrajePlano"]),
                                Cierre = dr["Cierre"] == DBNull.Value ? new DateTime?() : Convert.ToDateTime(dr["Cierre"]),
                                CerradoPorId = dr["CerradoPorId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["CerradoPorId"]),
                                CerradoPor = dr["CerradoPor"].ToString(),
                                MotivoCierreId = dr["MotivoCierreId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["MotivoCierreId"]),
                                MotivoCierreTitulo = dr["MotivoCierreTitulo"].ToString(),
                                ComentarioCierre = dr["ComentarioCierre"].ToString(),
                                ModificadoPorFecha = Convert.ToDateTime(dr["ModificadoPorFecha"]),
                                SolicitudTramId = dr["SolicitudTramId"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["SolicitudTramId"])
                            });
                        };
                    }
                }
            }

            return ingresos;
        }

        public NotificacionEmailDto GetNotificacionByCodigo(string codigo)
        {
            NotificacionEmailDto notif = null;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = new SqlCommand(string.Format("SELECT * FROM NotificacionEmail WHERE Codigo = '{0}' AND Activo = 1", codigo), con))
                {

                    SqlDataReader dr = null;
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    try
                    {
                        dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    }
                    catch (SqlException sex)
                    {
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                    }

                    if (dr != null)
                    {
                        while (dr.Read())
                        {
                            notif = new NotificacionEmailDto()
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Activo = Convert.ToBoolean(dr["Activo"]),
                                Asunto = Convert.ToString(dr["Asunto"]),
                                Codigo = Convert.ToString(dr["Codigo"]),
                                Descripcion = Convert.ToString(dr["Descripcion"]),
                                Mensaje = Convert.ToString(dr["Mensaje"]),
                                Periodicidad = dr["Periodicidad"] == DBNull.Value ? new int?() : Convert.ToInt32(dr["Periodicidad"]),
                                UsuarioActual = "Servicio de Notificaciones"
                            };
                        };

                        // Este servicio se implementó asumiendo q la periodicidad está definida en minutos, pero luego
                        // el cliente indicó q debía estar expresada en días y así se guarda ahora. Por tanto, lo siguiente
                        // es para llevar de días a minutos y conservar todo lo implementado aquí
                        if (notif != null)
                            notif.Periodicidad = notif.Periodicidad.HasValue
                                ? (notif.Periodicidad * 24 * 60)
                                : notif.Periodicidad;
                    }
                }
            }
            return notif;
        }

        public DateTime? GetUltimaNotificacionRegistroByReqId(string codigo, int reqId)
        {
            DateTime? fue = null;

            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = new SqlCommand(string.Format("SELECT MAX(FechaUltimoEnvio) AS FechaUltimoEnvio FROM NotificacionEmailRegistro NER " +
                    "INNER JOIN NotificacionEmail NE ON NER.NotificacionEmailId = NE.Id WHERE NE.Codigo = '{0}' AND NER.RequerimientoId = {1}", codigo, reqId), con))
                {

                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    try
                    {
                        var retorno = cmd.ExecuteScalar();
                        con.Close();
                        if (retorno != DBNull.Value && retorno != null)
                            fue = Convert.ToDateTime(retorno);
                    }
                    catch (SqlException sex)
                    {
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                    }
                }
            }

            return fue;
        }

        public bool SetUltimaNotificacionRegistro(string codigo, int reqId)
        {
            var modified = 0;

            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = new SqlCommand(string.Format("INSERT NotificacionEmailRegistro (NotificacionEmailId,RequerimientoId,FechaUltimoEnvio) VALUES ((SELECT TOP 1 Id FROM NotificacionEmail WHERE Codigo = '{0}'),{1},'{2}')", codigo, reqId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), con))
                {

                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    try
                    {
                        modified = cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (SqlException sex)
                    {
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                    }
                }
            }

            return modified > 0;
        }

        public UsuarioDto GetUsuarioById(int id)
        {
            UsuarioDto usuario = null;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = new SqlCommand(string.Format("SELECT * FROM Usuario WHERE Id = '{0}' AND Activo = 1", id), con))
                {

                    SqlDataReader dr = null;
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    try
                    {
                        dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    }
                    catch (SqlException sex)
                    {
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                    }

                    if (dr != null)
                    {
                        while (dr.Read())
                        {
                            usuario = new UsuarioDto()
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Activo = Convert.ToBoolean(dr["Activo"]),
                                Email = Convert.ToString(dr["Email"]),
                                NombresApellidos = Convert.ToString(dr["NombresApellidos"]),
                                Username = Convert.ToString(dr["Username"]),
                            };
                        };
                    }
                }
            }

            return usuario;
        }

    }
}
