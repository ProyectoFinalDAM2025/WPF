using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using app.Models.Usuarios;
using app.View.Usuarios.Notificaciones;
using app.ViewModel.Usuarios;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace app.View.Usuarios.CambiarContraseña
{
    /// <summary>
    /// Lógica de interacción para CambiarContraseña.xaml
    /// </summary>
    public partial class CambiarContraseña : Window
    {
        public readonly UsuarioViewModel _viewModel;

        UsuarioBase perfilEdita;
        string IDUsuario;
        Usuario usuarioEdita;
        public CambiarContraseña()
        {
            InitializeComponent();
            _viewModel = UsuarioViewModel.Instance; 
            DataContext = _viewModel;
            BindingExpression bindingPassword = txtPassword.GetBindingExpression(TextBox.TextProperty);
            BindingExpression bindingPasswordConfirm = txtPasswordConfirmacion.GetBindingExpression(TextBox.TextProperty);
            if (bindingPassword != null)
                bindingPassword?.UpdateSource();
            if (bindingPasswordConfirm != null)
                bindingPasswordConfirm?.UpdateSource();
            ValidPasswords();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            perfilEdita = _viewModel.AllPerfiles.FirstOrDefault(item => item._id == SettingsData.Default.idPerfil);
            IDUsuario = perfilEdita?.idUsuario;
            usuarioEdita = !string.IsNullOrEmpty(IDUsuario)
                ? _viewModel.AllUsers.FirstOrDefault(item => item._id == IDUsuario)
                : null;

            string emailSesion = SettingsData.Default.nombre;

            txtNombre.Text = !string.IsNullOrEmpty(perfilEdita?.nombre)
                ? $"{perfilEdita.nombre} {perfilEdita.apellido}".Trim()
                : emailSesion;
           
            txtEmail.Text = usuarioEdita?.email ?? emailSesion;
            string imageUrl = perfilEdita?.rutaFoto; // Ruta de la API

            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            BitmapImage bitmap = new BitmapImage();

            try
            {
                // Cargar la imagen desde la URL
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Asegura que la imagen se carga completamente
                bitmap.EndInit();

                // Crear un ImageBrush y asignarlo al Ellipse
                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = bitmap;
                imageBrush.Stretch = Stretch.UniformToFill; // Asegura que la imagen llena el Ellipse correctamente
                imgPerfil.Fill = imageBrush;
                //Guardo la imagen en una variable para despues usarla


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.Passwordd = "";
            _viewModel.PasswordConfirm = "";
            _viewModel.PasswordConfirm2 = "";
            txtPasswordActual.Text = "";
            this.Close();
        }

        private async void btnCambiar_Click(object sender, RoutedEventArgs e)
        {
            btnCambiar.IsEnabled = false;

            HttpResponseMessage response = await _viewModel.CambiarPassword(txtPasswordActual.Text, txtPassword.Text);
            if (response == null){

                Notificacion not = new Notificacion("Error de conexión.", "Por favor revise  su conexión al servidor.");
                not.Owner = this;
                not.ShowDialog();
                LimpiarCamposPassword();

            }
            else if (response.IsSuccessStatusCode)
            {
                var result200= await response.Content.ReadAsStringAsync();
                JObject respuesta = JsonConvert.DeserializeObject<JObject>(result200);
                Notificacion not = new Notificacion(
                    respuesta?["ReasonPhrase"]?.ToString() ?? "Contraseña actualizada",
                    ObtenerMensajeServidor(respuesta, "La contraseña se ha actualizado correctamente.")
                );
                not.Owner = this;
                not.ShowDialog();

                LimpiarCamposPassword();
                this.Close();
               
            }
            else {
                var resultError = await response.Content.ReadAsStringAsync();
                JObject error = JsonConvert.DeserializeObject<JObject>(resultError);
                Notificacion not = new Notificacion(
                    error?["ReasonPhrase"]?.ToString() ?? "Error",
                    ObtenerMensajeServidor(error, resultError)
                );
                not.Owner = this;
                not.ShowDialog();
                LimpiarCamposPassword();
            }
        }

        private void txtPasswordActual_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidPasswords();
        }

        private void txtPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidPasswords();
        }

        private void txtPasswordConfirmacion_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidPasswords();
        }

        public void ValidPasswords(){
            if (txtPasswordActual == null || txtPassword == null || txtPasswordConfirmacion == null || btnCambiar == null)
                return;

            bool errores = Validation.GetHasError(txtPassword) || Validation.GetHasError(txtPasswordConfirmacion);
            bool camposVacios = string.IsNullOrWhiteSpace(txtPasswordActual.Text) ||
                                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                                string.IsNullOrWhiteSpace(txtPasswordConfirmacion.Text);

            btnCambiar.IsEnabled = !errores && !camposVacios;
        }

        private void LimpiarCamposPassword()
        {
            txtPasswordActual.Text = "";
            txtPassword.Text = "";
            txtPasswordConfirmacion.Text = "";

            _viewModel.Passwordd = "";
            _viewModel.PasswordConfirm = "";
            _viewModel.PasswordConfirm2 = "";

            txtPassword.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            txtPasswordConfirmacion.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            ValidPasswords();
        }

        private string ObtenerMensajeServidor(JObject json, string contenidoOriginal)
        {
            JToken mensaje = json?["Message"] ?? json?["message"] ?? json?["Content"] ?? json?["content"];

            if (mensaje == null)
                return contenidoOriginal;

            return mensaje.Type == JTokenType.Array
                ? string.Join(Environment.NewLine, mensaje.Select(item => item.ToString()))
                : mensaje.ToString();
        }
    }
}
