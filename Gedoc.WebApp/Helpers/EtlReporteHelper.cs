using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using Gedoc.Helpers.Logging;
using Gedoc.WebApp.ServiceReferenceEtlReporte;

namespace Gedoc.WebApp.Helpers
{
    public class EtlReporteHelper
    {

        #region Interacción con el servicio Windows de Gedoc Etl
        public static string ProcesaPeticionSrvEtl(int idPeticion)
        {
            var texto = "ERROR";
            var bindingName = "NetTcpBinding_GedocEtl";
            var myChannelFactory = new ChannelFactory<IServiceInteract>(bindingName);

            IServiceInteract client = null;
            client = myChannelFactory.CreateChannel();

            try
            {
                if (idPeticion == 0) // Ejecutar Carga de Datos
                {
                    texto = client.ExecuteEtl();
                    if (texto == "OK")
                    {
                        texto = "Carga de datos finalizada.";
                    }
                    else
                    {
                        texto = "Error al realizar la carga de datos.";
                    }
                }
                else // Obtener estado del servicio
                {
                    texto = client.GetEstadoEjecucion();
                }
                ((ICommunicationObject)client).Close();
            }
            catch (EndpointNotFoundException exc)
            {
                Logger.LogError(exc);
                texto = "[No ha sido posible conectar con el servicio de carga de datos]";
                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                texto = "[Error al realizar la operación.]";
                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
            }
            return texto;
        }
        public static string ProcesaPeticionSrvEtlSelectivo(string[] destinos)
        {
            var texto = "ERROR";
            var bindingName = "NetTcpBinding_GedocEtl";
            var myChannelFactory = new ChannelFactory<IServiceInteract>(bindingName);

            IServiceInteract client = null;
            client = myChannelFactory.CreateChannel();

            try
            {
                texto = client.ExecuteEtlSelectivo(destinos);
                if (texto == "OK")
                {
                    texto = "Carga de datos finalizada.";
                }
                else
                {
                    texto = "Error al realizar la carga de datos.";
                }
                ((ICommunicationObject)client).Close();
            }
            catch (EndpointNotFoundException exc)
            {
                Logger.LogError(exc);
                texto = "[No ha sido posible conectar con el servicio de carga de datos]";
                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                texto = "[Error al realizar la operación.]";
                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
            }
            return texto;
        }
        #endregion
    }
}