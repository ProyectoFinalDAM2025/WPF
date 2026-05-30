using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using app.ViewModel.Usuarios;

namespace app.View.Usuarios.LogIn.CustomControls
{
    /// <summary>
    /// Lógica de interacción para BindablePasswordBox.xaml
    /// </summary>
    public partial class BindablePasswordBox : UserControl
    {
       
        //Para la contraseña se difine un dependencia estatica y solo de lectura (public static readonly DependencyProperty PasswordProperty;)
        //Despues se registra como propiedad de dependencia
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(SecureString), typeof(BindablePasswordBox));
        //Como primer parametro se envia el nombre la propiedad que se quiere registrar "PasswordBindable"
        //Como segunto el tipo de la propiedad
        //Como tercer parametro el tipo de dato del propietario de la propiedad, que es el control de usuario de "BindablePasswordBox"

        public SecureString Password
        {
            //Se devuelve el valor de la dependencia.
            get { return (SecureString)GetValue(PasswordProperty); }
            //Se estable el valor de propiedad de la dependencia
            set { SetValue(PasswordProperty, value); }
        }

        public BindablePasswordBox()
        {
            InitializeComponent();
            //Como ultimo se necesita establcer el valor del cuadro de contraseña estandar en la nueva propiedad de contraseña
            //Se subscribe el evento para generar el cambio cada vez que cambie la contraseña
            PasswordBoxEmail.PasswordChanged += OnPasswordChange;
 
        }

        private void OnPasswordChange(object sender, RoutedEventArgs e)
        {
              
            Password = PasswordBoxEmail.SecurePassword;
        }

        
    }
}
