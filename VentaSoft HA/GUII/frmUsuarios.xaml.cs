using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GUI.Utilidades;
using Entidades;
using Logica;
using DocumentFormat.OpenXml.Wordprocessing;

namespace GUI
{
    public partial class frmUsuarios : Window
    {
        private ObservableCollection<UsuarioGrid> usuariosGrid;

        public frmUsuarios()
        {
            InitializeComponent();
            usuariosGrid = new ObservableCollection<UsuarioGrid>();
            dgvdata.ItemsSource = usuariosGrid;
            Loaded += frmUsuarios_Load;
        }

        private void frmUsuarios_Load(object sender, RoutedEventArgs e)
        {
            // Cargar estados
            cboestado.Items.Add(new OpcionCombo() { Valor = 1, Texto = "Activo" });
            cboestado.Items.Add(new OpcionCombo() { Valor = 0, Texto = "No Activo" });
            cboestado.DisplayMemberPath = "Texto";
            cboestado.SelectedValuePath = "Valor";
            cboestado.SelectedIndex = 0;

            // Cargar roles
            List<Rol> listaRol = new RolService().Listar();
            foreach (Rol item in listaRol)
            {
                cborol.Items.Add(new OpcionCombo() { Valor = item.IdRol, Texto = item.Descripcion });
            }
            cborol.DisplayMemberPath = "Texto";
            cborol.SelectedValuePath = "Valor";
            cborol.SelectedIndex = 0;

            // Cargar opciones de búsqueda
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Documento", Texto = "Nro Documento" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "NombreCompleto", Texto = "Nombre Completo" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Correo", Texto = "Correo" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Rol", Texto = "Rol" });
            cbobusqueda.Items.Add(new OpcionCombo() { Valor = "Estado", Texto = "Estado" });
            cbobusqueda.DisplayMemberPath = "Texto";
            cbobusqueda.SelectedValuePath = "Valor";
            cbobusqueda.SelectedIndex = 0;

            // Cargar usuarios
            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            usuariosGrid.Clear();
            List<Usuario> listaUsuario = new UsuarioService().Listar();

            foreach (Usuario item in listaUsuario)
            {
                usuariosGrid.Add(new UsuarioGrid
                {
                    Id = item.IdUsuario,
                    Documento = item.Documento,
                    NombreCompleto = item.NombreCompleto,
                    Correo = item.Correo,
                    Clave = item.Clave,
                    IdRol = item.oRol.IdRol,
                    Rol = item.oRol.Descripcion,
                    EstadoValor = item.Estado ? 1 : 0,
                    Estado = item.Estado ? "Activo" : "No Activo"
                });
            }
        }

        private void btnguardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;

