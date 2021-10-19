using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Enum
{
    public enum Bandeja
    {
        [EnumDescriptionAttribute("Ingreso Central")]
        IngresoCentral = 12,     // 12 en Gedoc SP
        [EnumDescriptionAttribute("Asignación")]
        Asignacion = 4,         // 4 en Gedoc SP
        [EnumDescriptionAttribute("Priorización")]
        Priorizacion = 5,       // 5 en Gedoc SP
        [EnumDescriptionAttribute("Encargado UT")]
        EncargadoUt = 2,        // 2 en Gedoc SP
        [EnumDescriptionAttribute("Profesional UT")]
        ProfesionalUt = 1,      // 1 en Gedoc SP
        [EnumDescriptionAttribute("Secretaria UT")]
        SecretariaUt = 6,       // 6 en Gedoc SP
        [EnumDescriptionAttribute("Jefatura UT")]
        JefaturaUt = 3,         // 3 en Gedoc SP
        [EnumDescriptionAttribute("Administración")]
        Administracion = 10,         // 3 en Gedoc SP
        Despachos = 7,           // 7 en Gedoc SP
        DespachosIniciativas = 13,           // 13 en Gedoc SP
        Transparencia = 8,       // 8 en Gedoc SP
        Historico = 50,       // 50 en Gedoc SP
        PriorizacionEncargado = 51,       // Nueva. No es realmente una bandeja sino la grilla de la pestaña Priorización en la bandeja de encargado UT.
        PriorizacionSecretaria = 52,       // Nueva. No es realmente una bandeja sino la grilla de la pestaña Priorización en la bandeja de secretaria UT.
        PriorizacionProfesional = 53,       // Nueva. No es realmente una bandeja sino la grilla de la pestaña Priorización en la bandeja de secretaria UT.
        Oficio = 54

    }
}
