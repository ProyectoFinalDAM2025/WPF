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
using app.Models.Usuarios;
using app.View.Usuarios.Notificaciones;
using app.View.Usuarios.RegistroUsuarios;
using app.ViewModel.Usuarios;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace app.View.Usuarios.RegistroUsuarios
{
    /// <summary>
    /// Lógica de interacción para RegistroUsuario.xaml
    /// </summary>
    public partial class RegistroUsuario : Window
    {
        private readonly UsuarioViewModel _viewModel;
        public RegistroUsuario()
        {
            InitializeComponent();
            _viewModel = UsuarioViewModel.Instance;
            DataContext = _viewModel;
        }

        private void txtNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidarRegistroUsuario();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidarRegistroUsuario(); 
        }
        private void txtPasswordConfirmacion_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidarRegistroUsuario(); 
        }

        private void ValidarRegistroUsuario()
        {
            // Obtengo los valores ingresados en los campos de texto para email y contraseña
            string email = txtEmail.Text;
            string password = txtPassword.Password;
            string passwordConfirmacion = txtPasswordConfirmacion.Password;

            // Valido el email y la contraseña utilizando los métodos correspondientes
            bool isEmailValid = IsValidEmail(email);
            bool isPasswordValid = IsValidPassword(password);
            bool isPasswordValidConfirm = password == passwordConfirmacion;
        
            // Muestro u oculto el mensaje de error para el campos dependiendo de su validez
            ErrorTextEmail.Visibility = isEmailValid ? Visibility.Collapsed : Visibility.Visible;
            ErrorTextPassword.Visibility = isPasswordValid ? Visibility.Collapsed : Visibility.Visible;
            ErrorTextPasswordConfirmacion.Visibility = isPasswordValidConfirm ? Visibility.Collapsed : Visibility.Visible;

            btnEnviar.IsEnabled = isEmailValid && isPasswordValid && isPasswordValidConfirm;
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

        public bool IsValidPassword(string password)
        {
            //Si no es nula regreso false
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Verificar que la contraseña tenga al menos una letra mayúscula, una minúscula y un número
            //Explicación del patter al final del documento.
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$";
            return Regex.IsMatch(password, pattern);
        }

        private async void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            btnEnviar.IsEnabled = false;
            

            // Obtengo los valores ingresados en los campos de texto para email y contraseña
            string email = txtEmail.Text;
            string password = txtPassword.Password;

            try
            {
                Usuario registrarUsuario = new Usuario
                {
                    email = email,
                    password = password
                };

                var response = await _viewModel.RegistrarUsuario(registrarUsuario);
                if (response == null)
                {
                    MostrarNotificacion("Error de conexión", "No se pudo conectar con la API.");
                    return;
                }

                var result = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Menssage del registro  \nStatus: {response.StatusCode} \nContenido: {result}" );


                if (response.IsSuccessStatusCode)
                {

                    // Convertir la respuesta JSON a un objeto dynamic
                    dynamic responseData = JsonConvert.DeserializeObject<dynamic>(result);

                    if (responseData != null)
                    {
                        CodigoDeVerificacion codigo = new CodigoDeVerificacion()
                        {
                            Email = responseData.Data.email,
                            TemporalToken = responseData.Data.token,
                            EsRegistroInmediato = true
                        };
                        codigo.Owner = this.Owner;
                        this.Hide();
                        codigo.ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        MostrarNotificacion("Error", "La API no devolvió una respuesta válida.");
                        btnEnviar.IsEnabled = true;
                    }
                }
                else
                {
                    var error = JsonConvert.DeserializeObject<dynamic>(result);
                    MostrarNotificacion(error?.ReasonPhrase?.ToString() ?? "Error", error?.Message?.ToString() ?? result);
                    btnEnviar.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WPF : Ocurrió un error al cargar los datos : {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnEnviar.IsEnabled = true;
            }
            finally
            {
                btnEnviar.IsEnabled = true;
            }   


        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MostrarNotificacion(string header, string content)
        {
            Notificacion notificacion = new Notificacion(header, content);
            notificacion.Owner = this;
            notificacion.ShowDialog();
        }
    }
}
