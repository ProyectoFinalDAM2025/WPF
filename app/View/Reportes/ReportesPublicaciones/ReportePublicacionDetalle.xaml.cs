using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using app.Models.Reportes;

namespace app.View.Reportes.ReportesPublicaciones
{
    public partial class ReportePublicacionDetalle : Window
    {
        private readonly Reporte _reporte;
        private bool _videoConfigurado;
        public string AccionSolicitada { get; private set; }

        public ReportePublicacionDetalle(Reporte reporte)
        {
            InitializeComponent();
            _reporte = reporte;
            DataContext = reporte;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigurarDocumento();
        }

        private void ConfigurarDocumento()
        {
            _videoConfigurado = false;
            imgDocumento.Visibility = Visibility.Collapsed;
            panelVideo.Visibility = Visibility.Collapsed;
            pdfViewer.Visibility = Visibility.Collapsed;
            txtSinDocumento.Visibility = Visibility.Collapsed;
            btnAbrirDocumento.Visibility = string.IsNullOrWhiteSpace(_reporte.PublicacionArchivo)
                ? Visibility.Collapsed
                : Visibility.Visible;

            string tipo = (_reporte.PublicacionTipoArchivo ?? "").Trim();

            if (string.IsNullOrWhiteSpace(_reporte.PublicacionArchivo))
            {
                txtSinDocumento.Visibility = Visibility.Visible;
                return;
            }

            if (tipo.Equals("Foto", StringComparison.OrdinalIgnoreCase))
            {
                imgDocumento.Source = new BitmapImage(new Uri(!string.IsNullOrWhiteSpace(_reporte.PublicacionPreview)
                    ? _reporte.PublicacionPreview
                    : _reporte.PublicacionArchivo, UriKind.RelativeOrAbsolute));
                imgDocumento.Visibility = Visibility.Visible;
                return;
            }

            if (tipo.Equals("Video", StringComparison.OrdinalIgnoreCase))
            {
                mediaVideo.Source = new Uri(_reporte.PublicacionArchivo, UriKind.RelativeOrAbsolute);
                _videoConfigurado = true;
                panelVideo.Visibility = Visibility.Visible;
                return;
            }

            if (tipo.Equals("PDF", StringComparison.OrdinalIgnoreCase))
            {
                txtSinDocumento.Text = "PDF disponible. Usa el boton Abrir para verlo.";
                txtSinDocumento.Visibility = Visibility.Visible;
                return;
            }

            txtSinDocumento.Visibility = Visibility.Visible;
        }

        private void AbrirDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_reporte.PublicacionArchivo))
                return;

            try
            {
                Process.Start(new ProcessStartInfo(_reporte.PublicacionArchivo) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo abrir el documento.\nDetalle: " + ex.Message,
                    "Documento",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void Moderar_Click(object sender, RoutedEventArgs e)
        {
            AccionSolicitada = "Moderar";
            LiberarVideo();
            DialogResult = true;
            Close();
        }

        private void EliminarReporte_Click(object sender, RoutedEventArgs e)
        {
            AccionSolicitada = "Eliminar";
            LiberarVideo();
            DialogResult = true;
            Close();
        }

        private void Reproducir_Click(object sender, RoutedEventArgs e)
        {
            if (!_videoConfigurado || mediaVideo == null || mediaVideo.Source == null)
                return;

            mediaVideo.Play();
        }

        private void Pausar_Click(object sender, RoutedEventArgs e)
        {
            if (!_videoConfigurado || mediaVideo == null || mediaVideo.Source == null)
                return;

            mediaVideo.Pause();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            LiberarVideo();
            Close();
        }

        private void LiberarVideo()
        {
            if (!_videoConfigurado)
                return;

            try
            {
                if (mediaVideo != null)
                {
                    mediaVideo.Pause();
                    mediaVideo.Source = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error liberando video del reporte: " + ex.Message);
            }
            finally
            {
                _videoConfigurado = false;
            }
        }
    }
}
