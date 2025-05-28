using Entidades;
using Logica;
using DocumentFormat.OpenXml.Wordprocessing;

using GUI.Utilidades;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GUI
{
    public partial class frmCategoria : Window
    {
        public class CategoriaGrid
        {
            public string Id { get; set; }
            public string Descripcion { get; set; }
            public string EstadoValor { get; set; }
            public string Estado { get; set; }
        }

        private ObservableCollection<CategoriaGrid> categoriasGrid;
        private ObservableCollection<CategoriaGrid> categoriasOriginal;

        public frmCategoria()
        {
            InitializeComponent();
            categoriasGrid = new ObservableCollection<CategoriaGrid>();
            categoriasOriginal = new ObservableCollection<CategoriaGrid>();
            dgvdata.ItemsSource = categoriasGrid;
        }

        private void frmCategoria_Load(object sender, RoutedEventArgs e)
        {
            // Cargar estados
            cboestado.Items.Add(new OpcionCombo() { Valor = 1, Texto = "Activo" });
            cboestado.Items.Add(new OpcionCombo() { Valor = 0, Texto = "No Activo" });
            cboestado.DisplayMemberPath = "Texto";
            cboestado.SelectedValuePath = "Valor";
            cboestado.SelectedIndex = 0;

            // Cargar opciones de búsqueda
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Descripcion", Texto = "Descripción" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Estado", Texto = "Estado" });
            cbobusqueda.DisplayMemberPath = "Texto";
            cbobusqueda.SelectedValuePath = "Valor";
            cbobusqueda.SelectedIndex = 0;

            CargarCategorias();
        }

        private void CargarCategorias()
        {
            try
            {
                categoriasGrid.Clear();
                categoriasOriginal.Clear();
                var lista = new CategoriaService().Listar();

                foreach (Categoria item in lista)
                {
                    var categoriaGrid = new CategoriaGrid
                    {
                        Id = item.IdCategoria.ToString(),
                        Descripcion = item.Descripcion,
                        EstadoValor = item.Estado ? "1" : "0",
                        Estado = item.Estado ? "Activo" : "No Activo"
                    };

                    categoriasGrid.Add(categoriaGrid);
                    categoriasOriginal.Add(categoriaGrid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnguardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtdescripcion.Text))
            {
                MessageBox.Show("La descripción es obligatoria", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtdescripcion.Focus();
                return;
            }

            try
            {
                string mensaje = string.Empty;

                Categoria obj = new Categoria()
                {
                    IdCategoria = Convert.ToInt32(txtid.Text),
                    Descripcion = txtdescripcion.Text.Trim(),
                    Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1
                };

                if (obj.IdCategoria == 0)
                {
                    // Crear nueva categoría
                    int idgenerado = new    CategoriaService().Registrar(obj, out mensaje);

                    if (idgenerado != 0)
                    {
                        var nuevaCategoria = new CategoriaGrid
                        {
                            Id = idgenerado.ToString(),
                            Descripcion = txtdescripcion.Text,
                            EstadoValor = ((OpcionCombo)cboestado.SelectedItem).Valor.ToString(),
                            Estado = ((OpcionCombo)cboestado.SelectedItem).Texto
                        };

                        categoriasGrid.Add(nuevaCategoria);
                        categoriasOriginal.Add(nuevaCategoria);

                        Limpiar();
                        MessageBox.Show("Categoría registrada correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Editar categoría existente
                    bool resultado = new CategoriaService().Editar(obj, out mensaje);

                    if (resultado)
                    {
                        var categoriaGrid = categoriasGrid.FirstOrDefault(c => c.Id == txtid.Text);
                        var categoriaOriginal = categoriasOriginal.FirstOrDefault(c => c.Id == txtid.Text);

                        if (categoriaGrid != null)
                        {
                            categoriaGrid.Descripcion = txtdescripcion.Text;
                            categoriaGrid.EstadoValor = ((OpcionCombo)cboestado.SelectedItem).Valor.ToString();
                            categoriaGrid.Estado = ((OpcionCombo)cboestado.SelectedItem).Texto;
                        }

                        if (categoriaOriginal != null)
                        {
                            categoriaOriginal.Descripcion = txtdescripcion.Text;
                            categoriaOriginal.EstadoValor = ((OpcionCombo)cboestado.SelectedItem).Valor.ToString();
                            categoriaOriginal.Estado = ((OpcionCombo)cboestado.SelectedItem).Texto;
                        }

                        Limpiar();
                        MessageBox.Show("Categoría actualizada correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar categoría: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Limpiar()
        {
            txtindice.Text = "-1";
            txtid.Text = "0";
            txtdescripcion.Text = "";
            cboestado.SelectedIndex = 0;
            txtdescripcion.Focus();
        }

        private void btnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var categoria = button?.DataContext as CategoriaGrid;

            if (categoria != null)
            {
                var index = categoriasGrid.IndexOf(categoria);
                txtindice.Text = index.ToString();
                txtid.Text = categoria.Id;
                txtdescripcion.Text = categoria.Descripcion;

                foreach (OpcionCombo oc in cboestado.Items)
                {
                    if (Convert.ToInt32(oc.Valor) == Convert.ToInt32(categoria.EstadoValor))
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
                var result = MessageBox.Show("¿Está seguro de eliminar esta categoría?", "Confirmar eliminación",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        string mensaje = string.Empty;
                        Categoria obj = new Categoria()
                        {
                            IdCategoria = Convert.ToInt32(txtid.Text)
                        };

                        bool respuesta = new CategoriaService().Eliminar(obj, out mensaje);

                        if (respuesta)
                        {
                            var categoriaAEliminar = categoriasGrid.FirstOrDefault(c => c.Id == txtid.Text);
                            var categoriaOriginalAEliminar = categoriasOriginal.FirstOrDefault(c => c.Id == txtid.Text);

                            if (categoriaAEliminar != null)
                            {
                                categoriasGrid.Remove(categoriaAEliminar);
                            }

                            if (categoriaOriginalAEliminar != null)
                            {
                                categoriasOriginal.Remove(categoriaOriginalAEliminar);
                            }

                            Limpiar();
                            MessageBox.Show("Categoría eliminada correctamente", "Éxito",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar categoría: {ex.Message}", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar una categoría para eliminar", "Validación",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnbuscar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtbusqueda.Text))
            {
                // Si no hay texto de búsqueda, mostrar todas las categorías
                categoriasGrid.Clear();
                foreach (var categoria in categoriasOriginal)
                {
                    categoriasGrid.Add(categoria);
                }
                return;
            }

            try
            {
                string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();
                string textoBusqueda = txtbusqueda.Text.Trim().ToUpper();

                var categoriasFiltradas = categoriasOriginal.Where(c =>
                {
                    string valorCampo = "";
                    switch (columnaFiltro)
                    {
                        case "Descripcion":
                            valorCampo = c.Descripcion;
                            break;
                        case "Estado":
                            valorCampo = c.Estado;
                            break;
                    }
                    return valorCampo.ToUpper().Contains(textoBusqueda);
                }).ToList();

                categoriasGrid.Clear();
                foreach (var categoria in categoriasFiltradas)
                {
                    categoriasGrid.Add(categoria);
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

            // Restaurar todas las categorías
            categoriasGrid.Clear();
            foreach (var categoria in categoriasOriginal)
            {
                categoriasGrid.Add(categoria);
            }
        }

        private void btnlimpiar_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }
    }
}