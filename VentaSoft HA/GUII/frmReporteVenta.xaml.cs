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
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace GUI
{
    public partial class frmReporteVentas : Window
    {
        private ObservableCollection<ReporteVentaGrid> reporteVentas;
        private List<ReporteVentaGrid> reporteCompleto;

        public frmReporteVentas()
        {
            InitializeComponent();
            reporteVentas = new ObservableCollection<ReporteVentaGrid>();
            reporteCompleto = new List<ReporteVentaGrid>();
            dgvdata.ItemsSource = reporteVentas;
            Loaded += frmReporteVentas_Load;
        }

        private void frmReporteVentas_Load(object sender, RoutedEventArgs e)
        {
            // Configurar fechas por defecto
            txtfechainicio.SelectedDate = DateTime.Now.AddDays(-30);
            txtfechafin.SelectedDate = DateTime.Now;

            // Cargar opciones de búsqueda
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "FechaRegistro", Texto = "Fecha Registro" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "TipoDocumento", Texto = "Tipo Documento" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "NumeroDocumento", Texto = "Número Documento" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "UsuarioRegistro", Texto = "Usuario Registro" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "DocumentoCliente", Texto = "Documento Cliente" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "NombreCliente", Texto = "Nombre Cliente" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "CodigoProducto", Texto = "Código Producto" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "NombreProducto", Texto = "Nombre Producto" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Categoria", Texto = "Categoría" });

            cbobusqueda.DisplayMemberPath = "Texto";
            cbobusqueda.SelectedValuePath = "Valor";
            cbobusqueda.SelectedIndex = 0;

            ActualizarContadores();
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

        private void btnbuscarreporte_Click(object sender, RoutedEventArgs e)
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
                List<ReporteVenta> lista = new ReporteService().Venta(
                    txtfechainicio.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    txtfechafin.SelectedDate.Value.ToString("yyyy-MM-dd"));

                reporteVentas.Clear();
                reporteCompleto.Clear();

                foreach (ReporteVenta rv in lista)
                {
                    var reporteItem = new ReporteVentaGrid
                    {
                        FechaRegistro = rv.FechaRegistro,
                        TipoDocumento = rv.TipoDocumento,
                        NumeroDocumento = rv.NumeroDocumento,
                        MontoTotal = ConvertirADecimal(rv.MontoTotal),
                        UsuarioRegistro = rv.UsuarioRegistro,
                        DocumentoCliente = rv.DocumentoCliente,
                        NombreCliente = rv.NombreCliente,
                        CodigoProducto = rv.CodigoProducto,
                        NombreProducto = rv.NombreProducto,
                        Categoria = rv.Categoria,
                        PrecioVenta = ConvertirADecimal(rv.PrecioVenta),
                        Cantidad = ConvertirAEntero(rv.Cantidad),
                        SubTotal = ConvertirADecimal(rv.SubTotal)
                    };

                    reporteVentas.Add(reporteItem);
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
                reporteVentas.Clear();
                foreach (var item in reporteCompleto)
                {
                    reporteVentas.Add(item);
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
                    case "DocumentoCliente":
                        return r.DocumentoCliente.ToUpper().Contains(textoBusqueda);
                    case "NombreCliente":
                        return r.NombreCliente.ToUpper().Contains(textoBusqueda);
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

            reporteVentas.Clear();
            foreach (var item in reporteFiltrado)
            {
                reporteVentas.Add(item);
            }

            ActualizarContadores();
        }

        private void btnlimpiarbuscador_Click(object sender, RoutedEventArgs e)
        {
            txtbusqueda.Text = "";
            reporteVentas.Clear();
            foreach (var item in reporteCompleto)
            {
                reporteVentas.Add(item);
            }
            ActualizarContadores();
        }

        private void btnexportar_Click(object sender, RoutedEventArgs e)
        {
            if (reporteVentas.Count < 1)
            {
                MessageBox.Show("No hay registros para exportar", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = string.Format("ReporteVentas_{0}", DateTime.Now.ToString("ddMMyyyyHHmmss"));
            saveFileDialog.Filter = "Excel Files (.xlsx)|.xlsx|CSV Files (.csv)|.csv|All Files (.)|.";
            saveFileDialog.FilterIndex = 1; // Default to Excel

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();

                    if (extension == ".xlsx")
                    {
                        ExportarExcel(saveFileDialog.FileName);
                    }
                    else
                    {
                        ExportarCSV(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Reporte exportado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar el reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportarExcel(string fileName)
        {
            using (var workbook = SpreadsheetDocument.Create(fileName, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Reporte Ventas"
                };
                sheets.Append(sheet);

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Título del reporte
                var titleRow = new Row() { RowIndex = 1 };
                var titleCell = new Cell()
                {
                    CellReference = "A1",
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text("REPORTE DE VENTAS"))
                };
                titleRow.Append(titleCell);
                sheetData.Append(titleRow);

                // Fecha del reporte
                var dateRow = new Row() { RowIndex = 2 };
                var dateCell = new Cell()
                {
                    CellReference = "A2",
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}"))
                };
                dateRow.Append(dateCell);
                sheetData.Append(dateRow);

                // Rango de fechas
                var rangeRow = new Row() { RowIndex = 3 };
                var rangeCell = new Cell()
                {
                    CellReference = "A3",
                    DataType = CellValues.InlineString,

                    InlineString = new InlineString(new Text($"Período: {txtfechainicio.SelectedDate?.ToString("dd/MM/yyyy")} - {txtfechafin.SelectedDate?.ToString("dd/MM/yyyy")}"))
                };
                rangeRow.Append(rangeCell);
                sheetData.Append(rangeRow);

                // Fila vacía
                var emptyRow = new Row() { RowIndex = 4 };
                sheetData.Append(emptyRow);

                // Encabezados
                var headerRow = new Row() { RowIndex = 5 };
                string[] headers = { "Fecha Registro", "Tipo Documento", "Número Documento", "Monto Total", "Usuario Registro",
                           "Documento Cliente", "Nombre Cliente", "Código Producto", "Nombre Producto", "Categoría",
                           "Precio Venta", "Cantidad", "Sub Total" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = new Cell()
                    {
                        CellReference = GetCellReference(i + 1, 5),
                        DataType = CellValues.InlineString,
                        InlineString = new InlineString(new Text(headers[i]))
                    };
                    headerRow.Append(cell);
                }
                sheetData.Append(headerRow);

                // Datos
                uint rowIndex = 6;
                foreach (var item in reporteVentas)
                {
                    var dataRow = new Row() { RowIndex = rowIndex };

                    object[] values = {
                        item.FechaRegistro, item.TipoDocumento, item.NumeroDocumento, item.MontoTotal.ToString("F2"),
                        item.UsuarioRegistro, item.DocumentoCliente, item.NombreCliente, item.CodigoProducto,
                        item.NombreProducto, item.Categoria, item.PrecioVenta.ToString("F2"), item.Cantidad.ToString(),
                        item.SubTotal.ToString("F2")
                    };

                    for (int i = 0; i < values.Length; i++)
                    {
                        var cell = new Cell()
                        {
                            CellReference = GetCellReference(i + 1, (int)rowIndex),
                            DataType = CellValues.InlineString,
                            InlineString = new InlineString(new Text(values[i]?.ToString() ?? ""))
                        };
                        dataRow.Append(cell);
                    }
                    sheetData.Append(dataRow);
                    rowIndex++;
                }

                // Resumen
                var summaryRow1 = new Row() { RowIndex = rowIndex + 1 };
                sheetData.Append(summaryRow1);

                var summaryRow2 = new Row() { RowIndex = rowIndex + 2 };
                var summaryCell1 = new Cell()
                {
                    CellReference = GetCellReference(1, (int)(rowIndex + 2)),
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text("RESUMEN"))
                };
                summaryRow2.Append(summaryCell1);
                sheetData.Append(summaryRow2);

                var summaryRow3 = new Row() { RowIndex = rowIndex + 3 };
                var totalVentasCell = new Cell()
                {
                    CellReference = GetCellReference(1, (int)(rowIndex + 3)),
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text($"Total de Ventas: {reporteVentas.Sum(r => r.SubTotal).ToString("C", CultureInfo.CurrentCulture)}"))
                };
                summaryRow3.Append(totalVentasCell);
                sheetData.Append(summaryRow3);

                var summaryRow4 = new Row() { RowIndex = rowIndex + 4 };
                var totalRegistrosCell = new Cell()
                {
                    CellReference = GetCellReference(1, (int)(rowIndex + 4)),
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text($"Total de Registros: {reporteVentas.Count}"))
                };
                summaryRow4.Append(totalRegistrosCell);
                sheetData.Append(summaryRow4);

                workbook.Save();
            }
        }

        private void ExportarCSV(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8))
            {
                // Título y información del reporte
                writer.WriteLine("REPORTE DE VENTAS");
                writer.WriteLine($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}");
                writer.WriteLine($"Período: {txtfechainicio.SelectedDate?.ToString("dd/MM/yyyy")} - {txtfechafin.SelectedDate?.ToString("dd/MM/yyyy")}");
                writer.WriteLine();

                // Encabezados
                writer.WriteLine("\"Fecha Registro\",\"Tipo Documento\",\"Número Documento\",\"Monto Total\",\"Usuario Registro\",\"Documento Cliente\",\"Nombre Cliente\",\"Código Producto\",\"Nombre Producto\",\"Categoría\",\"Precio Venta\",\"Cantidad\",\"Sub Total\"");

                // Datos
                foreach (var item in reporteVentas)
                {
                    writer.WriteLine($"\"{EscaparCSV(item.FechaRegistro)}\",\"{EscaparCSV(item.TipoDocumento)}\",\"{EscaparCSV(item.NumeroDocumento)}\",\"{item.MontoTotal:F2}\",\"{EscaparCSV(item.UsuarioRegistro)}\",\"{EscaparCSV(item.DocumentoCliente)}\",\"{EscaparCSV(item.NombreCliente)}\",\"{EscaparCSV(item.CodigoProducto)}\",\"{EscaparCSV(item.NombreProducto)}\",\"{EscaparCSV(item.Categoria)}\",\"{item.PrecioVenta:F2}\",\"{item.Cantidad}\",\"{item.SubTotal:F2}\"");
                }

                // Resumen
                writer.WriteLine();
                writer.WriteLine("RESUMEN");
                writer.WriteLine($"\"Total de Ventas: {reporteVentas.Sum(r => r.SubTotal).ToString("C", CultureInfo.CurrentCulture)}\"");
                writer.WriteLine($"\"Total de Registros: {reporteVentas.Count}\"");
            }
        }

        private string EscaparCSV(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return "";

            return valor.Replace("\"", "\"\"");
        }

        private string GetCellReference(int column, int row)
        {
            string columnName = "";
            while (column > 0)
            {
                int modulo = (column - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                column = (column - modulo) / 26;
            }
            return columnName + row;
        }

        private void ActualizarContadores()
        {
            lblTotalRegistros.Text = $"({reporteVentas.Count} registros)";

            decimal totalGeneral = reporteVentas.Sum(r => r.SubTotal);
            lblTotalGeneral.Text = totalGeneral.ToString("C", CultureInfo.CurrentCulture);
        }
    }

    public class ReporteVentaGrid
    {
        public string FechaRegistro { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public decimal MontoTotal { get; set; }
        public string UsuarioRegistro { get; set; }
        public string DocumentoCliente { get; set; }
        public string NombreCliente { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal SubTotal { get; set; }
    }
}