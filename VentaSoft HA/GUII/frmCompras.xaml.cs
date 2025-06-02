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

        // Variables para controlar el modo edición
        private bool modoEdicion = false;
        private ProductoCompra productoEnEdicion = null;
        private int indiceProductoEnEdicion = -1;

        public frmCompras(Usuario oUsuario = null)
        {
            _Usuario = oUsuario;
            InitializeComponent();
            productosCompra = new ObservableCollection<ProductoCompra>();
            dgvdata.ItemsSource = productosCompra;
        }

        private void frmCompras_Load(object sender, RoutedEventArgs e)
        {
            txtfecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
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
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab ||
                e.Key == Key.Enter || e.Key == Key.Left || e.Key == Key.Right ||
                e.Key == Key.Home || e.Key == Key.End)
            {
                return;
            }

            if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                return;
            }

            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                return;
            }

            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                TextBox textBox = sender as TextBox;
                if (!textBox.Text.Contains("."))
                {
                    return;
                }
            }

            e.Handled = true;
        }

        private void txtprecioventa_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab ||
                e.Key == Key.Enter || e.Key == Key.Left || e.Key == Key.Right ||
                e.Key == Key.Home || e.Key == Key.End)
            {
                return;
            }

            if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                return;
            }

            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                return;
            }

            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                TextBox textBox = sender as TextBox;
                if (!textBox.Text.Contains("."))
                {
                    return;
                }
            }

            e.Handled = true;
        }

        private void btnagregarproducto_Click(object sender, RoutedEventArgs e)
        {
            decimal preciocompra = 0;
            decimal precioventa = 0;
            int cantidad = 0;

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

            // ✅ MODO EDICIÓN: Actualizar producto existente
            if (modoEdicion && productoEnEdicion != null)
            {
                // ✅ NUEVO: Crear un nuevo objeto con los datos actualizados
                var productoActualizado = new ProductoCompra
                {
                    IdProducto = productoEnEdicion.IdProducto,
                    Producto = productoEnEdicion.Producto,
                    PrecioCompra = preciocompra.ToString("0.00"),
                    PrecioVenta = precioventa.ToString("0.00"),
                    Cantidad = cantidad.ToString(),
                    SubTotal = (cantidad * preciocompra).ToString("0.00")
                };

                // ✅ NUEVO: Reemplazar el objeto completo en la colección
                productosCompra[indiceProductoEnEdicion] = productoActualizado;

                MessageBox.Show($"Producto '{productoActualizado.Producto}' actualizado correctamente",
                              "Producto Actualizado", MessageBoxButton.OK, MessageBoxImage.Information);

                SalirModoEdicion();
                calcularTotal();
                limpiarProducto();
                ActualizarVista(); // ✅ NUEVO: Forzar actualización
                txtcodproducto.Focus();
                return;
            }

            // MODO NORMAL: Buscar si el producto ya existe
            var productoExistente = productosCompra.FirstOrDefault(p => p.IdProducto == txtidproducto.Text);

            if (productoExistente != null)
            {
                var resultado = MessageBox.Show(
                    $"El producto '{txtproducto.Text}' ya está en la lista.\n\n" +
                    $"Cantidad actual: {productoExistente.Cantidad}\n" +
                    $"Precio compra actual: ${productoExistente.PrecioCompra}\n" +
                    $"Precio venta actual: ${productoExistente.PrecioVenta}\n\n" +
                    $"¿Desea actualizar la información?\n\n" +
                    $"• SÍ: Sumar cantidad ({cantidad}) y actualizar precios\n" +
                    $"• NO: Mantener información actual\n" +
                    $"• CANCELAR: No agregar nada",
                    "Producto Existente",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    int cantidadActual = Convert.ToInt32(productoExistente.Cantidad);
                    int nuevaCantidad = cantidadActual + cantidad;

                    // ✅ NUEVO: Crear objeto actualizado y reemplazar
                    var productoActualizado = new ProductoCompra
                    {
                        IdProducto = productoExistente.IdProducto,
                        Producto = productoExistente.Producto,
                        PrecioCompra = preciocompra.ToString("0.00"),
                        PrecioVenta = precioventa.ToString("0.00"),
                        Cantidad = nuevaCantidad.ToString(),
                        SubTotal = (nuevaCantidad * preciocompra).ToString("0.00")
                    };

                    var index = productosCompra.IndexOf(productoExistente);
                    productosCompra[index] = productoActualizado;

                    MessageBox.Show(
                        $"Producto actualizado:\n" +
                        $"• Nueva cantidad: {nuevaCantidad}\n" +
                        $"• Precio compra: ${preciocompra:0.00}\n" +
                        $"• Precio venta: ${precioventa:0.00}\n" +
                        $"• Nuevo subtotal: ${(nuevaCantidad * preciocompra):0.00}",
                        "Producto Actualizado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    calcularTotal();
                    limpiarProducto();
                    ActualizarVista(); // ✅ NUEVO: Forzar actualización
                    txtcodproducto.Focus();
                }
                else if (resultado == MessageBoxResult.No)
                {
                    limpiarProducto();
                    txtcodproducto.Focus();
                }
            }
            else
            {
                // AGREGAR NUEVO: Producto no existe
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
                ActualizarVista(); // ✅ NUEVO: Forzar actualización
                txtcodproducto.Focus();

                MessageBox.Show($"Producto '{txtproducto.Text}' agregado correctamente",
                              "Producto Agregado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ✅ NUEVO: Método para forzar actualización de la vista
        private void ActualizarVista()
        {
            try
            {
                // Método 1: Refrescar el DataGrid
                dgvdata.Items.Refresh();

                // Método 2: Alternativo - Reasignar el ItemsSource
                // dgvdata.ItemsSource = null;
                // dgvdata.ItemsSource = productosCompra;

                System.Diagnostics.Debug.WriteLine("✅ Vista actualizada correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error actualizando vista: {ex.Message}");
            }
        }

        private void btnEditarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var producto = button?.DataContext as ProductoCompra;

            if (producto != null)
            {
                try
                {
                    modoEdicion = true;
                    productoEnEdicion = producto;
                    indiceProductoEnEdicion = productosCompra.IndexOf(producto);

                    txtidproducto.Text = producto.IdProducto;
                    txtproducto.Text = producto.Producto;
                    txtpreciocompra.Text = producto.PrecioCompra;
                    txtprecioventa.Text = producto.PrecioVenta;
                    txtcantidad.Text = producto.Cantidad;

                    var productoInfo = new ProductoService().Listar()
                        .FirstOrDefault(p => p.IdProducto.ToString() == producto.IdProducto);

                    if (productoInfo != null)
                    {
                        txtcodproducto.Text = productoInfo.Codigo;
                        txtcodproducto.Background = new SolidColorBrush(Colors.LightBlue);
                    }

                    btnagregarproducto.Content = "💾 Actualizar";
                    btnagregarproducto.Background = new SolidColorBrush(Color.FromRgb(52, 152, 219));

                    MessageBox.Show($"Modo edición activado para '{producto.Producto}'.\n\n" +
                                  "Modifique los valores y presione 'Actualizar' para guardar los cambios.",
                                  "Modo Edición", MessageBoxButton.OK, MessageBoxImage.Information);

                    txtpreciocompra.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar producto para edición: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    SalirModoEdicion();
                }
            }
        }

        private void SalirModoEdicion()
        {
            modoEdicion = false;
            productoEnEdicion = null;
            indiceProductoEnEdicion = -1;

            btnagregarproducto.Content = "➕ Agregar";
            btnagregarproducto.Background = new SolidColorBrush(Color.FromRgb(230, 126, 34));
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var producto = button?.DataContext as ProductoCompra;

            if (producto != null)
            {
                var resultado = MessageBox.Show(
                    $"¿Está seguro de eliminar el producto '{producto.Producto}'?\n\n" +
                    $"Cantidad: {producto.Cantidad}\n" +
                    $"Subtotal: ${producto.SubTotal}",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    if (modoEdicion && productoEnEdicion == producto)
                    {
                        SalirModoEdicion();
                        limpiarProducto();
                    }

                    productosCompra.Remove(producto);
                    calcularTotal();
                    ActualizarVista(); // ✅ NUEVO: Forzar actualización

                    MessageBox.Show("Producto eliminado correctamente",
                                  "Producto Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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
            if (modoEdicion)
            {
                MessageBox.Show("Debe completar o cancelar la edición del producto antes de registrar la compra.",
                              "Edición Pendiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                    TipoDocumento = lbltipodocumento.Content.ToString(),
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

                    txtidproveedor.Text = "0";
                    txtdocproveedor.Text = "";
                    txtnombreproveedor.Text = "";
                    productosCompra.Clear();
                    calcularTotal();
                    limpiarProducto();
                    SalirModoEdicion();
                    ActualizarVista(); // ✅ NUEVO: Forzar actualización
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