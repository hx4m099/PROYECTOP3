using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Entidades;





namespace Logica
{
    public class RecuperacionService
    {
        private string connectionString;

        public RecuperacionService()
        {
      
            connectionString = ConfigurationManager.ConnectionStrings["conexion"].ConnectionString;
        }

        // Crear un nuevo código de recuperación
        public bool CrearCodigoRecuperacion(CodigoRecuperacion codigo, out string mensaje)
        {
            mensaje = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO CodigosRecuperacion 
                        (ChatId, DocumentoUsuario, CodigoVerificacion, FechaCreacion, FechaExpiracion, 
                         Estado, IntentosUsados, IpOrigen, UsernameTelegram, Observaciones)
                        VALUES 
                        (@ChatId, @DocumentoUsuario, @CodigoVerificacion, @FechaCreacion, @FechaExpiracion, 
                         @Estado, @IntentosUsados, @IpOrigen, @UsernameTelegram, @Observaciones)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ChatId", codigo.ChatId);
                        command.Parameters.AddWithValue("@DocumentoUsuario", codigo.DocumentoUsuario);
                        command.Parameters.AddWithValue("@CodigoVerificacion", codigo.CodigoVerificacion);
                        command.Parameters.AddWithValue("@FechaCreacion", codigo.FechaCreacion);
                        command.Parameters.AddWithValue("@FechaExpiracion", codigo.FechaExpiracion);
                        command.Parameters.AddWithValue("@Estado", codigo.Estado);
                        command.Parameters.AddWithValue("@IntentosUsados", codigo.IntentosUsados);
                        command.Parameters.AddWithValue("@IpOrigen", codigo.IpOrigen ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@UsernameTelegram", codigo.UsernameTelegram ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Observaciones", codigo.Observaciones ?? (object)DBNull.Value);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            mensaje = "Código de recuperación creado exitosamente";
                            return true;
                        }
                        else
                        {
                            mensaje = "No se pudo crear el código de recuperación";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = $"Error al crear código de recuperación: {ex.Message}";
                return false;
            }
        }

        // Buscar código de recuperación por código
        public CodigoRecuperacion BuscarCodigoPorCodigo(string codigoVerificacion)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT Id, ChatId, DocumentoUsuario, CodigoVerificacion, FechaCreacion, 
                               FechaExpiracion, Estado, IntentosUsados, IpOrigen, UsernameTelegram, 
                               FechaUso, Observaciones
                        FROM CodigosRecuperacion 
                        WHERE CodigoVerificacion = @CodigoVerificacion";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CodigoVerificacion", codigoVerificacion);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new CodigoRecuperacion
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    ChatId = Convert.ToInt64(reader["ChatId"]),
                                    DocumentoUsuario = reader["DocumentoUsuario"].ToString(),
                                    CodigoVerificacion = reader["CodigoVerificacion"].ToString(),
                                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                                    FechaExpiracion = Convert.ToDateTime(reader["FechaExpiracion"]),
                                    Estado = reader["Estado"].ToString(),
                                    IntentosUsados = Convert.ToInt32(reader["IntentosUsados"]),
                                    IpOrigen = reader["IpOrigen"]?.ToString(),
                                    UsernameTelegram = reader["UsernameTelegram"]?.ToString(),
                                    FechaUso = reader["FechaUso"] == DBNull.Value ? null : (DateTime?)reader["FechaUso"],
                                    Observaciones = reader["Observaciones"]?.ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar código: {ex.Message}");
            }
            return null;
        }

        // Marcar código como usado
        public bool MarcarCodigoComoUsado(string codigoVerificacion, out string mensaje)
        {
            mensaje = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE CodigosRecuperacion 
                        SET Estado = 'Usado', 
                            FechaUso = GETDATE(),
                            IntentosUsados = IntentosUsados + 1,
                            Observaciones = 'Código usado para restablecer contraseña'
                        WHERE CodigoVerificacion = @CodigoVerificacion";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CodigoVerificacion", codigoVerificacion);
                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            mensaje = "Código marcado como usado";
                            return true;
                        }
                        else
                        {
                            mensaje = "No se encontró el código";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = $"Error al marcar código como usado: {ex.Message}";
                return false;
            }
        }

        // Incrementar intentos de uso
        public bool IncrementarIntentos(string codigoVerificacion)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE CodigosRecuperacion 
                        SET IntentosUsados = IntentosUsados + 1
                        WHERE CodigoVerificacion = @CodigoVerificacion";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CodigoVerificacion", codigoVerificacion);
                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al incrementar intentos: {ex.Message}");
                return false;
            }
        }

        // Crear log de recuperación
        public bool CrearLogRecuperacion(LogRecuperacion log)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO LogsRecuperacion 
                        (DocumentoUsuario, ChatId, TipoAccion, Descripcion, FechaHora, 
                         IpOrigen, UsernameTelegram, Exitoso, DetalleError)
                        VALUES 
                        (@DocumentoUsuario, @ChatId, @TipoAccion, @Descripcion, @FechaHora, 
                         @IpOrigen, @UsernameTelegram, @Exitoso, @DetalleError)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DocumentoUsuario", log.DocumentoUsuario);
                        command.Parameters.AddWithValue("@ChatId", log.ChatId);
                        command.Parameters.AddWithValue("@TipoAccion", log.TipoAccion);
                        command.Parameters.AddWithValue("@Descripcion", log.Descripcion);
                        command.Parameters.AddWithValue("@FechaHora", log.FechaHora);
                        command.Parameters.AddWithValue("@IpOrigen", log.IpOrigen ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@UsernameTelegram", log.UsernameTelegram ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Exitoso", log.Exitoso);
                        command.Parameters.AddWithValue("@DetalleError", log.DetalleError ?? (object)DBNull.Value);

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear log: {ex.Message}");
                return false;
            }
        }

        // Limpiar códigos expirados
        public int LimpiarCodigosExpirados()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand("sp_LimpiarCodigosExpirados", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();

                        object result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al limpiar códigos expirados: {ex.Message}");
                return 0;
            }
        }

        // Obtener configuración del bot
        public string ObtenerConfiguracion(string clave, string valorPorDefecto = "")
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Valor FROM ConfiguracionBot WHERE Clave = @Clave AND Activo = 1";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Clave", clave);
                        connection.Open();

                        object result = command.ExecuteScalar();
                        return result?.ToString() ?? valorPorDefecto;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener configuración: {ex.Message}");
                return valorPorDefecto;
            }
        }

        // Generar código único
        public string GenerarCodigoUnico()
        {
            string codigo;
            do
            {
                Random random = new Random();
                codigo = random.Next(100000, 999999).ToString();
            }
            while (BuscarCodigoPorCodigo(codigo) != null);

            return codigo;
        }
    }
}