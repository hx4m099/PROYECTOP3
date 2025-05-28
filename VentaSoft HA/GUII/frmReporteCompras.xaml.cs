using Logica;
using GUI.Utilidades;
using Entidades;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;

namespace GUI
{
    public partial class frmReporteCompras : Window
    {
        private ObservableCollection<ReporteCompraGrid> reporteCompras;
        private List<ReporteCompraGrid> reporteCompleto;

        public frmReporteCompras()
        {
            InitializeComponent();
            reporteCompras = new ObservableCollection<ReporteCompraGrid>();
            reporteCompleto = new List<ReporteCompraGrid>();
            dgvdata.ItemsSource = reporteCompras;
            Loaded += frmReporteCompras_Load;
        }

        private void frmReporteCompras_Load(object sender, RoutedEventArgs e)
        {
            // Configurar fechas por defecto
            txtfechainicio.SelectedDate = DateTime.Now.AddDays(-30);
            txtfechafin.SelectedDate = DateTime.Now;

            // Cargar proveedores
            List<Proveedor> lista = new ProveedorService().Listar();
            cboproveedor.Items.Add(new OpcionCombo() { Valor = 0, Texto = "TODOS" });
            foreach (Proveedor item in lista)
            {
                cboproveedor.Items.Add(new OpcionCombo() { Valor = item.IdProveedor, Texto = item.RazonSocial });
            }
            cboproveedor.DisplayMemberPath = "Texto";
            cboproveedor.SelectedValuePath = "Valor";
            cboproveedor.SelectedIndex = 0;

            // Cargar opciones de búsqueda
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "FechaRegistro", Texto = "Fecha Registro" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "TipoDocumento", Texto = "Tipo Documento" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "NumeroDocumento", Texto = "Número Documento" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "UsuarioRegistro", Texto = "Usuario Registro" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "DocumentoProveedor", Texto = "Documento Proveedor" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "RazonSocial", Texto = "Razón Social" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "CodigoProducto", Texto = "Código Producto" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "NombreProducto", Texto = "Nombre Producto" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Categoria", Texto = "Categoría" });

            cbobusqueda.DisplayMemberPath = "Texto";
            cbobusqueda.SelectedValuePath = "Valor";
            cbobusqueda.SelectedIndex = 0;

            ActualizarContadores();
        }

