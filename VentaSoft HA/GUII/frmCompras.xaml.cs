using Logica;
using Entidades;
using GUI.Modales;
using GUI.Utilidades;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

namespace GUI
{
    public partial class frmCompras : Window
    {
        public class ProductoCompra
        {
            public string IdProducto { get; set; }
            public string Producto { get; set; }
            public string PrecioCompra { get; set; }
            public string PrecioVenta { get; set; }
            public string Cantidad { get; set; }
            public string SubTotal { get; set; }
        }

        private Usuario _Usuario;
        private ObservableCollection<ProductoCompra> productosCompra;

        public frmCompras(Usuario oUsuario = null)
        {
            _Usuario = oUsuario;
            InitializeComponent();
            productosCompra = new ObservableCollection<ProductoCompra>();
            dgvdata.ItemsSource = productosCompra;
        }

        private void frmCompras_Load(object sender, RoutedEventArgs e)
        {
            // Inicializar la fecha con la fecha actual
            txtfecha.Text = DateTime.Now.ToString("dd/MM/yyyy");

            // Enfocar el primer campo editable
            txtcodproducto.Focus();
        }

        private void btnbuscarproveedor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var modal = new mdProveedor();
                modal.Owner = this;
                var result = modal.ShowDialog();

                if (result == true && modal._Proveedor != null)
                {
                    txtidproveedor.Text = modal._Proveedor.IdProveedor.ToString();
                    txtdocproveedor.Text = modal._Proveedor.Documento;
                    txtnombreproveedor.Text = modal._Proveedor.RazonSocial;
                }
                else
                {
                    txtdocproveedor.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar proveedor: {ex.Message}", "Error",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnbuscarproducto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var modal = new mdProducto();
                modal.Owner = this;
                var result = modal.ShowDialog();

                if (result == true && modal._Producto != null)
                {
                    txtidproducto.Text = modal._Producto.IdProducto.ToString();
                    txtcodproducto.Text = modal._Producto.Codigo;
                    txtproducto.Text = modal._Producto.Nombre;
                    txtcodproducto.Background = new SolidColorBrush(Colors.Honeydew);
                    txtpreciocompra.Focus();
                }
                else
                {
                    txtcodproducto.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar producto: {ex.Message}", "Error",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtcodproducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    Producto oProducto = new ProductoService().Listar().Where(p => p.Codigo == txtcodproducto.Text && p.Estado == true).FirstOrDefault();

                    if (oProducto != null)
                    {
                        txtcodproducto.Background = new SolidColorBrush(Colors.Honeydew);
                        txtidproducto.Text = oProducto.IdProducto.ToString();
                        txtproducto.Text = oProducto.Nombre;
                        txtpreciocompra.Focus();
                    }
                    else
                    {
                        txtcodproducto.Background = new SolidColorBrush(Colors.MistyRose);
                        txtidproducto.Text = "0";
                        txtproducto.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al buscar producto: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void txtpreciocompra_KeyDown(object sender, KeyEventArgs e)
        {
            // Permitir teclas de control (Backspace, Delete, Tab, Enter, etc.)
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab ||
                e.Key == Key.Enter || e.Key == Key.Left || e.Key == Key.Right ||
                e.Key == Key.Home || e.Key == Key.End)
            {
                return;
            }

            // Permitir números del 0-9
            if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                return;
            }

            // Permitir números del teclado numérico
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                return;
            }

            // Permitir punto decimal (solo uno)
            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                TextBox textBox = sender as TextBox;
                if (!textBox.Text.Contains("."))
                {
                    return;
                }
            }

            // Bloquear cualquier otra tecla
            e.Handled = true;
        }

        private void txtprecioventa_KeyDown(object sender, KeyEventArgs e)
        {
            // Permitir teclas de control (Backspace, Delete, Tab, Enter, etc.)
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab ||
                e.Key == Key.Enter || e.Key == Key.Left || e.Key == Key.Right ||
                e.Key == Key.Home || e.Key == Key.End)
            {
                return;
            }

