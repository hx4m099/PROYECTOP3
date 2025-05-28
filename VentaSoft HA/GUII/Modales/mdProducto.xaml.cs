using Entidades;
using Logica;
using GUI.Utilidades;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GUI.Modales
{
    public partial class mdProducto : Window
    {
        public class ProductoModal
        {
            public string Id { get; set; }
            public string Codigo { get; set; }
            public string Nombre { get; set; }
            public string Categoria { get; set; }
            public string Stock { get; set; }
            public string PrecioCompra { get; set; }
            public string PrecioVenta { get; set; }
            public bool StockBajo { get; set; }
        }

        public Producto _Producto { get; set; }
        private ObservableCollection<ProductoModal> productosModal;
        private ObservableCollection<ProductoModal> productosOriginal;

        public mdProducto()
        {
            InitializeComponent();
            productosModal = new ObservableCollection<ProductoModal>();
            productosOriginal = new ObservableCollection<ProductoModal>();
            dgvdata.ItemsSource = productosModal;
        }

        private void mdProducto_Load(object sender, RoutedEventArgs e)
        {
            try
            {
                // Cargar opciones de búsqueda (solo columnas visibles)
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Codigo", Texto = "Código" });
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Nombre", Texto = "Nombre" });
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Categoria", Texto = "Categoría" });
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Stock", Texto = "Stock" });
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = "PrecioCompra", Texto = "P. Compra" });
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = "PrecioVenta", Texto = "P. Venta" });

                cbobusqueda.DisplayMemberPath = "Texto";
                cbobusqueda.SelectedValuePath = "Valor";
                cbobusqueda.SelectedIndex = 0;

                // Cargar productos
                var lista = new ProductoService().Listar();

                foreach (Producto item in lista)
                {
                    var productoModal = new ProductoModal
                    {
                        Id = item.IdProducto.ToString(),
                        Codigo = item.Codigo,
                        Nombre = item.Nombre,
                        Categoria = item.oCategoria.Descripcion,
                        Stock = item.Stock.ToString(),
                        PrecioCompra = item.PrecioCompra.ToString("0.00"),
                        PrecioVenta = item.PrecioVenta.ToString("0.00"),
                        StockBajo = item.Stock > 0 && item.Stock <= 5 // Considerar stock bajo si es <= 5
                    };

                    productosModal.Add(productoModal);
                    productosOriginal.Add(productoModal);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvdata_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvdata.SelectedItem != null)
            {
                var productoSeleccionado = dgvdata.SelectedItem as ProductoModal;
                if (productoSeleccionado != null)
                {
                    _Producto = new Producto()
                    {
                        IdProducto = Convert.ToInt32(productoSeleccionado.Id),
                        Codigo = productoSeleccionado.Codigo,
                        Nombre = productoSeleccionado.Nombre,
                        Stock = Convert.ToInt32(productoSeleccionado.Stock),
                        PrecioCompra = Convert.ToDecimal(productoSeleccionado.PrecioCompra),
                        PrecioVenta = Convert.ToDecimal(productoSeleccionado.PrecioVenta)
                    };

                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private void btnseleccionar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvdata.SelectedItem != null)
            {
                var productoSeleccionado = dgvdata.SelectedItem as ProductoModal;
                if (productoSeleccionado != null)
                {
                    _Producto = new Producto()
                    {
                        IdProducto = Convert.ToInt32(productoSeleccionado.Id),
                        Codigo = productoSeleccionado.Codigo,
                        Nombre = productoSeleccionado.Nombre,
                        Stock = Convert.ToInt32(productoSeleccionado.Stock),
                        PrecioCompra = Convert.ToDecimal(productoSeleccionado.PrecioCompra),
                        PrecioVenta = Convert.ToDecimal(productoSeleccionado.PrecioVenta)
                    };

                    this.DialogResult = true;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un producto", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btncancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnbuscar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtbusqueda.Text))
            {
                // Si no hay texto de búsqueda, mostrar todos los productos
                productosModal.Clear();
                foreach (var producto in productosOriginal)
                {
                    productosModal.Add(producto);
                }
                return;
            }

            try
            {
                string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();
                string textoBusqueda = txtbusqueda.Text.Trim().ToUpper();

                var productosFiltrados = productosOriginal.Where(p =>
                {
                    string valorCampo = "";
                    switch (columnaFiltro)
                    {
                        case "Codigo":
                            valorCampo = p.Codigo;
                            break;
                        case "Nombre":
                            valorCampo = p.Nombre;
                            break;
                        case "Categoria":
                            valorCampo = p.Categoria;
                            break;
                        case "Stock":
                            valorCampo = p.Stock;
                            break;
                        case "PrecioCompra":
                            valorCampo = p.PrecioCompra;
                            break;
                        case "PrecioVenta":
                            valorCampo = p.PrecioVenta;
                            break;
                    }
                    return valorCampo.ToUpper().Contains(textoBusqueda);
                }).ToList();

                productosModal.Clear();
                foreach (var producto in productosFiltrados)
                {
                    productosModal.Add(producto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnlimpiarbuscador_Click(object sender, RoutedEventArgs e)
        {
            txtbusqueda.Text = "";

            // Restaurar todos los productos
            productosModal.Clear();
            foreach (var producto in productosOriginal)
            {
                productosModal.Add(producto);
            }
        }
    }
}