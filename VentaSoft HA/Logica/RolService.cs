using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    internal class RolService
    {

        private CD_Rol objcd_rol = new CD_Rol();


        public List<Rol> Listar()
        {
            return objcd_rol.Listar();
        }
    }
}
