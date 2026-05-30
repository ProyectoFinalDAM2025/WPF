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

namespace app.View.Usuarios.Notificaciones
{
    /// <summary>
    /// Lógica de interacción para Notificacion.xaml
    /// </summary>
    public partial class Notificacion : Window
    {

        public string header;
        public string content;

        public Notificacion(string header, string content)
        {
            InitializeComponent();
            this.header = header;
            this.content = content;

        }

        private void BtnCorreoEnviado_Click(object sender, RoutedEventArgs e)
        {


            this.Close();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtHeader.Text = header;
            txtContent.Text = content;
        }
    }
}
