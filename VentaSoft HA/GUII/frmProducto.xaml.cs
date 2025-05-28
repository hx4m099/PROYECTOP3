using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Logica;
using Microsoft.Win32;
using System.IO;
using ClosedXML.Excel;
using System.Data;
using DocumentFormat.OpenXml.Wordprocessing;
using Entidades;
using GUI.Utilidades;

namespace GUI
{
    public partial class frmProducto : Window
    {
        private ObservableCollection<ProductoGrid> productosGrid;

        public frmProducto()
        {
            InitializeComponent();
            productosGrid = new ObservableCollection<ProductoGrid>();
            dgvdata.ItemsSource = productosGrid;
            Loaded += frmProducto_Load;
        }

        private void frmProducto_Load(object sender, RoutedEventArgs e)
        {
            // Cargar estados
            cboestado.Items.Add(new OpcionCombo() { Valor = 1, Texto = "Activo" });
            cboestado.Items.Add(new OpcionCombo() { Valor = 0, Texto = "No Activo" });
            cboestado.DisplayMemberPath = "Texto";
            cboestado.SelectedValuePath = "Valor";
            cboestado.SelectedIndex = 0;

            // Cargar categorías
            List<Categoria> listacategoria = new CategoriaService().Listar();
            foreach (Categoria item in listacategoria)
            {
                cbocategoria.Items.Add(new OpcionCombo() { Valor = item.IdCategoria, Texto = item.Descripcion });
            }
            cbocategoria.DisplayMemberPath = "Texto";
            cbocategoria.SelectedValuePath = "Valor";
            cbocategoria.SelectedIndex = 0;

            // Cargar opciones de búsqueda
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Codigo", Texto = "Código" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Nombre", Texto = "Nombre" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Descripcion", Texto = "Descripción" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Categoria", Texto = "Categoría" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Estado", Texto = "Estado" });
            cbobusqueda.DisplayMemberPath = "Texto";
            cbobusqueda.SelectedValuePath = "Valor";
            cbobusqueda.SelectedIndex = 0;

            // Cargar productos
            CargarProductos();
        }

        private void CargarProductos()
        {
            productosGrid.Clear();
            List<Producto> lista = new ProductoService().Listar();

            foreach (Producto item in lista)
            {
                productosGrid.Add(new ProductoGrid
                {
                    Id = item.IdProducto,
                    Codigo = item.Codigo,
                    Nombre = item.Nombre,
                    Descripcion = item.Descripcion,
                    IdCategoria = item.oCategoria.IdCategoria,
                    Categoria = item.oCategoria.Descripcion,
                    Stock = item.Stock,
                    PrecioCompra = item.PrecioCompra,
                    PrecioVenta = item.PrecioVenta,
                    EstadoValor = item.Estado ? 1 : 0,
                    Estado = item.Estado ? "Activo" : "No Activo"
                });
            }

            ActualizarContador();
        }

