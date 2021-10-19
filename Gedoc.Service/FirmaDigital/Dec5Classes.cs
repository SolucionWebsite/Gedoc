using System;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serialization;

namespace Gedoc.Service.FirmaDigital
{
    public class Dec5Response
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string Session_id { get; set; }
        public object Result { get; set; }
    }
    public class DocInfoDec5Response
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string Session_id { get; set; }
        public DocumentResult Result { get; set; }
    }

    public class LoginResult
    {
        public string User_name { get; set; }
        public string User_rut { get; set; }
        public string Audit { get; set; }
        public string[] Institutions { get; set; }
    }

    public class CreateDocResult
    {
        public string Code { get; set; }
        public string File { get; set; }
        public string File_mime { get; set; }
    }

    public class DocumentResult {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Type_code { get; set; }
        public string Country_code { get; set; }
        public int state { get; set; }
        public string Date { get; set; } // Formato:yyyy/mm/dd hh:ii
        public string End_date { get; set; } // Plazo de Disponibilidad
        public string Can_sign { get; set; } // Usuario en sesión está habilitado para firmar: 0 - No puede firmar/visar/rechazar documento   1 - Puede firmar/visar/rechazar documento
        public UserInfo[] Can_sign_info { get; set; }
        public string Security { get; set; }  //  Seguridad del documento:  0 - Baja  1 - Media  2 - Alta
        public string File_preview { get; set; }
        public UserInfo Creator { get; set; }
	public UserInfo[] Signers { get; set; }
        public string Preview { get; set; }
        public string Preview_mime { get; set; }
        public string File { get; set; } // Archivo del documento en base64
        public string File_mime { get; set; }
        public string File_size { get; set; }
        public string Md5 { get; set; }



    }

    public class Signers
    {
        public string[] Roles { get; set; }
        public string[] Institutions { get; set; }
        public string[] Emails { get; set; }
        public string[] Ruts { get; set; }
        public int[] Types { get; set; }
        public int[] Order { get; set; }
        public int[] Notify { get; set; }
    }

    public class UserInfo
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Institution { get; set; }
        public int Type { get; set; } // Tipo de Acción  0 - Firmar   2 - Visar
        public string Rut { get; set; }
        public string Email { get; set; }
    }

    // Tipos de firma para cada firmante
    public enum SignerType
    {
        Firma = 0,
        Visa = 2,
        Compartido = 5,
        FirmarPin = 10,
        FirmarHuella = 11,
        FirmarHsm = 12,
        FirmarToken = 13,
        FirmarFirmaMovilAv = 20,
        FirmarClaveUnica = 21,
        VisarPin = 32,
        VisarHuella = 42,
        VisarClaveUnica = 52

    }

    public enum DocumentState
    {
        PendienteFirma = 0,
        EnProcesoFirma = 1,
        Aprobado = 2,
        Rechazado = 3
    }

    public class JsonNetSerializer : IRestSerializer
    {
        public string Serialize(object obj) =>
            JsonConvert.SerializeObject(obj);

        public string Serialize(Parameter bodyParameter) =>
            JsonConvert.SerializeObject(bodyParameter.Value);

        public T Deserialize<T>(IRestResponse response) =>
            JsonConvert.DeserializeObject<T>(response.Content);

        public string[] SupportedContentTypes { get; } =
        {
            "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
        };

        public string ContentType { get; set; } = "application/json";

        public DataFormat DataFormat { get; } = DataFormat.Json;
    }

    public class SignerInfo
    {
        public SignerInfo(string rut, string pin, string email)
        {
            Rut = rut.Replace(".", "").Replace("-", "");
            Rut = Rut.Insert(Rut.Length - 1, "-");
            Pin = pin;
            Email = string.IsNullOrEmpty(email) ? "noreply@dibam.cl" : email;
        }
        public string Rut { get; set; }
        public string Pin { get; set; }
        public string Email { get; set; }
    }

}