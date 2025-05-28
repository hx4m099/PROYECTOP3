using Logica;
using Entidades;
using GUI.Utilidades;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GUI.Modales
{
    public partial class mdProveedor : Window
    {
        public class ProveedorModal
        {
            public string Id { get; set; }
            public string Documento { get; set; }
            public string RazonSocial { get; set; }
        }

        public Proveedor _Proveedor { get; set; }
        private ObservableCollection<ProveedorModal> proveedoresModal;
        private ObservableCollection<ProveedorModal> proveedoresOriginal;

        public mdProveedor()
        {
            InitializeComponent();
            proveedoresModal = new ObservableCollection<ProveedorModal>();
            proveedoresOriginal = new ObservableCollection<ProveedorModal>();
            dgvdata.ItemsSource = proveedoresModal;
        }

        private void mdProveedor_Load(object sender, RoutedEventArgs e)
        {
            try
            {
                // Cargar opciones de búsqueda (solo columnas visibles)
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Documento", Texto = "Nro Documento" });
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = "RazonSocial", Texto = "Razón Social" });
                cbobusqueda.DisplayMemberPath = "Texto";
                cbobusqueda.SelectedValuePath = "Valor";
                cbobusqueda.SelectedIndex = 0;

                // Limpiar colecciones antes de cargar
                proveedoresModal.Clear();
                proveedoresOriginal.Clear();

                // Cargar proveedores
                var lista = new ProveedorService().Listar();

                foreach (Proveedor item in lista)
                {
                    var proveedorModal = new ProveedorModal
                    {
                        Id = item.IdProveedor.ToString(),
                        Documento = item.Documento,
                        RazonSocial = item.RazonSocial
                    };

                    proveedoresModal.Add(proveedorModal);
                    proveedoresOriginal.Add(new ProveedorModal
                    {
                        Id = item.IdProveedor.ToString(),
                        Documento = item.Documento,
                        RazonSocial = item.RazonSocial
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar proveedores: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvdata_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvdata.SelectedItem != null)
            {
                var proveedorSeleccionado = dgvdata.SelectedItem as ProveedorModal;
                if (proveedorSeleccionado != null)
                {
                    _Proveedor = new Proveedor()
                    {
                        IdProveedor = Convert.ToInt32(proveedorSeleccionado.Id),
                        Documento = proveedorSeleccionado.Documento,
                        RazonSocial = proveedorSeleccionado.RazonSocial
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
                var proveedorSeleccionado = dgvdata.SelectedItem as ProveedorModal;
                if (proveedorSeleccionado != null)
                {
                    _Proveedor = new Proveedor()
                    {
                        IdProveedor = Convert.ToInt32(proveedorSeleccionado.Id),
                        Documento = proveedorSeleccionado.Documento,
                        RazonSocial = proveedorSeleccionado.RazonSocial
                    };

                    this.DialogResult = true;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un proveedor", "Validación",
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
                // Si no hay texto de búsqueda, mostrar todos los proveedores
                proveedoresModal.Clear();
                foreach (var proveedor in proveedoresOriginal)
                {
                    proveedoresModal.Add(proveedor);
                }
                return;
            }

            try
            {
                string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();
                string textoBusqueda = txtbusqueda.Text.Trim().ToUpper();

                var proveedoresFiltrados = proveedoresOriginal.Where(p =>
                {
                    string valorCampo = "";
                    switch (columnaFiltro)
                    {
                        case "Documento":
                            valorCampo = p.Documento;
                            break;
                        case "RazonSocial":
                            valorCampo = p.RazonSocial;
                            break;
                    }
                    return valorCampo.ToUpper().Contains(textoBusqueda);
                }).ToList();

                proveedoresModal.Clear();
                foreach (var proveedor in proveedoresFiltrados)
                {
                    proveedoresModal.Add(proveedor);
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

            // Restaurar todos los proveedores
            proveedoresModal.Clear();
            foreach (var proveedor in proveedoresOriginal)
            {
                proveedoresModal.Add(proveedor);
            }
        }
    }
}