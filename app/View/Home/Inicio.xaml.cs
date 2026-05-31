using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using app.View.Usuarios.InicioDeSesion;
using app.View.Usuarios.MainUsuarios;
using app.ViewModel.Usuarios;
using IntermodularWPF;
using app.View.Reportes.ReportesOfertas;
using app.View.Reportes.ReportesPublicaciones;

namespace app.View.Home
{
    /// <summary>
    /// Lógica de interacción para Inicio.xaml
    /// </summary>
    public partial class Inicio : Window
    {
        private readonly UsuarioViewModel _viewModel;
        public Inicio()
        {
            InitializeComponent();
            _viewModel = UsuarioViewModel.Instance;
            DataContext = _viewModel;
        }

        private void btnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            MainUsuario mainUsuario = new MainUsuario();
            mainUsuario.Show();
            this.Close();
        }

        private void btnReportesOfertas_Click(object sender, RoutedEventArgs e)
        {
            ReportesOfertas reportesOfertas = new ReportesOfertas();
            reportesOfertas.Show();
            this.Close();
        }

        private void btnReportesPublicaciones_Click(object sender, RoutedEventArgs e)
        {
            ReportesPublicaciones reportesPublicaciones = new ReportesPublicaciones();
            reportesPublicaciones.Show();
            this.Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var respose = await _viewModel.AccessToken();

            MessageBox.Show("Respuesta de verificacion: " + respose);

            if (respose == true)
            {
                MessageBox.Show("Hay token valido continua la session.");
                return;
            }
            else
            {
                //SettingsData.Default.token = "";
                //SettingsData.Default.appToken = "";
                //SettingsData.Default.idPerfil = "";
                //SettingsData.Default.Save();
                MessageBox.Show("Se borro el SettingsData debe de ir a login:  " + "\nToken: " + SettingsData.Default.token + "\nAppToken: " + SettingsData.Default.appToken + "\nID Perfil:" + SettingsData.Default.idPerfil);
                //Notificacion notInicio = new Notificacion("Session limada", "Por favor inicie limada de sessión");
                //notInicio.Owner = this;
                //notInicio.ShowDialog();
                LogIn log = new LogIn();
                log.Show();
                this.Close();
            }
        }
    }
}
