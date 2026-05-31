using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using app.View.Home;
using app.View.Usuarios.InicioDeSesion;
using app.View.Usuarios.MainUsuarios;
using app.ViewModel.Reportes;
using IntermodularWPF;

namespace app.View.Reportes.ReportesOfertas
{
    public partial class ReportesOfertas : Window
    {
        private readonly ReportesViewModel _viewModel;

        public ReportesOfertas()
        {
            InitializeComponent();
            _viewModel = new ReportesViewModel();
            DataContext = _viewModel;
            txtUsuarioRol.Text = SettingsData.Default.rol;
            txtUsuarioSession.Text = SettingsData.Default.nombre;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string resultado = await _viewModel.CargarReportesPorTipo("Oferta");
            if (resultado == SettingsData.Default._200)
                return;

            if (resultado == SettingsData.Default._419)
            {
                _viewModel.MostrarError("Session Terminada", "Por favor inicie session.");
                LogIn log = new LogIn();
                log.Show();
                Close();
                return;
            }

            _viewModel.MostrarError("Error", resultado);
        }

        private void ToggleButtonCambiarLista_Checked(object sender, RoutedEventArgs e)
        {
            DataGridReportes.Visibility = Visibility.Collapsed;
            ListViewReportes.Visibility = Visibility.Visible;
            btnToggleButtonCambiarLista.Content = "ListView";
            imgToggleButton.Source = new BitmapImage(new Uri("/View/Usuarios/MainUsuarios/imgListView.png", UriKind.RelativeOrAbsolute));
        }

        private void ToggleButtonCambiarLista_Unchecked(object sender, RoutedEventArgs e)
        {
            ListViewReportes.Visibility = Visibility.Collapsed;
            DataGridReportes.Visibility = Visibility.Visible;
            btnToggleButtonCambiarLista.Content = "DataGrid";
            imgToggleButton.Source = new BitmapImage(new Uri("/View/Usuarios/MainUsuarios/imgDataGrid.png", UriKind.RelativeOrAbsolute));
        }

        private void IrInicio_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Inicio inicio = new Inicio();
            inicio.Show();
            Close();
        }

        private void Admins_Click(object sender, RoutedEventArgs e)
        {
            MainUsuario mainUsuario = new MainUsuario();
            mainUsuario.Show();
            Close();
        }

        private void Publicaciones_Click(object sender, RoutedEventArgs e)
        {
            app.View.Reportes.ReportesPublicaciones.ReportesPublicaciones publicaciones = new app.View.Reportes.ReportesPublicaciones.ReportesPublicaciones();
            publicaciones.Show();
            Close();
        }
    }
}