        private void btnguardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtcodigo.Text))
            {
                MessageBox.Show("Debe ingresar el código del producto", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtcodigo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtnombre.Text))
            {
                MessageBox.Show("Debe ingresar el nombre del producto", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtnombre.Focus();
                return;
            }

            Producto obj = new Producto()
            {
                IdProducto = Convert.ToInt32(txtid.Text),
                Codigo = txtcodigo.Text,
                Nombre = txtnombre.Text,
                Descripcion = txtdescripcion.Text,
                oCategoria = new Categoria() { IdCategoria = Convert.ToInt32(((OpcionCombo)cbocategoria.SelectedItem).Valor) },
                Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1 ? true : false
            };

            if (obj.IdProducto == 0)
            {
                int idgenerado = new ProductoService().Registrar(obj, out mensaje);

                if (idgenerado != 0)
                {
                    productosGrid.Add(new ProductoGrid
                    {
                        Id = idgenerado,
                        Codigo = txtcodigo.Text,
                        Nombre = txtnombre.Text,
                        Descripcion = txtdescripcion.Text,
                        IdCategoria = Convert.ToInt32(((OpcionCombo)cbocategoria.SelectedItem).Valor),
                        Categoria = ((OpcionCombo)cbocategoria.SelectedItem).Texto.ToString(),
                        Stock = 0,
                        PrecioCompra = 0.00m,
                        PrecioVenta = 0.00m,
                        EstadoValor = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor),
                        Estado = ((OpcionCombo)cboestado.SelectedItem).Texto.ToString()
                    });

                    Limpiar();
                    ActualizarContador();
                    MessageBox.Show("Producto registrado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                bool resultado = new ProductoService().Editar(obj, out mensaje);

                if (resultado)
                {
                    var producto = productosGrid.FirstOrDefault(p => p.Id == obj.IdProducto);
                    if (producto != null)
                    {
                        producto.Codigo = txtcodigo.Text;
                        producto.Nombre = txtnombre.Text;
                        producto.Descripcion = txtdescripcion.Text;
                        producto.IdCategoria = Convert.ToInt32(((OpcionCombo)cbocategoria.SelectedItem).Valor);
                        producto.Categoria = ((OpcionCombo)cbocategoria.SelectedItem).Texto.ToString();
                        producto.EstadoValor = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor);
                        producto.Estado = ((OpcionCombo)cboestado.SelectedItem).Texto.ToString();
                    }

                    Limpiar();
                    MessageBox.Show("Producto actualizado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void Limpiar()
        {
            txtindice.Text = "-1";
            txtid.Text = "0";
            txtcodigo.Text = "";
            txtnombre.Text = "";
            txtdescripcion.Text = "";
            cbocategoria.SelectedIndex = 0;
            cboestado.SelectedIndex = 0;

            txtcodigo.Focus();
        }

        private void btnseleccionar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ProductoGrid producto = btn.DataContext as ProductoGrid;

            if (producto != null)
            {
                txtid.Text = producto.Id.ToString();
                txtcodigo.Text = producto.Codigo;
                txtnombre.Text = producto.Nombre;
                txtdescripcion.Text = producto.Descripcion;

                // Seleccionar categoría
                foreach (OpcionCombo oc in cbocategoria.Items)
                {
                    if (Convert.ToInt32(oc.Valor) == producto.IdCategoria)
                    {
                        cbocategoria.SelectedItem = oc;
                        break;
                    }
                }

                // Seleccionar estado
                foreach (OpcionCombo oc in cboestado.Items)
                {
                    if (Convert.ToInt32(oc.Valor) == producto.EstadoValor)
                    {
                        cboestado.SelectedItem = oc;
                        break;
                    }
                }
            }
        }

        private void btneliminar_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToInt32(txtid.Text) != 0)
            {
                if (MessageBox.Show("¿Desea eliminar el producto?", "Mensaje", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    string mensaje = string.Empty;
                    Producto obj = new Producto()
                    {
                        IdProducto = Convert.ToInt32(txtid.Text)
                    };

                    bool respuesta = new ProductoService().Eliminar(obj, out mensaje);

                    if (respuesta)
                    {
                        var producto = productosGrid.FirstOrDefault(p => p.Id == obj.IdProducto);
                        if (producto != null)
                        {
                            productosGrid.Remove(producto);
                        }
                        Limpiar();
                        ActualizarContador();
                        MessageBox.Show("Producto eliminado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un producto", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void btnbuscar_Click(object sender, RoutedEventArgs e)
        {
            string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();
            string textoBusqueda = txtbusqueda.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(textoBusqueda))
            {
                dgvdata.ItemsSource = productosGrid;
                ActualizarContador();
                return;
            }

            var productosFiltrados = productosGrid.Where(p =>
            {
                switch (columnaFiltro)
                {
                    case "Codigo":
                        return p.Codigo.ToUpper().Contains(textoBusqueda);
                    case "Nombre":
                        return p.Nombre.ToUpper().Contains(textoBusqueda);
                    case "Descripcion":
                        return p.Descripcion.ToUpper().Contains(textoBusqueda);
                    case "Categoria":
                        return p.Categoria.ToUpper().Contains(textoBusqueda);
                    case "Estado":
                        return p.Estado.ToUpper().Contains(textoBusqueda);
                    default:
                        return false;
                }
            }).ToList();

            dgvdata.ItemsSource = productosFiltrados;
            lblTotalProductos.Text = productosFiltrados.Count.ToString();
        }

        private void btnlimpiarbuscador_Click(object sender, RoutedEventArgs e)
        {
            txtbusqueda.Text = "";
            dgvdata.ItemsSource = productosGrid;
            ActualizarContador();
        }

        private void btnlimpiar_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }

        private void btnexportar_Click(object sender, RoutedEventArgs e)
        {
            if (productosGrid.Count < 1)
            {
                MessageBox.Show("No hay datos para exportar", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Crear DataTable con las columnas visibles
            DataTable dt = new DataTable();

            // Agregar columnas al DataTable
            dt.Columns.Add("Código", typeof(string));
            dt.Columns.Add("Nombre", typeof(string));
            dt.Columns.Add("Descripción", typeof(string));
            dt.Columns.Add("Categoría", typeof(string));
            dt.Columns.Add("Stock", typeof(int));
            dt.Columns.Add("Precio Compra", typeof(decimal));
            dt.Columns.Add("Precio Venta", typeof(decimal));
            dt.Columns.Add("Estado", typeof(string));

            // Agregar filas al DataTable
            foreach (var item in productosGrid)
            {
                dt.Rows.Add(new object[] {
                    item.Codigo,
                    item.Nombre,
                    item.Descripcion,
                    item.Categoria,
                    item.Stock,
                    item.PrecioCompra,
                    item.PrecioVenta,
                    item.Estado
                });
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = string.Format("ReporteProductos_{0}.xlsx", DateTime.Now.ToString("ddMMyyyyHHmmss"));
            saveFileDialog.Filter = "Excel Files (.xlsx)|.xlsx|All Files (.)|.";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var hoja = wb.Worksheets.Add(dt, "Reporte Productos");

                        // Ajustar el ancho de las columnas al contenido
                        hoja.ColumnsUsed().AdjustToContents();

                        // Aplicar formato a los encabezados
                        var headerRange = hoja.Range(1, 1, 1, dt.Columns.Count);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Aplicar bordes a toda la tabla
                        var dataRange = hoja.Range(1, 1, dt.Rows.Count + 1, dt.Columns.Count);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        // Formatear columnas de precios como moneda
                        var precioCompraColumn = hoja.Column(6); // Precio Compra
                        var precioVentaColumn = hoja.Column(7);  // Precio Venta
                        precioCompraColumn.Style.NumberFormat.Format = "$#,##0.00";
                        precioVentaColumn.Style.NumberFormat.Format = "$#,##0.00";

                        // Centrar la columna de Stock
                        var stockColumn = hoja.Column(5);
                        stockColumn.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        wb.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Reporte exportado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar el reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ActualizarContador()
        {
            lblTotalProductos.Text = productosGrid.Count.ToString();
        }
    }

    public class ProductoGrid
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int IdCategoria { get; set; }
        public string Categoria { get; set; }
        public int Stock { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int EstadoValor { get; set; }
        public string Estado { get; set; }
    }
}