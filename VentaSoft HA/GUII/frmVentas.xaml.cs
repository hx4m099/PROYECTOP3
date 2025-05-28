using Logica;
using GUI.Utilidades;
using Entidades;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GUI.Modales;
using System.Windows.Controls;
using System.ComponentModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Microsoft.Win32;
using System.IO;

namespace GUI
{
    public partial class frmVentas : Window
    {
        public class ProductoVenta : INotifyPropertyChanged
        {
            private string _cantidad;
            private string _subTotal;

            public string IdProducto { get; set; }
            public string Producto { get; set; }
            public string Precio { get; set; }
            public string Stock { get; set; }

            public string Cantidad
            {
                get => _cantidad;
                set
                {
                    _cantidad = value;
                    OnPropertyChanged(nameof(Cantidad));
                    CalcularSubTotal();
                }
            }

            public string SubTotal
            {
                get => _subTotal;
                set
                {
                    _subTotal = value;
                    OnPropertyChanged(nameof(SubTotal));
                }
            }

            private void CalcularSubTotal()
            {
                if (decimal.TryParse(Precio, out decimal precio) &&
                    int.TryParse(Cantidad, out int cantidad))
                {
                    SubTotal = (precio * cantidad).ToString("0.00");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Usuario _Usuario;
        private ObservableCollection<ProductoVenta> productosVenta;

        // Variables para manejar el descuento por cumpleaños
        private bool clienteCumpleanos = false;
        private decimal descuentoCumpleanos = 0.10m; // 10% de descuento por defecto
        private string idClienteSeleccionado = "0";
        private bool permitirCambiarDescuento = true;

        // Variables para la factura
        private string numeroDocumentoVenta = "";
        private bool ventaCompletada = false; // Nueva variable para controlar el estado

        public frmVentas(Usuario oUsuario = null)
        {
            _Usuario = oUsuario;
            InitializeComponent();
            productosVenta = new ObservableCollection<ProductoVenta>();

            // Suscribirse a cambios en la colección para recalcular total
            productosVenta.CollectionChanged += ProductosVenta_CollectionChanged;

            dgvdata.ItemsSource = productosVenta;

            Loaded += frmVentas_Load;

            // Suscribirse al evento LostFocus para verificar cumpleaños cuando se ingresa un cliente manualmente
            txtdocumentocliente.LostFocus += txtdocumentocliente_LostFocus;
        }

        private void ProductosVenta_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            // Si se agrega un producto y ya hay una venta completada, iniciar nueva venta
            if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && ventaCompletada)
            {
                IniciarNuevaVenta();
            }

            calcularTotal();
            // Actualizar resumen de productos
            if (lblResumenProductos != null)
            {
                lblResumenProductos.Text = $"Productos: {productosVenta.Count}";
            }
            // Actualizar resumen de total
            if (lblResumenTotal != null)
            {
                decimal total = productosVenta.Sum(p => decimal.TryParse(p.SubTotal, out decimal subtotal) ? subtotal : 0);
                lblResumenTotal.Text = $"Total: ${total:0.00}";
            }
        }

        private void frmVentas_Load(object sender, RoutedEventArgs e)
        {
            // Configurar fecha actual
            txtfecha.Text = DateTime.Now.ToString("dd/MM/yyyy");

            // Inicializar campos
            txtidproducto.Text = "0";
            txttotalpagar.Text = "0.00";
            txtcambio.Text = "0.00";

            // Inicializar estado de descuento
            clienteCumpleanos = false;
            OcultarPanelesDescuento();

            // Agregar menú contextual al panel de descuento
            AgregarMenuContextual();

            // Configurar información de usuario
            if (txtusuario != null && _Usuario != null)
            {
                txtusuario.Text = _Usuario.NombreCompleto;
            }

            // Inicializar la venta
            IniciarNuevaVenta();
        }

        private void AgregarMenuContextual()
        {
            if (permitirCambiarDescuento && panelDescuentoCumpleanos != null)
            {
                ContextMenu contextMenu = new ContextMenu();
                MenuItem menuItem = new MenuItem();
                menuItem.Header = "Configurar porcentaje de descuento";
                menuItem.Click += (s, e) => ConfigurarDescuentoCumpleanos();
                contextMenu.Items.Add(menuItem);
                panelDescuentoCumpleanos.ContextMenu = contextMenu;
            }
        }

        private void btnbuscarcliente_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Si hay una venta completada, iniciar nueva venta
                if (ventaCompletada)
                {
                    IniciarNuevaVenta();
                }

                var modal = new mdCliente();
                var result = modal.ShowDialog();

