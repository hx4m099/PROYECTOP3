using Entidades;
using Logica;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace GUI
{
    public partial class frmDetalleCompra : Window
    {
        public class ProductoCompra
        {
            public string Producto { get; set; }
            public string PrecioCompra { get; set; }
            public string Cantidad { get; set; }
            public string SubTotal { get; set; }
        }

        private ObservableCollection<ProductoCompra> productosCompra;

        public frmDetalleCompra()
        {
            InitializeComponent();
            productosCompra = new ObservableCollection<ProductoCompra>();
            dgvdata.ItemsSource = productosCompra;
        }

        private void btnbuscar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtbusqueda.Text))
            {
                MessageBox.Show("Ingrese un número de documento para buscar", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtbusqueda.Focus();
                return;
            }

            try
            {
                Compra oCompra = new CompraService().ObtenerCompra(txtbusqueda.Text);

                if (oCompra.IdCompra != 0)
                {
                    txtnumerodocumento.Text = oCompra.NumeroDocumento;
                    txtfecha.Text = oCompra.FechaRegistro;
                    txttipodocumento.Text = oCompra.TipoDocumento;
                    txtusuario.Text = oCompra.oUsuario.NombreCompleto;
                    txtdocproveedor.Text = oCompra.oProveedor.Documento;
                    txtnombreproveedor.Text = oCompra.oProveedor.RazonSocial;

                    productosCompra.Clear();
                    foreach (Detalle_Compra dc in oCompra.oDetalleCompra)
                    {
                        productosCompra.Add(new ProductoCompra
                        {
                            Producto = dc.oProducto.Nombre,
                            PrecioCompra = dc.PrecioCompra.ToString("0.00"),
                            Cantidad = dc.Cantidad.ToString(),
                            SubTotal = dc.MontoTotal.ToString("0.00")
                        });
                    }

                    txtmontototal.Text = oCompra.MontoTotal.ToString("0.00");

                    MessageBox.Show("Compra encontrada correctamente", "Éxito",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No se encontró ninguna compra con ese número de documento", "Sin resultados",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LimpiarFormulario();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar la compra: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnborrar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtfecha.Text = "";
            txttipodocumento.Text = "";
            txtusuario.Text = "";
            txtdocproveedor.Text = "";
            txtnombreproveedor.Text = "";
            txtnumerodocumento.Text = "";

            productosCompra.Clear();
            txtmontototal.Text = "0.00";
        }

        private void btndescargar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txttipodocumento.Text))
            {
                MessageBox.Show("No se encontraron resultados para generar el PDF", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string Texto_Html = Properties.Resources.PlantillaCompra.ToString();
                Negocio odatos = new NegocioService().ObtenerDatos();

                Texto_Html = Texto_Html.Replace("@nombrenegocio", odatos.Nombre.ToUpper());
                Texto_Html = Texto_Html.Replace("@docnegocio", odatos.RUC);
                Texto_Html = Texto_Html.Replace("@direcnegocio", odatos.Direccion);

                Texto_Html = Texto_Html.Replace("@tipodocumento", txttipodocumento.Text.ToUpper());
                Texto_Html = Texto_Html.Replace("@numerodocumento", txtnumerodocumento.Text);

                Texto_Html = Texto_Html.Replace("@docproveedor", txtdocproveedor.Text);
                Texto_Html = Texto_Html.Replace("@nombreproveedor", txtnombreproveedor.Text);
                Texto_Html = Texto_Html.Replace("@fecharegistro", txtfecha.Text);
                Texto_Html = Texto_Html.Replace("@usuarioregistro", txtusuario.Text);

                string filas = string.Empty;
                foreach (ProductoCompra producto in productosCompra)
                {
                    filas += "<tr>";
                    filas += "<td>" + producto.Producto + "</td>";
                    filas += "<td>" + producto.PrecioCompra + "</td>";
                    filas += "<td>" + producto.Cantidad + "</td>";
                    filas += "<td>" + producto.SubTotal + "</td>";
                    filas += "</tr>";
                }
                Texto_Html = Texto_Html.Replace("@filas", filas);
                Texto_Html = Texto_Html.Replace("@montototal", txtmontototal.Text);

                SaveFileDialog savefile = new SaveFileDialog();
                savefile.FileName = string.Format("Compra_{0}.pdf", txtnumerodocumento.Text);
                savefile.Filter = "Archivos PDF|*.pdf";
                savefile.Title = "Guardar Reporte de Compra";

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
                        MessageBox.Show("Documento PDF generado exitosamente", "Éxito",
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