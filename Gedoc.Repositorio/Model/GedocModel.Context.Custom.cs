//------------------------------------------------------------------------------
// Personalización de la clase GedocEntities
//------------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Linq;

namespace Gedoc.Repositorio.Model
{
    
    public partial class GedocEntities
    {
        public override int SaveChanges()
        {
            /* Se obtienen las entidades q se crean o modifican. */
            var changedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified/* ||
                            e.State == EntityState.Deleted*/)
                .ToList();

            var ahora = DateTime.Now;
            foreach (var entityEntry in changedEntries)
            {
                // TODO: si es un update y no viene valor en el campo de UsuarioModificacionId entonces asignarle el valor. Ahora no se tiene aquí este valor.
                // TODO: idema para UsuarioCreacionId si es un insert
                /* Si es un insert */
                if (entityEntry.State == EntityState.Added)
                {
                    /* Si la entidad tiene el campo FechaCreacion y se le hizo un insert entonces se asigna el valor de este campo con la fecha y hora actual*/
                    if (entityEntry.CurrentValues.PropertyNames.Contains("FechaCreacion"))
                    {
                        entityEntry.Property("FechaCreacion").CurrentValue = ahora;
                    }
                    /* Si es un insert entonces si el campo UsuarioModificacionId está vacío se le asigna el valor de UsuarioCreacionId  */
                    if (entityEntry.CurrentValues.PropertyNames.Contains("UsuarioCreacionId") &&
                        entityEntry.CurrentValues.PropertyNames.Contains("UsuarioModificacionId") &&
                        entityEntry.Property("UsuarioModificacionId").CurrentValue == null)
                    {
                        entityEntry.Property("UsuarioModificacionId").CurrentValue = entityEntry.Property("UsuarioCreacionId").CurrentValue;
                    }
                }
                /* Si la entidad tiene el campo FechaModificacion y se le hizo un update o insert entonces se actualiza el valor de este campo con la fecha y hora actual*/
                if (entityEntry.CurrentValues.PropertyNames.Contains("FechaModificacion"))
                {
                    entityEntry.Property("FechaModificacion").CurrentValue = ahora;
                }

            }
            return base.SaveChanges();
        }


    }
}
