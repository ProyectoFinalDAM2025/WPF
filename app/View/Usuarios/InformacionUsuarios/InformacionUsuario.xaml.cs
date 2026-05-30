using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using app.Models.Usuarios;
using app.Models.Usuarios.Perfiles;
using app.ViewModel.Usuarios;

namespace app.View.Usuarios.InformacionUsuarios
{
    /// <summary>
    /// Lógica de interacción para InformacionUsuario.xaml
    /// </summary>
    public partial class InformacionUsuario : Window
    {
        private const string ImagenPerfilPorDefecto = "/View/Usuarios/InformacionUsuarios/58ffe72b95350c2b3440659d5f9631ce.png";

        private readonly UsuarioViewModel _viewModel;
        private readonly string ID;

        public InformacionUsuario(string id, UsuarioViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            ID = id;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UsuarioBase infoPerfil = _viewModel.AllPerfiles.FirstOrDefault(item => item._id == ID);

            if (infoPerfil == null)
            {
                MessageBox.Show("No se pudo encontrar la información del administrador seleccionado.", "Información no disponible", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            Administrador administrador = infoPerfil as Administrador;
            string nombre = infoPerfil.nombre ?? "";
            string apellido = infoPerfil.apellido ?? "";
            string nombreCompleto = $"{nombre} {apellido}".Trim();

            txtNombre.Text = nombre;
            txtApellido.Text = apellido;
            txtNombreCompleto.Text = string.IsNullOrWhiteSpace(nombreCompleto) ? "Administrador" : nombreCompleto;
            txtRol.Text = string.IsNullOrWhiteSpace(infoPerfil.rol) ? "Administrador" : infoPerfil.rol;
            txtEmail.Text = administrador?._nivelDeAcceso ?? "No disponible";
            txtEstado.Text = string.IsNullOrWhiteSpace(infoPerfil.registro) ? "No disponible" : infoPerfil.registro;
            txtIdAdministrador.Text = infoPerfil._id ?? "";
            txtIdUsuario.Text = infoPerfil.idUsuario ?? "";

            CargarImagenPerfil(infoPerfil.rutaFoto);
        }

        private void CargarImagenPerfil(string imageUrl)
        {
            ImageBrush imageBrush = new ImageBrush
            {
                Stretch = Stretch.UniformToFill,
                ImageSource = CrearBitmap(!string.IsNullOrWhiteSpace(imageUrl) ? imageUrl : ImagenPerfilPorDefecto)
            };

            imgPerfil.Fill = imageBrush;
            imgPerfilGrande.ImageSource = imageBrush.ImageSource;
        }

        private BitmapImage CrearBitmap(string ruta)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(ruta, ruta.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? UriKind.Absolute : UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            catch
            {
                return new BitmapImage(new Uri(ImagenPerfilPorDefecto, UriKind.Relative));
            }
        }
    }
}
