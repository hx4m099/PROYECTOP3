using Entidades;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Logica;
using System.Windows;

namespace VentaSoftHA.Logica
{
    public class TelegramService
    {
        private static TelegramService _instance;
        private TelegramBotClient _botClient;
        private RecuperacionService _recuperacionService;
        private string _botToken;

        private TelegramService()
        {
            _recuperacionService = new RecuperacionService();
            _botToken = _recuperacionService.ObtenerConfiguracion("BotToken", "7455500441:AAHZfllp0i6kEPJyREiTlM3dO5-jsb3P92U");
            _botClient = new TelegramBotClient(_botToken);
             var _ = StartReceiver();
        }

        public static TelegramService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TelegramService();
            }
            return _instance;
        }

        private async Task StartReceiver()
        {
            var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            // CORREGIDO: Cambiar pollingErrorHandler por errorHandler
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            // Limpiar códigos expirados cada 30 minutos
            _ = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        _recuperacionService.LimpiarCodigosExpirados();
                        await Task.Delay(TimeSpan.FromMinutes(30), cts.Token);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en limpieza automática: {ex.Message}");
                    }
                }
            });
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await OnUpdateReceived(botClient, update, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar actualización: {ex.Message}");
            }
        }

        private async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error en el bot de Telegram: {exception.Message}");
        }

        private async Task OnUpdateReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message is Message message)
                {
                    string username = message.From?.Username ?? message.From?.FirstName ?? "Desconocido";

                    // Verificar si es una solicitud de recuperación de contraseña
                    if (message.Text != null && message.Text.StartsWith("/recuperar"))
                    {
                        await ProcesarSolicitudRecuperacion(botClient, message, username, cancellationToken);
                        return;
                    }

                    // Verificar si es un código de recuperación
                    if (message.Text != null && message.Text.Length == 6 && message.Text.All(char.IsDigit))
                    {
                        await ProcesarCodigoRecuperacion(botClient, message, username, cancellationToken);
                        return;
                    }

                    // Verificar comandos de ayuda
                    if (message.Text != null && (message.Text == "/start" || message.Text == "/help"))
                    {
                        await EnviarMensajeBienvenida(botClient, message, cancellationToken);
                        return;
                    }

                    // Mensaje por defecto
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "🔑 RECUPERACIÓN DE CONTRASEÑA\n\n" +
                              "Para recuperar tu contraseña, envía:\n" +
                              "/recuperar TU_DOCUMENTO\n\n" +
                              "Ejemplo: /recuperar 12345678\n\n" +
                              "Si ya tienes un código, simplemente envíalo.",
                        cancellationToken: cancellationToken
                    );
                }

                // Manejar callbacks de botones
                if (update.CallbackQuery != null)
                {
                    var callbackQuery = update.CallbackQuery;

                    if (callbackQuery.Data == "🔑 Recuperar Contraseña")
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: "🔑 RECUPERACIÓN DE CONTRASEÑA\n\n" +
                                  "Para recuperar tu contraseña, envía:\n" +
                                  "/recuperar TU_DOCUMENTO\n\n" +
                                  "Ejemplo: /recuperar 12345678\n\n" +
                                  "Te enviaremos un código de verificación.",
                            cancellationToken: cancellationToken
                        );
                    }
                    else if (callbackQuery.Data == "📞 Contacto")
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: "📞 CONTACTO DE SOPORTE\n\n" +
                                  "📱 WhatsApp: +57 300 123 4567\n" +
                                  "📧 Email: soporte@sistema.com\n" +
                                  "🕘 Horario: Lunes a viernes, 8 am a 6 pm",
                            cancellationToken: cancellationToken
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en OnUpdateReceived: {ex.Message}");
            }
        }

        private async Task EnviarMensajeBienvenida(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🔑 Recuperar Contraseña")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📞 Contacto")
                }
            });

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "🔐 SISTEMA DE RECUPERACIÓN DE CONTRASEÑAS\n\n" +
                      "¡Hola! Este bot te ayudará a recuperar tu contraseña.\n\n" +
                      "Selecciona una opción o envía directamente:\n" +
                      "/recuperar TU_DOCUMENTO",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
            );
        }

        private async Task ProcesarSolicitudRecuperacion(ITelegramBotClient botClient, Message message, string username, CancellationToken cancellationToken)
        {
            try
            {
                var partes = message.Text.Split(' ');
                if (partes.Length != 2)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "❌ Formato incorrecto.\n\n" +
                              "Usa: /recuperar TU_DOCUMENTO\n" +
                              "Ejemplo: /recuperar 12345678",
                        cancellationToken: cancellationToken
                    );

                    // Log del error
                    _recuperacionService.CrearLogRecuperacion(new LogRecuperacion
                    {
                        DocumentoUsuario = "DESCONOCIDO",
                        ChatId = message.Chat.Id,
                        TipoAccion = "Error",
                        Descripcion = "Formato incorrecto en comando /recuperar",
                        FechaHora = DateTime.Now,
                        UsernameTelegram = username,
                        Exitoso = false,
                        DetalleError = $"Comando recibido: {message.Text}"
                    });
                    return;
                }

                string documento = partes[1];

                // Verificar si el usuario existe
                var usuarioService = new UsuarioService();
                var usuario = usuarioService.Listar().FirstOrDefault(u => u.Documento == documento);

                if (usuario == null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "❌ No se encontró un usuario con ese documento.\n\n" +
                              "Verifica que el documento sea correcto o contacta al administrador.",
                        cancellationToken: cancellationToken
                    );

                    // Log del intento fallido
                    _recuperacionService.CrearLogRecuperacion(new LogRecuperacion
                    {
                        DocumentoUsuario = documento,
                        ChatId = message.Chat.Id,
                        TipoAccion = "UsuarioNoEncontrado",
                        Descripcion = "Intento de recuperación con documento inexistente",
                        FechaHora = DateTime.Now,
                        UsernameTelegram = username,
                        Exitoso = false
                    });
                    return;
                }

                // Verificar si el usuario está activo
                if (!usuario.Estado)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "❌ El usuario está inactivo.\n\n" +
                              "Contacta al administrador para reactivar tu cuenta.",
                        cancellationToken: cancellationToken
                    );

                    // Log del intento con usuario inactivo
                    _recuperacionService.CrearLogRecuperacion(new LogRecuperacion
                    {
                        DocumentoUsuario = documento,
                        ChatId = message.Chat.Id,
                        TipoAccion = "UsuarioInactivo",
                        Descripcion = "Intento de recuperación con usuario inactivo",
                        FechaHora = DateTime.Now,
                        UsernameTelegram = username,
                        Exitoso = false
                    });
                    return;
                }

                // Generar código de verificación único
                string codigoVerificacion = _recuperacionService.GenerarCodigoUnico();
                int tiempoExpiracion = Convert.ToInt32(_recuperacionService.ObtenerConfiguracion("TiempoExpiracionCodigo", "10"));

                // Crear registro en base de datos
                var codigoRecuperacion = new CodigoRecuperacion
                {
                    ChatId = message.Chat.Id,
                    DocumentoUsuario = documento,
                    CodigoVerificacion = codigoVerificacion,
                    FechaCreacion = DateTime.Now,
                    FechaExpiracion = DateTime.Now.AddMinutes(tiempoExpiracion),
                    Estado = "Pendiente",
                    IntentosUsados = 0,
                    UsernameTelegram = username,
                    Observaciones = "Código generado automáticamente"
                };

                string mensaje;
                bool codigoCreado = _recuperacionService.CrearCodigoRecuperacion(codigoRecuperacion, out mensaje);

                if (codigoCreado)
                {
                    // Enviar código al usuario
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"✅ Usuario encontrado: {usuario.NombreCompleto}\n\n" +
                              $"🔐 Tu código de verificación es:\n" +
                              $"{codigoVerificacion}\n\n" +
                              $"📝 Envía este código para continuar con la recuperación.\n" +
                              $"⏰ El código expira en {tiempoExpiracion} minutos.",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );

                    // Log exitoso
                    _recuperacionService.CrearLogRecuperacion(new LogRecuperacion
                    {
                        DocumentoUsuario = documento,
                        ChatId = message.Chat.Id,
                        TipoAccion = "CodigoEnviado",
                        Descripcion = $"Código de verificación enviado exitosamente. Expira en {tiempoExpiracion} minutos",
                        FechaHora = DateTime.Now,
                        UsernameTelegram = username,
                        Exitoso = true
                    });
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "❌ Error al generar el código de verificación.\n\n" +
                              "Intenta nuevamente o contacta al administrador.",
                        cancellationToken: cancellationToken
                    );

                    // Log del error
                    _recuperacionService.CrearLogRecuperacion(new LogRecuperacion
                    {
                        DocumentoUsuario = documento,
                        ChatId = message.Chat.Id,
                        TipoAccion = "Error",
                        Descripcion = "Error al crear código de verificación",
                        FechaHora = DateTime.Now,
                        UsernameTelegram = username,
                        Exitoso = false,
                        DetalleError = mensaje
                    });
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "❌ Error al procesar la solicitud.\n\n" +
                          "Intenta nuevamente o contacta al administrador.",
                    cancellationToken: cancellationToken
                );

                Console.WriteLine($"Error en ProcesarSolicitudRecuperacion: {ex.Message}");
            }
        }

        private async Task ProcesarCodigoRecuperacion(ITelegramBotClient botClient, Message message, string username, CancellationToken cancellationToken)
        {
            try
            {
                string codigo = message.Text;

                // Buscar código en base de datos
                var codigoRecuperacion = _recuperacionService.BuscarCodigoPorCodigo(codigo);

                if (codigoRecuperacion == null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "❌ Código inválido.\n\n" +
                              "Solicita un nuevo código con:\n" +
                              "/recuperar TU_DOCUMENTO",
                        cancellationToken: cancellationToken
                    );

                    // Log del intento fallido
                    _recuperacionService.CrearLogRecuperacion(new LogRecuperacion
                    {
                        DocumentoUsuario = "DESCONOCIDO",
                        ChatId = message.Chat.Id,
                        TipoAccion = "CodigoInvalido",
                        Descripcion = "Intento de uso de código inexistente",
                        FechaHora = DateTime.Now,
                        UsernameTelegram = username,
                        Exitoso = false,
                        DetalleError = $"Código enviado: {codigo}"
                    });
                    return;
                }

                // Verificar si el código ha expirado
                if (DateTime.Now > codigoRecuperacion.FechaExpiracion || codigoRecuperacion.Estado != "Pendiente")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "❌ El código ha expirado o ya fue usado.\n\n" +
                              "Solicita uno nuevo con:\n" +
                              "/recuperar TU_DOCUMENTO",
                        cancellationToken: cancellationToken
                    );

                    // Log del código expirado
                    _recuperacionService.CrearLogRecuperacion(new LogRecuperacion
                    {
                        DocumentoUsuario = codigoRecuperacion.DocumentoUsuario,
                        ChatId = message.Chat.Id,
                        TipoAccion = "CodigoExpirado",
                        Descripcion = "Intento de uso de código expirado o ya usado",
                        FechaHora = DateTime.Now,
                        UsernameTelegram = username,
                        Exitoso = false
                    });
                    return;
                }

                // Verificar que sea el mismo chat
                if (codigoRecuperacion.ChatId != message.Chat.Id)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "❌ Código inválido.",
                        cancellationToken: cancellationToken
                    );

                    // Incrementar intentos fallidos
                    _recuperacionService.IncrementarIntentos(codigo);
                    return;
                }

                // Generar nueva contraseña temporal
                int longitudContrasena = Convert.ToInt32(_recuperacionService.ObtenerConfiguracion("LongitudContrasena", "8"));
                string nuevaContrasena = GenerarContrasenaTemporalAsync(longitudContrasena);

                // Buscar usuario y actualizar contraseña
                var usuarioService = new UsuarioService();
                var usuario = usuarioService.Listar().FirstOrDefault(u => u.Documento == codigoRecuperacion.DocumentoUsuario);

                if (usuario != null)
                {
                    usuario.Clave = nuevaContrasena;
                    string mensajeActualizacion;
                    bool actualizado = usuarioService.Editar(usuario, out mensajeActualizacion);

                    if (actualizado)
                    {
                        // Marcar código como usado
                        string mensajeCodigo;
                        _recuperacionService.MarcarCodigoComoUsado(codigo, out mensajeCodigo);

                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"✅ ¡Contraseña restablecida exitosamente!\n\n" +
                                  $"👤 Usuario: {usuario.NombreCompleto}\n" +
                                  $"📄 Documento: {usuario.Documento}\n" +
                                  $"🔑 Nueva contraseña: {nuevaContrasena}\n\n" +
                                  $"⚠ IMPORTANTE:\n" +
                                  $"• Cambia esta contraseña después de iniciar sesión\n" +
                                  $"• No compartas esta información\n\n" +
                                  $"🔐 Ya puedes iniciar sesión en el sistema.",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );

                        // Log exitoso
                        _recuperacionService.CrearLogRecuperacion(new LogRecuperacion
                        {
                            DocumentoUsuario = codigoRecuperacion.DocumentoUsuario,
                            ChatId = message.Chat.Id,
                            TipoAccion = "ContrasenaRestablecida",
                            Descripcion = "Contraseña restablecida exitosamente",
                            FechaHora = DateTime.Now,
                            UsernameTelegram = username,
                            Exitoso = true
                        });
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"❌ Error al actualizar la contraseña.\n\n" +
                                  $"Detalle: {mensajeActualizacion}\n" +
                                  $"Contacta al administrador del sistema.",
                            cancellationToken: cancellationToken
                        );

                        // Log del error
                        _recuperacionService.CrearLogRecuperacion(new LogRecuperacion
                        {
                            DocumentoUsuario = codigoRecuperacion.DocumentoUsuario,
                            ChatId = message.Chat.Id,
                            TipoAccion = "ErrorActualizacion",
                            Descripcion = "Error al actualizar contraseña en base de datos",
                            FechaHora = DateTime.Now,
                            UsernameTelegram = username,
                            Exitoso = false,
                            DetalleError = mensajeActualizacion
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "❌ Error al procesar el código.\n\n" +
                          "Intenta nuevamente o contacta al administrador.",
                    cancellationToken: cancellationToken
                );

                Console.WriteLine($"Error en ProcesarCodigoRecuperacion: {ex.Message}");
            }
        }

        private string GenerarContrasenaTemporalAsync(int longitud = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, longitud)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void AbrirTelegramRecuperacion()
        {
            try
            {
                var service = new RecuperacionService();
                string botUsername = service.ObtenerConfiguracion("BotUsername");
                string telegramUrl = $"https://t.me/{botUsername}";

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = telegramUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al abrir Telegram: {ex.Message}");

                var service = new RecuperacionService();
                string botUsername = service.ObtenerConfiguracion("BotUsername", "VentaHABot");
                Console.WriteLine("No se pudo abrir el enlace de Telegram. Copia manualmente la URL:");
                Console.WriteLine($"https://t.me/{botUsername}");
            }
        }



    }

}
