using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Entidades;

namespace GUI
{
    public partial class Inicio : Window
    {
        private Usuario _Usuario;
        private List<Permiso> _Permisos; // Lista de permisos del usuario

        // Constructor que recibe usuario Y permisos
        public Inicio(Usuario oUsuario = null, List<Permiso> permisos = null)
        {
            _Usuario = oUsuario;
            _Permisos = permisos ?? new List<Permiso>();

            InitializeComponent();

            if (_Usuario != null)
            {
                lblusuario.Text = _Usuario.NombreCompleto;
            }

            // Aplicar permisos al menú
            AplicarPermisos();

            // Mostrar dashboard por defecto
            MostrarDashboard();
        }

        // ✅ MODIFICADO: Método para aplicar permisos usando los nombres de la base de datos
        private void AplicarPermisos()
        {
            try
            {
                if (_Permisos == null || !_Permisos.Any())
                {
                    // Si no hay permisos, ocultar todo el menú
                    OcultarTodosLosMenus();
                    return;
                }

                // Verificar cada elemento del menú según los permisos
                // Usando los nombres exactos de la base de datos

                // Usuarios
                menuusuarios.Visibility = TienePermiso("menuusuarios") ? Visibility.Visible : Visibility.Collapsed;

                // Mantenedor (submenús de mantenimiento)
                bool tieneMantenedor = TienePermiso("menumantenedor");
                menumantenedor.Visibility = tieneMantenedor ? Visibility.Visible : Visibility.Collapsed;

                // Ventas
                bool tieneVentas = TienePermiso("menuventas");
                menuventas.Visibility = tieneVentas ? Visibility.Visible : Visibility.Collapsed;

                // Compras
                bool tieneCompras = TienePermiso("menucompras");
                menucompras.Visibility = tieneCompras ? Visibility.Visible : Visibility.Collapsed;

                // Clientes
                menuclientes.Visibility = TienePermiso("menuclientes") ? Visibility.Visible : Visibility.Collapsed;

                // Proveedores
                menuproveedores.Visibility = TienePermiso("menuproveedores") ? Visibility.Visible : Visibility.Collapsed;

                // Reportes
                bool tieneReportes = TienePermiso("menureportes");
                menureportes.Visibility = tieneReportes ? Visibility.Visible : Visibility.Collapsed;

           
                // Debug: Mostrar información de permisos aplicados
                System.Diagnostics.Debug.WriteLine("=== PERMISOS APLICADOS ===");
                System.Diagnostics.Debug.WriteLine($"Usuario: {_Usuario?.NombreCompleto}");
                System.Diagnostics.Debug.WriteLine($"Rol: {_Usuario?.oRol?.Descripcion}");
                System.Diagnostics.Debug.WriteLine($"Total permisos: {_Permisos.Count}");
                foreach (var permiso in _Permisos)
                {
                    System.Diagnostics.Debug.WriteLine($"✓ {permiso.NombreMenu}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aplicar permisos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método para verificar si el usuario tiene un permiso específico
        private bool TienePermiso(string nombreMenu)
        {
            return _Permisos?.Any(p => p.NombreMenu.Equals(nombreMenu, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        // Método para ocultar todos los menús
        private void OcultarTodosLosMenus()
        {
            menuusuarios.Visibility = Visibility.Collapsed;
            menumantenedor.Visibility = Visibility.Collapsed;
            menuventas.Visibility = Visibility.Collapsed;
            menucompras.Visibility = Visibility.Collapsed;
            menuclientes.Visibility = Visibility.Collapsed;
            menuproveedores.Visibility = Visibility.Collapsed;
            menureportes.Visibility = Visibility.Collapsed;
          
        }

        // ✅ MODIFICADO: Verificar permisos antes de abrir formularios usando los nombres de la BD
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
            if (!TienePermiso("menuusuarios"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (!TienePermiso("menumantenedor"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (!TienePermiso("menumantenedor"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (!TienePermiso("menumantenedor"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (!TienePermiso("menuventas"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (!TienePermiso("menucompras"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (!TienePermiso("menucompras"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (!TienePermiso("menuclientes"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (!TienePermiso("menuproveedores"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (!TienePermiso("menureportes"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (!TienePermiso("menureportes"))
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección.", "Acceso Denegado",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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