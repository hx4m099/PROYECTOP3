using System;

namespace Entidades
{
    public class CodigoRecuperacion
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public string DocumentoUsuario { get; set; }
        public string CodigoVerificacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public string Estado { get; set; } // Pendiente, Usado, Expirado
        public int IntentosUsados { get; set; }
        public string IpOrigen { get; set; }
        public string UsernameTelegram { get; set; }
        public DateTime? FechaUso { get; set; }
        public string Observaciones { get; set; }
    }

    public class LogRecuperacion
    {
        public int Id { get; set; }
        public string DocumentoUsuario { get; set; }
        public long ChatId { get; set; }
        public string TipoAccion { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaHora { get; set; }
        public string IpOrigen { get; set; }
        public string UsernameTelegram { get; set; }
        public bool Exitoso { get; set; }
        public string DetalleError { get; set; }
    }

    public class ConfiguracionBot
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Valor { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}