        private void btnbuscarresultado_Click(object sender, RoutedEventArgs e)
        {
            if (!txtfechainicio.SelectedDate.HasValue || !txtfechafin.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar ambas fechas", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (txtfechainicio.SelectedDate > txtfechafin.SelectedDate)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha fin", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                int idproveedor = Convert.ToInt32(((OpcionCombo)cboproveedor.SelectedItem).Valor.ToString());

                List<ReporteCompra> lista = new ReporteService().Compra(
                    txtfechainicio.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    txtfechafin.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    idproveedor);

                reporteCompras.Clear();
                reporteCompleto.Clear();

                foreach (ReporteCompra rc in lista)
                {
                    var reporteItem = new ReporteCompraGrid
                    {
                        FechaRegistro = rc.FechaRegistro,
                        TipoDocumento = rc.TipoDocumento,
                        NumeroDocumento = rc.NumeroDocumento,
                        MontoTotal = ConvertirADecimal(rc.MontoTotal),
                        UsuarioRegistro = rc.UsuarioRegistro,
                        DocumentoProveedor = rc.DocumentoProveedor,
                        RazonSocial = rc.RazonSocial,
                        CodigoProducto = rc.CodigoProducto,
                        NombreProducto = rc.NombreProducto,
                        Categoria = rc.Categoria,
                        PrecioCompra = ConvertirADecimal(rc.PrecioCompra),
                        PrecioVenta = ConvertirADecimal(rc.PrecioVenta),
                        Cantidad = ConvertirAEntero(rc.Cantidad),
                        SubTotal = ConvertirADecimal(rc.SubTotal)
                    };

                    reporteCompras.Add(reporteItem);
                    reporteCompleto.Add(reporteItem);
                }

                ActualizarContadores();
                MessageBox.Show($"Se encontraron {lista.Count} registros", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Métodos auxiliares para conversión segura
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

        private void btnbuscar_Click(object sender, RoutedEventArgs e)
        {
            if (reporteCompleto.Count == 0)
            {
                MessageBox.Show("Primero debe generar un reporte", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();
            string textoBusqueda = txtbusqueda.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(textoBusqueda))
            {
                reporteCompras.Clear();
                foreach (var item in reporteCompleto)
                {
                    reporteCompras.Add(item);
                }
                ActualizarContadores();
                return;
            }

            var reporteFiltrado = reporteCompleto.Where(r =>
            {
                switch (columnaFiltro)
                {
                    case "FechaRegistro":
                        return r.FechaRegistro.ToUpper().Contains(textoBusqueda);
                    case "TipoDocumento":
                        return r.TipoDocumento.ToUpper().Contains(textoBusqueda);
                    case "NumeroDocumento":
                        return r.NumeroDocumento.ToUpper().Contains(textoBusqueda);
                    case "UsuarioRegistro":
                        return r.UsuarioRegistro.ToUpper().Contains(textoBusqueda);
                    case "DocumentoProveedor":
                        return r.DocumentoProveedor.ToUpper().Contains(textoBusqueda);
                    case "RazonSocial":
                        return r.RazonSocial.ToUpper().Contains(textoBusqueda);
                    case "CodigoProducto":
                        return r.CodigoProducto.ToUpper().Contains(textoBusqueda);
                    case "NombreProducto":
                        return r.NombreProducto.ToUpper().Contains(textoBusqueda);
                    case "Categoria":
                        return r.Categoria.ToUpper().Contains(textoBusqueda);
                    default:
                        return false;
                }
            }).ToList();

            reporteCompras.Clear();
            foreach (var item in reporteFiltrado)
            {
                reporteCompras.Add(item);
            }

            ActualizarContadores();
        }

        private void btnlimpiarbuscador_Click(object sender, RoutedEventArgs e)
        {
            txtbusqueda.Text = "";
            reporteCompras.Clear();
            foreach (var item in reporteCompleto)
            {
                reporteCompras.Add(item);
            }
            ActualizarContadores();
        }

        // MÉTODO MEJORADO PARA EXPORTAR A EXCEL
        private void btnexportar_Click(object sender, RoutedEventArgs e)
        {
            if (reporteCompras.Count < 1)
            {
                MessageBox.Show("No hay registros para exportar", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = string.Format("ReporteCompras_{0}.xlsx", DateTime.Now.ToString("ddMMyyyyHHmmss"));
            saveFileDialog.Filter = "Excel Files (.xlsx)|.xlsx|All Files (.)|.";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Reporte de Compras");

                        // Configurar título principal
                        worksheet.Cell(1, 1).Value = "REPORTE DE COMPRAS";
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                        worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.DarkBlue;
                        worksheet.Range(1, 1, 1, 14).Merge();
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Información del reporte
                        worksheet.Cell(2, 1).Value = $"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                        worksheet.Cell(2, 1).Style.Font.FontSize = 10;
                        worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

                        if (txtfechainicio.SelectedDate.HasValue && txtfechafin.SelectedDate.HasValue)
                        {
                            worksheet.Cell(3, 1).Value = $"Período: {txtfechainicio.SelectedDate.Value:dd/MM/yyyy} - {txtfechafin.SelectedDate.Value:dd/MM/yyyy}";
                            worksheet.Cell(3, 1).Style.Font.FontSize = 10;
                            worksheet.Cell(3, 1).Style.Font.FontColor = XLColor.Gray;
                        }

                        // Proveedor seleccionado
                        var proveedorSeleccionado = ((OpcionCombo)cboproveedor.SelectedItem).Texto;
                        worksheet.Cell(4, 1).Value = $"Proveedor: {proveedorSeleccionado}";
                        worksheet.Cell(4, 1).Style.Font.FontSize = 10;
                        worksheet.Cell(4, 1).Style.Font.FontColor = XLColor.Gray;

                        // Fila de inicio para los datos
                        int filaInicio = 6;

                        // Definir encabezados
                        string[] encabezados = {
                            "Fecha Registro", "Tipo Documento", "Número Documento", "Monto Total",
                            "Usuario Registro", "Documento Proveedor", "Razón Social", "Código Producto",
                            "Nombre Producto", "Categoría", "Precio Compra", "Precio Venta", "Cantidad", "Sub Total"
                        };

                        // Escribir encabezados
                        for (int i = 0; i < encabezados.Length; i++)
                        {
                            var cell = worksheet.Cell(filaInicio, i + 1);
                            cell.Value = encabezados[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.FontColor = XLColor.White;
                            cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        // Escribir datos
                        int filaActual = filaInicio + 1;
                        foreach (var item in reporteCompras)
                        {
                            worksheet.Cell(filaActual, 1).Value = item.FechaRegistro;
                            worksheet.Cell(filaActual, 2).Value = item.TipoDocumento;
                            worksheet.Cell(filaActual, 3).Value = item.NumeroDocumento;
                            worksheet.Cell(filaActual, 4).Value = item.MontoTotal;
                            worksheet.Cell(filaActual, 5).Value = item.UsuarioRegistro;
                            worksheet.Cell(filaActual, 6).Value = item.DocumentoProveedor;
                            worksheet.Cell(filaActual, 7).Value = item.RazonSocial;
                            worksheet.Cell(filaActual, 8).Value = item.CodigoProducto;
                            worksheet.Cell(filaActual, 9).Value = item.NombreProducto;
                            worksheet.Cell(filaActual, 10).Value = item.Categoria;
                            worksheet.Cell(filaActual, 11).Value = item.PrecioCompra;
                            worksheet.Cell(filaActual, 12).Value = item.PrecioVenta;
                            worksheet.Cell(filaActual, 13).Value = item.Cantidad;
                            worksheet.Cell(filaActual, 14).Value = item.SubTotal;

                            // Formatear celdas de moneda
                            worksheet.Cell(filaActual, 4).Style.NumberFormat.Format = "$#,##0.00";
                            worksheet.Cell(filaActual, 11).Style.NumberFormat.Format = "$#,##0.00";
                            worksheet.Cell(filaActual, 12).Style.NumberFormat.Format = "$#,##0.00";
                            worksheet.Cell(filaActual, 14).Style.NumberFormat.Format = "$#,##0.00";

                            // Formatear cantidad
                            worksheet.Cell(filaActual, 13).Style.NumberFormat.Format = "#,##0";

                            // Aplicar bordes a toda la fila
                            for (int col = 1; col <= 14; col++)
                            {
                                worksheet.Cell(filaActual, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(filaActual, col).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            }

                            // Alternar color de filas para mejor legibilidad
                            if (filaActual % 2 == 0)
                            {
                                for (int col = 1; col <= 14; col++)
                                {
                                    worksheet.Cell(filaActual, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                                }
                            }

                            filaActual++;
                        }

                        // Agregar fila de totales
                        int filaTotales = filaActual + 1;
                        worksheet.Cell(filaTotales, 1).Value = "TOTALES:";
                        worksheet.Cell(filaTotales, 1).Style.Font.Bold = true;
                        worksheet.Cell(filaTotales, 1).Style.Font.FontColor = XLColor.DarkBlue;

                        // Calcular totales
                        decimal totalCompras = reporteCompras.Sum(r => r.SubTotal);
                        decimal totalVentaPotencial = reporteCompras.Sum(r => r.PrecioVenta * r.Cantidad);
                        decimal margenPotencial = totalVentaPotencial - totalCompras;

                        worksheet.Cell(filaTotales, 4).Value = reporteCompras.Sum(r => r.MontoTotal);
                        worksheet.Cell(filaTotales, 4).Style.NumberFormat.Format = "$#,##0.00";
                        worksheet.Cell(filaTotales, 4).Style.Font.Bold = true;
                        worksheet.Cell(filaTotales, 4).Style.Fill.BackgroundColor = XLColor.LightYellow;

                        worksheet.Cell(filaTotales, 13).Value = reporteCompras.Sum(r => r.Cantidad);
                        worksheet.Cell(filaTotales, 13).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(filaTotales, 13).Style.Font.Bold = true;

                        worksheet.Cell(filaTotales, 14).Value = totalCompras;
                        worksheet.Cell(filaTotales, 14).Style.NumberFormat.Format = "$#,##0.00";
                        worksheet.Cell(filaTotales, 14).Style.Font.Bold = true;
                        worksheet.Cell(filaTotales, 14).Style.Fill.BackgroundColor = XLColor.LightYellow;

                        // Agregar información adicional
                        worksheet.Cell(filaTotales + 2, 1).Value = "Resumen Financiero:";
                        worksheet.Cell(filaTotales + 2, 1).Style.Font.Bold = true;
                        worksheet.Cell(filaTotales + 2, 1).Style.Font.FontColor = XLColor.DarkBlue;

                        worksheet.Cell(filaTotales + 3, 1).Value = "Total en Compras:";
                        worksheet.Cell(filaTotales + 3, 2).Value = totalCompras;
                        worksheet.Cell(filaTotales + 3, 2).Style.NumberFormat.Format = "$#,##0.00";

                        worksheet.Cell(filaTotales + 4, 1).Value = "Venta Potencial:";
                        worksheet.Cell(filaTotales + 4, 2).Value = totalVentaPotencial;
                        worksheet.Cell(filaTotales + 4, 2).Style.NumberFormat.Format = "$#,##0.00";

                        worksheet.Cell(filaTotales + 5, 1).Value = "Margen Potencial:";
                        worksheet.Cell(filaTotales + 5, 2).Value = margenPotencial;
                        worksheet.Cell(filaTotales + 5, 2).Style.NumberFormat.Format = "$#,##0.00";
                        worksheet.Cell(filaTotales + 5, 2).Style.Font.Bold = true;
                        worksheet.Cell(filaTotales + 5, 2).Style.Fill.BackgroundColor = XLColor.LightGreen;

                        // Ajustar ancho de columnas automáticamente
                        worksheet.ColumnsUsed().AdjustToContents();

                        // Asegurar que las columnas no sean demasiado anchas
                        foreach (var column in worksheet.ColumnsUsed())
                        {
                            if (column.Width > 50)
                                column.Width = 50;
                            if (column.Width < 10)
                                column.Width = 10;
                        }

                        // Congelar paneles (encabezados)
                        worksheet.SheetView.FreezeRows(filaInicio);

                        // Aplicar filtros automáticos
                        var rangoTabla = worksheet.Range(filaInicio, 1, filaActual - 1, 14);
                        rangoTabla.SetAutoFilter();

                        // Guardar el archivo
                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Reporte exportado correctamente a Excel", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar el reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ActualizarContadores()
        {
            lblTotalRegistros.Text = $"({reporteCompras.Count} registros)";

            decimal totalCompras = reporteCompras.Sum(r => r.SubTotal);
            lblTotalCompras.Text = totalCompras.ToString("C", CultureInfo.CurrentCulture);

            decimal totalVentaPotencial = reporteCompras.Sum(r => r.PrecioVenta * r.Cantidad);
            decimal margenPotencial = totalVentaPotencial - totalCompras;
            lblMargenPotencial.Text = margenPotencial.ToString("C", CultureInfo.CurrentCulture);
        }
    }

    public class ReporteCompraGrid
    {
        public string FechaRegistro { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public decimal MontoTotal { get; set; }
        public string UsuarioRegistro { get; set; }
        public string DocumentoProveedor { get; set; }
        public string RazonSocial { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal SubTotal { get; set; }
    }
}