                if (result == true)
                {
                    txtdocumentocliente.Text = modal._Cliente.Documento;
                    txtnombrecliente.Text = modal._Cliente.NombreCompleto;
                    idClienteSeleccionado = modal._Cliente.IdCliente.ToString();

                    // Verificar si el cliente está cumpliendo años
                    VerificarCumpleanosCliente(modal._Cliente);

                    // Recalcular total inmediatamente después de seleccionar cliente
                    calcularTotal();
                }
                else
                {
                    txtdocumentocliente.Focus();
                    // Resetear estado de cumpleaños si se cancela la selección
                    ResetearDescuentoCumpleanos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar cliente: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtdocumentocliente_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtdocumentocliente.Text))
            {
                try
                {
                    // Si hay una venta completada, iniciar nueva venta
                    if (ventaCompletada)
                    {
                        IniciarNuevaVenta();
                    }

                    System.Diagnostics.Debug.WriteLine($"=== BUSCANDO CLIENTE ===");
                    System.Diagnostics.Debug.WriteLine($"Documento: {txtdocumentocliente.Text.Trim()}");

                    // CAMBIO: Usar BuscarPorDocumento del ClienteService actualizado
                    var clienteService = new ClienteService();
                    var cliente = clienteService.BuscarPorDocumento(txtdocumentocliente.Text.Trim());

                    if (cliente != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Cliente encontrado: {cliente.NombreCompleto}");
                        txtnombrecliente.Text = cliente.NombreCompleto;
                        idClienteSeleccionado = cliente.IdCliente.ToString();
                        VerificarCumpleanosCliente(cliente);
                        calcularTotal();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Cliente no encontrado");
                        ResetearDescuentoCumpleanos();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error verificando cumpleaños: {ex.Message}");
                    ResetearDescuentoCumpleanos();
                }
            }
            else
            {
                ResetearDescuentoCumpleanos();
            }
        }

