using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using app.Models.Usuarios;
using app.View.Home;
using app.View.Usuarios.MainUsuarios;
using app.View.Usuarios.Notificaciones;
using app.View.Usuarios.RecordarContraseñas;
using app.View.Usuarios.RegistroUsuarios;
using app.ViewModel.Usuarios;
using IntermodularWPF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace app.View.Usuarios.InicioDeSesion
{
    /// <summary>
    /// Lógica de interacción para LogIn.xaml
    /// </summary>
    public partial class LogIn : Window
    {

        private readonly UsuarioViewModel _viewModel;
        public LogIn()
        {
            InitializeComponent();
            _viewModel = UsuarioViewModel.Instance;
            DataContext = _viewModel;
        }


        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordarContraseña recordar = new RecordarContraseña();
            recordar.Show();
            this.Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (SettingsData.Default.token != "")
            {

                var respose = await _viewModel.AccessToken();

                //MessageBox.Show("Respuesta de verificacion: "+respose);

                if (respose == true)
                {
                    Inicio init = new Inicio();
                    init.Show();
                    this.Close();
                }

            }
        }

        public void VoidViejoLogIn(){

        //            private void TextBoxEmail_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    //ValidateLogin();
        //}
        //private void PasswordBoxEmail_PasswordChanged(object sender, RoutedEventArgs e)
        //{
        //    //ValidateLogin();

            //}


        //public bool IsValidEmail(string email)
        //{

        //    //Si no es nula regreso false
        //    if (string.IsNullOrWhiteSpace(email))
        //        return false;

        //    // Expresión regular para validar un email, se agrega using System.Text.RegularExpressions;
        //    //Explicación del patter al final del documento.
        //    string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        //    return Regex.IsMatch(email, pattern);
        //}

        //public bool IsValidPassword(string password)
        //{
        //    //Si no es nula regreso false
        //    if (string.IsNullOrWhiteSpace(password))
        //        return false;

        //    // Verificar que la contraseña tenga al menos una letra mayúscula, una minúscula y un número
        //    //Explicación del patter al final del documento.
        //    string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$";
        //    return Regex.IsMatch(password, pattern);
        //}


        //private void ValidateLogin()
        //{
        //    // Obtengo los valores ingresados en los campos de texto para email y contraseña
        //    string email = TextBoxEmail.Text;
        //    //string password = PasswordBoxEmail.Password;

        //    // Valido el email y la contraseña utilizando los métodos correspondientes
        //    bool isEmailValid = IsValidEmail(email);
        //    //bool isPasswordValid = IsValidPassword(password);

        //    // Muestro u oculto el mensaje de error para el campos dependiendo de su validez
        //    //ErrorTextEmail.Visibility = isEmailValid ? Visibility.Collapsed : Visibility.Visible;
        //    //ErrorTextPassword.Visibility = isPasswordValid ? Visibility.Collapsed : Visibility.Visible;

        //    // Habilito el botón de inicio de sesión solo si ambos campos son válidos
        //    //BtnLogin.IsEnabled = isEmailValid && isPasswordValid;
        //}

        //private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        //{
        //    BtnLogin.IsEnabled = false;

        //    // Configurar los datos que se enviarán al servidor en el cuerpo de la solicitud
        //    Usuario data = new Usuario { email = TextBoxEmail.Text, /*password = PasswordBoxEmail.Password */};

        //    var response = await _viewModel.LogIn(data);


        //    if (response == null)
        //    {
        //        Notificacion not = new Notificacion("Error de conexión.", "Por favor revise  su conexión al servidor.");
        //        not.Owner = this;
        //        not.ShowDialog();
        //        BtnLogin.IsEnabled = true;
        //    }
        //    else if (response.IsSuccessStatusCode)
        //    {
        //        var result = await response.Content.ReadAsStringAsync();

        //        dynamic responseData = JsonConvert.DeserializeObject<dynamic>(result);

        //        if (responseData != null)
        //        {
        //            // Obtener los datos del usuario y mostrarlos en la pantalla principal

        //            if (responseData.data.privileges == "Administrador")
        //            {

        //                CodigoDeVerificacion code = new CodigoDeVerificacion
        //                {
        //                    Email = responseData.data.email,
        //                    EmailApp = responseData.data.emailApp,
        //                    ID = responseData.data.id,
        //                    Privileges = responseData.data.privileges
        //                };
        //                code.Owner = this;
        //                bool? resultCode = code.ShowDialog();

        //                MessageBox.Show("Result Code: " + resultCode.Value.ToString());

        //                if (resultCode == true)
        //                {
        //                    Inicio h = new Inicio();
        //                    h.Show();
        //                    this.Close();
        //                }
        //                else if (resultCode == false)
        //                {
        //                    MessageBox.Show("Se cancelo la verificación");
        //                }


        //            }
        //            else if (responseData.data.user != null)
        //            {
        //                UserSession.Instance.Token = responseData.data.token;
        //                UserSession.Instance.AppToken = responseData.data.appToken;
        //                UserSession.Instance.Data = JObject.FromObject(responseData.data.user);

        //                SettingsData.Default.token = responseData.data.token;
        //                SettingsData.Default.appToken = responseData.data.appToken;
        //                SettingsData.Default.idPerfil = responseData.data.user._id;
        //                SettingsData.Default.rol = responseData.data.user.rol;
        //                SettingsData.Default.nombre = responseData.data.user.nombre;
        //                SettingsData.Default.Save();



        //                Inicio user = new Inicio();
        //                user.Show();
        //                this.Close();
        //            }

        //            BtnLogin.IsEnabled = true;

        //        }
        //        else
        //        {
        //            var resultError = await response.Content.ReadAsStringAsync();
        //            var error = JsonConvert.DeserializeObject<Exception>(resultError);
        //            MessageBox.Show(error + " : WPF = ERROR 500");
        //            BtnLogin.IsEnabled = true;
        //        }
        //    }
        //    else
        //    {
        //        var resultError = await response.Content.ReadAsStringAsync();
        //        var error = JsonConvert.DeserializeObject<dynamic>(resultError);
        //        Notificacion not = new Notificacion(error.ReasonPhrase.ToString(), error.Content.ToString());
        //        not.Owner = this;
        //        not.ShowDialog();
        //        BtnLogin.IsEnabled = true;
        //    }
        //}
    }

    }


}
