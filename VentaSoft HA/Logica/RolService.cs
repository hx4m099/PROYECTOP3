using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class RolService
    {

        private RolRepository rolRepository = new RolRepository();


        public List<Rol> Listar()
        {
            return rolRepository.Listar();
        }
    }
}
