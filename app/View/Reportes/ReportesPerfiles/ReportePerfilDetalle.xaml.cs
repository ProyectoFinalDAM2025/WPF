using System.Windows;
using app.Models.Reportes;

namespace app.View.Reportes.ReportesPerfiles
{
    public partial class ReportePerfilDetalle : Window
    {
        public string AccionSolicitada { get; private set; }

        public ReportePerfilDetalle(Reporte reporte)
        {
            InitializeComponent();
            DataContext = reporte;
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

        private void EliminarPerfil_Click(object sender, RoutedEventArgs e)
        {
            AccionSolicitada = "EliminarPerfil";
            DialogResult = true;
            Close();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
