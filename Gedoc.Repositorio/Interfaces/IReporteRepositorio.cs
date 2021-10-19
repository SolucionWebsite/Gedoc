using Gedoc.Repositorio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Repositorio.Interfaces
{
    public interface IReporteRepositorio
    {
        List<Reporte> GetReportes();
    }
}
