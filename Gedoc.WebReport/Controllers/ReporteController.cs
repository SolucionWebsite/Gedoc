using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Gedoc.Helpers.Enum;
using Gedoc.Repositorio.Model;
using Gedoc.WebReport.Models;

namespace Gedoc.WebReport.Controller
{

    public class ReporteController : ApiController
    {
        private GedocEntities db = new GedocEntities();
        // GET api/<controller>

        [HttpGet]
        public IEnumerable<Generico> Get()
        {
            var model = db.ListaValor
                .Where(q=>q.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo && q.IdLista == (int)Mantenedor.CategoriaMn)
                .Select(q=> new Generico { Id= q.Codigo.ToString(), Nombre = q.Titulo } )
                .ToArray();
            return model;

        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}