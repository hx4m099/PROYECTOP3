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
    public partial class mdCliente : Window
    {
        public class ClienteModal
        {
            public string Documento { get; set; }
            public string NombreCompleto { get; set; }
        }

        public Cliente _Cliente { get; set; }
        private ObservableCollection<ClienteModal> clientesModal;
        private ObservableCollection<ClienteModal> clientesOriginal;

        public mdCliente()
        {
            InitializeComponent();
            clientesModal = new ObservableCollection<ClienteModal>();
            clientesOriginal = new ObservableCollection<ClienteModal>();
            dgvdata.ItemsSource = clientesModal;
        }

        private void mdCliente_Load(object sender, RoutedEventArgs e)
        {
            try
            {
                // Cargar opciones de búsqueda
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Documento", Texto = "Nro Documento" });
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = "NombreCompleto", Texto = "Nombre Completo" });
                cbobusqueda.DisplayMemberPath = "Texto";
                cbobusqueda.SelectedValuePath = "Valor";
                cbobusqueda.SelectedIndex = 0;

                // Cargar clientes activos
                var lista = new ClienteService().Listar();

                foreach (Cliente item in lista)
                {
                    if (item.Estado)
                    {
                        var clienteModal = new ClienteModal
                        {
                            Documento = item.Documento,
                            NombreCompleto = item.NombreCompleto
                        };

                        clientesModal.Add(clienteModal);
                        clientesOriginal.Add(clienteModal);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvdata_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvdata.SelectedItem != null)
            {
                var clienteSeleccionado = dgvdata.SelectedItem as ClienteModal;
                if (clienteSeleccionado != null)
                {
                    _Cliente = new Cliente()
                    {
                        Documento = clienteSeleccionado.Documento,
                        NombreCompleto = clienteSeleccionado.NombreCompleto
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
                var clienteSeleccionado = dgvdata.SelectedItem as ClienteModal;
                if (clienteSeleccionado != null)
                {
                    _Cliente = new Cliente()
                    {
                        Documento = clienteSeleccionado.Documento,
                        NombreCompleto = clienteSeleccionado.NombreCompleto
                    };

                    this.DialogResult = true;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un cliente", "Validación",
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
                // Si no hay texto de búsqueda, mostrar todos los clientes
                clientesModal.Clear();
                foreach (var cliente in clientesOriginal)
                {
                    clientesModal.Add(cliente);
                }
                return;
            }

            try
            {
                string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();
                string textoBusqueda = txtbusqueda.Text.Trim().ToUpper();

                var clientesFiltrados = clientesOriginal.Where(c =>
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
                    }
                    return valorCampo.ToUpper().Contains(textoBusqueda);
                }).ToList();

                clientesModal.Clear();
                foreach (var cliente in clientesFiltrados)
                {
                    clientesModal.Add(cliente);
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

            // Restaurar todos los clientes
            clientesModal.Clear();
            foreach (var cliente in clientesOriginal)
            {
                clientesModal.Add(cliente);
            }
        }

        private void btnregistrarcliente_Click(object sender, RoutedEventArgs e)
        {
            frmClientes ventana = new frmClientes();
            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                try
                {
                    // Recargar lista
                    clientesModal.Clear();
                    clientesOriginal.Clear();

                    var lista = new ClienteService().Listar();

                    foreach (Cliente item in lista)
                    {
                        if (item.Estado)
                        {
                            var nuevo = new ClienteModal()
                            {
                                Documento = item.Documento,
                                NombreCompleto = item.NombreCompleto
                            };

                            clientesModal.Add(nuevo);
                            clientesOriginal.Add(nuevo);
                        }
                    }

                    MessageBox.Show("Cliente agregado correctamente. Lista actualizada.", "Información",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al recargar clientes: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}