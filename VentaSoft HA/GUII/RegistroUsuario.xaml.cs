using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Entidades;
using Logica;
using GUI.Utilidades;

namespace GUI
{
    public partial class RegistroUsuario : Window
    {
        public RegistroUsuario()
        {
            InitializeComponent();
            CargarRoles();
        }

        private void CargarRoles()
        {
            try
            {
                // Cargar roles disponibles
                List<Rol> listaRol = new RolService().Listar();
                
                // Filtrar solo roles básicos para registro público (opcional)
                // Puedes comentar esta línea si quieres mostrar todos los roles
                listaRol = listaRol.Where(r => r.Descripcion.ToLower() != "administrador").ToList();
                
                foreach (Rol item in listaRol)
                {
                    cboRol.Items.Add(new OpcionCombo() { Valor = item.IdRol, Texto = item.Descripcion });
                }
                
                cboRol.DisplayMemberPath = "Texto";
                cboRol.SelectedValuePath = "Valor";
                
                if (cboRol.Items.Count > 0)
                    cboRol.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar roles: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos
                if (!ValidarCampos())
                    return;

                // Crear objeto usuario
                Usuario nuevoUsuario = new Usuario()
                {
                    IdUsuario = 0,
                    Documento = txtDocumento.Text.Trim(),
                    NombreCompleto = txtNombreCompleto.Text.Trim(),
                    Correo = txtCorreo.Text.Trim(),
                    Clave = txtClave.Password,
                    oRol = new Rol() { IdRol = Convert.ToInt32(((OpcionCombo)cboRol.SelectedItem).Valor) },
                    Estado = true // Por defecto activo
                };

                // Registrar usuario
                string mensaje = string.Empty;
                int idUsuarioGenerado = new UsuarioService().Registrar(nuevoUsuario, out mensaje);

                if (idUsuarioGenerado != 0)
                {
                    MessageBox.Show("Usuario registrado exitosamente", "Registro Exitoso", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Cerrar ventana con resultado exitoso
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(mensaje, "Error en el Registro", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar usuario: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarCampos()
        {
            // Validar documento
            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MessageBox.Show("Debe ingresar el número de documento", "Validación", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDocumento.Focus();
                return false;
            }

            // Validar que el documento sea numérico
            if (!txtDocumento.Text.All(char.IsDigit))
            {
                MessageBox.Show("El documento debe contener solo números", "Validación", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDocumento.Focus();
                return false;
            }

            // Validar nombre completo
            if (string.IsNullOrWhiteSpace(txtNombreCompleto.Text))
            {
                MessageBox.Show("Debe ingresar el nombre completo", "Validación", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombreCompleto.Focus();
                return false;
            }

            // Validar correo
            if (string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                MessageBox.Show("Debe ingresar el correo electrónico", "Validación", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCorreo.Focus();
                return false;
            }

            // Validar formato de correo
            if (!EsCorreoValido(txtCorreo.Text))
            {
                MessageBox.Show("El formato del correo electrónico no es válido", "Validación", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCorreo.Focus();
                return false;
            }

            // Validar contraseña
            if (string.IsNullOrWhiteSpace(txtClave.Password))
            {
                MessageBox.Show("Debe ingresar una contraseña", "Validación", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtClave.Focus();
                return false;
            }

            // Validar longitud de contraseña
            if (txtClave.Password.Length < 6)
            {
                MessageBox.Show("La contraseña debe tener al menos 6 caracteres", "Validación", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtClave.Focus();
                return false;
            }

            // Validar confirmación de contraseña
            if (txtClave.Password != txtConfirmarClave.Password)
            {
                MessageBox.Show("Las contraseñas no coinciden", "Validación", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtConfirmarClave.Focus();
                return false;
            }

            // Validar rol seleccionado
            if (cboRol.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un rol", "Validación", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                cboRol.Focus();
                return false;
            }

            // Validar que el documento no exista
            try
            {
                var usuarioExistente = new UsuarioService().Listar()
                    .FirstOrDefault(u => u.Documento == txtDocumento.Text.Trim());
                
                if (usuarioExistente != null)
                {
                    MessageBox.Show("Ya existe un usuario con este número de documento", "Validación", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtDocumento.Focus();
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al validar documento: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool EsCorreoValido(string correo)
        {
            try
            {
                string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(correo, patron);
            }
            catch
            {
                return false;
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtDocumento.Focus();
        }
    }
}