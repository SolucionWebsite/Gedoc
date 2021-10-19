using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Logging;
using Gedoc.Repositorio.Interfaces;

namespace Gedoc.Service.EmailService
{
    public interface INotificacionService
    {

        Dictionary<string, string> VariablesMensaje {get; set;}
        string UsuarioActual { get; set; }

        #region Notificaciones Requerimientos
        ResultadoOperacion NotificacionCierre(RequerimientoDto requerimiento);
        ResultadoOperacion NotificacionAsignacionUt(RequerimientoDto requerimiento, bool esReasignacion);
        ResultadoOperacion NotificacionProcesoMasivo(Dictionary<string, string> variablesEmail,
            int[] idDestinatarios, int[] idUnidadTec, string adjunto);
        ResultadoOperacion NotificacionAsignacionProfUt(RequerimientoDto requerimiento);
        ResultadoOperacion NotificacionAsignacionTemp(RequerimientoDto requerimiento);
        ResultadoOperacion NotificacionAsignacionUtApoyo(RequerimientoDto requerimiento);
        ResultadoOperacion NotificacionAsignacionUtCopia(RequerimientoDto requerimiento, int utNotificar);
        ResultadoOperacion NotificacionAsignacionUtConoc(RequerimientoDto requerimiento);
        ResultadoOperacion NotificacionLiberacionAsignacionTemporal(RequerimientoDto requerimiento);
        ResultadoOperacion NotificacionReasignacion(RequerimientoDto requerimiento);
        #endregion

        #region Notificaciones Despachos
        ResultadoOperacion NotificacionDespachoNuevo(DespachoDto despacho);
        ResultadoOperacion NotificacionDespachoInicNuevo(DespachoIniciativaDto despacho);
        ResultadoOperacion NotificacionDestCopiaDespachoNuevo(DespachoDto despacho);
        ResultadoOperacion NotificacionDestCopiaDespachoinicNuevo(DespachoIniciativaDto despacho);
        ResultadoOperacion NotificacionDespachoCierre(DespachoDto despacho);
        ResultadoOperacion NotificacionDestCopiaDespachoCierre(DespachoDto despacho);
        ResultadoOperacion NotificacionDespachoInicCierre(DespachoIniciativaDto despacho);
        ResultadoOperacion NotificacionDestCopiaDespachoInicCierre(DespachoIniciativaDto despacho);
        ResultadoOperacion NotificacionCambioPriorizacion(RequerimientoDto requerimiento);
        ResultadoOperacion NotificacionForzarPriorizacion(RequerimientoDto requerimiento);
        #endregion

        ResultadoOperacion EnviarNotificacionProcesosMasivos(int idProceso, int[] idIngresos,
            int idUtAsign, int idUtReAsign, int idProfAsig, int idProfReAsig);



    }
}