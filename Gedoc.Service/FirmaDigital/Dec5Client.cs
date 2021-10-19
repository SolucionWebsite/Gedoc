using System.Net;
using Gedoc.Helpers;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Gedoc.Service.FirmaDigital
{
    public class Dec5Client
    {
        private IRestClient _client;
        private string _sessionId = "";
        private SignerInfo _signerInfo;

        private readonly string _accessToken = WebConfigValues.FirmaDigApiKey;

        private readonly string _apiurlbase = WebConfigValues.FirmaDigUrlBase;

        // Credenciales para login en firmador
        private readonly string _loginUserName = WebConfigValues.FirmaDigLoginName;

        private readonly string _loginUserPin = WebConfigValues.FirmaDigLoginPin;

        private readonly string _userRole = WebConfigValues.FirmaDigRoleFirmante.Count > 0
            ? WebConfigValues.FirmaDigRoleFirmante[0]
            : "";

        private readonly string _intitution = WebConfigValues.FirmaDigInstitucion;

        // TODO: Código de Tipo de Documento del documento a firmar. Precisar si siempre se va a asociar los Oficios de
        // Gedoc a un mismo tipo de documento ya existente en el firmador, o el tipo de documento va a depender de algún
        // dato del Oficio de Gedoc. PAra este último caso precisar entonces si se maneja desde Gedoc la creación de nuevos tipos de
        // documentos en el firmador. Ahora se deja un único tipo de documento para todos los documentos de Gedoc a firmar
        private readonly string _typecode = WebConfigValues.FirmaDigTipoDoc;

        public Dec5Client(SignerInfo signerInfo)
        {
            _sessionId = Login();
            _signerInfo = signerInfo;
        }

        private IRestClient Client
        {
            get
            {
                _client = _client ?? new RestClient(
                              _apiurlbase); //.UseNewtonsoftJson(); //.UseSerializer(new JsonNetSerializer());
                return _client;
            }
        }

        public string Login()
        {
            var request = new RestRequest("auth/login", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-API-KEY", _accessToken);

            request.AddParameter("user_name", _loginUserName);
            request.AddParameter("user_pin", _loginUserPin);

            var response = Client.Execute<Dec5Response>(request);

            return response.StatusCode == HttpStatusCode.OK
                ? response.Data.Session_id
                : "";
        }

        public Dec5Response CreateSignHsm(bool returnFile, string docName, string docBase64)
        {
            if (string.IsNullOrWhiteSpace(_sessionId))
            {
                return new Dec5Response {Status = -1, Message = "No hay sesión activa"};
            }

            var request = new RestRequest("documents/create_sign_hsm", Method.POST);
            //var request = new RestRequest("documents/create", Method.POST); // Crear documento, sin firmar
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-API-KEY", _accessToken);

            var signers = GetSigners();
            request.AddJsonBody(
                new
                {
                    session_id = _sessionId,
                    institution = _intitution,
                    user_rut = _signerInfo.Rut,
                    user_pin = _signerInfo.Pin,
                    return_file = returnFile ? 1 : 0,
                    type_code = _typecode,
                    name = docName,
                    comment = "DOCUMENTO GENERADO DESDE GEDOC",
                    signers_roles = signers.Roles,
                    signers_institutions = signers.Institutions,
                    signers_ruts = signers.Ruts,
                    signers_emails = signers.Emails,
                    signers_type = signers.Types,
                    signers_notify = signers.Notify,
                    signers_order = signers.Order,

                    file = docBase64,
                    file_mime = "application/pdf",
                });

            var response = Client.Execute<Dec5Response>(request);

            return response.Data != null && response.Data.Status > 0
                ? response.Data
                : new Dec5Response {Status = (int) response.StatusCode, Message = response.ErrorMessage};
        }

        public DocInfoDec5Response GetDocumentoInfo(string docCode, bool obtenerDoc)
        {
            var request = new RestRequest("documents", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-API-KEY", _accessToken);

            request.AddParameter("session_id", _sessionId);
            request.AddParameter("institution", _intitution);
            request.AddParameter("code", docCode);
            //Información adicional de los documentos, separados por coma
            //    - signers : Firmantes del documento
            //    - file : Archivo
            //    - thumb : Thumbnail del archivo
            //    - tags : Tags del documento
            //    - folders : Carpetas asociadas al documento
            //    -related : Documentos relacionados
            //    -comments : Comentarios del documento
            //    - qr{ size} : Codigo QR en base64, donde { size} es el tamaño de imagen(min 50, max 400, default 196) Ej.qr196
            request.AddParameter("extra", "signers" + (obtenerDoc ? ",file" : ""));

            Client.UseNewtonsoftJson();
            var response = Client.Execute<DocInfoDec5Response>(request);

            return response.Data is DocInfoDec5Response && response.Data.Status > 0
                ? ((DocInfoDec5Response) response.Data)
                : new DocInfoDec5Response {Status = (int) response.StatusCode, Message = response.ErrorMessage};
        }

        public Dec5Response EliminarDocumento(string docCode, bool aPapelera)
        {
            var request = new RestRequest(aPapelera ? "documents/papelera" : "documents/eliminar_archivo", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-API-KEY", _accessToken);

            request.AddParameter("session_id", _sessionId);
            request.AddParameter("code", docCode);

            var response = Client.Execute<Dec5Response>(request);

            return response.Data != null && response.Data.Status > 0
                ? response.Data
                : new Dec5Response {Status = (int) response.StatusCode, Message = response.ErrorMessage};
        }

        private Signers GetSigners()
        {
            // TODO: precisar los datos del firmante (o los firmantes) a quien corresponden: al usuario de Jefatura CMN q está
            // enviando a firma, al director CMN, u otro.
            return new Signers
            {
                Roles = new[] {_userRole}, // **************
                Institutions = new[] {"SNPC"},
                Ruts = new[] {_signerInfo.Rut}, // ***
                Emails = new[]
                {
                    _signerInfo.Email
                }, // ** si se especifca email "any" da error de "El rut que firma no está definido dentro de los firmantas del documento"
                Types = new[] {(int) SignerType.Firma},
                Notify = new[] {0}, // TODO: ¿notificar?
                Order = new[] {1}
            };

            // Ejemplo utilizando más de un firmante:
            //return new Signers
            //{
            //    Roles = new[] { "DIRECTOR NACIONAL", "VISUALIZA ACTA" },
            //    Institutions = new[] { "SNPC", "SNPC" },
            //    Ruts = new[] { "16955079-4", "any" },
            //    Emails = new[] { "elias.figueroa@patrimoniocultural.gob.cl", "any" },
            //    Types = new[] { (int)SignerType.Firma, (int)SignerType.Compartido },
            //    Notify = new[] { 0, 0 },
            //    Order = new[] { 1, 0 }
            //};
        }
    }
}