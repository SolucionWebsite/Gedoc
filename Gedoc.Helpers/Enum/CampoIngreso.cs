using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Enum
{
    public enum CampoIngreso
    {
        PanelDocumento,
        Doc_DocIng, // el aspecto por defecto es solo lectura
        Doc_FechaIng,  // el aspecto por defecto es solo lectura
        Doc_TipoTramite, // el aspecto por defecto es visible y habilitado
        Doc_CanalLlegada,  // el aspecto por defecto es deshabilitado
        Doc_TipoDoc, // el aspecto por defecto es visible y habilitado
        Doc_Siac, // el aspecto por defecto es visible y habilitado
        Doc_ObsTipoDoc, // el aspecto por defecto es visible y habilitado
        Doc_FechaDoc, // el aspecto por defecto es visible y habilitado
        Doc_SoloMesAnno, // el aspecto por defecto es visible y habilitado, depende generalmente de FechaDoc
        Doc_Estado, // el aspecto por defecto es oculto
        Doc_Etapa, // el aspecto por defecto es oculto
        PanelAdjuntos,
        Adj_AdjuntaDoc, // el aspecto por defecto es visible y habilitado
        Adj_CantAdj, // el aspecto por defecto es visible y habilitado
        Adj_TipoAdj, // el aspecto por defecto es visible y habilitado
        Adj_Soporte, // el aspecto por defecto es visible y habilitado
        Adj_ObsAdj, // el aspecto por defecto es visible y habilitado
        PanelRemitente,
        Rem_Remitente, // el aspecto por defecto es visible y habilitado
        PanelProyecto,
        Proy_NombreProy, // el aspecto por defecto es visible y habilitado
        Proy_NumCaso, // el aspecto por defecto es visible y habilitado
        Proy_NombreCaso, // el aspecto por defecto es visible y habilitado
        Proy_Materia, // el aspecto por defecto es visible y habilitado
        Proy_ProyActiv, // el aspecto por defecto es oculto
        Proy_Etiqueta, // el aspecto por defecto es visible y habilitado
        PanelMonNac,
        Mn_CategoriaMn, // el aspecto por defecto es visible y habilitado
        Mn_CodigoMn, // el aspecto por defecto es visible y habilitado
        Mn_DenomOf, // el aspecto por defecto es visible y habilitado
        Mn_OtrasDenom, // el aspecto por defecto es visible y habilitado
        Mn_NombreUsoActual, // el aspecto por defecto es visible y habilitado
        Mn_DireccionMn, // el aspecto por defecto es visible y habilitado
        Mn_RefLocal, // el aspecto por defecto es visible y habilitado
        Mn_Region, // el aspecto por defecto es visible y habilitado
        Mn_Provincia, // el aspecto por defecto es visible y habilitado
        Mn_Comuna, // el aspecto por defecto es visible y habilitado
        Mn_Rol, // el aspecto por defecto es visible y habilitado
        PanelGeneralHistorico,
        PanelGeneral,
        Gral_FormaLlegada, // el aspecto por defecto es visible y habilitado
        Gral_ObsFormaLlegada, // el aspecto por defecto es visible y habilitado
        Gral_Caracter, // el aspecto por defecto es visible y habilitado
        Gral_ObsCaracter, // el aspecto por defecto es visible y habilitado
        Gral_Redireccionado, // el aspecto por defecto es visible y habilitado
        Gral_NumTicket, // el aspecto por defecto es visible y habilitado
        Gral_ReqAnterior, // el aspecto por defecto es visible y habilitado
        Gral_ReqNoRegistrado, // el aspecto por defecto es visible y habilitado
        PanelAsignacion,
        Asig_EnviarAsign, // el aspecto por defecto es solo lectura
        Asig_UtAsign, // el aspecto por defecto es solo lectura
        Asig_UtCopia, // el aspecto por defecto es solo lectura
        Asig_UtConoc, // el aspecto por defecto es solo lectura
        Asig_UtTemp, // el aspecto por defecto es solo lectura
        Asig_ReqResp, // el aspecto por defecto es solo lectura
        Asig_ComentarioAsign, // el aspecto por defecto es solo lectura
        Asig_LiberarAsign, // el aspecto por defecto es oculto
        PanelReasignacionUt,
        PanelAsignacionProfUt,
        //PanelReasignacionProfUt,
        PanelUtReasigProf,
        PanelPriorizacion,
        Prio_EnviarPrio, // el aspecto por defecto es oculto
        Prio_TablaPlazos, // el aspecto por defecto es visible y habilitado
        Prio_Prioridad, // el aspecto por defecto es visible y habilitado
        Prio_Solurgencia, // el aspecto por defecto es visible y habilitado
        Prio_EnviarUt, // el aspecto por defecto es oculto
        Prio_EnviarUtTemp, // el aspecto por defecto es oculto
        Prio_FechaAsigUt, // el aspecto por defecto es oculto
        PanelCierre,
        PanelSolicReasignacion,
        PanelOtrosCampos, // el aspecto por defecto es oculto,
        PanelTransparencia // el aspecto por defecto es oculto


    }
}
