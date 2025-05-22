using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class ProveedorService
    {
        private ProveedorRepository productoRepository = new ProveedorRepository();


        public List<Proveedor> Listar()
        {
            return productoRepository.Listar();
        }

        public int Registrar(Proveedor obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.Documento == "")
            {
                Mensaje += "Es necesario el documento del Proveedor\n";
            }

            if (obj.RazonSocial == "")
            {
                Mensaje += "Es necesario la razon social del Proveedor\n";
            }

            if (obj.Correo == "")
            {
                Mensaje += "Es necesario la correo del Proveedor\n";
            }

            if (Mensaje != string.Empty)
            {
                return 0;
            }
            else
            {
                return productoRepository.Registrar(obj, out Mensaje);
            }


        }


        public bool Editar(Proveedor obj, out string Mensaje)
        {

            Mensaje = string.Empty;

            if (obj.Documento == "")
            {
                Mensaje += "Es necesario el documento del Proveedor\n";
            }

            if (obj.RazonSocial == "")
            {
                Mensaje += "Es necesario la razon social del Proveedor\n";
            }

            if (obj.Correo == "")
            {
                Mensaje += "Es necesario la correo del Proveedor\n";
            }



            if (Mensaje != string.Empty)
            {
                return false;
            }
            else
            {
                return productoRepository.Editar(obj, out Mensaje);
            }


        }


        public bool Eliminar(Proveedor obj, out string Mensaje)
        {
            return productoRepository.Eliminar(obj, out Mensaje);
        }

    }
}
