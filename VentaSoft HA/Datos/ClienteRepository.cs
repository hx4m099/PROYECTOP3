using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class ClienteRepository
    {
        public List<Cliente> Listar()
        {
            List<Cliente> lista = new List<Cliente>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    string query = "SELECT IdCliente, Documento, NombreCompleto, Correo, Telefono, FechaNacimiento, Estado FROM CLIENTE";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Cliente()
                            {
                                IdCliente = Convert.ToInt32(dr["IdCliente"]),
                                Documento = dr["Documento"].ToString(),
                                NombreCompleto = dr["NombreCompleto"].ToString(),
                                Correo = dr["Correo"].ToString(),
                                Telefono = dr["Telefono"].ToString(),
                                FechaNacimiento = dr["FechaNacimiento"] != DBNull.Value ? Convert.ToDateTime(dr["FechaNacimiento"]) : (DateTime?)null,
                                Estado = Convert.ToBoolean(dr["Estado"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lista = new List<Cliente>();
            }

            return lista;
        }

        public int Registrar(Cliente obj, out string Mensaje)
        {
            int idgenerado = 0;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    // Usar SQL directo para registrar
                    string query = @"
                        IF NOT EXISTS (SELECT * FROM CLIENTE WHERE Documento = @Documento)
                        BEGIN
                            INSERT INTO CLIENTE(Documento, NombreCompleto, Correo, Telefono, FechaNacimiento, Estado) 
                            VALUES (@Documento, @NombreCompleto, @Correo, @Telefono, @FechaNacimiento, @Estado);
                            SELECT SCOPE_IDENTITY();
                        END
                        ELSE
                        BEGIN
                            SELECT 0;
                        END
                    ";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@Documento", obj.Documento);
                    cmd.Parameters.AddWithValue("@NombreCompleto", obj.NombreCompleto);
                    cmd.Parameters.AddWithValue("@Correo", obj.Correo);
                    cmd.Parameters.AddWithValue("@Telefono", obj.Telefono);

                    if (obj.FechaNacimiento.HasValue)
                        cmd.Parameters.AddWithValue("@FechaNacimiento", obj.FechaNacimiento.Value);
                    else
                        cmd.Parameters.AddWithValue("@FechaNacimiento", DBNull.Value);

                    cmd.Parameters.AddWithValue("@Estado", obj.Estado);

                    oconexion.Open();
                    var result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        idgenerado = Convert.ToInt32(result);
                        if (idgenerado > 0)
                            Mensaje = "Cliente registrado correctamente";
                        else
                            Mensaje = "El numero de documento ya existe";
                    }
                }
            }
            catch (Exception ex)
            {
                idgenerado = 0;
                Mensaje = ex.Message;
            }

            return idgenerado;
        }

        public bool Editar(Cliente obj, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    // Usar SQL directo para editar
                    string query = @"
                        IF NOT EXISTS (SELECT * FROM CLIENTE WHERE Documento = @Documento AND IdCliente != @IdCliente)
                        BEGIN
                            UPDATE CLIENTE SET 
                                Documento = @Documento,
                                NombreCompleto = @NombreCompleto,
                                Correo = @Correo,
                                Telefono = @Telefono,
                                FechaNacimiento = @FechaNacimiento,
                                Estado = @Estado
                            WHERE IdCliente = @IdCliente;
                            SELECT @@ROWCOUNT;
                        END
                        ELSE
                        BEGIN
                            SELECT 0;
                        END
                    ";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@IdCliente", obj.IdCliente);
                    cmd.Parameters.AddWithValue("@Documento", obj.Documento);
                    cmd.Parameters.AddWithValue("@NombreCompleto", obj.NombreCompleto);
                    cmd.Parameters.AddWithValue("@Correo", obj.Correo);
                    cmd.Parameters.AddWithValue("@Telefono", obj.Telefono);

                    if (obj.FechaNacimiento.HasValue)
                        cmd.Parameters.AddWithValue("@FechaNacimiento", obj.FechaNacimiento.Value);
                    else
                        cmd.Parameters.AddWithValue("@FechaNacimiento", DBNull.Value);

                    cmd.Parameters.AddWithValue("@Estado", obj.Estado);

                    oconexion.Open();
                    int filasAfectadas = Convert.ToInt32(cmd.ExecuteScalar());

                    respuesta = filasAfectadas > 0;
                    if (respuesta)
                        Mensaje = "Cliente actualizado correctamente";
                    else
                        Mensaje = "No se pudo actualizar el cliente o el documento ya existe";
                }
            }
            catch (Exception ex)
            {
                respuesta = false;
                Mensaje = ex.Message;
            }

            return respuesta;
        }

        public bool Eliminar(Cliente obj, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM CLIENTE WHERE IdCliente = @IdCliente", oconexion);
                    cmd.Parameters.AddWithValue("@IdCliente", obj.IdCliente);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    respuesta = cmd.ExecuteNonQuery() > 0;

                    if (respuesta)
                        Mensaje = "Cliente eliminado correctamente";
                    else
                        Mensaje = "No se pudo eliminar el cliente";
                }
            }
            catch (Exception ex)
            {
                respuesta = false;
                Mensaje = ex.Message;
            }

            return respuesta;
        }
    }
}