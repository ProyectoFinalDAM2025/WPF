using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using app.Models.Usuarios;
using app.Models.Usuarios.Perfiles;
using app.View.Usuarios.Notificaciones;
using IntermodularWPF;
using Newtonsoft.Json;
using app.Models.ApiRouteUsuario;
using System.Windows.Input;
using System.Security;
using app.Models.IUsuariosRepository;
using app.ViewModel.Repositories.RepositoryUsuarios;
using System.Threading;
using System.Security.Principal;
using app.View.Home;
using app.View.Usuarios.RegistroUsuarios;
using app.View.Usuarios.InicioDeSesion;
using Newtonsoft.Json.Linq;

namespace app.ViewModel.Usuarios
{
    public class UsuarioViewModel : ViewModelBase
    {

        //Variables-Binding LogIn para el inicio de sesion y propiedades de tipo comando que validaran el login
        private string _logInMail = "";
        private SecureString _password;
        private string _logInError = "";
        private bool _isViewVisible = true;
        private IUsuarioRepository  _usuarioRepository;
                
                //Estos comando se inician en el constructor del UsuarioViewModel                

        public ICommand LogInCommand { get; } //No se implenta set dado que solo la clase command deberia inicilizarla
        public ICommand DeleteCommand { get; }
        public string LogInMail { get => _logInMail; set { _logInMail = value; OnPropertyChanged("LogInMail"); } }
        public SecureString Password { get => _password; set { _password = value; OnPropertyChanged("Password"); } }
        public string LogInError { get => _logInError; set { _logInError = value; OnPropertyChanged("LogInError"); } }
        public bool IsViewVisible { get => _isViewVisible; set { _isViewVisible = value; OnPropertyChanged("IsViewVisible"); } }



        //Datos Pre-Registros ValidationRule
        private string _emailPreregistro = "";
        private string _emailPreregistroConfirmacion = "";
        private string _emailPreregistroConfirmacion2 = "";
        private string _rolSeleccionado = "";
        //Datos CambioPassword ValidationRule
        private string _passwordd = "";
        private string _passwordConfirm = "";
        private string _passwordConfirm2 = "";
        public string EmailPreRegistro { get => _emailPreregistro; set { _emailPreregistro = value; OnPropertyChanged("EmailPreRegistro"); } }
        public string EmailPreregistroConfirmacion { get => _emailPreregistroConfirmacion; set { _emailPreregistroConfirmacion = value; OnPropertyChanged("EmailPreregistroConfirmacion"); } }
        public string EmailPreregistroConfirmacion2 { get => _emailPreregistroConfirmacion2; set { _emailPreregistroConfirmacion2 = value; OnPropertyChanged("EmailPreregistroConfirmacion2"); } }
        public string RolSeleccionado { get => _rolSeleccionado; set  { _rolSeleccionado = value; OnPropertyChanged("RolSeleccionado"); } }
        public string Passwordd { get => _passwordd;  set { _passwordd = value; OnPropertyChanged("Passwordd"); } }
        public string PasswordConfirm { get => _passwordConfirm;  set { _passwordConfirm = value; OnPropertyChanged("PasswordConfirm"); } }
        public string PasswordConfirm2 { get => _passwordConfirm2;  set { _passwordConfirm2 = value; OnPropertyChanged("PasswordConfirm2"); } }



        private ObservableCollection<UsuarioBase> allPerfiles;

        private ObservableCollection<Usuario> allUsers;
        public ObservableCollection<UsuarioBase> AllPerfiles { get => allPerfiles; set { allPerfiles = value; OnPropertyChanged("AllPerfiles"); } }
        public ObservableCollection<Usuario> AllUsers { get => allUsers; set { allUsers = value; OnPropertyChanged("AllUsers"); } }

        private UsuarioBase usuarioSeleccionado;

