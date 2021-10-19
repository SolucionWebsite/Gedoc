using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers
{
    public class WebConfigValues
    {
        #region Datos para envío de emails
        public static string SmtpClientHost
        {
            get { return GetStrintConfigData("SmtpClient_Host"); }
        }

        public static int SmtpClientPort
        {
            get { return GetIntConfigData("SmtpClient_Port"); }
        }

        public static bool SmtpClientEnableSsl
        {
            get { return GetBooleanConfigData("SmtpClient_EnableSsl"); }
        }

        public static string SmtpClientUser
        {
            get { return GetStrintConfigData("SmtpClient_User"); }
        }

        public static string SmtpClientPassword
        {
            get { return GetStrintConfigData("SmtpClient_Password"); }
        }

        public static string RemitenteEmail
        {
            get { return GetStrintConfigData("RemitenteEmail"); }
        }

        public static string RemitenteNombre
        {
            get { return GetStrintConfigData("RemitenteNombre"); }
        }

        public static bool EsEmailDesarrollo
        {
            get { return GetBooleanConfigData("Desa.isEmailDesarrollo"); }
        }

        public static string EmailDesarrollo
        {
            get { return GetStrintConfigData("Desa.EmailDesarrollo"); }
        }

        public static string DestinatariosEmailsPara
        {
            get { return GetStrintConfigData("DestinatariosEmails.Para"); }
        }

        public static string DestinatariosEmailsCopia
        {
            get { return GetStrintConfigData("DestinatariosEmails.Copia"); }
        }

        public static string DestinatariosEmailsCopiaOculta
        {
            get { return GetStrintConfigData("DestinatariosEmails.CopiaOculta"); }
        }
        #endregion


        #region Opciones generales
        public static int IdSistemaGedoc
        {
            get { return GetIntConfigData("IdSistemaGedoc"); }
        }

        public static int DiasHabilesUltRegistros
        {
            get
            {
                var valor = GetIntConfigData("DiasHabilesUltRegistros");
                return valor == 0 ? 180 : valor;
            }
        }

        public static string[] AdjuntosPermitidos
        {
            get
            {
                var arrResult = GetArrayConfigData("AdjuntosPermitidos");
                return arrResult.Length == 0 ? new string[] { ".doc", ".docx", ".xls", ".xlsx", ".dwg", ".ppt", ".pptx", ".pdf", ".eml", ".msg" } : arrResult;
            }
        }

        public static int SqlCommandTimeOut
        {
            get
            {
                var valor = GetIntConfigData("SqlCommandTimeOut");
                return valor == 0 ? 300 : valor;
            }
        }
        #endregion

        #region Autentificación

        public static string Url_MenuSistemas
        {
            get { return GetStrintConfigData("Url_MenuSistemas"); }
        }

        public static string UrlAuthentication
        {
            get { return GetStrintConfigData("LoginAuthenticationSystem"); }
        }

        public static string LogOffAuthenticationSystem
        {
            get { return GetStrintConfigData("LogOffAuthenticationSystem"); }
        }


        public static string Auth_Token
        {
            get { return GetStrintConfigData("Auth_Token"); }
        }

        public static string Auth_Token_Validate
        {
            get { return GetStrintConfigData("Auth_Token_Validate"); }
        }

        public static string LDAPPath
        {
            get { return GetStrintConfigData("LDAPPath"); }
        }

        public static string ADAdminUser
        {
            get { return GetStrintConfigData("ADAdminUser"); }
        }

        public static string ADAdminPassword
        {
            get { return GetStrintConfigData("ADAdminPassword"); }
        }
        #endregion

        #region Interoperación siatema de Trámites

        public static string UrlAuthTramitesWeb
        {
            get { return GetStrintConfigData("UrlAuthTramitesWeb"); }
        }

        public static string SecretKeyTramitesWeb
        {
            get { return GetStrintConfigData("SecretKeyTramitesWeb"); }
        }

        public static string UrlTramitesWeb
        {
            get { return GetStrintConfigData("UrlTramitesWeb"); }
        }

        #endregion

        #region Opciones de PDF de Oficios
        public static string HiQPdfSerialNumber
        {
            get { return GetStrintConfigData("HiQPdf.SerialNumber"); }
        }
        public static string PdfOficioPaperSize
        {
            get { return GetStrintConfigData("Pdf.Oficio.PaperSize"); }
        }
        public static string PdfOficioMargenSup
        {
            get { return GetStrintConfigData("Pdf.Oficio.MargenSuperior"); }
        }
        public static string PdfOficioMargenDer
        {
            get { return GetStrintConfigData("Pdf.Oficio.MargenDerecho"); }
        }
        public static string PdfOficioMargenInf
        {
            get { return GetStrintConfigData("Pdf.Oficio.MargenInferior"); }
        }

        public static string PdfOficioMargenIzq
        {
            get { return GetStrintConfigData("Pdf.Oficio.MargenIzquierdo"); }
        }
        #endregion

        #region Firma digital
        public static string FirmaDigApiKey
        {
            get { return GetStrintConfigData("FirmaDig.ApiKey"); }
        }

        public static string FirmaDigUrlBase
        {
            get { return GetStrintConfigData("FirmaDig.UrlBase"); }
        }

        public static string FirmaDigLoginName
        {
            get { return GetStrintConfigData("FirmaDig.LoginName"); }
        }

        public static string FirmaDigLoginPin
        {
            get { return GetStrintConfigData("FirmaDig.LoginPin"); }
        }

        public static List<string> FirmaDigRoleFirmante
        {
            get
            {
                var arrResult = GetArrayConfigData("FirmaDig.RolesFirmante");
                return arrResult.ToList();
            }
        }

        public static string FirmaDigInstitucion
        {
            get { return GetStrintConfigData("FirmaDig.Institucion"); }
        }

        public static string FirmaDigTipoDoc
        {
            get { return GetStrintConfigData("FirmaDig.TipoDocumento"); }
        }
        #endregion

        #region String keys
        public static string RegmonUrl
        {
            get { return GetStrintConfigData("RegmonUrl"); }
        }

        public static string CategoriasMnBusqRegMon
        {
            get
            {
                return GetStrintConfigData("CategoriasBusqueda");
            }
        }

        public static string UsuariosEditaReqSinRestriccion
        {
            get
            {
                return GetStrintConfigData("UsuariosEditaReqSinRestriccion");
            }
        }

        #region Opciones de repositorio y conexión a Sharepoint

        public static bool EsRepositorioLocal
        {
            get { return GetBooleanConfigData("esRepositorioLocal"); }
        }

        public static string PathRepositorioLocal
        {
            get { return GetStrintConfigData("PathRepositorioLocal"); }
        }

        public static string SP_URL
        {
            get { return GetStrintConfigData("SP_URL"); }
        }

        public static string SP_User
        {
            get { return GetStrintConfigData("SP_User"); }
        }

        public static string SP_Pass
        {
            get { return GetStrintConfigData("SP_Pass"); }
        }

        public static string SP_Domain
        {
            get { return GetStrintConfigData("SP_Domain"); }
        }

        public static string SP_Biblioteca
        {
            get { return GetStrintConfigData("SP_Biblioteca"); }
        }

        public static int SP_TimeOut
        {
            get { return GetIntConfigData("SP_Tiemout"); }
        }
        #endregion


        #endregion


        #region Int keys

        public static int Ut_Transparencia
        {
            get { return GetIntConfigData("Ut_Transparencia"); }
        }

        public static int Ut_JefaturaCmn
        {
            get { return GetIntConfigData("Ut_JefaturaCmn"); }
        }

        #endregion


        #region Boolean keys

        #endregion


        #region Arrays keys
        public static List<string> CategoriasMnBusqRegMonArr
        {
            get
            {
                var arrResult = GetArrayConfigData("CategoriasBusqueda");
                return arrResult.ToList();
            }
        }


        #endregion

        private static bool GetBooleanConfigData(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                return bool.Parse(ConfigurationManager.AppSettings[key]);
            }
            else
            {
                return false;
            }
        }

        private static int GetIntConfigData(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                var valorStr = ConfigurationManager.AppSettings[key];
                int.TryParse(valorStr, out var valorInt);
                return Convert.ToInt32(valorInt);
            }
            else
            {
                return 0;
            }
        }

        private static string GetStrintConfigData(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                return ConfigurationManager.AppSettings[key];
            }
            else
            {
                return string.Empty;
            }
        }

        private static string[] GetArrayConfigData(string key)
        {
            string[] retorno = new string[0];

            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                retorno = ConfigurationManager.AppSettings[key].Split(',');
            }

            return retorno;
        }
    }
}
