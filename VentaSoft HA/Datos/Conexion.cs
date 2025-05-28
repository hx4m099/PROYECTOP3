using System;
using System.Configuration;

namespace Datos
{
    public class Conexion
    {
        // Correct way of getting the connection string
        public static string cadena = ConfigurationManager.ConnectionStrings["conexion"].ConnectionString;
    }
}
