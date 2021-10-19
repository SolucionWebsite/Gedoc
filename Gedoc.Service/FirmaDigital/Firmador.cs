using System;
using System.Collections.Generic;
using System.IO;
using Gedoc.Helpers;

namespace Gedoc.Service.FirmaDigital
{
    public class Firmador
    {
        private readonly string loginName = ""; 

        public ResultadoOperacion Firmar(string docName, Stream file, SignerInfo signerInfo)
        {
            file.Position = 0;
            var br = new BinaryReader(file);
            var bytes = br.ReadBytes((int)file.Length);
            return Firmar(docName, bytes, signerInfo);
        }

        public ResultadoOperacion Firmar(string docName, byte[] file, SignerInfo signerInfo)
        {
            var resultado = new ResultadoOperacion(1, "Documento firmado con éxito.", null);
            // El Pin del usuario en el firmador se almacena encriptado en la BD de Gedoc, es necesario desencriptarlo:
            signerInfo.Pin = AESThenHMAC.SimpleDecryptWithPassword(signerInfo.Pin, GeneralData.SEC_KEY);
            var firmadorClient = new Dec5Client(signerInfo);

            string base64String = Convert.ToBase64String(file, 0, file.Length);
            var resultadoApi = firmadorClient.CreateSignHsm(true, docName, base64String);

            if (resultadoApi.Status == 200)
            {
                // Creación y firma del oficio Ok
                if (resultadoApi.Result is CreateDocResult result)
                {
                    // Se consultan los datos del documento creado para tomar la URL del documento
                    var resultInfoDoc = ConsultaInfoDoc(result.Code, firmadorClient);

                    resultado.Extra = new Dictionary<string, object>
                    {
                        { "code", result.Code},
                        { "file", result.File},
                        { "url_preview", (resultInfoDoc.Extra as DocumentResult)?.File_preview }
                    };
                } else if (resultadoApi.Result is Dictionary<string, object> resultDic)
                {
                    // Se consultan los datos del documento creado para tomar la URL del documento
                    // var resultInfoDoc = ConsultaInfoDoc(resultDic.ContainsKey("code") ? resultDic["code"].ToString() : "", firmadorClient);
                    resultDic["url_preview"] = ""; //(resultInfoDoc.Extra as DocumentResult)?.File_preview;

                    resultado.Extra = resultDic;
                    //resultDic.TryGetValue("code", out var code);
                    //resultDic.TryGetValue("file", out var fileSigned);
                    //resultado.Extra = new CreateDocResult { Code = code?.ToString(), File = fileSigned?.ToString() };
                }
            }
            else
            {
                // hubo algún error
                if (resultadoApi.Result == null)
                {
                    // hubo error en la llamada a la api, no se contractó a la api, por ej error url not found
                    resultado.Codigo = -1;
                    resultado.Mensaje = "Ha ocurrido un error en la llamada al servicio de firma digital. Error devuelto:  <br/>" + resultadoApi.Message;
                }
                else
                {
                    // Se ejecutó la llamada a la api del firmador pero este devolvió algún error
                    // hubo error al crear o firmar el oficio, la llamada a la api está bien.
                    var result = resultadoApi.Result as CreateDocResult;
                    var resultDic = resultadoApi.Result as Dictionary<string, object>;
                    if ((result != null && !string.IsNullOrWhiteSpace(result.Code)) ||
                        (resultDic != null && resultDic.ContainsKey("code")))
                    {
                        // hubo error en la firma pero se creó el documento en el firmador
                        var docCode = result != null ? result.Code : resultDic["code"].ToString();
                        resultado.Codigo = -2;
                        //// Se consultan los datos del documento creado para tomar el documento firmado
                        //var resultInfoDoc = ConsultaInfoDoc(docCode, firmadorClient, true);
                        //resultado.Extra = new Dictionary<string, object>
                        //{
                        //    { "code", docCode},
                        //    { "file", (resultInfoDoc.Extra is DocumentResult docResult) ? docResult.File : ""}
                        //};
                        resultado.Extra = new Dictionary<string, object>
                        {
                            { "code", docCode},
                            { "file", ""}
                        };

                        resultado.Mensaje = /*"No fue posible firmar en estos momentos el documento. Error devuelto: <br/>" +*/ resultadoApi.Message;
                    }
                    else
                    {
                        resultado.Codigo = -1;
                        resultado.Mensaje = "Ha ocurrido un error en la llamada al servicio de firma digital. Error devuelto: <br/>" + resultadoApi.Message;
                    }
                }
            }

            return resultado;
        }

        public ResultadoOperacion ConsultaInfoDoc(string docCode, Dec5Client dec5Client = null, bool obtenerDoc = false)
        {
            var resultado = new ResultadoOperacion(1, "Ok.", null);
            var firmadorClient = dec5Client ?? new Dec5Client(null);
            var resultadoApi = firmadorClient.GetDocumentoInfo(docCode, obtenerDoc);

            if (resultadoApi.Status == 200)
            {
                // Info de doc recibida ok
                if (resultadoApi.Result is DocumentResult result)
                {
                    resultado.Extra = resultadoApi.Result;
                }
            }
            else
            {
                // hubo algún error
                if (resultadoApi.Result == null)
                {
                    // hubo error en la llamada a la api, no se contractó a la api, por ej error url not found
                    resultado.Codigo = -1;
                    resultado.Mensaje = "Ha ocurrido un error en la llamada al servicio de firma digital. Error devuelto:  <br/>" + resultadoApi.Message;
                }
                else
                {
                    // Se ejecutó la llamada a la api del firmador pero este devolvió algún error
                    resultado.Codigo = -1;
                    resultado.Mensaje = "Ha ocurrido un error en la llamada al servicio de firma digital. Error devuelto: <br/>" + resultadoApi.Message;
                }
            }

            return resultado;
        }

        public ResultadoOperacion EliminarDocumento(string docCode)
        {
            var resultado = new ResultadoOperacion(1, "Documento eliminado con éxito.", null);
            var firmadorClient = new Dec5Client(null);
            var resultadoApi = firmadorClient.EliminarDocumento(docCode, true);

            if (resultadoApi.Status == 200)
            {
                // Doc. eliminado Ok
            }
            else
            {
                // hubo algún error
                if (resultadoApi.Result == null)
                {
                    // hubo error en la llamada a la api, no se contractó a la api, por ej error url not found
                    resultado.Codigo = -1;
                    resultado.Mensaje = "Ha ocurrido un error en la llamada al servicio de firma digital. Error devuelto:  <br/>" + resultadoApi.Message;
                }
                else
                {
                    // Se ejecutó la llamada a la api del firmador pero este devolvió algún error
                    resultado.Codigo = -1;
                    resultado.Mensaje = "Ha ocurrido un error en la llamada al servicio de firma digital. Error devuelto: <br/>" + resultadoApi.Message;
                }
            }

            return resultado;
        }

    }
}