            // Permitir números del 0-9
            if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                return;
            }

            // Permitir números del teclado numérico
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                return;
            }

            // Permitir punto decimal (solo uno)
            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                TextBox textBox = sender as TextBox;
                if (!textBox.Text.Contains("."))
                {
                    return;
                }
            }

            // Bloquear cualquier otra tecla
            e.Handled = true;
        }

        private void btnagregarproducto_Click(object sender, RoutedEventArgs e)
        {
            decimal preciocompra = 0;
            decimal precioventa = 0;
            int cantidad = 0;
            bool producto_existe = false;

            if (int.Parse(txtidproducto.Text) == 0)
            {
                MessageBox.Show("Debe seleccionar un producto", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtpreciocompra.Text, out preciocompra))
            {
                MessageBox.Show("Precio Compra - Formato moneda incorrecto", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtpreciocompra.Focus();
                return;
            }

            if (!decimal.TryParse(txtprecioventa.Text, out precioventa))
            {
                MessageBox.Show("Precio Venta - Formato moneda incorrecto", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtprecioventa.Focus();
                return;
            }

            if (!int.TryParse(txtcantidad.Text, out cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Cantidad debe ser un número mayor a 0", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtcantidad.Focus();
                return;
            }

            foreach (ProductoCompra producto in productosCompra)
            {
                if (producto.IdProducto == txtidproducto.Text)
                {
                    producto_existe = true;
                    break;
                }
            }

            if (!producto_existe)
            {
                productosCompra.Add(new ProductoCompra
                {
                    IdProducto = txtidproducto.Text,
                    Producto = txtproducto.Text,
                    PrecioCompra = preciocompra.ToString("0.00"),
                    PrecioVenta = precioventa.ToString("0.00"),
                    Cantidad = cantidad.ToString(),
                    SubTotal = (cantidad * preciocompra).ToString("0.00")
                });

                calcularTotal();
                limpiarProducto();
                txtcodproducto.Focus();
            }
            else
            {
                MessageBox.Show("El producto ya está agregado", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var producto = button?.DataContext as ProductoCompra;

            if (producto != null)
            {
                productosCompra.Remove(producto);
                calcularTotal();
            }
        }

        private void limpiarProducto()
        {
            txtidproducto.Text = "0";
            txtcodproducto.Text = "";
            txtcodproducto.Background = new SolidColorBrush(Colors.White);
            txtproducto.Text = "";
            txtpreciocompra.Text = "";
            txtprecioventa.Text = "";
            txtcantidad.Text = "1";
        }

        private void calcularTotal()
        {
            decimal total = 0;
            foreach (ProductoCompra producto in productosCompra)
            {
                total += Convert.ToDecimal(producto.SubTotal);
            }
            txttotalpagar.Text = total.ToString("0.00");
        }

        private void btnregistrar_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToInt32(txtidproveedor.Text) == 0)
            {
                MessageBox.Show("Debe seleccionar un proveedor", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (productosCompra.Count < 1)
            {
                MessageBox.Show("Debe ingresar productos en la compra", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DataTable detalle_compra = new DataTable();
                detalle_compra.Columns.Add("IdProducto", typeof(int));
                detalle_compra.Columns.Add("PrecioCompra", typeof(decimal));
                detalle_compra.Columns.Add("PrecioVenta", typeof(decimal));
                detalle_compra.Columns.Add("Cantidad", typeof(int));
                detalle_compra.Columns.Add("MontoTotal", typeof(decimal));

                foreach (ProductoCompra producto in productosCompra)
                {
                    detalle_compra.Rows.Add(new object[] {
                        Convert.ToInt32(producto.IdProducto),
                        producto.PrecioCompra,
                        producto.PrecioVenta,
                        producto.Cantidad,
                        producto.SubTotal
                    });
                }

                int idcorrelativo = new CompraService().ObtenerCorrelativo();
                string numerodocumento = string.Format("{0:00000}", idcorrelativo);

                Compra oCompra = new Compra()
                {
                    oUsuario = new Usuario() { IdUsuario = _Usuario.IdUsuario },
                    oProveedor = new Proveedor() { IdProveedor = Convert.ToInt32(txtidproveedor.Text) },
                    TipoDocumento = lbltipodocumento.Content.ToString(), // Usar el Label en lugar del ComboBox
                    NumeroDocumento = numerodocumento,
                    MontoTotal = Convert.ToDecimal(txttotalpagar.Text)
                };

                string mensaje = string.Empty;
                bool respuesta = new CompraService().Registrar(oCompra, detalle_compra, out mensaje);

                if (respuesta)
                {
                    var result = MessageBox.Show($"Número de compra generada:\n{numerodocumento}\n\n¿Desea copiar al portapapeles?",
                                               "Éxito", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                        Clipboard.SetText(numerodocumento);

                    // Limpiar formulario
                    txtidproveedor.Text = "0";
                    txtdocproveedor.Text = "";
                    txtnombreproveedor.Text = "";
                    productosCompra.Clear();
                    calcularTotal();
                    limpiarProducto();
                }
                else
                {
                    MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar la compra: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}