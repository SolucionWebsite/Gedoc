using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Gedoc.Helpers;
using Gedoc.Helpers.Logging;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Implementacion
{
    public class RepositorioBase
    {
        public readonly TransactionOptions IsolationTs = new TransactionOptions
            {IsolationLevel = IsolationLevel.ReadUncommitted};

        private DbContext _db;

        protected GedocEntities db
        {
            get
            {
                if (_db != null) return (GedocEntities) _db;
                _db = new GedocEntities();
                AllowSerialization = true;
                _db.Database.CommandTimeout = WebConfigValues.SqlCommandTimeOut;
                return (GedocEntities) _db;
            }
        }

        public virtual bool AllowSerialization
        {
            get { return _db.Configuration.ProxyCreationEnabled; }
            set { _db.Configuration.ProxyCreationEnabled = !value; }
        }

        public virtual T Obtener<T>(Expression<Func<T, bool>> predicado,
            params Expression<Func<T, object>>[] includeExpressions) where T : class
        {
            if (predicado == null) throw new ApplicationException("El valor del predicado debe ser válido");
            if (includeExpressions.Any())
            {
                foreach (var property in includeExpressions)
                {
                    db.Set<T>().Include(property);
                }
            }

            return db.Set<T>().SingleOrDefault(predicado);
        }

        public virtual ResultadoOperacion Actualizar<T>(T entity, string[] fieldsModified = null,
            params Expression<Func<T, object>>[] includeExpressions)
            where T : class
        {
            var estadoOperacion = new ResultadoOperacion(1, "Datos actualizados con éxito.", null);
            try
            {
                db.Set<T>().Attach(entity);
                db.Entry(entity).State = EntityState.Modified;
                if (fieldsModified != null && fieldsModified.Any())
                {
                    foreach (var property in fieldsModified)
                    {
                        db.Entry<T>(entity).Property(property).IsModified = true;
                    }
                }

                estadoOperacion.Codigo = db.SaveChanges() > 0 ? 1 : -1;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                estadoOperacion.Codigo = -1;
                estadoOperacion.Mensaje = "Ha ocurrido un error al grabar los datos.";
                estadoOperacion.Extra = e.Message;
            }

            return estadoOperacion;
        }

        public virtual ResultadoOperacion Eliminar<T>(Expression<Func<T, bool>> predicado) where T : class
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito.", null);

            var entity = db.Set<T>().FirstOrDefault(predicado);
            if (entity == null)
            {
                resultado = new ResultadoOperacion(-1,
                    "Error al realizar la operación. No se encontró el registro a eliminar.", null);
            }

            db.Set<T>().Remove(entity);
            var ok = db.SaveChanges() > 0;
            if (!ok)
                resultado = new ResultadoOperacion(-1, "Error al realizar la operación. ", null);

            return resultado;
        }
    }
}