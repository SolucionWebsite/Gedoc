using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gedoc.WebApp.Helpers
{
    public class IgnoreAccessFilter : ActionFilterAttribute
    {
        public string UrlBase { get; set; } // Url de chequeo de acceso en mantendor de Menú. Dejar vacío para no realizar el chequeo.

        // Sólo para indicar acciones de controladores q no van a tener el chequeo de si el usuario tiene acceso en el mantenedor de Menú.
        // Solo aplcable para acciones q no son llamadas Ajax
        // Por ejemplo, para alguna acción q devuelva una página q es co´mún a todos los usuarios sin restricción, por ejemplo la página principal,
        // o las páginas q se cargan dentro de dialogo mediante iframe y q no aparecen en el mantenedor de Menú, por ejemplo adicionar requermientos
        // a un caso dentro de la página de mantenedor de Caso
    }
}