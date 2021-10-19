using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Enum
{
    public enum FlujoIngreso
    {
        [EnumDescriptionAttribute("Nuevo Requerimiento")]
        NuevoRequerimiento = 1,

        [EnumDescriptionAttribute("Ingreso Central")]
        IngresoCentral = 2,

        [EnumDescriptionAttribute("Priorización")]
        Priorizacion = 3,

        [EnumDescriptionAttribute("Asignación de Unidad Técnica")]
        AsignacionUt = 4,

        [EnumDescriptionAttribute("Asginación de Unidad Técnica Temporal")]
        AsignacionUtTemp = 5,

        [EnumDescriptionAttribute("Asignación Profesional UT")]
        AsignacionProfUt = 6,

        [EnumDescriptionAttribute("Reasignación Profesional UT")]
        ReasignacionProfUt = 7,

        [EnumDescriptionAttribute("Editar Campos UT")]
        EditarCamposUt = 8,

        [EnumDescriptionAttribute("Editar Requerimiento")]
        EditarIngreso = 9,

        [EnumDescriptionAttribute("Cierre de Requerimiento")]
        RequerimientoCierre = 10,

        [EnumDescriptionAttribute("Requerimiento Histórico")]
        Historico = 11,

        [EnumDescriptionAttribute("Despacho")]
        Despacho = 12,

        [EnumDescriptionAttribute("Despacho Iniciativas CMN")]
        DespachoInic = 13,

        [EnumDescriptionAttribute("Bitácora")]
        Bitacora = 14,

        [EnumDescriptionAttribute("Adjunto")]
        Adjunto = 15,

        [EnumDescriptionAttribute("Editar Despacho")]
        DespachoEdicion = 16,

        [EnumDescriptionAttribute("Cerrar Despacho")]
        DespachoCierre = 17,

        [EnumDescriptionAttribute("Editar Despacho Iniciativas")]
        DespachoInicEdicion = 18,

        [EnumDescriptionAttribute("Cerrar Despacho Iniciativas")]
        DespachoInicCierre = 19,

        [EnumDescriptionAttribute("Reasignación UT")]
        ReasignacionUt = 20,

        [EnumDescriptionAttribute("Bitácora Despacho Iniciativas")]
        BitacoraDespachoInic = 21,

        [EnumDescriptionAttribute("Nuevo Requerimiento desde WSS")]
        NuevoRequerimientoWss = 22,

        [EnumDescriptionAttribute("Oficio")]
        Oficio = 23
    }
}