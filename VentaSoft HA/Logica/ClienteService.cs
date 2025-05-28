using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logica
{
    public class ClienteService
    {
        private ClienteRepository clienteRepository = new ClienteRepository();

        public List<Cliente> Listar()
        {
            return clienteRepository.Listar();
        }

        // Método para obtener clientes que cumplen años hoy
        public List<Cliente> ObtenerClientesCumpleañosHoy()
        {
            var clientes = clienteRepository.Listar();
            var hoy = DateTime.Today;

            return clientes.Where(c => c.FechaNacimiento.HasValue &&
                                      c.FechaNacimiento.Value.Month == hoy.Month &&
                                      c.FechaNacimiento.Value.Day == hoy.Day &&
                                      c.Estado).ToList();
        }

        // Método para calcular la edad del cliente
        public int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;

            if (fechaNacimiento.Date > hoy.AddYears(-edad))
                edad--;

            return edad;
        }

        // NUEVO MÉTODO: Verificar si un cliente específico cumple años hoy
        public bool EsCumpleanosHoy(Cliente cliente)
        {
            if (cliente == null || !cliente.FechaNacimiento.HasValue)
                return false;

            DateTime hoy = DateTime.Today;
            DateTime fechaNacimiento = cliente.FechaNacimiento.Value.Date;

            // Verificar si hoy es el cumpleaños (mismo día y mes)
            return hoy.Day == fechaNacimiento.Day && hoy.Month == fechaNacimiento.Month;
        }

        // NUEVO MÉTODO: Buscar cliente por documento
        public Cliente BuscarPorDocumento(string documento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documento))
                    return null;

                return clienteRepository.Listar()
                    .FirstOrDefault(c => c.Documento == documento.Trim() && c.Estado);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error buscando cliente por documento: {ex.Message}");
                return null;
            }
        }

        public int Registrar(Cliente obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.Documento == "")
            {
                Mensaje += "Es necesario el documento del Cliente\n";
            }

            if (obj.NombreCompleto == "")
            {
                Mensaje += "Es necesario el nombre completo del Cliente\n";
            }

            if (obj.Correo == "")
            {
                Mensaje += "Es necesario el correo del Cliente\n";
            }

            // Validación opcional para fecha de nacimiento
            if (obj.FechaNacimiento.HasValue)
            {
                if (obj.FechaNacimiento.Value > DateTime.Today)
                {
                    Mensaje += "La fecha de nacimiento no puede ser futura\n";
                }

                var edad = CalcularEdad(obj.FechaNacimiento.Value);
                if (edad > 120)
                {
                    Mensaje += "La fecha de nacimiento no es válida\n";
                }
            }

            if (Mensaje != string.Empty)
            {
                return 0;
            }
            else
            {
                return clienteRepository.Registrar(obj, out Mensaje);
            }
        }

        public bool Editar(Cliente obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.Documento == "")
            {
                Mensaje += "Es necesario el documento del Cliente\n";
            }

            if (obj.NombreCompleto == "")
            {
                Mensaje += "Es necesario el nombre completo del Cliente\n";
            }

            if (obj.Correo == "")
            {
                Mensaje += "Es necesario el correo del Cliente\n";
            }

            // Validación para fecha de nacimiento en edición
            if (obj.FechaNacimiento.HasValue)
            {
                if (obj.FechaNacimiento.Value > DateTime.Today)
                {
                    Mensaje += "La fecha de nacimiento no puede ser futura\n";
                }

                var edad = CalcularEdad(obj.FechaNacimiento.Value);
                if (edad > 120)
                {
                    Mensaje += "La fecha de nacimiento no es válida\n";
                }
            }

            if (Mensaje != string.Empty)
            {
                return false;
            }
            else
            {
                return clienteRepository.Editar(obj, out Mensaje);
            }
        }

        public bool Eliminar(Cliente obj, out string Mensaje)
        {
            return clienteRepository.Eliminar(obj, out Mensaje);
        }
    }
}