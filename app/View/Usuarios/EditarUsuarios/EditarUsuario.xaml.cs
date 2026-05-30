using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using app.Models.Usuarios;
using app.View.Usuarios.Notificaciones;
using app.ViewModel.Usuarios;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace app.View.Usuarios.EditarUsuarios
{
    public partial class EditarUsuario : Window
    {
        private readonly UsuarioViewModel _viewModel;
        private readonly string ID;
        private readonly string ROL;
        private UsuarioBase usuarioEdita;
        private byte[] imagenCargadaBytes;
        private bool isImgChange;

        public EditarUsuario(string id, string rol, UsuarioViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            ID = id;
            ROL = rol;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            usuarioEdita = _viewModel.AllPerfiles.FirstOrDefault(item => item._id == ID);

            if (usuarioEdita == null)
            {
                MostrarNotificacion("Error", "No se encontro el administrador seleccionado.");
                Close();
                return;
            }

            txtNombre.Text = usuarioEdita.nombre;
            txtApellidos.Text = usuarioEdita.apellido;
            txtRol.SelectedIndex = 0;
            txtEmail.Text = usuarioEdita is app.Models.Usuarios.Perfiles.Administrador administrador
                ? administrador._nivelDeAcceso
                : "";
            chkActivo.IsChecked = usuarioEdita.registro == "Activo";

            CargarFotoActual(usuarioEdita.rutaFoto);
            ValidateForm();
        }

        private void CargarFotoActual(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                miEllipse.Fill = new ImageBrush(bitmap);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    imagenCargadaBytes = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar imagen de administrador: {ex.Message}");
            }
        }

        private void txtNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void txtApellidos_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void txtRol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateForm();
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void chkActivo_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ValidateForm();
        }

        private void btnCargarImg_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".png",
                Filter = "Archivos de imagen (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    imagenCargadaBytes = File.ReadAllBytes(dlg.FileName);

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(dlg.FileName);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    miEllipse.Fill = new ImageBrush(bitmapImage);
                    isImgChange = true;
                    ValidateForm();
                }
                catch (Exception ex)
                {
                    MostrarNotificacion("Error", $"Ocurrio un error al cargar la imagen: {ex.Message}");
                }
            }
        }

        private void ValidateForm()
        {
            if (txtNombre == null || txtApellidos == null || txtEmail == null || txtRol == null || btnEditarPerfil == null)
                return;

            bool isNombreValid = IsValidField(txtNombre.Text);
            bool isApellidoValid = IsValidField(txtApellidos.Text);
            bool isEmailValid = IsValidEmail(txtEmail.Text);
            bool isRolValid = txtRol.SelectedItem != null;
            bool isFotoCargada = imagenCargadaBytes != null || !string.IsNullOrWhiteSpace(usuarioEdita?.rutaFoto);

            ErrorTextNombre.Visibility = isNombreValid ? Visibility.Collapsed : Visibility.Visible;
            ErrorTextApellidos.Visibility = isApellidoValid ? Visibility.Collapsed : Visibility.Visible;
            ErrorTextEmail.Visibility = isEmailValid ? Visibility.Collapsed : Visibility.Visible;
            ErrorTextRol.Visibility = isRolValid ? Visibility.Collapsed : Visibility.Visible;
            ErrorTextFoto.Visibility = isFotoCargada ? Visibility.Collapsed : Visibility.Visible;

            btnEditarPerfil.IsEnabled = isNombreValid && isApellidoValid && isEmailValid && isRolValid && isFotoCargada;
        }

        private bool IsValidField(string field)
        {
            return !string.IsNullOrWhiteSpace(field) && field.Trim().Length > 2;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private async void btnEditarPerfil_Click(object sender, RoutedEventArgs e)
        {
            MultipartFormDataContent formContent = new MultipartFormDataContent();

            bool activo = chkActivo.IsChecked == true;
            bool activoOriginal = usuarioEdita.registro == "Activo";

            if (txtNombre.Text.Trim() != usuarioEdita.nombre)
                formContent.Add(new StringContent(txtNombre.Text.Trim()), "Nombre");

            if (txtApellidos.Text.Trim() != usuarioEdita.apellido)
                formContent.Add(new StringContent(txtApellidos.Text.Trim()), "Apellido");

            if (activo != activoOriginal)
                formContent.Add(new StringContent(activo ? "1" : "0"), "Activo");

            if (isImgChange)
            {
                ByteArrayContent imagenContent = new ByteArrayContent(imagenCargadaBytes);
                imagenContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                formContent.Add(imagenContent, "FotoPerfil", "foto-perfil.jpg");
            }

            if (!formContent.Any())
            {
                MostrarNotificacion("Sin cambios", "No has modificado ningun dato del administrador.");
                return;
            }

            try
            {
                HttpResponseMessage respuesta = await _viewModel.EditarPerfil(ID, ROL, formContent);

                if (respuesta == null)
                {
                    MostrarNotificacion("Error de conexion", "No se pudo conectar con la API.");
                    return;
                }

                string respuestaContenido = await respuesta.Content.ReadAsStringAsync();

                if (respuesta.IsSuccessStatusCode)
                {
                    JObject json = JsonConvert.DeserializeObject<JObject>(respuestaContenido);
                    MostrarNotificacion(
                        json?["ReasonPhrase"]?.ToString() ?? "Admin actualizado",
                        ObtenerMensajeServidor(json, "El administrador se ha actualizado correctamente.")
                    );

                    await _viewModel.CargarTodosLosUsuarios();
                    Close();
                }
                else
                {
                    JObject json = JsonConvert.DeserializeObject<JObject>(respuestaContenido);
                    MostrarNotificacion(
                        json?["ReasonPhrase"]?.ToString() ?? "Error",
                        ObtenerMensajeServidor(json, respuestaContenido)
                    );
                }
            }
            catch (Exception ex)
            {
                MostrarNotificacion("Error", $"Ocurrio un error al editar el administrador: {ex.Message}");
            }
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

        private void MostrarNotificacion(string header, string content)
        {
            Notificacion notificacion = new Notificacion(header, content);
            notificacion.Owner = this;
            notificacion.ShowDialog();
        }
    }
}
