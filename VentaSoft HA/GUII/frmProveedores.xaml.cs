using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GUI.Utilidades;
using Entidades;
using Logica;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using GUI.Modales;
using OpenXmlBorder = DocumentFormat.OpenXml.Spreadsheet.Border;
using WpfBorder = System.Windows.Controls.Border;

namespace GUI
{
    public partial class frmProveedores : Window
    {
        private ObservableCollection<ProveedorGrid> proveedoresGrid;

        public frmProveedores()
        {
            InitializeComponent();
            proveedoresGrid = new ObservableCollection<ProveedorGrid>();
            dgvdata.ItemsSource = proveedoresGrid;
            Loaded += frmProveedores_Load;
        }

        private void frmProveedores_Load(object sender, RoutedEventArgs e)
        {
            // Cargar estados
            cboestado.Items.Add(new OpcionCombo() { Valor = 1, Texto = "Activo" });
            cboestado.Items.Add(new OpcionCombo() { Valor = 0, Texto = "No Activo" });
            cboestado.DisplayMemberPath = "Texto";
            cboestado.SelectedValuePath = "Valor";
            cboestado.SelectedIndex = 0;

            // Cargar opciones de búsqueda
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Documento", Texto = "Nro Documento" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "RazonSocial", Texto = "Razón Social" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Correo", Texto = "Correo" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Telefono", Texto = "Teléfono" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Estado", Texto = "Estado" });
            cbobusqueda.DisplayMemberPath = "Texto";
            cbobusqueda.SelectedValuePath = "Valor";
            cbobusqueda.SelectedIndex = 0;

