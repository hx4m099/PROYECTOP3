using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using Logica;
using Entidades;
using System.Windows.Media;

namespace GUI
{
    public partial class Dashboard : UserControl, INotifyPropertyChanged
    {
        private VentaService ventaService;
        private DispatcherTimer timer;

        // Propiedades para binding
        private decimal _ventasTotales;
        private int _productosVendidos;
        private int _clientesActivos;
        private decimal _ventaPromedio;
        private string _ultimaActualizacion;

        public decimal VentasTotales
        {
            get => _ventasTotales;
            set { _ventasTotales = value; OnPropertyChanged(nameof(VentasTotales)); }
        }

        public int ProductosVendidos
        {
            get => _productosVendidos;
            set { _productosVendidos = value; OnPropertyChanged(nameof(ProductosVendidos)); }
        }

        public int ClientesActivos
        {
            get => _clientesActivos;
            set { _clientesActivos = value; OnPropertyChanged(nameof(ClientesActivos)); }
        }

        public decimal VentaPromedio
        {
            get => _ventaPromedio;
            set { _ventaPromedio = value; OnPropertyChanged(nameof(VentaPromedio)); }
        }

        public string UltimaActualizacion
        {
            get => _ultimaActualizacion;
            set { _ultimaActualizacion = value; OnPropertyChanged(nameof(UltimaActualizacion)); }
        }

        public Func<double, string> CurrencyFormatter { get; set; }

        public Dashboard()
        {
            InitializeComponent();
            DataContext = this;
            ventaService = new VentaService();

            CurrencyFormatter = value => value.ToString("C");

            InitializeTimer();
            CargarDatosReales();
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(2); // Actualizar cada 2 minutos
            timer.Tick += Timer_Tick;
            timer.Start();

            UltimaActualizacion = DateTime.Now.ToString("HH:mm:ss");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CargarDatosReales();
            UltimaActualizacion = DateTime.Now.ToString("HH:mm:ss");
        }

