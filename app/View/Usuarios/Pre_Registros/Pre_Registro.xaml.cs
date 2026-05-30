using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using app.Models.Usuarios;
using app.View.Usuarios.Notificaciones;
using app.View.Usuarios.RegistroUsuarios;
using app.ViewModel.Usuarios;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace app.View.Usuarios.Pre_Registros
{
    /// <summary>
    /// Lógica de interacción para Pre_Registro.xaml
    /// </summary>
    public partial class Pre_Registro : Window
    {
        private readonly UsuarioViewModel _viewModel;
        public Pre_Registro()
        {
            InitializeComponent();
            _viewModel = UsuarioViewModel.Instance;
            DataContext = _viewModel;
            _viewModel.RolSeleccionado = "Administrador";
            txtRol.SelectedIndex = 0;

            BindingExpression bindingEmailConfirmacion = txtEmailConfirmar.GetBindingExpression(TextBox.TextProperty);
            if (bindingEmailConfirmacion != null)
                bindingEmailConfirmacion?.UpdateSource();

            BindingExpression bindingEmail = txtEmail.GetBindingExpression(TextBox.TextProperty);
            if (bindingEmail != null)
                bindingEmail?.UpdateSource();

            BindingExpression binding = txtRol.GetBindingExpression(ComboBox.SelectedItemProperty);
            if (binding != null)
                binding?.UpdateSource();  // Forzar la validación al cargar

            ValidateForm();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.EmailPreregistroConfirmacion2 = String.Empty;
            _viewModel.EmailPreregistroConfirmacion = String.Empty;
            _viewModel.EmailPreRegistro = String.Empty;
            _viewModel.RolSeleccionado = null;

            this.Close();
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void txtEmailConfirmar_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void txtRol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateForm();
        }

        public void ValidateForm() {
            if (txtEmail == null || txtEmailConfirmar == null || txtRol == null || btPre_Registro == null)
                return;

            bool errores = Validation.GetHasError(txtEmail) || Validation.GetHasError(txtEmailConfirmar) || Validation.GetHasError(txtRol);
            bool camposVacios = string.IsNullOrWhiteSpace(txtEmail.Text) ||
                                string.IsNullOrWhiteSpace(txtEmailConfirmar.Text) ||
                                txtRol.SelectedItem == null;

            btPre_Registro.IsEnabled = !errores && !camposVacios;
        }

        private async void btPre_Registro_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsData.Default.rol == "Administrador")
            {
                btPre_Registro.IsEnabled = false;

                Usuario preUsers = new Usuario
                {
                    email = txtEmail.Text
                };

                try
                {
                    var response = await _viewModel.PreRegistrarAdministrador(preUsers.email);
                    if (response == null)
                    {
                        MostrarNotificacion("Error de conexión", "No se pudo conectar con la API.");
                        return;
                    }

                    var content = await response.Content.ReadAsStringAsync();

                    Debug.WriteLine($"Status: {response.StatusCode}\n Content: {content}");

                    if (response.IsSuccessStatusCode)
                    {
                        MostrarNotificacion(
                            "Pre-registro creado",
                            $"Usuario: {preUsers.email}\nLa contraseña temporal y el código de verificación se han enviado."
                        );
                        this.Close();
                    }
                    else
                    {
                        JObject error = JsonConvert.DeserializeObject<JObject>(content);
                        MostrarNotificacion(
                            error?["ReasonPhrase"]?.ToString() ?? "Error",
                            ObtenerMensajeServidor(error, content)
                        );
                        btPre_Registro.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MostrarNotificacion("Error", $"WPF : Ocurrió un error al cargar los datos : {ex.Message}");
                    btPre_Registro.IsEnabled = true;
                }
                finally
                {
                    _viewModel.EmailPreregistroConfirmacion2 = String.Empty;
                    _viewModel.EmailPreregistroConfirmacion = String.Empty;
                    _viewModel.EmailPreRegistro = String.Empty;
                    _viewModel.RolSeleccionado = null;
                    btPre_Registro.IsEnabled = true;
                }
            }
            else {
                MostrarNotificacion("Error de permisos", "No tienes la autorización requerida.");
            }
            

            
        }

        private void MostrarNotificacion(string header, string content)
        {
            Notificacion notificacion = new Notificacion(header, content);
            notificacion.Owner = this;
            notificacion.ShowDialog();
        }

        private string ObtenerMensajeServidor(JObject error, string contenidoOriginal)
        {
            JToken mensaje = error?["Message"] ?? error?["message"] ?? error?["Content"] ?? error?["content"];

            if (mensaje == null)
                return contenidoOriginal;

            return mensaje.Type == JTokenType.Array
                ? string.Join(Environment.NewLine, mensaje.Select(item => item.ToString()))
                : mensaje.ToString();
        }
    }
}
