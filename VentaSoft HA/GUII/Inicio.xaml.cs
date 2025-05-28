using System;
using System.Windows;
using System.Windows.Controls;
using Entidades;



namespace GUI
{
    public partial class Inicio : Window
    {
        private Usuario _Usuario;

        private void btnsalir_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("¿Está seguro que desea salir del sistema?",
                                       "Confirmar",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void menuusuarios_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmUsuarios = new frmUsuarios();
                frmUsuarios.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir usuarios: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void submenucategoria_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmCategorias = new frmCategoria();
                frmCategorias.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir categorías: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void submenuproducto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmProductos = new frmProducto();
                frmProductos.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir productos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void submenunegocio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmNegocio = new frmNegocio();
                frmNegocio.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir negocio: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void submenuregistrarventa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmVentas = new frmVentas(_Usuario);
                frmVentas.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir ventas: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void submenuregistrarcompra_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmCompras = new frmCompras(_Usuario);
                frmCompras.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir compras: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void submenutverdetallecompra_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmDetalleCompra = new frmDetalleCompra();
                frmDetalleCompra.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir detalle de compra: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void menuclientes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmClientes = new frmClientes();
                frmClientes.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir clientes: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void menuproveedores_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmProveedores = new frmProveedores();
                frmProveedores.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir proveedores: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void submenureportecompras_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmReporteCompras = new frmReporteCompras();
                frmReporteCompras.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir reporte de compras: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void submenureporteventas_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frmReporteVentas = new frmReporteVentas();
                frmReporteVentas.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir reporte de ventas: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Agregar este método a tu clase Inicio existente para mostrar el dashboard

        private void MostrarDashboard()
        {
            try
            {
                // Limpiar el contenido actual del contenedor
                if (contenedor.Child != null)
                {
                    contenedor.Child = null;
                }

                // Ocultar el contenido de bienvenida
                welcomeContent.Visibility = Visibility.Collapsed;

                // Crear y agregar el dashboard
                var dashboard = new Dashboard();
                contenedor.Child = dashboard;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Modifica tu constructor para evitar duplicados
        public Inicio(Usuario oUsuario = null)
        {
            _Usuario = oUsuario;
            InitializeComponent();

            if (_Usuario != null)
            {
                lblusuario.Text = _Usuario.NombreCompleto;
            }

            // Mostrar dashboard por defecto
            MostrarDashboard();
        }
        private void menuacercade_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("VentaSoft HA - Sistema de Ventas\n" +
                          "Versión 1.0\n\n" +
                          "Sistema de gestión de ventas desarrollado en WPF\n" +
                          "© 2024 - Todos los derechos reservados",
                          "Acerca de VentaSoft HA",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }
    }
}