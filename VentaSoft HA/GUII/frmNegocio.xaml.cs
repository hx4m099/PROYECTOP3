using Entidades;
using Logica;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GUI
{
    public partial class frmNegocio : Window
    {
        public frmNegocio()
        {
            InitializeComponent();
            // Suscribirse al evento Loaded correctamente
            Loaded += frmNegocio_Load;
        }

        public BitmapImage ByteToImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            try
            {
                BitmapImage image = new BitmapImage();
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze(); // Importante para uso en UI
                }
                return image;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al convertir imagen: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void ActualizarVistaPrevia()
        {
            try
            {
                lblNombrePreview.Text = string.IsNullOrEmpty(txtnombre.Text) ? "Nombre del Negocio" : txtnombre.Text;
                lblRucPreview.Text = string.IsNullOrEmpty(txtruc.Text) ? "R.U.C." : txtruc.Text;
                lblDireccionPreview.Text = string.IsNullOrEmpty(txtdireccion.Text) ? "Dirección" : txtdireccion.Text;

                if (picLogo.Source != null)
                {
                    previewLogo.Source = picLogo.Source;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't show to user for preview updates
                System.Diagnostics.Debug.WriteLine($"Error actualizando vista previa: {ex.Message}");
            }
        }

        private void frmNegocio_Load(object sender, RoutedEventArgs e)
        {
            try
            {
                // Cargar logo existente
                bool obtenido = true;
                byte[] byteimage = new NegocioService().ObtenerLogo(out obtenido);

                if (obtenido && byteimage != null && byteimage.Length > 0)
                {
                    var logoImage = ByteToImage(byteimage);
                    if (logoImage != null)
                    {
                        picLogo.Source = logoImage;
                        previewLogo.Source = logoImage;

                        // Ocultar placeholder si existe
                        if (placeholderIcon != null)
                            placeholderIcon.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    // Mostrar placeholder si no hay logo
                    if (placeholderIcon != null)
                        placeholderIcon.Visibility = Visibility.Visible;
                }

                // Cargar datos del negocio
                Negocio datos = new NegocioService().ObtenerDatos();
                if (datos != null)
                {
                    txtnombre.Text = datos.Nombre ?? "";
                    txtruc.Text = datos.RUC ?? "";
                    txtdireccion.Text = datos.Direccion ?? "";
                }

                // Suscribir eventos para actualización en tiempo real
                txtnombre.TextChanged += (s, args) => ActualizarVistaPrevia();
                txtruc.TextChanged += (s, args) => ActualizarVistaPrevia();
                txtdireccion.TextChanged += (s, args) => ActualizarVistaPrevia();

                // Actualizar vista previa inicial
                ActualizarVistaPrevia();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos del negocio: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnsubir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog oOpenFileDialog = new OpenFileDialog();
                oOpenFileDialog.Filter = "Archivos de Imagen|*.jpg;*.jpeg;*.png;*.bmp;*.gif|" +
                                       "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                                       "PNG (*.png)|*.png|" +
                                       "Bitmap (*.bmp)|*.bmp|" +
                                       "Todos los archivos (*.*)|*.*";
                oOpenFileDialog.Title = "Seleccionar Logo del Negocio";
                oOpenFileDialog.Multiselect = false;

                if (oOpenFileDialog.ShowDialog() == true)
                {
                    string filePath = oOpenFileDialog.FileName;

                    // Verificar que el archivo existe
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show("El archivo seleccionado no existe", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Verificar tamaño del archivo (máximo 5MB)
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length > 5 * 1024 * 1024) // 5MB
                    {
                        MessageBox.Show("El archivo es demasiado grande. Máximo 5MB permitido.", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Leer archivo
                    byte[] byteimage = File.ReadAllBytes(filePath);

                    if (byteimage == null || byteimage.Length == 0)
                    {
                        MessageBox.Show("Error al leer el archivo de imagen", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Validar que es una imagen válida
                    var testImage = ByteToImage(byteimage);
                    if (testImage == null)
                    {
                        MessageBox.Show("El archivo seleccionado no es una imagen válida", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Guardar en base de datos
                    string mensaje = string.Empty;
                    bool respuesta = new NegocioService().ActualizarLogo(byteimage, out mensaje);

                    if (respuesta)
                    {
                        // Actualizar interfaz
                        picLogo.Source = testImage;
                        previewLogo.Source = testImage;

                        // Ocultar placeholder
                        if (placeholderIcon != null)
                            placeholderIcon.Visibility = Visibility.Collapsed;

                        MessageBox.Show("Logo actualizado correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Error al guardar el logo: {mensaje}", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("No tiene permisos para acceder al archivo seleccionado", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"Error de entrada/salida: {ioEx.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al cargar la imagen: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnguardarcambios_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(txtnombre.Text))
                {
                    MessageBox.Show("El nombre del negocio es obligatorio", "Validación",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtnombre.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtruc.Text))
                {
                    MessageBox.Show("El R.U.C. es obligatorio", "Validación",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtruc.Focus();
                    return;
                }

                // Validar formato de RUC (básico)
                if (txtruc.Text.Trim().Length < 8)
                {
                    MessageBox.Show("El R.U.C. debe tener al menos 8 caracteres", "Validación",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtruc.Focus();
                    return;
                }

                string mensaje = string.Empty;

                Negocio obj = new Negocio()
                {
                    Nombre = txtnombre.Text.Trim(),
                    RUC = txtruc.Text.Trim(),
                    Direccion = txtdireccion.Text.Trim()
                };

                bool respuesta = new NegocioService().GuardarDatos(obj, out mensaje);

                if (respuesta)
                {
                    MessageBox.Show("Los cambios fueron guardados exitosamente", "Éxito",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    ActualizarVistaPrevia();
                }
                else
                {
                    MessageBox.Show($"No se pudieron guardar los cambios: {mensaje}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar los cambios: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método para limpiar logo
        private void btnlimpiarlo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("¿Está seguro que desea eliminar el logo?", "Confirmar",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    string mensaje = string.Empty;
                    bool respuesta = new NegocioService().ActualizarLogo(null, out mensaje);

                    if (respuesta)
                    {
                        picLogo.Source = null;
                        previewLogo.Source = null;

                        if (placeholderIcon != null)
                            placeholderIcon.Visibility = Visibility.Visible;

                        MessageBox.Show("Logo eliminado correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Error al eliminar el logo: {mensaje}", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el logo: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnsalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}