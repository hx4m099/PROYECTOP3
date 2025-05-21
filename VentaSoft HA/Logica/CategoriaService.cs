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
        private CategoriaRepository objcategoriaRepository = new CategoriaRepository();

        public List<Categoria> Listar()
        {
            return objcategoriaRepository.Listar();
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
                return objcategoriaRepository.Registrar(obj, out Mensaje);
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
                return objcategoriaRepository.Editar(obj, out Mensaje);
            }


        }


        public bool Eliminar(Categoria obj, out string Mensaje)
        {
            return objcategoriaRepository.Eliminar(obj, out Mensaje);
        }


    }
}
