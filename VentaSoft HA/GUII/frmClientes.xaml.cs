using Entidades;
using Logica;
using GUI.Utilidades;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GUI
{
    public partial class frmClientes : Window
    {
        public class ClienteGrid
        {
            public string Id { get; set; }
            public string Documento { get; set; }
            public string NombreCompleto { get; set; }
            public string Correo { get; set; }
            public string Telefono { get; set; }
            public string FechaNacimiento { get; set; }
            // ELIMINADA LA PROPIEDAD EDAD
            public string EstadoValor { get; set; }
            public string Estado { get; set; }
        }

        private ObservableCollection<ClienteGrid> clientesGrid;

        public frmClientes()
        {
            InitializeComponent();
            clientesGrid = new ObservableCollection<ClienteGrid>();
            dgvdata.ItemsSource = clientesGrid;
        }

        private void frmClientes_Load(object sender, RoutedEventArgs e)
        {
            // Cargar estados
            cboestado.Items.Add(new OpcionCombo() { Valor = 1, Texto = "Activo" });
            cboestado.Items.Add(new OpcionCombo() { Valor = 0, Texto = "No Activo" });
            cboestado.DisplayMemberPath = "Texto";
            cboestado.SelectedValuePath = "Valor";
            cboestado.SelectedIndex = 0;

            // Cargar opciones de búsqueda
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Documento", Texto = "Documento" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "NombreCompleto", Texto = "Nombre Completo" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Correo", Texto = "Correo" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Telefono", Texto = "Teléfono" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Estado", Texto = "Estado" });
            cbobusqueda.DisplayMemberPath = "Texto";
            cbobusqueda.SelectedValuePath = "Valor";
            cbobusqueda.SelectedIndex = 0;

            CargarClientes();
            VerificarCumpleanos();
        }

        private void VerificarCumpleanos()
        {
            try
            {
                var clientesCumpleanos = new ClienteService().ObtenerClientesCumpleañosHoy();

                if (clientesCumpleanos.Any())
                {
                    string mensaje = "¡Feliz cumpleaños a:\n\n";
                    foreach (var cliente in clientesCumpleanos)
                    {
                        var edad = new ClienteService().CalcularEdad(cliente.FechaNacimiento.Value);
                        mensaje += $"🎂 {cliente.NombreCompleto} ({edad} años)\n";
                    }
                    mensaje += "\n¡No olvides aplicar el descuento de cumpleaños!";

                    MessageBox.Show(mensaje, "Cumpleaños de Hoy",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error verificando cumpleaños: {ex.Message}");
            }
        }

        private void CargarClientes()
        {
            try
            {
                clientesGrid.Clear();
                var lista = new ClienteService().Listar();

                foreach (Cliente item in lista)
                {
                    string fechaNacimientoTexto = item.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "No especificada";

                    clientesGrid.Add(new ClienteGrid
                    {
                        Id = item.IdCliente.ToString(),
                        Documento = item.Documento,
                        NombreCompleto = item.NombreCompleto,
                        Correo = item.Correo,
                        Telefono = item.Telefono,
                        FechaNacimiento = fechaNacimientoTexto,
                        // ELIMINADA LA ASIGNACIÓN DE EDAD
                        EstadoValor = item.Estado ? "1" : "0",
                        Estado = item.Estado ? "Activo" : "No Activo"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnguardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtdocumento.Text))
            {
                MessageBox.Show("El documento es obligatorio", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtdocumento.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtnombrecompleto.Text))
            {
                MessageBox.Show("El nombre completo es obligatorio", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtnombrecompleto.Focus();
                return;
            }

            try
            {
                string mensaje = string.Empty;

                Cliente obj = new Cliente()
                {
                    IdCliente = Convert.ToInt32(txtid.Text),
                    Documento = txtdocumento.Text.Trim(),
                    NombreCompleto = txtnombrecompleto.Text.Trim(),
                    Correo = txtcorreo.Text.Trim(),
                    Telefono = txttelefono.Text.Trim(),
                    FechaNacimiento = dtpFechaNacimiento.SelectedDate,
                    Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1
                };

                if (obj.IdCliente == 0)
                {
                    // Crear nuevo cliente
                    int idgenerado = new ClienteService().Registrar(obj, out mensaje);

                    if (idgenerado != 0)
                    {
                        string fechaNacimientoTexto = obj.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "No especificada";

                        clientesGrid.Add(new ClienteGrid
                        {
                            Id = idgenerado.ToString(),
                            Documento = txtdocumento.Text,
                            NombreCompleto = txtnombrecompleto.Text,
                            Correo = txtcorreo.Text,
                            Telefono = txttelefono.Text,
                            FechaNacimiento = fechaNacimientoTexto,
                            // ELIMINADA LA ASIGNACIÓN DE EDAD
                            EstadoValor = ((OpcionCombo)cboestado.SelectedItem).Valor.ToString(),
                            Estado = ((OpcionCombo)cboestado.SelectedItem).Texto
                        });

                        Limpiar();
                        MessageBox.Show("Cliente registrado correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);

                        // ELIMINADAS ESTAS LÍNEAS QUE CERRABAN LA VENTANA
                        // this.DialogResult = true;
                        // this.Close();
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Editar cliente existente
                    bool resultado = new ClienteService().Editar(obj, out mensaje);

                    if (resultado)
                    {
                        var clienteGrid = clientesGrid.FirstOrDefault(c => c.Id == txtid.Text);
                        if (clienteGrid != null)
                        {
                            clienteGrid.Documento = txtdocumento.Text;
                            clienteGrid.NombreCompleto = txtnombrecompleto.Text;
                            clienteGrid.Correo = txtcorreo.Text;
                            clienteGrid.Telefono = txttelefono.Text;

                            string fechaNacimientoTexto = obj.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "No especificada";
                            clienteGrid.FechaNacimiento = fechaNacimientoTexto;
                            // ELIMINADA LA ASIGNACIÓN DE EDAD

                            clienteGrid.EstadoValor = ((OpcionCombo)cboestado.SelectedItem).Valor.ToString();
                            clienteGrid.Estado = ((OpcionCombo)cboestado.SelectedItem).Texto;
                        }

                        Limpiar();
                        MessageBox.Show("Cliente actualizado correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);

                        // ELIMINADAS ESTAS LÍNEAS QUE CERRABAN LA VENTANA
                        // this.DialogResult = true; 
                        // this.Close();

                        // RECARGAR LA LISTA PARA ASEGURAR QUE SE VEA LA ACTUALIZACIÓN
                        CargarClientes();
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cliente: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Limpiar()
        {
            txtindice.Text = "-1";
            txtid.Text = "0";
            txtdocumento.Text = "";
            txtnombrecompleto.Text = "";
            txtcorreo.Text = "";
            txttelefono.Text = "";
            dtpFechaNacimiento.SelectedDate = null;
            cboestado.SelectedIndex = 0;
            txtdocumento.Focus();
        }

        private void btnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var cliente = button?.DataContext as ClienteGrid;

            if (cliente != null)
            {
                var index = clientesGrid.IndexOf(cliente);
                txtindice.Text = index.ToString();
                txtid.Text = cliente.Id;
                txtdocumento.Text = cliente.Documento;
                txtnombrecompleto.Text = cliente.NombreCompleto;
                txtcorreo.Text = cliente.Correo;
                txttelefono.Text = cliente.Telefono;

                // Cargar fecha de nacimiento
                if (cliente.FechaNacimiento != "No especificada")
                {
                    if (DateTime.TryParseExact(cliente.FechaNacimiento, "dd/MM/yyyy",
                                             null, System.Globalization.DateTimeStyles.None,
                                             out DateTime fecha))
                    {
                        dtpFechaNacimiento.SelectedDate = fecha;
                    }
                }
                else
                {
                    dtpFechaNacimiento.SelectedDate = null;
                }

                foreach (OpcionCombo oc in cboestado.Items)
                {
                    if (Convert.ToInt32(oc.Valor) == Convert.ToInt32(cliente.EstadoValor))
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
                var result = MessageBox.Show("¿Está seguro de eliminar este cliente?", "Confirmar eliminación",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        string mensaje = string.Empty;
                        Cliente obj = new Cliente()
                        {
                            IdCliente = Convert.ToInt32(txtid.Text)
                        };

                        bool respuesta = new ClienteService().Eliminar(obj, out mensaje);

                        if (respuesta)
                        {
                            var clienteAEliminar = clientesGrid.FirstOrDefault(c => c.Id == txtid.Text);
                            if (clienteAEliminar != null)
                            {
                                clientesGrid.Remove(clienteAEliminar);
                            }

                            Limpiar();
                            MessageBox.Show("Cliente eliminado correctamente", "Éxito",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar cliente: {ex.Message}", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un cliente para eliminar", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnbuscar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtbusqueda.Text))
            {
                CargarClientes();
                return;
            }

            try
            {
                string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();
                string textoBusqueda = txtbusqueda.Text.Trim().ToUpper();

                var clientesFiltrados = clientesGrid.Where(c =>
                {
                    string valorCampo = "";
                    switch (columnaFiltro)
                    {
                        case "Documento":
                            valorCampo = c.Documento;
                            break;
                        case "NombreCompleto":
                            valorCampo = c.NombreCompleto;
                            break;
                        case "Correo":
                            valorCampo = c.Correo;
                            break;
                        case "Telefono":
                            valorCampo = c.Telefono;
                            break;
                        case "Estado":
                            valorCampo = c.Estado;
                            break;
                    }
                    return valorCampo.ToUpper().Contains(textoBusqueda);
                }).ToList();

                clientesGrid.Clear();
                foreach (var cliente in clientesFiltrados)
                {
                    clientesGrid.Add(cliente);
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
            CargarClientes();
        }

        private void btnlimpiar_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }
    }
}