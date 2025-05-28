using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Entidades;
using Logica;
using VentaSoftHA.Logica; 

namespace GUI
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btncancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btningresar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos vacíos
                if (string.IsNullOrWhiteSpace(txtdocumento.Text))
                {
                    MessageBox.Show("Debe ingresar el documento", "Validación",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtdocumento.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtclave.Password))
                {
                    MessageBox.Show("Debe ingresar la contraseña", "Validación",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtclave.Focus();
                    return;
                }

                // Buscar usuario
                Usuario ousuario = new UsuarioService().Listar()
                    .Where(u => u.Documento == txtdocumento.Text && u.Clave == txtclave.Password)
                    .FirstOrDefault();

                if (ousuario != null)
                {
                    // Verificar si el usuario está activo
                    if (!ousuario.Estado)
                    {
                        MessageBox.Show("El usuario está inactivo. Contacte al administrador.",
                                      "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Abrir ventana principal
                    Inicio form = new Inicio(ousuario);
                    form.Show();
                    this.Hide();

                    // Suscribirse al evento Closing
                    form.Closing += frm_closing;
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos", "Error de Autenticación",
                                  MessageBoxButton.OK, MessageBoxImage.Error);

                    // Limpiar campos
                    txtdocumento.Text = "";
                    txtclave.Password = "";
                    txtdocumento.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento para recuperar contraseña via Telegram
        private void linkRecuperarContrasena_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var resultado = MessageBox.Show(
                    "🔑 RECUPERACIÓN DE CONTRASEÑA VÍA TELEGRAM\n\n" +
                    "Para recuperar tu contraseña:\n\n" +
                    "1️⃣ Se abrirá nuestro bot de Telegram\n" +
                    "2️⃣ Envía: /recuperar TU_DOCUMENTO\n" +
                    "3️⃣ Recibirás un código de verificación\n" +
                    "4️⃣ Envía el código para obtener tu nueva contraseña\n\n" +
                    "⚠ Necesitas tener Telegram instalado\n\n" +
                    "¿Deseas continuar?",
                    "Recuperar Contraseña",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    // Inicializar el servicio de Telegram si no está iniciado
                    TelegramService.GetInstance();

                    // Abrir Telegram
                    TelegramService.AbrirTelegramRecuperacion();

                    MessageBox.Show(
                        "✅ Se ha abierto Telegram\n\n" +
                        "📱 Busca nuestro bot y sigue las instrucciones:\n" +
                        "• Envía: /recuperar TU_DOCUMENTO\n" +
                        "• Luego envía el código que recibas\n\n" +
                        "🔄 Una vez recuperada tu contraseña, regresa aquí para iniciar sesión",
                        "Telegram Abierto",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error al abrir Telegram: {ex.Message}\n\n" +
                    "💡 Asegúrate de tener Telegram instalado o contacta al administrador.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Evento para el hipervínculo de registro
        private void linkRegistrarse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistroUsuario formRegistro = new RegistroUsuario();
                formRegistro.Owner = this;

                // Mostrar como diálogo modal
                bool? resultado = formRegistro.ShowDialog();

                if (resultado == true)
                {
                    MessageBox.Show("Usuario registrado exitosamente. Ahora puede iniciar sesión.",
                                  "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Limpiar campos de login
                    txtdocumento.Text = "";
                    txtclave.Password = "";
                    txtdocumento.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el formulario de registro: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento para cuando se cierra la ventana principal
        private void frm_closing(object sender, CancelEventArgs e)
        {
            // Limpiar campos de login
            txtdocumento.Text = "";
            txtclave.Password = "";

            Login login = new Login();
            login.Show();

            // Enfocar el campo de documento
            txtdocumento.Focus();
        }

        // Evento para manejar Enter en los campos
        private void txtdocumento_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                txtclave.Focus();
            }
        }

        private void txtclave_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btningresar_Click(sender, null);
            }
        }

        // Evento cuando se carga la ventana
        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            txtdocumento.Focus();
        }
    }
}