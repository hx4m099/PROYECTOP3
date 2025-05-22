using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datos;

namespace Logica
{
    public class NegocioService
    {
        private NegocioRepository negocioRepository = new NegocioRepository();


        public Negocio ObtenerDatos()
        {
            return negocioRepository.ObtenerDatos();
        }

        public bool GuardarDatos(Negocio obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.Nombre == "")
            {
                Mensaje += "Es necesario el nombre\n";
            }

            if (obj.RUC == "")
            {
                Mensaje += "Es necesario el numero de RUC\n";
            }

            if (obj.Direccion == "")
            {
                Mensaje += "Es necesario la direccion\n";
            }

            if (Mensaje != string.Empty)
            {
                return false;
            }
            else
            {
                return negocioRepository.GuardarDatos(obj, out Mensaje);
            }


        }

        public byte[] ObtenerLogo(out bool obtenido)
        {
            return negocioRepository.ObtenerLogo(out obtenido);
        }


        public bool ActualizarLogo(byte[] imagen, out string mensaje)
        {
            return negocioRepository.ActualizarLogo(imagen, out mensaje);
        }

    }
}