        private void VerificarCumpleanosCliente(Cliente cliente)
        {
            try
            {
                // Resetear estado inicial
                clienteCumpleanos = false;

                if (cliente != null)
                {
                    // CAMBIO: Usar EsCumpleanosHoy del ClienteService actualizado
                    var clienteService = new ClienteService();
                    clienteCumpleanos = clienteService.EsCumpleanosHoy(cliente);

                    System.Diagnostics.Debug.WriteLine($"=== VERIFICANDO CUMPLEAÑOS ===");
                    System.Diagnostics.Debug.WriteLine($"Cliente: {cliente.NombreCompleto}");
                    System.Diagnostics.Debug.WriteLine($"Fecha nacimiento: {cliente.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "No especificada"}");
                    System.Diagnostics.Debug.WriteLine($"Fecha actual: {DateTime.Today:dd/MM/yyyy}");
                    System.Diagnostics.Debug.WriteLine($"¿Es cumpleaños?: {clienteCumpleanos}");

                    if (clienteCumpleanos)
                    {
                        int edad = clienteService.CalcularEdad(cliente.FechaNacimiento.Value);
                        System.Diagnostics.Debug.WriteLine($"🎉 ¡CUMPLEAÑOS DETECTADO! Edad: {edad}");

                        // Mostrar panel de descuento
                        MostrarPanelDescuento(cliente, edad);

                        // Mensaje de felicitación
                        string mensajeCumpleanos = $"🎉 ¡FELIZ CUMPLEAÑOS! 🎉\n\n" +
                                                 $"Hoy {cliente.NombreCompleto} cumple {edad} años.\n\n" +
                                                 $"Se aplicará automáticamente un descuento del {descuentoCumpleanos * 100:0}% " +
                                                 $"al total de la compra.\n\n" +
                                                 $"¡Que tengas un día maravilloso! 🎂";

                        MessageBox.Show(mensajeCumpleanos, "¡Feliz Cumpleaños!",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ No es cumpleaños del cliente");
                        OcultarPanelesDescuento();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Cliente es null");
                    OcultarPanelesDescuento();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en VerificarCumpleanosCliente: {ex.Message}");
                ResetearDescuentoCumpleanos();
            }
        }

        private void MostrarPanelDescuento(Cliente cliente, int edad)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Mostrando panel de descuento...");

                if (panelDescuentoCumpleanos != null)
                {
                    panelDescuentoCumpleanos.Visibility = Visibility.Visible;
                    System.Diagnostics.Debug.WriteLine("✅ panelDescuentoCumpleanos visible");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ panelDescuentoCumpleanos es null");
                }

                if (lblMensajeCumpleanos != null)
                {
                    lblMensajeCumpleanos.Text = $"¡{cliente.NombreCompleto} cumple {edad} años hoy!";
                    System.Diagnostics.Debug.WriteLine("✅ lblMensajeCumpleanos actualizado");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ lblMensajeCumpleanos es null");
                }

                if (lblDescuentoAplicado != null)
                {
                    lblDescuentoAplicado.Text = $"Descuento del {descuentoCumpleanos * 100:0}% aplicado";
                    System.Diagnostics.Debug.WriteLine("✅ lblDescuentoAplicado actualizado");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ lblDescuentoAplicado es null");
                }

                System.Diagnostics.Debug.WriteLine($"Panel de descuento configurado. clienteCumpleanos = {clienteCumpleanos}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error mostrando panel descuento: {ex.Message}");
            }
        }

        private void OcultarPanelesDescuento()
        {
            try
            {
                if (panelDescuentoCumpleanos != null)
                    panelDescuentoCumpleanos.Visibility = Visibility.Collapsed;
                if (panelInfoDescuento != null)
                    panelInfoDescuento.Visibility = Visibility.Collapsed;
                if (lblTotalOriginal != null)
                    lblTotalOriginal.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ocultando paneles: {ex.Message}");
            }
        }

        private void ResetearDescuentoCumpleanos()
        {
            clienteCumpleanos = false;
            idClienteSeleccionado = "0";
            OcultarPanelesDescuento();
            calcularTotal();
        }

        private int CalcularEdad(DateTime fechaNacimiento, DateTime fechaActual)
        {
            int edad = fechaActual.Year - fechaNacimiento.Year;

            if (fechaActual.Month < fechaNacimiento.Month ||
                (fechaActual.Month == fechaNacimiento.Month && fechaActual.Day < fechaNacimiento.Day))
            {
                edad--;
            }

            return edad;
        }

        private void ConfigurarDescuentoCumpleanos()
        {
            if (permitirCambiarDescuento && clienteCumpleanos)
            {
                Window dialogoDescuento = new Window()
                {
                    Title = "Configurar Descuento",
                    Width = 350,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    ResizeMode = ResizeMode.NoResize
                };

                StackPanel panel = new StackPanel() { Margin = new Thickness(20) };

                TextBlock lblTexto = new TextBlock()
                {
                    Text = "Ingrese el porcentaje de descuento por cumpleaños:",
                    Margin = new Thickness(0, 0, 0, 10),
                    TextWrapping = TextWrapping.Wrap
                };

                TextBox txtDescuento = new TextBox()
                {
                    Text = (descuentoCumpleanos * 100).ToString(),
                    Margin = new Thickness(0, 0, 0, 10),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                TextBlock lblInfo = new TextBlock()
                {
                    Text = "Rango permitido: 0% - 50%",
                    FontSize = 10,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 15)
                };

                StackPanel panelBotones = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                Button btnAceptar = new Button()
                {
                    Content = "Aceptar",
                    Width = 70,
                    Height = 25,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                Button btnCancelar = new Button()
                {
                    Content = "Cancelar",
                    Width = 70,
                    Height = 25
                };

                btnAceptar.Click += (s, e) =>
                {
                    if (decimal.TryParse(txtDescuento.Text, out decimal nuevoDescuento) &&
                        nuevoDescuento >= 0 && nuevoDescuento <= 50)
                    {
                        descuentoCumpleanos = nuevoDescuento / 100;
                        if (lblDescuentoAplicado != null)
                            lblDescuentoAplicado.Text = $"Descuento del {nuevoDescuento:0}% aplicado";
                        calcularTotal();
                        dialogoDescuento.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("Por favor ingrese un valor entre 0 y 50", "Valor inválido",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                };

                btnCancelar.Click += (s, e) => dialogoDescuento.DialogResult = false;

                panelBotones.Children.Add(btnAceptar);
                panelBotones.Children.Add(btnCancelar);

                panel.Children.Add(lblTexto);
                panel.Children.Add(txtDescuento);
                panel.Children.Add(lblInfo);
                panel.Children.Add(panelBotones);

                dialogoDescuento.Content = panel;
                dialogoDescuento.ShowDialog();
            }
        }

        private void btnbuscarproducto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Si hay una venta completada, iniciar nueva venta
                if (ventaCompletada)
                {
                    IniciarNuevaVenta();
                }

                var modal = new mdProducto();
                var result = modal.ShowDialog();

                if (result == true)
                {
                    txtidproducto.Text = modal._Producto.IdProducto.ToString();
                    txtcodproducto.Text = modal._Producto.Codigo;
                    txtproducto.Text = modal._Producto.Nombre;
                    txtprecio.Text = modal._Producto.PrecioVenta.ToString("0.00");
                    txtstock.Text = modal._Producto.Stock.ToString();
                    txtcantidad.Focus();
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
                    // Si hay una venta completada, iniciar nueva venta
                    if (ventaCompletada)
                    {
                        IniciarNuevaVenta();
                    }

                    Producto oProducto = new ProductoService().Listar()
                        .Where(p => p.Codigo == txtcodproducto.Text && p.Estado == true)
                        .FirstOrDefault();

                    if (oProducto != null)
                    {
                        txtcodproducto.Background = new SolidColorBrush(Colors.Honeydew);
                        txtidproducto.Text = oProducto.IdProducto.ToString();
                        txtproducto.Text = oProducto.Nombre;
                        txtprecio.Text = oProducto.PrecioVenta.ToString("0.00");
                        txtstock.Text = oProducto.Stock.ToString();
                        txtcantidad.Focus();
                    }
                    else
                    {
                        txtcodproducto.Background = new SolidColorBrush(Colors.MistyRose);
                        txtidproducto.Text = "0";
                        txtproducto.Text = "";
                        txtprecio.Text = "";
                        txtstock.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al buscar producto: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void txtprecio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            e.Handled = !regex.IsMatch(((TextBox)sender).Text + e.Text);
        }

        private void txtpagocon_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            e.Handled = !regex.IsMatch(((TextBox)sender).Text + e.Text);
        }

        private void txtpagocon_TextChanged(object sender, TextChangedEventArgs e)
        {
            calcularCambio();
        }

        private void btnagregarproducto_Click(object sender, RoutedEventArgs e)
        {
            decimal precio = 0;
            int cantidad = 0;
            int stock = 0;
            bool producto_existe = false;

            if (int.Parse(txtidproducto.Text) == 0)
            {
                MessageBox.Show("Debe seleccionar un producto", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtprecio.Text, out precio))
            {
                MessageBox.Show("Precio - Formato incorrecto", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtprecio.Focus();
                return;
            }

            if (!int.TryParse(txtcantidad.Text, out cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Cantidad debe ser un número mayor a 0", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtcantidad.Focus();
                return;
            }

            if (!int.TryParse(txtstock.Text, out stock))
            {
                MessageBox.Show("Stock - Formato incorrecto", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verificar stock disponible considerando productos ya agregados
            int cantidadYaAgregada = productosVenta
                .Where(p => p.IdProducto == txtidproducto.Text)
                .Sum(p => int.Parse(p.Cantidad));

            if ((cantidad + cantidadYaAgregada) > stock)
            {
                MessageBox.Show($"Stock insuficiente. Disponible: {stock}, Ya agregado: {cantidadYaAgregada}",
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtcantidad.Focus();
                return;
            }

            foreach (ProductoVenta producto in productosVenta)
            {
                if (producto.IdProducto == txtidproducto.Text)
                {
                    producto_existe = true;
                    break;
                }
            }

            if (!producto_existe)
            {
                var nuevoProducto = new ProductoVenta
                {
                    IdProducto = txtidproducto.Text,
                    Producto = txtproducto.Text,
                    Precio = precio.ToString("0.00"),
                    Stock = txtstock.Text,
                    Cantidad = cantidad.ToString(),
                    SubTotal = (cantidad * precio).ToString("0.00")
                };

                // Suscribirse a cambios de propiedades para recalcular total
                nuevoProducto.PropertyChanged += NuevoProducto_PropertyChanged;

                productosVenta.Add(nuevoProducto);
                limpiarProducto();
                txtcodproducto.Focus();

                // Forzar recálculo del total después de agregar producto
                calcularTotal();
            }
            else
            {
                MessageBox.Show("El producto ya está agregado. Use la tabla para modificar la cantidad.",
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NuevoProducto_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            calcularTotal();
        }

        private void limpiarProducto()
        {
            txtidproducto.Text = "0";
            txtcodproducto.Text = "";
            txtcodproducto.Background = new SolidColorBrush(Colors.White);
            txtproducto.Text = "";
            txtprecio.Text = "";
            txtstock.Text = "";
            txtcantidad.Text = "1";
        }

        private void calcularTotal()
        {
            try
            {
                decimal subtotal = 0;

                // Calcular subtotal de todos los productos
                foreach (ProductoVenta producto in productosVenta)
                {
                    if (decimal.TryParse(producto.SubTotal, out decimal subtotalProducto))
                    {
                        subtotal += subtotalProducto;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"=== CALCULANDO TOTAL ===");
                System.Diagnostics.Debug.WriteLine($"Subtotal: ${subtotal:0.00}");
                System.Diagnostics.Debug.WriteLine($"Cliente cumpleaños: {clienteCumpleanos}");
                System.Diagnostics.Debug.WriteLine($"Porcentaje descuento: {descuentoCumpleanos * 100}%");

                // Aplicar descuento por cumpleaños si corresponde
                if (clienteCumpleanos && subtotal > 0)
                {
                    decimal montoDescuento = subtotal * descuentoCumpleanos;
                    decimal totalConDescuento = subtotal - montoDescuento;

                    System.Diagnostics.Debug.WriteLine($"Monto descuento: ${montoDescuento:0.00}");
                    System.Diagnostics.Debug.WriteLine($"Total con descuento: ${totalConDescuento:0.00}");

                    // Mostrar información del descuento
                    MostrarInfoDescuento(subtotal, montoDescuento);

                    // Establecer el total final
                    txttotalpagar.Text = totalConDescuento.ToString("0.00");
                    txttotalpagar.Foreground = new SolidColorBrush(Colors.Green);

                    System.Diagnostics.Debug.WriteLine("✅ Descuento aplicado correctamente");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Sin descuento aplicado");

                    // Sin descuento
                    txttotalpagar.Text = subtotal.ToString("0.00");
                    txttotalpagar.Foreground = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // Verde del XAML

                    // Ocultar información de descuento
                    if (panelInfoDescuento != null)
                        panelInfoDescuento.Visibility = Visibility.Collapsed;
                    if (lblTotalOriginal != null)
                        lblTotalOriginal.Visibility = Visibility.Collapsed;
                }

                // Recalcular cambio después de actualizar el total
                calcularCambio();

                System.Diagnostics.Debug.WriteLine($"Total final mostrado: {txttotalpagar.Text}");
                System.Diagnostics.Debug.WriteLine($"=== FIN CÁLCULO ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en calcularTotal: {ex.Message}");
                MessageBox.Show($"Error calculando total: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarInfoDescuento(decimal subtotal, decimal montoDescuento)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Mostrando información del descuento...");

                if (panelInfoDescuento != null)
                {
                    panelInfoDescuento.Visibility = Visibility.Visible;
                    System.Diagnostics.Debug.WriteLine("✅ panelInfoDescuento visible");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ panelInfoDescuento es null");
                }

                if (lblTotalOriginal != null)
                {
                    lblTotalOriginal.Text = subtotal.ToString("0.00");
                    lblTotalOriginal.Visibility = Visibility.Visible;
                    System.Diagnostics.Debug.WriteLine($"✅ lblTotalOriginal: {subtotal:0.00}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ lblTotalOriginal es null");
                }

                if (lblMontoDescuento != null)
                {
                    lblMontoDescuento.Text = $"- {montoDescuento:0.00}";
                    System.Diagnostics.Debug.WriteLine($"✅ lblMontoDescuento: - {montoDescuento:0.00}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ lblMontoDescuento es null");
                }

                System.Diagnostics.Debug.WriteLine("Información de descuento configurada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error mostrando info descuento: {ex.Message}");
            }
        }

        private void btneliminar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var producto = button?.DataContext as ProductoVenta;

            if (producto != null)
            {
                var result = MessageBox.Show($"¿Está seguro de eliminar el producto '{producto.Producto}'?",
                                           "Confirmar eliminación",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    productosVenta.Remove(producto);
                    // El evento CollectionChanged llamará automáticamente a calcularTotal()
                }
            }
        }

        private void dgvdata_CellEditEnding(object sender, DataGridCellEditEndingEventArgs args)
        {
            if (args.Column.Header.ToString() == "Cant.")
            {
                var producto = args.Row.Item as ProductoVenta;
                var textBox = args.EditingElement as TextBox;

                if (producto != null && textBox != null)
                {
                    if (int.TryParse(textBox.Text, out int nuevaCantidad) && nuevaCantidad > 0)
                    {
                        int stockDisponible = int.Parse(producto.Stock);

                        if (nuevaCantidad <= stockDisponible)
                        {
                            producto.Cantidad = nuevaCantidad.ToString();
                            // El PropertyChanged llamará automáticamente a calcularTotal()
                        }
                        else
                        {
                            MessageBox.Show($"Cantidad no puede ser mayor al stock disponible ({stockDisponible})",
                                          "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                            args.Cancel = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cantidad debe ser un número mayor a 0",
                                      "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                        args.Cancel = true;
                    }
                }
            }
        }

        private void txttotalpagar_TextChanged(object sender, TextChangedEventArgs e)
        {
            calcularCambio();
        }

        private void calcularCambio()
        {
            if (txttotalpagar == null || txtpagocon == null || txtcambio == null)
                return;

            if (decimal.TryParse(txttotalpagar.Text, out decimal total) &&
                decimal.TryParse(txtpagocon.Text, out decimal pago))
            {
                decimal cambio = pago - total;
                txtcambio.Text = cambio.ToString("0.00");

                // Cambiar color según el cambio
                if (cambio < 0)
                    txtcambio.Foreground = new SolidColorBrush(Colors.Red);
                else
                    txtcambio.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                txtcambio.Text = "0.00";
                txtcambio.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void btncrearventa_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtdocumentocliente.Text))
            {
                MessageBox.Show("Debe ingresar el documento del cliente", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtnombrecliente.Text))
            {
                MessageBox.Show("Debe ingresar el nombre del cliente", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (productosVenta.Count < 1)
            {
                MessageBox.Show("Debe ingresar productos en la venta", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtpagocon.Text, out decimal pago) || pago <= 0)
            {
                MessageBox.Show("Debe ingresar un monto de pago válido", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtpagocon.Focus();
                return;
            }

            decimal total = Convert.ToDecimal(txttotalpagar.Text);
            if (pago < total)
            {
                MessageBox.Show("El pago no puede ser menor al total", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtpagocon.Focus();
                return;
            }

            try
            {
                VentaService ventaService = new VentaService();

                // Verificar y restar stock
                foreach (ProductoVenta producto in productosVenta)
                {
                    int idProducto = Convert.ToInt32(producto.IdProducto);
                    int cantidad = Convert.ToInt32(producto.Cantidad);

                    if (!ventaService.RestarStock(idProducto, cantidad))
                    {
                        MessageBox.Show($"Error al actualizar stock del producto: {producto.Producto}",
                                      "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                DataTable detalle_venta = new DataTable();
                detalle_venta.Columns.Add("IdProducto", typeof(int));
                detalle_venta.Columns.Add("PrecioVenta", typeof(decimal));
                detalle_venta.Columns.Add("Cantidad", typeof(int));
                detalle_venta.Columns.Add("SubTotal", typeof(decimal));

                foreach (ProductoVenta producto in productosVenta)
                {
                    detalle_venta.Rows.Add(new object[] {
                        Convert.ToInt32(producto.IdProducto),
                        Convert.ToDecimal(producto.Precio),
                        Convert.ToInt32(producto.Cantidad),
                        Convert.ToDecimal(producto.SubTotal)
                    });
                }

                int idcorrelativo = ventaService.ObtenerCorrelativo();
                string numerodocumento = string.Format("{0:00000}", idcorrelativo);

                // Calcular el subtotal original para el registro
                decimal subtotalOriginal = productosVenta.Sum(p => Convert.ToDecimal(p.SubTotal));

                Venta oVenta = new Venta()
                {
                    oUsuario = new Usuario() { IdUsuario = _Usuario.IdUsuario },
                    TipoDocumento = txttipodocumento.Text, // Cambiar de ComboBox a TextBox
                    NumeroDocumento = numerodocumento,
                    DocumentoCliente = txtdocumentocliente.Text,
                    NombreCliente = txtnombrecliente.Text,
                    MontoPago = pago,
                    MontoCambio = Convert.ToDecimal(txtcambio.Text),
                    MontoTotal = total, // Este es el total con descuento aplicado
                    DescuentoAplicado = clienteCumpleanos ? descuentoCumpleanos : 0
                };

                string mensaje = string.Empty;
                bool respuesta = ventaService.Registrar(oVenta, detalle_venta, out mensaje);

                if (respuesta)
                {
                    // Guardar número de documento para la factura
                    numeroDocumentoVenta = numerodocumento;
                    ventaCompletada = true; // Marcar venta como completada

                    // Actualizar información de venta en la interfaz
                    if (txtnumerodocumento != null)
                        txtnumerodocumento.Text = numerodocumento;
                    if (txtestado != null)
                    {
                        txtestado.Text = "Completada";
                        txtestado.Background = new SolidColorBrush(Color.FromRgb(212, 237, 218)); // Verde claro
                        txtestado.Foreground = new SolidColorBrush(Color.FromRgb(21, 87, 36)); // Verde oscuro
                    }

                    // Mostrar mensaje de éxito
                    string mensajeExito = $"¡Venta registrada exitosamente!\n\nNúmero de venta: {numerodocumento}";

                    // Agregar información del descuento si se aplicó
                    if (clienteCumpleanos)
                    {
                        decimal montoDescuento = subtotalOriginal - total;
                        mensajeExito += $"\n\n🎉 Descuento por cumpleaños aplicado: {descuentoCumpleanos * 100:0}%";
                        mensajeExito += $"\nSubtotal: ${subtotalOriginal:0.00}";
                        mensajeExito += $"\nDescuento: -${montoDescuento:0.00}";
                        mensajeExito += $"\nTotal final: ${total:0.00}";
                        mensajeExito += $"\nAhorro: ${montoDescuento:0.00}";
                    }

                    mensajeExito += $"\n\n💡 Puede descargar el PDF de la factura usando el botón 'Guardar PDF'";
                    mensajeExito += $"\n💡 Para iniciar una nueva venta, agregue un producto o seleccione un cliente";

                    MessageBox.Show(mensajeExito, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // NO limpiar formulario automáticamente - esperar a que el usuario inicie nueva venta
                }
                else
                {
                    // Si falla el registro, restaurar el stock
                    foreach (ProductoVenta producto in productosVenta)
                    {
                        int idProducto = Convert.ToInt32(producto.IdProducto);
                        int cantidad = Convert.ToInt32(producto.Cantidad);
                        ventaService.SumarStock(idProducto, cantidad);
                    }

                    MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar la venta: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarFormulario()
        {
            txtdocumentocliente.Text = "";
            txtnombrecliente.Text = "";
            productosVenta.Clear();
            limpiarProducto();
            txtpagocon.Text = "";
            txtcambio.Text = "0.00";
            txtcambio.Foreground = new SolidColorBrush(Colors.Black);

            // Resetear estado de cumpleaños
            ResetearDescuentoCumpleanos();
        }

        private void IniciarNuevaVenta()
        {
            // Limpiar datos de factura anterior solo si hay una venta completada
            if (ventaCompletada)
            {
                numeroDocumentoVenta = "";
                ventaCompletada = false;

                // Limpiar formulario
                LimpiarFormulario();
            }

            // Resetear información de venta
            if (txtnumerodocumento != null)
                txtnumerodocumento.Text = "Pendiente...";
            if (txtestado != null)
            {
                txtestado.Text = "En proceso";
                txtestado.Background = new SolidColorBrush(Color.FromRgb(255, 243, 205)); // Amarillo claro
                txtestado.Foreground = new SolidColorBrush(Color.FromRgb(133, 100, 4)); // Amarillo oscuro
            }

            System.Diagnostics.Debug.WriteLine($"Nueva venta iniciada. numeroDocumentoVenta = '{numeroDocumentoVenta}', ventaCompletada = {ventaCompletada}");
        }

        private void dgvdata_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Método vacío pero necesario para el evento
        }

        // Método para probar el descuento manualmente (para debugging)
        public void ActivarDescuentoPrueba()
        {
            clienteCumpleanos = true;
            descuentoCumpleanos = 0.10m; // 10% de prueba

            System.Diagnostics.Debug.WriteLine("🧪 Descuento de prueba activado");

            if (panelDescuentoCumpleanos != null)
            {
                panelDescuentoCumpleanos.Visibility = Visibility.Visible;
                if (lblMensajeCumpleanos != null)
                    lblMensajeCumpleanos.Text = "¡Cliente de prueba cumple años hoy!";
                if (lblDescuentoAplicado != null)
                    lblDescuentoAplicado.Text = $"Descuento del {descuentoCumpleanos * 100:0}% aplicado";
            }

            calcularTotal();

            MessageBox.Show("Descuento de prueba activado (10%). Agregue productos para ver el efecto.",
                          "Prueba de Descuento", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnDescuentoCumpleanos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Verificar si hay un cliente seleccionado
                if (string.IsNullOrWhiteSpace(txtdocumentocliente.Text) || string.IsNullOrWhiteSpace(txtnombrecliente.Text))
                {
                    MessageBox.Show("Debe seleccionar un cliente primero", "Validación",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Buscar cliente por documento
                var clienteService = new ClienteService();
                var cliente = clienteService.BuscarPorDocumento(txtdocumentocliente.Text.Trim());

                if (cliente == null)
                {
                    MessageBox.Show("Cliente no encontrado", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Verificar si el cliente tiene fecha de nacimiento
                if (!cliente.FechaNacimiento.HasValue)
                {
                    MessageBox.Show("El cliente no tiene fecha de nacimiento registrada.\n\nPor favor, actualice la información del cliente en el módulo de gestión de clientes.",
                                  "Información faltante", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Verificar si es cumpleaños del cliente
                bool esCumpleanos = clienteService.EsCumpleanosHoy(cliente);

                if (esCumpleanos)
                {
                    // Es cumpleaños, aplicar descuento automáticamente
                    int edad = clienteService.CalcularEdad(cliente.FechaNacimiento.Value);

                    clienteCumpleanos = true;
                    MostrarPanelDescuento(cliente, edad);
                    calcularTotal();

                    MessageBox.Show($"🎉 ¡Feliz cumpleaños {cliente.NombreCompleto}!\n\n" +
                                  $"Hoy cumple {edad} años.\n" +
                                  $"Se ha aplicado un descuento del {descuentoCumpleanos * 100:0}% automáticamente.",
                                  "¡Feliz Cumpleaños!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // No es cumpleaños, preguntar si quiere aplicar descuento manual
                    var fechaNacimiento = cliente.FechaNacimiento.Value;
                    var proximoCumpleanos = new DateTime(DateTime.Today.Year, fechaNacimiento.Month, fechaNacimiento.Day);

                    if (proximoCumpleanos < DateTime.Today)
                        proximoCumpleanos = proximoCumpleanos.AddYears(1);

                    var diasParaCumpleanos = (proximoCumpleanos - DateTime.Today).Days;

                    var resultado = MessageBox.Show(
                        $"Hoy no es cumpleaños de {cliente.NombreCompleto}.\n\n" +
                        $"Fecha de nacimiento: {fechaNacimiento:dd/MM/yyyy}\n" +
                        $"Próximo cumpleaños: {proximoCumpleanos:dd/MM/yyyy} ({diasParaCumpleanos} días)\n\n" +
                        $"¿Desea aplicar el descuento por cumpleaños de todas formas?",
                        "Aplicar descuento manual",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        int edad = clienteService.CalcularEdad(cliente.FechaNacimiento.Value);

                        clienteCumpleanos = true;
                        MostrarPanelDescuento(cliente, edad);
                        calcularTotal();

                        MessageBox.Show($"Descuento por cumpleaños aplicado manualmente para {cliente.NombreCompleto}.\n\n" +
                                      $"Descuento: {descuentoCumpleanos * 100:0}%",
                                      "Descuento aplicado", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar descuento por cumpleaños: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Error en btnDescuentoCumpleanos_Click: {ex.Message}");
            }
        }

        private void btnguardarPDF_Click(object sender, RoutedEventArgs e)
        {
            // Verificar si hay una venta completada para generar PDF
            if (string.IsNullOrEmpty(numeroDocumentoVenta))
            {
                MessageBox.Show($"No hay ninguna venta registrada para generar PDF.\n\nPrimero debe completar una venta.\n\nEstado actual:\n- Venta completada: {ventaCompletada}\n- Número documento: '{numeroDocumentoVenta ?? "null"}'",
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Intentando generar PDF para venta: {numeroDocumentoVenta}");

                // Buscar la venta por número de documento
                Venta oVenta = new VentaService().ObtenerVenta(numeroDocumentoVenta);

                if (oVenta.IdVenta == 0)
                {
                    MessageBox.Show("No se pudo encontrar la información de la venta para generar el PDF.",
                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string mensajeDescuento = string.Empty;

                if (clienteCumpleanos && descuentoCumpleanos > 0)
                {
                    mensajeDescuento = $"🎉 Descuento especial por cumpleaños aplicado: {(descuentoCumpleanos * 100):0}%";
                }
                else
                {
                    mensajeDescuento = string.Empty;
                }

               
                // Generar PDF usando la misma lógica que frmDetalleVenta
                string Texto_Html = Properties.Resources.PlantillaVenta.ToString();
                Negocio odatos = new NegocioService().ObtenerDatos();

                Texto_Html = Texto_Html.Replace("@nombrenegocio", odatos.Nombre.ToUpper());
                Texto_Html = Texto_Html.Replace("@docnegocio", odatos.RUC);
                Texto_Html = Texto_Html.Replace("@direcnegocio", odatos.Direccion);

                Texto_Html = Texto_Html.Replace("@tipodocumento", oVenta.TipoDocumento.ToUpper());
                Texto_Html = Texto_Html.Replace("@numerodocumento", oVenta.NumeroDocumento);

                Texto_Html = Texto_Html.Replace("@doccliente", oVenta.DocumentoCliente);
                Texto_Html = Texto_Html.Replace("@nombrecliente", oVenta.NombreCliente);
                Texto_Html = Texto_Html.Replace("@fecharegistro", oVenta.FechaRegistro);
                Texto_Html = Texto_Html.Replace("@usuarioregistro", _Usuario.NombreCompleto);
                Texto_Html = Texto_Html.Replace("@descuentocumple", mensajeDescuento);

                string filas = string.Empty;
                foreach (Detalle_Venta dv in oVenta.oDetalle_Venta)
                {
                    filas += "<tr>";
                    filas += "<td>" + dv.oProducto.Nombre + "</td>";
                    filas += "<td>" + dv.PrecioVenta.ToString("0.00") + "</td>";
                    filas += "<td>" + dv.Cantidad.ToString() + "</td>";
                    filas += "<td>" + dv.SubTotal.ToString("0.00") + "</td>";
                    filas += "</tr>";
                }
                Texto_Html = Texto_Html.Replace("@filas", filas);
                Texto_Html = Texto_Html.Replace("@montototal", oVenta.MontoTotal.ToString("0.00"));
                Texto_Html = Texto_Html.Replace("@pagocon", oVenta.MontoPago.ToString("0.00"));
                Texto_Html = Texto_Html.Replace("@cambio", oVenta.MontoCambio.ToString("0.00"));

                SaveFileDialog savefile = new SaveFileDialog();
                savefile.FileName = string.Format("Venta_{0}.pdf", oVenta.NumeroDocumento);
                savefile.Filter = "Archivos PDF|*.pdf";
                savefile.Title = "Guardar Factura PDF";

                if (savefile.ShowDialog() == true)
                {
                    using (FileStream stream = new FileStream(savefile.FileName, FileMode.Create))
                    {
                        Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 25);
                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();

                        bool obtenido = true;
                        byte[] byteImage = new NegocioService().ObtenerLogo(out obtenido);

                        if (obtenido)
                        {
                            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(byteImage);
                            img.ScaleToFit(60, 60);
                            img.Alignment = iTextSharp.text.Image.UNDERLYING;
                            img.SetAbsolutePosition(pdfDoc.Left, pdfDoc.GetTop(51));
                            pdfDoc.Add(img);
                        }

                        using (StringReader sr = new StringReader(Texto_Html))
                        {
                            XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                        }

                        pdfDoc.Close();
                        stream.Close();
                        MessageBox.Show("Factura PDF guardada exitosamente", "Éxito",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
