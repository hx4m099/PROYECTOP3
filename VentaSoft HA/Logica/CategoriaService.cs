using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using   Datos;

namespace Logica
{
    public class CategoriaService
    {
        private CategoriaRepository categoriaRepository = new CategoriaRepository();

        public List<Categoria> Listar()
        {
            return categoriaRepository.Listar();
        }

        public int Registrar(Categoria obj, out string Mensaje)
        {
            Mensaje = string.Empty;


            if (obj.Descripcion == "")
            {
                Mensaje += "Es necesario la descripcion de la Categoria\n";
            }

            if (Mensaje != string.Empty)
            {
                return 0;
            }
            else
            {
                return categoriaRepository.Registrar(obj, out Mensaje);
            }


        }


        public bool Editar(Categoria obj, out string Mensaje)
        {

            Mensaje = string.Empty;

            if (obj.Descripcion == "")
            {
                Mensaje += "Es necesario la descripcion de la Categoria\n";
            }

            if (Mensaje != string.Empty)
            {
                return false;
            }
            else
            {
                return categoriaRepository.Editar(obj, out Mensaje);
            }


        }


        public bool Eliminar(Categoria obj, out string Mensaje)
        {
            return categoriaRepository.Eliminar(obj, out Mensaje);
        }


    }
}
