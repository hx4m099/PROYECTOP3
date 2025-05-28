using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class PermisoService
    {
        private PermisoRepository permisoRepository = new PermisoRepository();


        public List<Permiso> Listar(int IdUsuario)
        {
            return permisoRepository.Listar(IdUsuario);
        }
    }
}
