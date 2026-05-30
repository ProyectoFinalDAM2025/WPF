using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using app.View.Usuarios.InicioDeSesion;
using app.View.Usuarios.Notificaciones;
using app.ViewModel.Usuarios;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace app.View.Usuarios.RecordarContraseñas
{
    /// <summary>
    /// Lógica de interacción para RecordarContraseña.xaml
    /// </summary>
    public partial class RecordarContraseña : Window
    {
        private readonly UsuarioViewModel  _viewModel;
        public RecordarContraseña()
        {
            InitializeComponent();
            _viewModel = UsuarioViewModel.Instance;
            DataContext = _viewModel;
        }

        private async void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            btnEnviar.IsEnabled = false;
            HttpResponseMessage response = await _viewModel.RecuperarPassword(txtEmail.Text.Trim());

            if (response == null)
            {
                MostrarNotificacion("Error", SettingsData.Default._503);
                btnEnviar.IsEnabled = true;
            }
            else if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<JObject>(content);
                MostrarNotificacion(
                    ObtenerTexto(result, "ReasonPhrase", "reasonPhrase", "header") ?? "Contraseña recuperada",
                    ObtenerMensaje(result) ?? "Se ha enviado una nueva contraseña al correo indicado."
                );
                InicioDeSesion.LogIn log = new InicioDeSesion.LogIn();
                log.Show();
                this.Close();
            }
            else {
                var contentErrorr = await response.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<JObject>(contentErrorr);
                MostrarNotificacion(
                    ObtenerTexto(error, "ReasonPhrase", "reasonPhrase", "header") ?? "Error",
                    ObtenerMensaje(error) ?? "No se pudo recuperar la contraseña."
                );
                btnEnviar.IsEnabled = true;
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            InicioDeSesion.LogIn log = new InicioDeSesion.LogIn();
            log.Show();
            this.Close();
        }

        private void TextBoxEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidarEmailUsuario();

        }

        private void TextBoxEmailConfirmar_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidarEmailUsuario();
        }

        private void ValidarEmailUsuario()
        {
            // Obtengo los valores ingresados en los campos de texto para email
            string email = txtEmail.Text;
            string emailConfirmar = txtEmailConfirmar.Text;
            

            // Valido el email
            bool isEmailValid = IsValidEmail(email);
            bool isEmailConfirmVaid = IsValidEmail(emailConfirmar);
            bool isEquals = txtEmail.Text == emailConfirmar;    

            // Muestro u oculto el mensaje de error para el campos dependiendo de su validez
            ErrorTextEmail.Visibility = isEmailValid ? Visibility.Collapsed : Visibility.Visible;
            ErrorTextEmailConfirmar.Visibility = isEmailConfirmVaid ? Visibility.Collapsed : Visibility.Visible;
            ErrorTextEquals.Visibility = isEquals ? Visibility.Collapsed : Visibility.Visible;

            // Habilito el botón de inicio de sesión solo si ambos campos son válidos
            btnEnviar.IsEnabled = isEmailValid && isEmailConfirmVaid && isEquals;
        }

        private void MostrarNotificacion(string titulo, string mensaje)
        {
            Notificacion notificacion = new Notificacion(titulo, mensaje);
            notificacion.Owner = this;
            notificacion.ShowDialog();
        }

        private string ObtenerMensaje(JObject json)
        {
            JToken mensajeToken = json?["Message"]
                ?? json?["message"]
                ?? json?["Content"]
                ?? json?["content"];

            if (mensajeToken == null)
                return null;

            return mensajeToken.Type == JTokenType.Array
                ? string.Join(Environment.NewLine, mensajeToken.Select(item => item.ToString()))
                : mensajeToken.ToString();
        }

        private string ObtenerTexto(JObject json, params string[] keys)
        {
            if (json == null)
                return null;

            foreach (string key in keys)
            {
                string value = json[key]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }

        public bool IsValidEmail(string email)
        {

            //Si no es nula regreso false
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Expresión regular para validar un email, se agrega using System.Text.RegularExpressions;
            //Explicación del patter al final del documento.
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

    }
}
