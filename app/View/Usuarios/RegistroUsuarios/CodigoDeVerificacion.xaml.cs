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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using app.Models.Usuarios;
using app.View.Usuarios.Notificaciones;
using app.ViewModel.Usuarios;
using Newtonsoft.Json;

namespace app.View.Usuarios.RegistroUsuarios
{
    /// <summary>
    /// Lógica de interacción para CodigoDeVerificacion.xaml
    /// </summary>
    public partial class CodigoDeVerificacion : Window
    {
        private readonly UsuarioViewModel _viewModel;
        public string Email { get; set; }
        public string ID { get; set; }
        public string EmailApp { get; set; }

        public string Privileges { get; set; } 
        public string TemporalToken { get; set; }
        public bool EsRegistroInmediato { get; set; }
        public string PermisosEspeciales { get; set; }
        public bool Activo { get; set; } = true;
        public byte[] ImagenAdministradorBytes { get; set; }
        public CodigoDeVerificacion()
        {
            InitializeComponent();
            _viewModel = UsuarioViewModel.Instance;
            DataContext = _viewModel;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Asignar valores a los campos de texto al cargar el diálogo
            TextEmail.Text = Email;
        }


        private void txtCodigo_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateCode();
        }
        private void ValidateCode()
        {
            
            string codigo = txtCodigo.Text;
          

            // Valido 
            bool isCodeValid = IsValidCode(codigo);
          

            // Muestro u oculto el mensaje de error para el campos dependiendo de su validez
            ErrorTextCodigo.Visibility = isCodeValid ? Visibility.Collapsed : Visibility.Visible;

            // Habilito el botón 
            btnEnviar.IsEnabled = isCodeValid;
        }

        public bool IsValidCode(string codigo)
        {
            
            //Si no es nula regreso false
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            if (!int.TryParse(codigo, out int val)) 
                return false;

            if (codigo.Length <= 5)
                return false;

            return true;
        }

        private async void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            Usuario usuario = new Usuario
            {
                email = Email,
                verificationCode = txtCodigo.Text

            };

            string tokenSesion = SettingsData.Default.token;
            if (!string.IsNullOrEmpty(TemporalToken))
                SettingsData.Default.token = TemporalToken;

            var response = await _viewModel.ValidarUsuario(usuario);

            SettingsData.Default.token = tokenSesion;

            if (response == null)
            {
                MostrarNotificacion("Error de conexión", "No se pudo conectar con la API.");
                return;
            }

            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (EsRegistroInmediato) {
                    var usuarioActual = await _viewModel.ObtenerUsuarioActual(TemporalToken);
                    string idUsuario = usuarioActual?["IDUsuario"]?.ToString();

                    if (string.IsNullOrEmpty(idUsuario))
                    {
                        MostrarNotificacion("Error", "No se pudo obtener el usuario verificado desde la API.");
                        return;
                    }

                    RegistroPerfil perfil = new RegistroPerfil(){
                        EmailApp = Email,
                        IdUsuario = idUsuario,
                        Privileges = "Administrador",
                        TemporalToken = TemporalToken,
                        EsRegistroInmediato = true,
                        PermisosEspeciales = PermisosEspeciales,
                        Activo = Activo,
                        ImagenAdministradorBytes = ImagenAdministradorBytes
                    };
                    this.DialogResult = true;

                    perfil.Owner = this.Owner;
                    this.Hide();
                   
                    perfil.ShowDialog();
                    
                    this.Close();
                }
                else {
                    MostrarNotificacion("Usuario verificado", "El usuario ya puede iniciar sesión para completar su perfil.");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            else 
            {
                var error = JsonConvert.DeserializeObject<dynamic>(result);
                MostrarNotificacion(error?.ReasonPhrase?.ToString() ?? "Error", error?.Message?.ToString() ?? result);
            }

         
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {

            this.DialogResult = false;
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