            // Cargar proveedores
            CargarProveedores();
        }

        private void CargarProveedores()
        {
            proveedoresGrid.Clear();
            List<Proveedor> lista = new ProveedorService().Listar();

            foreach (Proveedor item in lista)
            {
                proveedoresGrid.Add(new ProveedorGrid
                {
                    Id = item.IdProveedor,
                    Documento = item.Documento,
                    RazonSocial = item.RazonSocial,
                    Correo = item.Correo,
                    Telefono = item.Telefono,
                    EstadoValor = item.Estado ? 1 : 0,
                    Estado = item.Estado ? "Activo" : "No Activo"
                });
            }

            ActualizarContadores();
        }

        private void btnguardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtdocumento.Text))
            {
                MessageBox.Show("Debe ingresar el número de documento", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtdocumento.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtrazonsocial.Text))
            {
                MessageBox.Show("Debe ingresar la razón social", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtrazonsocial.Focus();
                return;
            }

            // Validar formato de correo
            if (!string.IsNullOrWhiteSpace(txtcorreo.Text) && !IsValidEmail(txtcorreo.Text))
            {
                MessageBox.Show("El formato del correo electrónico no es válido", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtcorreo.Focus();
                return;
            }

            Proveedor obj = new Proveedor()
            {
                IdProveedor = Convert.ToInt32(txtid.Text),
                Documento = txtdocumento.Text,
                RazonSocial = txtrazonsocial.Text,
                Correo = txtcorreo.Text,
                Telefono = txttelefono.Text,
                Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1 ? true : false
            };

            if (obj.IdProveedor == 0)
            {
                int idgenerado = new ProveedorService().Registrar(obj, out mensaje);

                if (idgenerado != 0)
                {
                    proveedoresGrid.Add(new ProveedorGrid
                    {
                        Id = idgenerado,
                        Documento = txtdocumento.Text,
                        RazonSocial = txtrazonsocial.Text,
                        Correo = txtcorreo.Text,
                        Telefono = txttelefono.Text,
                        EstadoValor = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor),
                        Estado = ((OpcionCombo)cboestado.SelectedItem).Texto.ToString()
                    });

                    Limpiar();
                    ActualizarContadores();
                    MessageBox.Show("Proveedor registrado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                bool resultado = new ProveedorService().Editar(obj, out mensaje);

                if (resultado)
                {
                    var proveedor = proveedoresGrid.FirstOrDefault(p => p.Id == obj.IdProveedor);
                    if (proveedor != null)
                    {
                        proveedor.Documento = txtdocumento.Text;
                        proveedor.RazonSocial = txtrazonsocial.Text;
                        proveedor.Correo = txtcorreo.Text;
                        proveedor.Telefono = txttelefono.Text;
                        proveedor.EstadoValor = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor);
                        proveedor.Estado = ((OpcionCombo)cboestado.SelectedItem).Texto.ToString();
                    }

                    Limpiar();
                    ActualizarContadores();
                    MessageBox.Show("Proveedor actualizado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
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
            txtdocumento.Text = "";
            txtrazonsocial.Text = "";
            txtcorreo.Text = "";
            txttelefono.Text = "";
            cboestado.SelectedIndex = 0;

            txtdocumento.Focus();
        }

        private void btnseleccionar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ProveedorGrid proveedor = btn.DataContext as ProveedorGrid;

            if (proveedor != null)
            {
                txtid.Text = proveedor.Id.ToString();
                txtdocumento.Text = proveedor.Documento;
                txtrazonsocial.Text = proveedor.RazonSocial;
                txtcorreo.Text = proveedor.Correo;
                txttelefono.Text = proveedor.Telefono;

                // Seleccionar estado
                foreach (OpcionCombo oc in cboestado.Items)
                {
                    if (Convert.ToInt32(oc.Valor) == proveedor.EstadoValor)
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
                if (MessageBox.Show("¿Desea eliminar el proveedor?", "Mensaje", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    string mensaje = string.Empty;
                    Proveedor obj = new Proveedor()
                    {
                        IdProveedor = Convert.ToInt32(txtid.Text)
                    };

                    bool respuesta = new ProveedorService().Eliminar(obj, out mensaje);

                    if (respuesta)
                    {
                        var proveedor = proveedoresGrid.FirstOrDefault(p => p.Id == obj.IdProveedor);
                        if (proveedor != null)
                        {
                            proveedoresGrid.Remove(proveedor);
                        }
                        Limpiar();
                        ActualizarContadores();
                        MessageBox.Show("Proveedor eliminado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un proveedor", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void btnbuscar_Click(object sender, RoutedEventArgs e)
        {
            string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();
            string textoBusqueda = txtbusqueda.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(textoBusqueda))
            {
                dgvdata.ItemsSource = proveedoresGrid;
                ActualizarContadores();
                return;
            }

            var proveedoresFiltrados = proveedoresGrid.Where(p =>
            {
                switch (columnaFiltro)
                {
                    case "Documento":
                        return p.Documento.ToUpper().Contains(textoBusqueda);
                    case "RazonSocial":
                        return p.RazonSocial.ToUpper().Contains(textoBusqueda);
                    case "Correo":
                        return p.Correo.ToUpper().Contains(textoBusqueda);
                    case "Telefono":
                        return p.Telefono.ToUpper().Contains(textoBusqueda);
                    case "Estado":
                        return p.Estado.ToUpper().Contains(textoBusqueda);
                    default:
                        return false;
                }
            }).ToList();

            dgvdata.ItemsSource = proveedoresFiltrados;
            lblTotalProveedores.Text = proveedoresFiltrados.Count.ToString();
            lblProveedoresActivos.Text = proveedoresFiltrados.Count(p => p.Estado == "Activo").ToString();
        }

        private void btnlimpiarbuscador_Click(object sender, RoutedEventArgs e)
        {
            txtbusqueda.Text = "";
            dgvdata.ItemsSource = proveedoresGrid;
            ActualizarContadores();
        }

        private void btnlimpiar_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }

        private void btnexportar_Click(object sender, RoutedEventArgs e)
        {
            if (proveedoresGrid.Count < 1)
            {
                MessageBox.Show("No hay datos para exportar", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = string.Format("ReporteProveedores_{0}", DateTime.Now.ToString("ddMMyyyyHHmmss"));
            saveFileDialog.Filter = "Excel Files (.xlsx)|.xlsx|CSV Files (.csv)|.csv";
            saveFileDialog.DefaultExt = "xlsx";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string extension = Path.GetExtension(saveFileDialog.FileName).ToLower();

                    if (extension == ".xlsx")
                    {
                        ExportarAExcel(saveFileDialog.FileName);
                    }
                    else
                    {
                        ExportarACSV(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Reporte exportado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar el reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportarAExcel(string fileName)
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                // Crear workbook
                WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                // Crear worksheet
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Crear sheets
                Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
                Sheet sheet = new Sheet()
                {
                    Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Proveedores"
                };
                sheets.Append(sheet);

                // Crear stylesheet para formato
                WorkbookStylesPart stylesPart = workbookpart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = CreateStylesheet();

                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Agregar título
                Row titleRow = new Row() { RowIndex = 1 };
                Cell titleCell = new Cell() { CellReference = "A1", DataType = CellValues.String, StyleIndex = 2 };
                titleCell.CellValue = new CellValue("REPORTE DE PROVEEDORES");
                titleRow.AppendChild(titleCell);
                sheetData.AppendChild(titleRow);

                // Agregar fecha de generación
                Row dateRow = new Row() { RowIndex = 2 };
                Cell dateCell = new Cell() { CellReference = "A2", DataType = CellValues.String, StyleIndex = 0 };
                dateCell.CellValue = new CellValue($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                dateRow.AppendChild(dateCell);
                sheetData.AppendChild(dateRow);

                // Fila vacía
                Row emptyRow = new Row() { RowIndex = 3 };
                sheetData.AppendChild(emptyRow);

                // Agregar encabezados
                Row headerRow = new Row() { RowIndex = 4 };
                string[] headers = { "Nro Documento", "Razón Social", "Correo Electrónico", "Teléfono", "Estado" };
                string[] columnReferences = { "A4", "B4", "C4", "D4", "E4" };

                for (int i = 0; i < headers.Length; i++)
                {
                    Cell headerCell = new Cell()
                    {
                        CellReference = columnReferences[i],
                        DataType = CellValues.String,
                        StyleIndex = 1 // Estilo para encabezados
                    };
                    headerCell.CellValue = new CellValue(headers[i]);
                    headerRow.AppendChild(headerCell);
                }
                sheetData.AppendChild(headerRow);

                // Agregar datos
                uint rowIndex = 5;
                foreach (var proveedor in proveedoresGrid)
                {
                    Row dataRow = new Row() { RowIndex = rowIndex };

                    // Documento
                    Cell docCell = new Cell()
                    {
                        CellReference = $"A{rowIndex}",
                        DataType = CellValues.String,
                        StyleIndex = 0
                    };
                    docCell.CellValue = new CellValue(proveedor.Documento ?? "");
                    dataRow.AppendChild(docCell);

                    // Razón Social
                    Cell razonCell = new Cell()
                    {
                        CellReference = $"B{rowIndex}",
                        DataType = CellValues.String,
                        StyleIndex = 0
                    };
                    razonCell.CellValue = new CellValue(proveedor.RazonSocial ?? "");
                    dataRow.AppendChild(razonCell);

                    // Correo
                    Cell correoCell = new Cell()
                    {
                        CellReference = $"C{rowIndex}",
                        DataType = CellValues.String,
                        StyleIndex = 0
                    };
                    correoCell.CellValue = new CellValue(proveedor.Correo ?? "");
                    dataRow.AppendChild(correoCell);

                    // Teléfono
                    Cell telefonoCell = new Cell()
                    {
                        CellReference = $"D{rowIndex}",
                        DataType = CellValues.String,
                        StyleIndex = 0
                    };
                    telefonoCell.CellValue = new CellValue(proveedor.Telefono ?? "");
                    dataRow.AppendChild(telefonoCell);

                    // Estado
                    Cell estadoCell = new Cell()
                    {
                        CellReference = $"E{rowIndex}",
                        DataType = CellValues.String,
                        StyleIndex = 0
                    };
                    estadoCell.CellValue = new CellValue(proveedor.Estado ?? "");
                    dataRow.AppendChild(estadoCell);

                    sheetData.AppendChild(dataRow);
                    rowIndex++;
                }

                // Agregar resumen al final
                rowIndex += 2;
                Row summaryRow = new Row() { RowIndex = rowIndex };
                Cell summaryCell = new Cell()
                {
                    CellReference = $"A{rowIndex}",
                    DataType = CellValues.String,
                    StyleIndex = 1
                };
                summaryCell.CellValue = new CellValue($"Total de proveedores: {proveedoresGrid.Count}");
                summaryRow.AppendChild(summaryCell);
                sheetData.AppendChild(summaryRow);

                rowIndex++;
                Row activosRow = new Row() { RowIndex = rowIndex };
                Cell activosCell = new Cell()
                {
                    CellReference = $"A{rowIndex}",
                    DataType = CellValues.String,
                    StyleIndex = 1
                };
                activosCell.CellValue = new CellValue($"Proveedores activos: {proveedoresGrid.Count(p => p.Estado == "Activo")}");
                activosRow.AppendChild(activosCell);
                sheetData.AppendChild(activosRow);

                workbookpart.Workbook.Save();
            }
        }

        private void ExportarACSV(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8))
            {
                // Escribir título y fecha
                writer.WriteLine("REPORTE DE PROVEEDORES");
                writer.WriteLine($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine(); // Línea vacía

                // Escribir encabezados
                writer.WriteLine("\"Nro Documento\",\"Razón Social\",\"Correo Electrónico\",\"Teléfono\",\"Estado\"");

                // Escribir datos
                foreach (var proveedor in proveedoresGrid)
                {
                    string documento = EscaparCSV(proveedor.Documento);
                    string razonSocial = EscaparCSV(proveedor.RazonSocial);
                    string correo = EscaparCSV(proveedor.Correo);
                    string telefono = EscaparCSV(proveedor.Telefono);
                    string estado = EscaparCSV(proveedor.Estado);

                    writer.WriteLine($"\"{documento}\",\"{razonSocial}\",\"{correo}\",\"{telefono}\",\"{estado}\"");
                }

                // Escribir resumen
                writer.WriteLine();
                writer.WriteLine($"\"Total de proveedores: {proveedoresGrid.Count}\"");
                writer.WriteLine($"\"Proveedores activos: {proveedoresGrid.Count(p => p.Estado == "Activo")}\"");
            }
        }

        private string EscaparCSV(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return "";

            // Escapar comillas dobles duplicándolas
            return valor.Replace("\"", "\"\"");
        }

        private Stylesheet CreateStylesheet()
        {
            Stylesheet stylesheet = new Stylesheet();

            // Fonts
            Fonts fonts = new Fonts();
            fonts.Count = 3;

            // Font normal
            Font font0 = new Font();
            font0.FontSize = new FontSize() { Val = 11 };
            font0.FontName = new FontName() { Val = "Calibri" };
            fonts.AppendChild(font0);

            // Font bold para encabezados
            Font font1 = new Font();
            font1.Bold = new Bold();
            font1.FontSize = new FontSize() { Val = 11 };
            font1.FontName = new FontName() { Val = "Calibri" };
            fonts.AppendChild(font1);

            // Font para título
            Font font2 = new Font();
            font2.Bold = new Bold();
            font2.FontSize = new FontSize() { Val = 14 };
            font2.FontName = new FontName() { Val = "Calibri" };
            fonts.AppendChild(font2);

            // Fills
            Fills fills = new Fills();
            fills.Count = 2;

            Fill fill0 = new Fill();
            fill0.PatternFill = new PatternFill() { PatternType = PatternValues.None };
            fills.AppendChild(fill0);

            Fill fill1 = new Fill();
            fill1.PatternFill = new PatternFill() { PatternType = PatternValues.Gray125 };
            fills.AppendChild(fill1);

            // Borders - Using fully qualified name to avoid conflict
            Borders borders = new Borders();
            borders.Count = 1;

            DocumentFormat.OpenXml.Spreadsheet.Border border0 = new DocumentFormat.OpenXml.Spreadsheet.Border();
            border0.LeftBorder = new LeftBorder();
            border0.RightBorder = new RightBorder();
            border0.TopBorder = new TopBorder();
            border0.BottomBorder = new BottomBorder();
            border0.DiagonalBorder = new DiagonalBorder();
            borders.AppendChild(border0);

            // Cell formats
            CellFormats cellFormats = new CellFormats();
            cellFormats.Count = 3;

            // Formato normal
            CellFormat cellFormat0 = new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 };
            cellFormats.AppendChild(cellFormat0);

            // Formato para encabezados
            CellFormat cellFormat1 = new CellFormat() { FontId = 1, FillId = 1, BorderId = 0 };
            cellFormats.AppendChild(cellFormat1);

            // Formato para título
            CellFormat cellFormat2 = new CellFormat() { FontId = 2, FillId = 0, BorderId = 0 };
            cellFormats.AppendChild(cellFormat2);

            stylesheet.Fonts = fonts;
            stylesheet.Fills = fills;
            stylesheet.Borders = borders;
            stylesheet.CellFormats = cellFormats;

            return stylesheet;
        }

        private void ActualizarContadores()
        {
            lblTotalProveedores.Text = proveedoresGrid.Count.ToString();
            lblProveedoresActivos.Text = proveedoresGrid.Count(p => p.Estado == "Activo").ToString();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }

    public class ProveedorGrid
    {
        public int Id { get; set; }
        public string Documento { get; set; }
        public string RazonSocial { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public int EstadoValor { get; set; }
        public string Estado { get; set; }
    }
}