        private void CargarDatosReales()
        {
            try
            {
                CargarKPIsReales();
                CargarGraficoVentasReales();
                CargarGraficoProductosReales();
                CargarDatosStockReales();
                CargarDatosCategorias();
                CargarVentasRecientesReales();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarKPIsReales()
        {
            try
            {
                var fechaActual = DateTime.Now;

                // USAR DATOS REALES DE TU VENTASERVICE
                VentasTotales = ventaService.ObtenerTotalVentasDelMes(fechaActual.Month, fechaActual.Year);
                ProductosVendidos = ventaService.ObtenerTotalProductosVendidosDelMes(fechaActual.Month, fechaActual.Year);
                ClientesActivos = ventaService.ObtenerClientesActivosDelMes(fechaActual.Month, fechaActual.Year);

                // Calcular venta promedio
                int totalVentas = ventaService.ObtenerCorrelativo() - 1;
                VentaPromedio = totalVentas > 0 ? VentasTotales / totalVentas : 0;
            }
            catch (Exception ex)
            {
                VentasTotales = 0;
                ProductosVendidos = 0;
                ClientesActivos = 0;
                VentaPromedio = 0;

                MessageBox.Show($"Error al cargar KPIs: {ex.Message}", "Advertencia",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CargarGraficoVentasReales()
        {
            try
            {
                // USAR DATOS REALES
                var ventasPorMes = ventaService.ObtenerVentasPorMes(DateTime.Now.Year);

                chartVentas.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Ventas",
                        Values = new ChartValues<decimal>(ventasPorMes.Values),
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 8,
                        Fill = new SolidColorBrush(Color.FromArgb(50, 74, 144, 226)),
                        Stroke = new SolidColorBrush(Color.FromRgb(74, 144, 226)),
                        StrokeThickness = 3
                    }
                };

                chartVentas.AxisX.Clear();
                chartVentas.AxisX.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Mes",
                    Labels = ventasPorMes.Keys.ToArray(),
                    Separator = new LiveCharts.Wpf.Separator { Step = 1 }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar gráfico de ventas: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarGraficoProductosReales()
        {
            try
            {
                // USAR DATOS REALES
                var productosMasVendidos = ventaService.ObtenerProductosMasVendidos(6);

                chartProductos.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Productos Vendidos",
                        Values = new ChartValues<int>(productosMasVendidos.Select(p => p.TotalVendido)),
                        Fill = new SolidColorBrush(Color.FromRgb(80, 200, 120))
                    }
                };

                chartProductos.AxisX.Clear();
                chartProductos.AxisX.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Productos",
                    Labels = productosMasVendidos.Select(p => p.Nombre).ToArray(),
                    Separator = new LiveCharts.Wpf.Separator { Step = 1 }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar gráfico de productos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosStockReales()
        {
            try
            {
                // USAR DATOS REALES
                var productos = ventaService.ObtenerProductosConStock();
                var productosMasVendidos = ventaService.ObtenerProductosMasVendidos(50); // Obtener más para hacer match

                var datosStock = productos.Select(p =>
                {
                    var productoVendido = productosMasVendidos.FirstOrDefault(pv => pv.Nombre == p.Nombre);
                    int vendidos = productoVendido?.TotalVendido ?? 0;

                    return new ProductoStock
                    {
                        Nombre = p.Nombre,
                        Categoria = p.oCategoria.Descripcion,
                        Stock = p.Stock,
                        Vendidos = vendidos,
                        EstadoStock = p.Stock < 10 ? "Bajo" : p.Stock < 50 ? "Normal" : "Alto"
                    };
                }).ToList();

                dgStock.ItemsSource = datosStock;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos de stock: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosCategorias()
        {
            try
            {
                // CALCULAR DATOS REALES POR CATEGORÍA
                var productos = ventaService.ObtenerProductosConStock();
                var productosMasVendidos = ventaService.ObtenerProductosMasVendidos(100);

                var ventasPorCategoria = productos
                    .GroupBy(p => p.oCategoria.Descripcion)
                    .Select(g =>
                    {
                        var ventasCategoria = productosMasVendidos
                            .Where(pv => productos.Any(p => p.Nombre == pv.Nombre && p.oCategoria.Descripcion == g.Key))
                            .Sum(pv => pv.TotalVendido);

                        return new { Categoria = g.Key, TotalVendido = ventasCategoria };
                    })
                    .Where(x => x.TotalVendido > 0)
                    .ToList();

                var totalGeneral = ventasPorCategoria.Sum(x => x.TotalVendido);

                var categorias = new List<CategoriaVenta>();
                var colores = new[] {
                    Color.FromRgb(59, 130, 246),
                    Color.FromRgb(16, 185, 129),
                    Color.FromRgb(245, 158, 11),
                    Color.FromRgb(239, 68, 68),
                    Color.FromRgb(139, 92, 246)
                };

                for (int i = 0; i < ventasPorCategoria.Count && i < colores.Length; i++)
                {
                    var venta = ventasPorCategoria[i];
                    var porcentaje = totalGeneral > 0 ? (decimal)venta.TotalVendido / totalGeneral * 100 : 0;

                    categorias.Add(new CategoriaVenta
                    {
                        Nombre = venta.Categoria,
                        Porcentaje = porcentaje,
                        Monto = VentasTotales * (porcentaje / 100),
                        Color = new SolidColorBrush(colores[i])
                    });
                }

                // Configurar gráfico circular
                chartCategorias.Series = new SeriesCollection();
                foreach (var categoria in categorias)
                {
                    chartCategorias.Series.Add(new PieSeries
                    {
                        Title = categoria.Nombre,
                        Values = new ChartValues<decimal> { categoria.Porcentaje },
                        Fill = categoria.Color,
                        DataLabels = true,
                        LabelPoint = chartPoint => $"{categoria.Nombre}: {chartPoint.Y:F1}%"
                    });
                }

                lstCategorias.ItemsSource = categorias;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos de categorías: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarVentasRecientesReales()
        {
            try
            {
                // USAR DATOS REALES
                var ventas = ventaService.ObtenerVentasRecientes(10);
                var ventasRecientes = ventas.Select(v => new VentaReciente
                {
                    NumeroVenta = v.NumeroDocumento,
                    Cliente = string.IsNullOrEmpty(v.NombreCliente) ? "Cliente General" : v.NombreCliente,
                    Total = v.MontoTotal,
                    CantidadProductos = ObtenerCantidadProductosVenta(v.IdVenta),
                    Fecha = DateTime.TryParse(v.FechaRegistro, out DateTime fecha) ? fecha : DateTime.Now
                }).ToList();

                lstVentasRecientes.ItemsSource = ventasRecientes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ventas recientes: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int ObtenerCantidadProductosVenta(int idVenta)
        {
            try
            {
                var detalles = ventaService.ObtenerDetalleVenta(idVenta);
                return detalles?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private void btnActualizarVentas_Click(object sender, RoutedEventArgs e)
        {
            CargarDatosReales();
            MessageBox.Show("Datos actualizados correctamente", "Información",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Clases auxiliares para el dashboard
    public class ProductoStock
    {
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public int Stock { get; set; }
        public int Vendidos { get; set; }
        public string EstadoStock { get; set; }
    }

    public class CategoriaVenta
    {
        public string Nombre { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Monto { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    public class VentaReciente
    {
        public string NumeroVenta { get; set; }
        public string Cliente { get; set; }
        public decimal Total { get; set; }
        public int CantidadProductos { get; set; }
        public DateTime Fecha { get; set; }
    }
}