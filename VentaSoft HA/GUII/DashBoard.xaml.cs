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
        private ReporteService reporteService; // ✅ NUEVO: Usar el mismo servicio que el reporte
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
            reporteService = new ReporteService(); // ✅ NUEVO: Inicializar ReporteService

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

        // ✅ MODIFICADO: Usar ReporteService para obtener datos consistentes
        private void CargarKPIsReales()
        {
            try
            {
                var fechaActual = DateTime.Now;

                // ✅ NUEVO: Usar el mismo rango de fechas que el reporte por defecto
                var fechaInicio = fechaActual.AddDays(-30).ToString("yyyy-MM-dd");
                var fechaFin = fechaActual.ToString("yyyy-MM-dd");

                // ✅ NUEVO: Usar ReporteService para obtener datos consistentes
                var reporteVentas = reporteService.Venta(fechaInicio, fechaFin);

                if (reporteVentas != null && reporteVentas.Any())
                {
                    // Calcular totales usando los mismos datos que el reporte
                    VentasTotales = reporteVentas.Sum(r => ConvertirADecimal(r.SubTotal));
                    ProductosVendidos = reporteVentas.Sum(r => ConvertirAEntero(r.Cantidad));

                    // Contar clientes únicos
                    ClientesActivos = reporteVentas
                        .Where(r => !string.IsNullOrEmpty(r.DocumentoCliente))
                        .Select(r => r.DocumentoCliente)
                        .Distinct()
                        .Count();

                    // Calcular venta promedio por transacción
                    var ventasUnicas = reporteVentas
                        .GroupBy(r => r.NumeroDocumento)
                        .Select(g => g.Sum(r => ConvertirADecimal(r.SubTotal)))
                        .ToList();

                    VentaPromedio = ventasUnicas.Any() ? ventasUnicas.Average() : 0;
                }
                else
                {
                    VentasTotales = 0;
                    ProductosVendidos = 0;
                    ClientesActivos = 0;
                    VentaPromedio = 0;
                }

                // ✅ NUEVO: Debug para verificar consistencia
                System.Diagnostics.Debug.WriteLine("=== DASHBOARD KPIs ===");
                System.Diagnostics.Debug.WriteLine($"Período: {fechaInicio} a {fechaFin}");
                System.Diagnostics.Debug.WriteLine($"Registros encontrados: {reporteVentas?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Ventas Totales: {VentasTotales:C}");
                System.Diagnostics.Debug.WriteLine($"Productos Vendidos: {ProductosVendidos}");
                System.Diagnostics.Debug.WriteLine($"Clientes Activos: {ClientesActivos}");
                System.Diagnostics.Debug.WriteLine($"Venta Promedio: {VentaPromedio:C}");
                System.Diagnostics.Debug.WriteLine("=====================");
            }
            catch (Exception ex)
            {
                VentasTotales = 0;
                ProductosVendidos = 0;
                ClientesActivos = 0;
                VentaPromedio = 0;

                System.Diagnostics.Debug.WriteLine($"Error en CargarKPIsReales: {ex.Message}");
                MessageBox.Show($"Error al cargar KPIs: {ex.Message}", "Advertencia",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ✅ NUEVO: Métodos auxiliares para conversión (igual que en el reporte)
        private decimal ConvertirADecimal(object valor)
        {
            if (valor == null) return 0;

            if (decimal.TryParse(valor.ToString(), out decimal resultado))
                return resultado;

            return 0;
        }

        private int ConvertirAEntero(object valor)
        {
            if (valor == null) return 0;

            if (int.TryParse(valor.ToString(), out int resultado))
                return resultado;

            return 0;
        }

        // ✅ MODIFICADO: Usar ReporteService para gráficos también
        private void CargarGraficoVentasReales()
        {
            try
            {
                var fechaActual = DateTime.Now;
                var ventasPorMes = new Dictionary<string, decimal>();

                // Obtener datos de los últimos 12 meses
                for (int i = 11; i >= 0; i--)
                {
                    var fecha = fechaActual.AddMonths(-i);
                    var fechaInicio = new DateTime(fecha.Year, fecha.Month, 1).ToString("yyyy-MM-dd");
                    var fechaFin = new DateTime(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month)).ToString("yyyy-MM-dd");

                    var reporteMes = reporteService.Venta(fechaInicio, fechaFin);
                    var totalMes = reporteMes?.Sum(r => ConvertirADecimal(r.SubTotal)) ?? 0;

                    ventasPorMes.Add(fecha.ToString("MMM"), totalMes);
                }

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
                System.Diagnostics.Debug.WriteLine($"Error en CargarGraficoVentasReales: {ex.Message}");
                MessageBox.Show($"Error al cargar gráfico de ventas: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ MODIFICADO: Usar ReporteService para productos más vendidos
        private void CargarGraficoProductosReales()
        {
            try
            {
                var fechaActual = DateTime.Now;
                var fechaInicio = fechaActual.AddDays(-30).ToString("yyyy-MM-dd");
                var fechaFin = fechaActual.ToString("yyyy-MM-dd");

                var reporteVentas = reporteService.Venta(fechaInicio, fechaFin);

                if (reporteVentas != null && reporteVentas.Any())
                {
                    var productosMasVendidos = reporteVentas
                        .GroupBy(r => r.NombreProducto)
                        .Select(g => new
                        {
                            Nombre = g.Key,
                            TotalVendido = g.Sum(r => ConvertirAEntero(r.Cantidad))
                        })
                        .OrderByDescending(p => p.TotalVendido)
                        .Take(6)
                        .ToList();

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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en CargarGraficoProductosReales: {ex.Message}");
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
                var fechaActual = DateTime.Now;
                var fechaInicio = fechaActual.AddDays(-30).ToString("yyyy-MM-dd");
                var fechaFin = fechaActual.ToString("yyyy-MM-dd");

                var reporteVentas = reporteService.Venta(fechaInicio, fechaFin);
                var ventasPorProducto = reporteVentas?
                    .GroupBy(r => r.NombreProducto)
                    .ToDictionary(g => g.Key, g => g.Sum(r => ConvertirAEntero(r.Cantidad))) ??
                    new Dictionary<string, int>();

                var datosStock = productos.Select(p =>
                {
                    int vendidos = ventasPorProducto.ContainsKey(p.Nombre) ? ventasPorProducto[p.Nombre] : 0;

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
                System.Diagnostics.Debug.WriteLine($"Error en CargarDatosStockReales: {ex.Message}");
                MessageBox.Show($"Error al cargar datos de stock: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosCategorias()
        {
            try
            {
                var fechaActual = DateTime.Now;
                var fechaInicio = fechaActual.AddDays(-30).ToString("yyyy-MM-dd");
                var fechaFin = fechaActual.ToString("yyyy-MM-dd");

                var reporteVentas = reporteService.Venta(fechaInicio, fechaFin);

                if (reporteVentas != null && reporteVentas.Any())
                {
                    var ventasPorCategoria = reporteVentas
                        .GroupBy(r => r.Categoria)
                        .Select(g => new
                        {
                            Categoria = g.Key,
                            TotalVendido = g.Sum(r => ConvertirADecimal(r.SubTotal))
                        })
                        .Where(x => x.TotalVendido > 0)
                        .OrderByDescending(x => x.TotalVendido)
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
                        var porcentaje = totalGeneral > 0 ? (venta.TotalVendido / totalGeneral) * 100 : 0;

                        categorias.Add(new CategoriaVenta
                        {
                            Nombre = venta.Categoria,
                            Porcentaje = porcentaje,
                            Monto = venta.TotalVendido,
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en CargarDatosCategorias: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Error en CargarVentasRecientesReales: {ex.Message}");
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

        private void chartCategorias_Loaded(object sender, RoutedEventArgs e)
        {

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