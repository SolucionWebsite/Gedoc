﻿using System;
 using System.Collections;
 using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
 using System.Web.DynamicData;
 using System.Web.Mvc;
 using Gedoc.Helpers;
 using Gedoc.Helpers.Dto;
 using Gedoc.Repositorio.Model;
using Gedoc.WebApp.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
 using Menu = Gedoc.Repositorio.Model.Menu;

 namespace Gedoc.WebApp.Controllers
{
    public class MenuController : BaseController
    {
        private GedocEntities db = new GedocEntities();

        [Authorize]
        public ActionResult Index()
        {
            var menus = GetMenusPrimerNivel();
            ViewBag.MenusPrimerNivel = new SelectList(menus, "IdMenu", "Nombre");
            return View();
        }

        public List<Menu> GetMenusPrimerNivel()
        {
            var menus = db.Menu.Where(m => m.IdMenuPadre == 0 || m.IdMenuPadre == null).OrderBy(m => m.Orden).ToList();
            menus.Insert(0, new Menu() { IdMenu = 0, Nombre = " " });
            return menus;
        }

        public ActionResult GetMenuRaiz()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<GenericoDto>>(new List<GenericoDto>(), resultadoOper);
            var menus = GetMenusPrimerNivel();
            resultado.Data = menus.Select(m => new GenericoDto{ IdInt = m.IdMenu, Titulo = m.Nombre}).ToList();
            return Json(resultado);
        }

        public ActionResult Menu_Read([DataSourceRequest]DataSourceRequest request)
        {
            var result = db.Menu.Include(m => m.Menu2).Select(menu => new MenuModel
            {
                IdMenu = menu.IdMenu,
                IdMenuPadre = menu.IdMenuPadre ?? 0,
                Nombre = menu.Nombre,
                NombrePadre = menu.Menu2.Nombre,
                Controller = menu.Controller,
                Action = menu.Action,
                Activo = menu.Activo ?? false,
                Url = menu.Url,
                Target = menu.Target,
                Orden = menu.Orden
            });          
            return Json(result.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Menu_Create([DataSourceRequest]DataSourceRequest request, MenuModel menu)
        {
            if (ModelState.IsValid)
            {
                var entity = new Menu
                {
                    IdMenuPadre = menu.IdMenuPadre == 0 ? null : menu.IdMenuPadre,
                    Nombre = menu.Nombre,
                    Controller = menu.Controller,
                    Action = menu.Action,
                    Activo = menu.Activo,
                    Url = menu.Url,
                    Target = menu.Target,
                    Orden = menu.Orden
                };

                db.Menu.Add(entity);
                db.SaveChanges();
                menu.IdMenu = entity.IdMenu;
                UpdateMenuOrden(entity.IdMenu, entity.IdMenuPadre.GetValueOrDefault(0), entity.Orden);
            }

            return Json(new[] { menu }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Menu_Update([DataSourceRequest]DataSourceRequest request, MenuModel menu)
        {
            if (ModelState.IsValid)
            {
                var entity = db.Menu.FirstOrDefault(a => a.IdMenu == menu.IdMenu);
                if (entity != null)
                {
                    //entity.IdMenu = menu.IdMenu;
                    entity.IdMenuPadre = menu.IdMenuPadre == 0 ? null : menu.IdMenuPadre;
                    entity.Nombre = menu.Nombre;
                    //entity.Controller = menu.Controller;
                    //entity.Action = menu.Action;
                    entity.Activo = menu.Activo;
                    entity.Url = menu.Url;
                    //entity.Target = menu.Target;
                    entity.Orden = menu.Orden;
                    db.SaveChanges();
                    UpdateMenuOrden(entity.IdMenu, entity.IdMenuPadre.GetValueOrDefault(0), entity.Orden);
                }           
            }
            return Json(new[] { menu }.ToDataSourceResult(request, ModelState));
        }

        private void UpdateMenuOrden(int idMenu, int idMenuPadre, int orden)
        {
            var menus = db.Menu
                .Where(m => m.IdMenu != idMenu && 
                            (m.IdMenuPadre == idMenuPadre || (idMenuPadre == 0 && m.IdMenuPadre == null)) && /*IdMenuPadre es posible q lo mismo sea null q 0, por eso la condición*/
                            m.Orden >= orden)
                .OrderBy(m => m.Orden);
            foreach (var menu in menus)
            {
                if (menu.Orden <= orden) {
                    menu.Orden = ++orden;
                }
                else
                {
                    break;
                }
            }
            db.SaveChanges();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Menu_Destroy([DataSourceRequest]DataSourceRequest request, Menu menu)
        {
            if (ModelState.IsValid)
            {
                var entity = db.Menu.FirstOrDefault(a => a.IdMenu == menu.IdMenu);
                if (entity != null)
                {
                    entity.Rol.Clear();
                    db.Menu.Remove(entity);
                }
                db.SaveChanges();
            }
            return Json(new[] { menu }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

        public JsonResult GetMenus()
        {
            if (User.Identity.IsAuthenticated && Session["IdUsuario"] != null)
            {
                var misRoles = (IEnumerable<string>)Session["MisRoles"];
                var menus =
                    (from m in db.Menu orderby m.Orden where m.Activo == true select m);
                var menusFiltrado = new List<Menu>();
                foreach (var menu in menus)
                {
                    var rolesAsociados = menu.Rol.Any(r => misRoles.Contains(r.Titulo));
                    if (rolesAsociados) menusFiltrado.Add(menu);
                }

                var separadorHtml = "<div class='k-separator w-100'></div>";
                var result =
                    (from m in menusFiltrado.Distinct()
                        orderby m.Orden
                        where (m.IdMenuPadre == 0 || m.IdMenuPadre == null) // TODO: modificado Ernesto, confirmar
                           && m.Activo == true
                     select m)
                    .Select(item =>
                        new
                        {
                            Name = item.Nombre,
                            Url = item.Url,
                            Hijos = menusFiltrado
                                .Where((m) => m.IdMenuPadre == item.IdMenu && m.Activo == true)
                                .Select((mi) => new
                                {
                                    Name = mi.Nombre.Trim().Equals("separador", StringComparison.InvariantCultureIgnoreCase)
                                            ? separadorHtml
                                            : mi.Nombre,
                                    Url = mi.Url,
                                    encoded = !mi.Nombre.Trim().Equals("separador", StringComparison.InvariantCultureIgnoreCase), 
                                    enabled = !mi.Nombre.Trim().Equals("separador", StringComparison.InvariantCultureIgnoreCase)
                                })
                        }
                    );
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //return Redirect("/Home/Index");
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
