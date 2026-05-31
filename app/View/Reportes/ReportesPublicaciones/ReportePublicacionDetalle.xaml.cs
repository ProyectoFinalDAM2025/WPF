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
                panelVideo.Visibility = Visibility.Visible;
                return;
            }

            if (tipo.Equals("PDF", StringComparison.OrdinalIgnoreCase))
            {
                pdfViewer.Visibility = Visibility.Visible;
                pdfViewer.Navigate(_reporte.PublicacionArchivo);
                return;
            }

            txtSinDocumento.Visibility = Visibility.Visible;
        }

        private void AbrirDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_reporte.PublicacionArchivo))
                return;

            Process.Start(new ProcessStartInfo(_reporte.PublicacionArchivo) { UseShellExecute = true });
        }

        private void Moderar_Click(object sender, RoutedEventArgs e)
        {
            AccionSolicitada = "Moderar";
            DialogResult = true;
            Close();
        }

        private void EliminarReporte_Click(object sender, RoutedEventArgs e)
        {
            AccionSolicitada = "Eliminar";
            DialogResult = true;
            Close();
        }

        private void Reproducir_Click(object sender, RoutedEventArgs e)
        {
            mediaVideo.Play();
        }

        private void Pausar_Click(object sender, RoutedEventArgs e)
        {
            mediaVideo.Pause();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mediaVideo.Stop();
            mediaVideo.Source = null;
        }
    }
}