            // Validar contraseñas
            if (txtclave.Password != txtconfirmarclave.Password)
            {
                MessageBox.Show("Las contraseñas no coinciden", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Usuario objusuario = new Usuario()
            {
                IdUsuario = Convert.ToInt32(txtid.Text),
                Documento = txtdocumento.Text,
                NombreCompleto = txtnombrecompleto.Text,
                Correo = txtcorreo.Text,
                Clave = txtclave.Password,
                oRol = new Rol() { IdRol = Convert.ToInt32(((OpcionCombo)cborol.SelectedItem).Valor) },
                Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1 ? true : false
            };

            if (objusuario.IdUsuario == 0)
            {
                int idusuariogenerado = new UsuarioService().Registrar(objusuario, out mensaje);

                if (idusuariogenerado != 0)
                {
                    usuariosGrid.Add(new UsuarioGrid
                    {
                        Id = idusuariogenerado,
                        Documento = txtdocumento.Text,
                        NombreCompleto = txtnombrecompleto.Text,
                        Correo = txtcorreo.Text,
                        Clave = txtclave.Password,
                        IdRol = Convert.ToInt32(((OpcionCombo)cborol.SelectedItem).Valor),
                        Rol = ((OpcionCombo)cborol.SelectedItem).Texto.ToString(),
                        EstadoValor = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor),
                        Estado = ((OpcionCombo)cboestado.SelectedItem).Texto.ToString()
                    });

                    Limpiar();
                    MessageBox.Show("Usuario registrado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                bool resultado = new UsuarioService().Editar(objusuario, out mensaje);

                if (resultado)
                {
                    var usuario = usuariosGrid.FirstOrDefault(u => u.Id == objusuario.IdUsuario);
                    if (usuario != null)
                    {
                        usuario.Documento = txtdocumento.Text;
                        usuario.NombreCompleto = txtnombrecompleto.Text;
                        usuario.Correo = txtcorreo.Text;
                        usuario.Clave = txtclave.Password;
                        usuario.IdRol = Convert.ToInt32(((OpcionCombo)cborol.SelectedItem).Valor);
                        usuario.Rol = ((OpcionCombo)cborol.SelectedItem).Texto.ToString();
                        usuario.EstadoValor = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor);
                        usuario.Estado = ((OpcionCombo)cboestado.SelectedItem).Texto.ToString();
                    }

                    Limpiar();
                    MessageBox.Show("Usuario actualizado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
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
            txtnombrecompleto.Text = "";
            txtcorreo.Text = "";
            txtclave.Password = "";
            txtconfirmarclave.Password = "";
            cborol.SelectedIndex = 0;
            cboestado.SelectedIndex = 0;

            txtdocumento.Focus();
        }

        private void btnseleccionar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            UsuarioGrid usuario = btn.DataContext as UsuarioGrid;

            if (usuario != null)
            {
                txtid.Text = usuario.Id.ToString();
                txtdocumento.Text = usuario.Documento;
                txtnombrecompleto.Text = usuario.NombreCompleto;
                txtcorreo.Text = usuario.Correo;
                txtclave.Password = usuario.Clave;
                txtconfirmarclave.Password = usuario.Clave;

                // Seleccionar rol
                foreach (OpcionCombo oc in cborol.Items)
                {
                    if (Convert.ToInt32(oc.Valor) == usuario.IdRol)
                    {
                        cborol.SelectedItem = oc;
                        break;
                    }
                }

                // Seleccionar estado
                foreach (OpcionCombo oc in cboestado.Items)
                {
                    if (Convert.ToInt32(oc.Valor) == usuario.EstadoValor)
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
                if (MessageBox.Show("¿Desea eliminar el usuario?", "Mensaje", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    string mensaje = string.Empty;
                    Usuario objusuario = new Usuario()
                    {
                        IdUsuario = Convert.ToInt32(txtid.Text)
                    };

                    bool respuesta = new UsuarioService().Eliminar(objusuario, out mensaje);

                    if (respuesta)
                    {
                        var usuario = usuariosGrid.FirstOrDefault(u => u.Id == objusuario.IdUsuario);
                        if (usuario != null)
                        {
                            usuariosGrid.Remove(usuario);
                        }
                        Limpiar();
                        MessageBox.Show("Usuario eliminado correctamente", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un usuario", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void btnbuscar_Click(object sender, RoutedEventArgs e)
        {
            string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();
            string textoBusqueda = txtbusqueda.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(textoBusqueda))
            {
                dgvdata.ItemsSource = usuariosGrid;
                return;
            }

            var usuariosFiltrados = usuariosGrid.Where(u =>
            {
                switch (columnaFiltro)
                {
                    case "Documento":
                        return u.Documento.ToUpper().Contains(textoBusqueda);
                    case "NombreCompleto":
                        return u.NombreCompleto.ToUpper().Contains(textoBusqueda);
                    case "Correo":
                        return u.Correo.ToUpper().Contains(textoBusqueda);
                    case "Rol":
                        return u.Rol.ToUpper().Contains(textoBusqueda);
                    case "Estado":
                        return u.Estado.ToUpper().Contains(textoBusqueda);
                    default:
                        return false;
                }
            }).ToList();

            dgvdata.ItemsSource = usuariosFiltrados;
        }

        private void btnlimpiarbuscador_Click(object sender, RoutedEventArgs e)
        {
            txtbusqueda.Text = "";
            dgvdata.ItemsSource = usuariosGrid;
        }

        private void btnlimpiar_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }

        private void btnsalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class UsuarioGrid
    {
        public int Id { get; set; }
        public string Documento { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Clave { get; set; }
        public int IdRol { get; set; }
        public string Rol { get; set; }
        public int EstadoValor { get; set; }
        public string Estado { get; set; }
    }
}