        public UsuarioBase UsuarioSeleccionado { get => usuarioSeleccionado; set { usuarioSeleccionado = value; OnPropertyChanged("UsuarioSeleccionado"); CommandManager.InvalidateRequerySuggested(); } } 
        //CommandManager.InvalidateRequerySuggested(); Notifica que CanExecute ha cambiado
      
        private static UsuarioViewModel _instance;
        public static UsuarioViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UsuarioViewModel();
                }
                return _instance;
            }
        }

        private UsuarioViewModel()
        {
            _usuarioRepository = new RepositoryUsuario();

            //Se inicializa los comando mediante la clase generica que cree en el directorio raiz de viewModel
            LogInCommand = new ViewModelCommand(ExecuteLogInCommand, CanExecuteLogInCommand);

            DeleteCommand = new ViewModelCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);

            // Constructor privado para evitar instancias externas
            allPerfiles = new ObservableCollection<UsuarioBase>();
            allUsers = new ObservableCollection<Usuario>();

            _ = CargarTodosLosUsuarios();
        }


        //Commands LogIn
        private bool CanExecuteLogInCommand(object obj)
        {
            bool validData;
            string patternEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

 
            if (string.IsNullOrEmpty(LogInMail) || LogInMail.Length < 3 || !Regex.IsMatch(LogInMail, patternEmail) ||
                Password == null || Password.Length < 3) 
                return false;
            else
                validData = true;
            return validData;    
        }

        private async void ExecuteLogInCommand(object obj)
        {

            dynamic isValidUsuario = await _usuarioRepository.AuthenticateUser(new NetworkCredential(LogInMail, Password));

            if (isValidUsuario != null)
            {
                JObject loginResponse = isValidUsuario as JObject ?? JObject.FromObject(isValidUsuario);
                JToken dataToken = loginResponse["Data"] ?? loginResponse["data"];

                if (!(dataToken is JObject data))
                {
                    LogInError = ObtenerMensajeLogin(loginResponse);
                    ShowNotification(new
                    {
                        ReasonPhrase = "Credenciales incorrectas",
                        Content = LogInError
                    });
                    return;
                }

                string rol = data["rol"]?.ToString() ?? "";

                if (rol != "Administrador")
                {
                    LogInError = "Solo los administradores pueden acceder a esta aplicación.";
                    ShowNotification(new
                    {
                        ReasonPhrase = "Acceso denegado",
                        Content = LogInError
                    });
                    return;
                }

                if (data["token"] != null)
                {
                    string token = data["token"].ToString();
                    JToken profileToken = data["profile"];
                    bool tienePerfil = profileToken != null && profileToken.Type != Newtonsoft.Json.Linq.JTokenType.Null;

                    if (!tienePerfil)
                    {
                        JObject usuarioActual = await ObtenerUsuarioActual(token);
                        string idUsuario = usuarioActual?["IDUsuario"]?.ToString();
                        string email = usuarioActual?["email"]?.ToString() ?? LogInMail;
                        bool emailVerificado = usuarioActual?["email_verified_at"] != null &&
                                                usuarioActual["email_verified_at"].Type != Newtonsoft.Json.Linq.JTokenType.Null;

                        Window ventanaActualRegistro = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

                        if (string.IsNullOrEmpty(idUsuario))
                        {
                            LogInError = "No se pudo obtener el usuario desde la API.";
                            ShowNotification(new
                            {
                                ReasonPhrase = "Login",
                                Content = LogInError
                            });
                            return;
                        }

                        if (!emailVerificado)
                        {
                            CodigoDeVerificacion codigo = new CodigoDeVerificacion
                            {
                                Email = email,
                                TemporalToken = token,
                                EsRegistroInmediato = true,
                                Owner = ventanaActualRegistro
                            };
                            codigo.ShowDialog();
                            await AbrirInicioSiSesionValida();
                            return;
                        }

                        RegistroPerfil perfil = new RegistroPerfil
                        {
                            EmailApp = email,
                            IdUsuario = idUsuario,
                            Privileges = "Administrador",
                            TemporalToken = token,
                            EsRegistroInmediato = true,
                            Owner = ventanaActualRegistro
                        };
                        perfil.ShowDialog();
                        await AbrirInicioSiSesionValida();
                        return;
                    }

                    JObject profile = profileToken as JObject ?? new JObject();

                    string idPerfil = profile["IDAdministrador"]?.ToString()
                        ?? profile["IDUsuario"]?.ToString()
                        ?? "";

                    SettingsData.Default.token = token;
                    SettingsData.Default.appToken = "";
                    SettingsData.Default.idPerfil = idPerfil;
                    SettingsData.Default.rol = rol;
                    SettingsData.Default.nombre = profile["user"]?["email"]?.ToString()
                        ?? LogInMail;
                    SettingsData.Default.Save();

                    UserSession.Instance.Token = SettingsData.Default.token;
                    UserSession.Instance.AppToken = "";
                    UserSession.Instance.CurrentId = idPerfil;
                    UserSession.Instance.Data = profile;

                    Inicio user = new Inicio();

                    var ventanaActual = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                    if (ventanaActual != null)
                    {
                        user.Show();
                        ventanaActual.Close();
                    }

                }
            }

        }

        private string ObtenerMensajeLogin(JObject loginResponse)
        {
            JToken mensajeToken = loginResponse["Message"]
                ?? loginResponse["message"]
                ?? loginResponse["Content"]
                ?? loginResponse["content"]
                ?? loginResponse["Data"]
                ?? loginResponse["data"];

            string mensaje = mensajeToken?.ToString();
            return string.IsNullOrWhiteSpace(mensaje)
                ? "No se pudo iniciar sesión. Revisa el email y la contraseña."
                : mensaje;
        }

        //Commands Delete
        private bool CanExecuteDeleteCommand(object obj)
        {
            Console.WriteLine("Comando Verficacion.");
            return obj is UsuarioBase usuario && !string.IsNullOrEmpty(usuario._id);
        }

        private async void ExecuteDeleteCommand(object obj)
        {

            Console.WriteLine("Comando ejecutado correctamente.");

            if (!(obj is UsuarioBase usuarioEliminar))
                return;

            if (usuarioEliminar._id == SettingsData.Default.idPerfil)
            {
                MessageBox.Show("No puedes eliminar el administrador con la sesión iniciada.", "Acción no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("¿Estás seguro de que quieres eliminar la cuenta seleccionada?",
                                         "Confirmar eliminación",
                                         MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                var response = await _usuarioRepository.Delete(usuarioEliminar._id.ToString(), usuarioEliminar.rol.ToString());
            

                if (response)
                {
                    MessageBox.Show("Usuario eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Aquí podrías actualizar la lista de usuarios eliminando el usuario borrado
                    //allPerfiles.Remove(UsuarioSeleccionado);
                    await CargarTodosLosUsuarios();
                    UsuarioSeleccionado = null; // Deseleccionar usuario después de eliminar
                }
                else
                {
                    //MessageBox.Show("Hubo un error al eliminar el usuario. Intenta de nuevo.");
                    //UsuarioSeleccionado = null; // Deseleccionar usuario después de eliminar
                }
            }
        }



        //OK EX
        public async Task<string> CargarTodosLosUsuarios()
        {
            using (var client = new HttpClient()) {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SettingsData.Default.token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage responseAdministradores = await client.GetAsync(ApiRouteUsuarios.Administrador.GetAll);
                    var jsonAdministrador = await responseAdministradores.Content.ReadAsStringAsync();

                    if (responseAdministradores.StatusCode == HttpStatusCode.Unauthorized ||
                        responseAdministradores.StatusCode == HttpStatusCode.Forbidden)
                    {
                        ClearSettings();
                        return SettingsData.Default._419;
                    }

                    if (responseAdministradores.StatusCode == HttpStatusCode.NotFound)
                        return SettingsData.Default._404;

                    if (responseAdministradores.IsSuccessStatusCode)
                    {
                        var administradoresResponse = JsonConvert.DeserializeObject<JObject>(jsonAdministrador);
                        var administradores = administradoresResponse["Data"] as JArray;

                        AllPerfiles = new ObservableCollection<UsuarioBase>();
                        AllUsers= new ObservableCollection<Usuario>();

                        if (administradores != null)
                        {
                            foreach (var administrador in administradores)
                            {
                                string idAdministrador = administrador["IDAdministrador"]?.ToString();
                                string idUsuario = administrador["IDUsuario"]?.ToString();
                                string email = administrador["user"]?["email"]?.ToString();
                                string rol = administrador["user"]?["rol"]?.ToString() == "admin" ? "Administrador" : administrador["user"]?["rol"]?.ToString();
                                string activo = administrador["Activo"]?.ToObject<bool>() == true ? "Activo" : "Inactivo";
                                string fotoPerfil = administrador["FotoPerfil"]?.ToString();

                                if (string.IsNullOrEmpty(idAdministrador))
                                    continue;

                                AllPerfiles.Add(new Administrador
                                {
                                    _id = idAdministrador,
                                    idUsuario = idUsuario,
                                    nombre = administrador["Nombre"]?.ToString(),
                                    apellido = administrador["Apellido"]?.ToString(),
                                    rol = !string.IsNullOrEmpty(rol) ? rol : "Administrador",
                                    registro = activo,
                                    baja = activo == "Activo" ? "false" : "true",
                                    rutaFoto = !string.IsNullOrEmpty(fotoPerfil) ? $"http://127.0.0.1:8000/storage/{fotoPerfil}" : "",
                                    _nivelDeAcceso = email,
                                    _responsableDeArea = activo
                                });
                            }
                        }

                        OnPropertyChanged("AllPerfiles");
                        OnPropertyChanged("AllUsers");
                        return SettingsData.Default._200;
                    }
                    else {
                        dynamic administradorResponse = JsonConvert.DeserializeObject<dynamic>(jsonAdministrador);
                        if (administradorResponse != null && administradorResponse.ReasonPhrase == "Token Expired")
                        {
                            ClearSettings();    
                            return SettingsData.Default._419;
                        }
                        else {
                            string message = administradorResponse?.Message != null
                                ? administradorResponse.Message.ToString()
                                : jsonAdministrador;

                            Debug.Write($"WPF : Error administradores : " +
                                $"\nStatus: {responseAdministradores.StatusCode} , Contenido : {jsonAdministrador} ");

                            return message;
                        }
                    }     
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Mensaje: " + e.Message);
                    return SettingsData.Default._503;
                }
            }
        }

        private async Task AbrirInicioSiSesionValida()
        {
            if (!await AccessToken())
                return;

            Inicio inicio = new Inicio();
            var ventanaActual = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            inicio.Show();
            ventanaActual?.Close();
        }

        public async Task<bool> EmailDisponible(string emailDisponible)
        {
            using (var cliente = new HttpClient())
            {
                try
                {
                    var json = JsonConvert.SerializeObject(new {email = emailDisponible});
                    var content = new StringContent (json,Encoding.UTF8,"application/json");

                    var responseEmail = await cliente.PostAsync(ApiRouteUsuarios.Usuario.EmailDisponible, content);

                    if (responseEmail.IsSuccessStatusCode)
                    {
                            return true;
                    }
                    else
                    {
                        Debug.WriteLine($"Errors: {responseEmail.StatusCode}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<HttpResponseMessage> EditarPerfil(string id,string rol, MultipartFormDataContent usuarioEditar) {
            string rutaPerfilEditar = "";

            if (rol == "Administrador"){ rutaPerfilEditar = $"{ApiRouteUsuarios.Administrador.Editar}/{id}"; }
            else { throw new InvalidOperationException("La aplicación WPF solo gestiona administradores."); }

            using (var client = new HttpClient()) {
                try {

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",SettingsData.Default.token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    usuarioEditar.Add(new StringContent("PUT"), "_method");

                    HttpResponseMessage response = await client.PostAsync(rutaPerfilEditar, usuarioEditar);

                    var respuestaContenido = await response.Content.ReadAsStringAsync();
                    // Imprimir la respuesta para depuración
                    Debug.WriteLine($"Código de estado: {response.StatusCode}");
                    Debug.WriteLine($"Contenido de la respuesta: {respuestaContenido}");

                    if (response.IsSuccessStatusCode) {
                        dynamic respuestaContenidoJson = JsonConvert.DeserializeObject<dynamic>(respuestaContenido);
                        Debug.WriteLine($"Usuario Editado. {respuestaContenidoJson.user}");
                        return response;
                    } else {
                        Debug.WriteLine($"Error al Editar.");
                        return response;
                    }
                    
                } catch (Exception e) { throw new Exception("WPF : ViewModel : "+e.Message); }
            }


        }

        public async void Buscar(string rol,MultipartFormDataContent usuarioBuscar) {
            if (rol == "Administrador")
            {
                var resultadoCarga = await CargarTodosLosUsuarios();
                if (resultadoCarga != SettingsData.Default._200)
                {
                    dynamic content = new { ReasonPhrase = "Error", Content = resultadoCarga };
                    ShowNotification(content);
                    return;
                }

                string nombre = "";
                string apellido = "";
                string email = "";
                string situacion = "";

                foreach (var item in usuarioBuscar)
                {
                    string fieldName = item.Headers.ContentDisposition.Name.Trim('"');
                    string value = await item.ReadAsStringAsync();

                    if (fieldName == "nombre")
                        nombre = value;
                    else if (fieldName == "apellido")
                        apellido = value;
                    else if (fieldName == "email")
                        email = value;
                    else if (fieldName == "situacion")
                        situacion = value;
                }

                var filtrados = AllPerfiles.Where(usuario =>
                    (string.IsNullOrEmpty(nombre) || (!string.IsNullOrEmpty(usuario.nombre) && usuario.nombre.IndexOf(nombre, StringComparison.OrdinalIgnoreCase) >= 0)) &&
                    (string.IsNullOrEmpty(apellido) || (!string.IsNullOrEmpty(usuario.apellido) && usuario.apellido.IndexOf(apellido, StringComparison.OrdinalIgnoreCase) >= 0)) &&
                    (string.IsNullOrEmpty(email) || (usuario is Administrador administrador && !string.IsNullOrEmpty(administrador._nivelDeAcceso) && administrador._nivelDeAcceso.IndexOf(email, StringComparison.OrdinalIgnoreCase) >= 0)) &&
                    (string.IsNullOrEmpty(situacion) || usuario.registro == situacion)
                ).ToList();

                AllPerfiles = new ObservableCollection<UsuarioBase>(filtrados);
                OnPropertyChanged("AllPerfiles");
                return;
            }

            dynamic rolNoSoportado = new { ReasonPhrase = "Rol no soportado", Content = "La aplicación WPF solo gestiona administradores." };
            ShowNotification(rolNoSoportado);
        }

        //MODULADO
        public async Task<bool> AccessToken()
        {
            using (var client = new HttpClient())
            {
                try
                {//Se envia el token
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SettingsData.Default.token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
 
                    var response = await client.GetAsync(ApiRouteUsuarios.Usuario.AccessToken);

                    if (response == null) {
                        //Caso(s): El servidor esta apagado
                        dynamic nullContet = new { ReasonPhrase = "Error de conexión.",Content = "Por favor revise su conexión al servidor." };
                        ShowNotification(nullContet);
                        return false;
                    }
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    //Caso(s): Respuesta Efectiva 
                    if (response.IsSuccessStatusCode)
                    {
                        dynamic responseData = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                        string rol = responseData.Rol != null ? responseData.Rol.ToString() : "";

                        if (rol == "Administrador")
                            return true;

                        ClearSettings();
                        dynamic deniedContent = new
                        {
                            ReasonPhrase = "Acceso denegado",
                            Content = "Solo los administradores pueden acceder a esta aplicación."
                        };
                        ShowNotification(deniedContent);
                        return false;
                    }
                    else {
                        MessageBox.Show("Entro no success ?");
                        //Caso(s): Verifico el contenido de la respuesta porque puede ser JSON/HTML
                        string contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";
                        if (contentType == "application/json") { //Si es JSON
                            var errorContet = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                            ShowNotification(errorContet);
                            ClearSettings(); //Borro SettingData dado que es provable que aqui contenga un token expirado.
                            return false;
                        }else { //SI es HTML

                            string contenidoExtraido = ExtractPreContent(jsonContent);
                            dynamic errorHtml = new{ ReasonPhrase = response.ReasonPhrase, Content = contenidoExtraido};
                            ShowNotification(errorHtml);
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    //Caso(s): Manejar otros errores
                    Debug.WriteLine("Error desconocido: " + e);
                    dynamic exceptionContet = new { ReasonPhrase = "Exception Access Token.", Content = e.Message.ToString() };
                    ShowNotification(exceptionContet);
                    return false;
                }
            }
        }

        public async Task CerrarSesion()
        {
            string token = SettingsData.Default.token;

            if (!string.IsNullOrWhiteSpace(token))
            {
                using (var cliente = new HttpClient())
                {
                    try
                    {
                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        await cliente.PostAsync(ApiRouteUsuarios.Usuario.LogOut, null);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Exception CerrarSesion: {ex.Message}");
                    }
                }
            }

            ClearSettings();
            UserSession.Instance.Token = "";
            UserSession.Instance.AppToken = "";
            UserSession.Instance.CurrentId = "";
            UserSession.Instance.Data = new JObject();
        }

        public async Task<HttpResponseMessage> RecuperarPassword(string _email) {

            using (var cliente = new HttpClient()) {
                try {

                    var json = JsonConvert.SerializeObject(new {email = _email });
                    var content = new StringContent(json, Encoding.UTF8, "application/json"); 

                    var response = await cliente.PostAsync(ApiRouteUsuarios.Usuario.RecuperarPassword, content);

                    var contentError = await response.Content.ReadAsStringAsync();


                    Debug.WriteLine("Status: \n" + response.StatusCode);
                    Debug.WriteLine("Contenido: \n" + contentError);

                    return response;
                }
                catch (Exception) {
                    return null;
                }
            }
        
        }

        public async Task<HttpResponseMessage> CambiarPassword(string currentPassword, string newPassword) {

            using (var client = new HttpClient()) {

                try {
                    var json = JsonConvert.SerializeObject(new
                    {
                        current_password = currentPassword,
                        password = newPassword,
                        password_confirmation = newPassword
                    });

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",SettingsData.Default.token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.PostAsync(ApiRouteUsuarios.Usuario.CambiarPassword, content);

                    var contentError = await response.Content.ReadAsStringAsync();

                    Debug.WriteLine("Status: \n" + response.StatusCode);
                    Debug.WriteLine("Contenido: \n" + contentError);

                    return response; 

                } catch (Exception) {
                    return null;
                }
                


            }
        }

        //Registro Usuarios/Perfiles
        public async Task<HttpResponseMessage> RegistrarUsuario(Usuario UsuarioNuevo)
        {
            using (var cliente = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(UsuarioNuevo);
                Debug.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await cliente.PostAsync(ApiRouteUsuarios.Usuario.Registro, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("Usuario creado correctamente : En ViewModel");
                        return response;
                    }
                    else
                    {
                        Debug.WriteLine($"Error : {response.StatusCode} : En ViewModel");
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message} : En ViewModel");
                    return null;
                }
            }
        }

        public async Task<HttpResponseMessage> PreRegistrarAdministrador(string email)
        {
            using (var cliente = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(new { email = email });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SettingsData.Default.token);
                    cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await cliente.PostAsync(ApiRouteUsuarios.Usuario.PreRegistro, content);

                    Debug.WriteLine($"PreRegistro Status: {response.StatusCode}");
                    Debug.WriteLine($"PreRegistro Content: {await response.Content.ReadAsStringAsync()}");

                    return response;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception PreRegistrarAdministrador: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<HttpResponseMessage> ValidarUsuario(Usuario VerificarUsuario)
        {
            using (var cliente = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(new
                {
                    email = VerificarUsuario.email,
                    code = VerificarUsuario.verificationCode
                });
                Debug.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SettingsData.Default.token);
                    cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await cliente.PostAsync(ApiRouteUsuarios.Usuario.Verificar, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("WPF : Usuario verificado correctamente");
                        return response;
                    }
                    else
                    {
                        Debug.WriteLine($"WPF : Error : {response.StatusCode}");
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<JObject> ObtenerUsuarioActual(string token)
        {
            using (var cliente = new HttpClient())
            {
                try
                {
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await cliente.GetAsync(ApiRouteUsuarios.Usuario.CurrentUser);
                    var content = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                        return null;

                    return JsonConvert.DeserializeObject<JObject>(content);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception ObtenerUsuarioActual: {ex.Message}");
                    return null;
                }
            }
        }

        //Notificacion base
        private void ShowNotification(dynamic content)
        {
            string titulo = "Error";
            string mensaje = "No se pudo procesar la respuesta del servidor.";

            try
            {
                JObject json = content is JObject jObject ? jObject : JObject.FromObject(content);

                titulo = json["ReasonPhrase"]?.ToString()
                    ?? json["reasonPhrase"]?.ToString()
                    ?? json["StatusCode"]?.ToString()
                    ?? titulo;

                JToken mensajeToken = json["Message"]
                    ?? json["message"]
                    ?? json["Content"]
                    ?? json["content"];

                if (mensajeToken != null)
                {
                    mensaje = mensajeToken.Type == JTokenType.Array
                        ? string.Join(Environment.NewLine, mensajeToken.Select(item => item.ToString()))
                        : mensajeToken.ToString();
                }
            }
            catch
            {
                mensaje = content?.ToString() ?? mensaje;
            }

            Notificacion not = new Notificacion(titulo, mensaje);
            //Se busca la venta actual para poder bloquear la ventana que lo ejecuta.
            not.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            not.ShowDialog();
        }

        //En caso nesecario se borran los datos almacenados.
        private void ClearSettings()
        {
            SettingsData.Default.token = "";
            SettingsData.Default.appToken = "";
            SettingsData.Default.idPerfil = "";
            SettingsData.Default.rol = "";
            SettingsData.Default.nombre = "";
            SettingsData.Default.Save();
        }

        //Funcion que gestiona una solicitud HTML del servidor devolviendo el pre
        string ExtractPreContent(string html)
        {
            var match = Regex.Match(html, @"<pre>(.*?)<\/pre>", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value : "No se encontró contenido en <pre>";
        }

    }
}


//NOTAS

//Inicializar un HTTPResponseMessage
//HttpResponseMessage response = new HttpResponseMessage
//{
//    StatusCode = HttpStatusCode.OK,
//    Content = new StringContent("{\"header\": \"Titulo\", \"content\": \"Contenido de la respuesta\"}", Encoding.UTF8, "application/json"),
//    Headers = {
//        { "Header-Name", "Header-Value" }
//    }
//};

//string jsonContent = "";
