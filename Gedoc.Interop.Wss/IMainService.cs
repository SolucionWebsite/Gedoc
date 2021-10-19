using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Gedoc.Interop.Wss.Data;

namespace Gedoc.Interop.Wss
{
    [ServiceContract(Namespace = "http://dibam.cl/gedocservice")]
    public interface IMainService
    {
        /// <summary>
        /// Devuelve los datos del ingreso especificado
        /// </summary>
        /// <param name="numero">Número de ingreso a devolver los datos</param>
        /// <returns>Datos del ingreso especificado</returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/ingreso/{numero}")]
        [return: MessageParameter(Name = "datos")]
        ResultadoDatosIngreso GetIngreso(string numero);

        /// <summary>
        /// Devuelve el estado de la solicitud especificada
        /// </summary>
        /// <param name="idSolicitud">Id de la solicitud a devolver el estado</param>
        /// <returns>Estado de la solicitud especificada</returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/solicitud/{idSolicitud}")]
        [return: MessageParameter(Name = "datos")]
        ResultadoEstadoSolicitud GetEstadoSolicitud(string idSolicitud);

        /// <summary>
        /// Devuelve el fichero de Despacho especificado
        /// </summary>
        /// <param name="numero">Número del despacho</param>
        /// <param name="clave">Clave de seguridad</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/despachoadj/{numero}?clave={clave}")]
        [return: MessageParameter(Name = "datos")]
        Stream GetAdjuntoIngreso(string numero, string clave);

        /// <summary>
        /// Crea un nuevo ingreso de acuerdo a los datos especificados
        /// </summary>
        /// <param name="datos">Datos del nuevo ingreso</param>
        /// <returns>Resultado de la operación de crear el nuevo ingreso.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/ingreso")]
        ResultadoNuevoIngreso NuevoIngreso(DatosNuevoIngreso datos);

    }

}
