using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using app.Models.ApiRouteUsuario;
using app.Models.Reportes;
using app.View.Reportes.ReportesOfertas;
using app.View.Reportes.ReportesPerfiles;
using app.View.Reportes.ReportesPublicaciones;
using app.View.Usuarios.Notificaciones;
using IntermodularWPF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace app.ViewModel.Reportes
{
    public class ReportesViewModel : ViewModelBase
    {
        private ObservableCollection<Reporte> _reportes;
        private Reporte _reporteSeleccionado;
        private string _tipoEntidadActual;

        public ObservableCollection<Reporte> Reportes
        {
            get => _reportes;
            set { _reportes = value; OnPropertyChanged("Reportes"); }
        }

        public Reporte ReporteSeleccionado
        {
            get => _reporteSeleccionado;
            set { _reporteSeleccionado = value; OnPropertyChanged("ReporteSeleccionado"); CommandManager.InvalidateRequerySuggested(); }
        }

        public ICommand VerCommand { get; }
        public ICommand EliminarCommand { get; }

        public ReportesViewModel()
        {
            Reportes = new ObservableCollection<Reporte>();
            VerCommand = new ViewModelCommand(ExecuteVerCommand, CanExecuteReporteCommand);
            EliminarCommand = new ViewModelCommand(ExecuteEliminarCommand, CanExecuteReporteCommand);
        }

        public async Task<string> CargarReportesPorTipo(string tipoEntidad)
        {
            _tipoEntidadActual = tipoEntidad;

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SettingsData.Default.token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(ApiRouteUsuarios.Reporte.GetAll);
                    string json = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.Unauthorized ||
                        response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        ClearSettings();
                        return SettingsData.Default._419;
                    }

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return SettingsData.Default._404;

                    if (!response.IsSuccessStatusCode)
                        return ObtenerMensajeError(json, response.ReasonPhrase);

                    Reportes = new ObservableCollection<Reporte>();
                    JArray data = ObtenerArrayReportes(json);

                    foreach (JToken item in data)
                    {
                        string tipo = Valor(item, "TipoEntidad", "tipoEntidad", "tipo_entidad");

                        if (!EsTipoSolicitado(tipo, tipoEntidad))
                            continue;

                        JToken entidad = Child(item, "Entidad") ?? Child(item, "entidad");
                        JToken empresa = Child(entidad, "empresa");
                        JToken categoria = Child(entidad, "categoria");
                        JToken sector = Child(empresa, "sector");
                        JToken user = Child(entidad, "user");
                        JToken autorEmpresa = Child(user, "empresa");
                        JToken autorDesempleado = Child(user, "desempleado");
                        JToken perfilEmpresa = Child(entidad, "empresa");
                        JToken perfilDesempleado = Child(entidad, "desempleado");
                        JToken perfilSector = Child(perfilEmpresa, "sector");
                        JToken documento = PrimerDocumento(entidad);
                        string perfilNombreEmpresa = Valor(perfilEmpresa, "NombreEmpresa", "nombreEmpresa");
                        string perfilNombre = Valor(perfilDesempleado, "Nombre", "nombre");
                        string perfilApellido = Valor(perfilDesempleado, "Apellido", "apellido");
                        string perfilNombreCompleto = !string.IsNullOrWhiteSpace(perfilNombreEmpresa)
                            ? perfilNombreEmpresa
                            : $"{perfilNombre} {perfilApellido}".Trim();
                        string perfilFoto = !string.IsNullOrWhiteSpace(Valor(perfilEmpresa, "Foto", "foto"))
                            ? Valor(perfilEmpresa, "Foto", "foto")
                            : Valor(perfilDesempleado, "Foto", "foto");

                        string publicacionArchivo = Valor(entidad, "Archivo", "archivo");
                        string publicacionThumbnail = Valor(entidad, "Thumbnail", "thumbnail");
                        string publicacionPreview = Valor(entidad, "Preview", "preview");

                        if (string.IsNullOrWhiteSpace(publicacionArchivo))
                            publicacionArchivo = Valor(documento, "URL", "url");

                        if (string.IsNullOrWhiteSpace(publicacionThumbnail))
                            publicacionThumbnail = Valor(documento, "Thumbnail", "thumbnail");

                        if (string.IsNullOrWhiteSpace(publicacionPreview))
                            publicacionPreview = Valor(documento, "Preview", "preview");

                        Reportes.Add(new Reporte
                        {
                            IDReporte = Valor(item, "IDReporte", "idReporte", "id_reporte", "id", "_id"),
                            TipoEntidad = tipo,
                            IDEntidad = Valor(item, "IDEntidad", "idEntidad", "id_entidad"),
                            TituloEntidad = Valor(item, "TituloEntidad", "tituloEntidad", "titulo_entidad", "Titulo", "titulo"),
                            Motivo = Valor(item, "Motivo", "motivo", "Razon", "razon"),
                            Descripcion = Valor(item, "Descripcion", "descripcion", "Contenido", "contenido", "Detalle", "detalle"),
                            Estado = Valor(item, "Estado", "estado"),
                            UsuarioReporta = Valor(item, "UsuarioReporta", "usuarioReporta", "usuario_reporta", "Usuario", "usuario", "Email", "email"),
                            FechaCreacion = Valor(item, "FechaReporte", "fechaReporte", "FechaCreacion", "fechaCreacion", "created_at", "CreatedAt"),
                            OfertaTitulo = Valor(entidad, "Titulo", "titulo"),
                            OfertaDescripcion = Valor(entidad, "Descripcion", "descripcion"),
                            OfertaUbicacion = Valor(entidad, "Ubicacion", "ubicacion"),
                            OfertaEstado = Valor(entidad, "Estado", "estado"),
                            OfertaFechaPublicacion = Valor(entidad, "FechaPublicacion", "fechaPublicacion"),
                            OfertaCategoria = Valor(categoria, "Nombre", "nombre"),
                            EmpresaNombre = Valor(empresa, "NombreEmpresa", "nombreEmpresa"),
                            EmpresaUbicacion = Valor(empresa, "Ubicacion", "ubicacion"),
                            EmpresaSitioWeb = Valor(empresa, "SitioWeb", "sitioWeb"),
                            EmpresaSector = Valor(sector, "Nombre", "nombre"),
                            EmpresaFoto = NormalizarRutaArchivo(Valor(empresa, "Foto", "foto"), "/View/Home/Logo.png"),
                            PublicacionContenido = Valor(entidad, "Contenido", "contenido"),
                            PublicacionFecha = Valor(entidad, "FechaPublicacion", "fechaPublicacion"),
                            PublicacionGrupo = Valor(Child(entidad, "grupo"), "Nombre", "nombre"),
                            PublicacionTipoArchivo = Valor(entidad, "TipoArchivo", "tipoArchivo"),
                            PublicacionArchivo = NormalizarRutaArchivo(publicacionArchivo, ""),
                            PublicacionThumbnail = NormalizarRutaArchivo(publicacionThumbnail, ""),
                            PublicacionPreview = NormalizarRutaArchivo(publicacionPreview, ""),
                            PublicacionAutor = ObtenerAutorPublicacion(autorEmpresa, autorDesempleado, user),
                            PublicacionAutorEmail = Valor(user, "email", "Email"),
                            PublicacionAutorFoto = NormalizarRutaArchivo(
                                !string.IsNullOrWhiteSpace(Valor(autorEmpresa, "Foto", "foto"))
                                    ? Valor(autorEmpresa, "Foto", "foto")
                                    : Valor(autorDesempleado, "Foto", "foto"),
                                "/View/Home/Logo.png"),
                            PerfilNombre = !string.IsNullOrWhiteSpace(perfilNombreCompleto)
                                ? perfilNombreCompleto
                                : Valor(entidad, "email", "Email"),
                            PerfilEmail = Valor(entidad, "email", "Email"),
                            PerfilRol = Valor(entidad, "rol", "Rol"),
                            PerfilFoto = NormalizarRutaArchivo(perfilFoto, "/View/Home/Logo.png"),
                            PerfilUbicacion = !string.IsNullOrWhiteSpace(Valor(perfilEmpresa, "Ubicacion", "ubicacion"))
                                ? Valor(perfilEmpresa, "Ubicacion", "ubicacion")
                                : Valor(perfilDesempleado, "Ubicacion", "ubicacion"),
                            PerfilSitioWeb = Valor(perfilEmpresa, "SitioWeb", "sitioWeb"),
                            PerfilSector = Valor(perfilSector, "Nombre", "nombre"),
                            PerfilDocumento = !string.IsNullOrWhiteSpace(Valor(perfilEmpresa, "CIF", "cif"))
                                ? Valor(perfilEmpresa, "CIF", "cif")
                                : Valor(perfilDesempleado, "DNI", "dni"),
                            PerfilDisponibilidad = Valor(perfilDesempleado, "Disponibilidad", "disponibilidad"),
                            PerfilPortafolio = Valor(perfilDesempleado, "Porfolios", "porfolios")
                        });
                    }

                    OnPropertyChanged("Reportes");
                    return SettingsData.Default._200;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error cargando reportes: " + ex.Message);
                    return $"{SettingsData.Default._503}\nDetalle: {ex.Message}";
                }
            }
        }

        private static JArray ObtenerArrayReportes(string json)
        {
            JToken root = JsonConvert.DeserializeObject<JToken>(json);

            if (root is JArray array)
                return array;

            if (!(root is JObject obj))
                return new JArray();

            return obj["Data"] as JArray
                ?? obj["data"] as JArray
                ?? obj["Reportes"] as JArray
                ?? obj["reportes"] as JArray
                ?? new JArray();
        }

        private static string Valor(JToken item, params string[] keys)
        {
            if (!(item is JObject itemObject))
                return "";

            foreach (string key in keys)
            {
                JToken value = itemObject[key];
                if (value == null || value.Type == JTokenType.Null)
                    continue;

                if (value is JObject obj)
                    return obj["email"]?.ToString()
                        ?? obj["Nombre"]?.ToString()
                        ?? obj["nombre"]?.ToString()
                        ?? obj.ToString(Formatting.None);

                return value.ToString();
            }

            return "";
        }

        private static JToken Child(JToken item, string key)
        {
            return item is JObject obj ? obj[key] : null;
        }

        private static JToken PrimerDocumento(JToken entidad)
        {
            JToken documentos = Child(entidad, "documentos") ?? Child(entidad, "Documentos");
            return documentos is JArray array && array.Count > 0 ? array[0] : null;
        }

        private static string NormalizarRutaArchivo(string ruta, string fallback)
        {
            return ApiRouteUsuarios.ResolvePublicFileUrl(ruta, fallback);
        }

        private static string ObtenerAutorPublicacion(JToken empresa, JToken desempleado, JToken user)
        {
            string nombreEmpresa = Valor(empresa, "NombreEmpresa", "nombreEmpresa");
            if (!string.IsNullOrWhiteSpace(nombreEmpresa))
                return nombreEmpresa;

            string nombre = Valor(desempleado, "Nombre", "nombre");
            string apellido = Valor(desempleado, "Apellido", "apellido");
            string nombreCompleto = $"{nombre} {apellido}".Trim();

            return !string.IsNullOrWhiteSpace(nombreCompleto)
                ? nombreCompleto
                : Valor(user, "email", "Email");
        }

        private static bool EsTipoSolicitado(string tipoReporte, string tipoSolicitado)
        {
            string reporte = NormalizarTipo(tipoReporte);
            string solicitado = NormalizarTipo(tipoSolicitado);

            if (reporte == solicitado)
                return true;

            return solicitado == "Oferta"
                ? reporte == "Ofertas"
                : solicitado == "Publicacion"
                    ? reporte == "Publicaciones"
                    : reporte == "Usuarios" || reporte == "Perfil" || reporte == "Perfiles";
        }

        private static string NormalizarTipo(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return "";

            return tipo.Trim()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("Á", "A")
                .Replace("É", "E")
                .Replace("Í", "I")
                .Replace("Ó", "O")
                .Replace("Ú", "U");
        }

        private static string ObtenerMensajeError(string json, string fallback)
        {
            try
            {
                JObject error = JsonConvert.DeserializeObject<JObject>(json);
                return error["Message"]?.ToString()
                    ?? error["message"]?.ToString()
                    ?? error["Content"]?.ToString()
                    ?? error["content"]?.ToString()
                    ?? fallback;
            }
            catch
            {
                return string.IsNullOrWhiteSpace(json) ? fallback : json;
            }
        }

        private bool CanExecuteReporteCommand(object obj)
        {
            return obj is Reporte reporte && !string.IsNullOrEmpty(reporte.IDReporte);
        }

        private async void ExecuteVerCommand(object obj)
        {
            if (!(obj is Reporte reporte))
                return;

            if (EsTipoSolicitado(reporte.TipoEntidad, "Oferta"))
            {
                ReporteOfertaDetalle detalle = new ReporteOfertaDetalle(reporte);
                detalle.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                bool? resultado = detalle.ShowDialog();
                if (resultado == true)
                    await EjecutarAccionDetalle(detalle.AccionSolicitada, reporte);
                return;
            }

            if (EsTipoSolicitado(reporte.TipoEntidad, "Publicacion"))
            {
                ReportePublicacionDetalle detalle = new ReportePublicacionDetalle(reporte);
                detalle.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                bool? resultado = detalle.ShowDialog();
                if (resultado == true)
                    await EjecutarAccionDetalle(detalle.AccionSolicitada, reporte);
                return;
            }

            if (EsTipoSolicitado(reporte.TipoEntidad, "Usuario"))
            {
                ReportePerfilDetalle detalle = new ReportePerfilDetalle(reporte);
                detalle.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                bool? resultado = detalle.ShowDialog();
                if (resultado == true)
                    await EjecutarAccionDetalle(detalle.AccionSolicitada, reporte);
            }
        }

        private async Task EjecutarAccionDetalle(string accion, Reporte reporte)
        {
            if (accion == "Eliminar")
            {
                ExecuteEliminarCommand(reporte);
                return;
            }

            if (accion == "Moderar")
                await ConfirmarModerarReporte(reporte);

            if (accion == "EliminarPerfil")
                await ConfirmarEliminarPerfil(reporte);
        }

        private async Task ConfirmarModerarReporte(Reporte reporte)
        {
            MessageBoxResult confirmacion = MessageBox.Show(
                "Se ocultara el contenido reportado, se dejara un mensaje de moderacion y se bloqueara la edicion. El propietario aun podra eliminarlo. Deseas continuar?",
                "Moderar contenido",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            string resultado = await ModerarReporte(reporte.IDReporte);

            if (resultado == SettingsData.Default._200)
            {
                MessageBox.Show("Contenido moderado correctamente.", "Reportes", MessageBoxButton.OK, MessageBoxImage.Information);
                await CargarReportesPorTipo(_tipoEntidadActual);
                ReporteSeleccionado = null;
                return;
            }

            if (resultado == SettingsData.Default._419)
            {
                MostrarError("Session Terminada", "Por favor inicie session.");
                return;
            }

            MostrarError("Error", resultado);
        }

        private async void ExecuteEliminarCommand(object obj)
        {
            if (!(obj is Reporte reporte))
                return;

            MessageBoxResult confirmacion = MessageBox.Show(
                "El reporte se eliminara sin afectar la oferta o publicacion reportada. ¿Deseas continuar?",
                "Eliminar reporte",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            string resultado = await EliminarReporte(reporte.IDReporte);

            if (resultado == SettingsData.Default._200)
            {
                MessageBox.Show("Reporte eliminado correctamente.", "Reportes", MessageBoxButton.OK, MessageBoxImage.Information);
                await CargarReportesPorTipo(_tipoEntidadActual);
                ReporteSeleccionado = null;
                return;
            }

            if (resultado == SettingsData.Default._419)
            {
                MostrarError("Session Terminada", "Por favor inicie session.");
                return;
            }

            MostrarError("Error", resultado);
        }

        private async Task<string> EliminarReporte(string idReporte)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SettingsData.Default.token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.DeleteAsync($"{ApiRouteUsuarios.Reporte.Eliminar}/{idReporte}");
                    string json = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.Unauthorized ||
                        response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        ClearSettings();
                        return SettingsData.Default._419;
                    }

                    if (response.IsSuccessStatusCode)
                        return SettingsData.Default._200;

                    return ObtenerMensajeError(json, response.ReasonPhrase);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error eliminando reporte: " + ex.Message);
                    return $"{SettingsData.Default._503}\nDetalle: {ex.Message}";
                }
            }
        }

        private async Task<string> ModerarReporte(string idReporte)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SettingsData.Default.token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.PostAsync($"{ApiRouteUsuarios.Reporte.Moderar}/{idReporte}/moderar", new StringContent(string.Empty));
                    string json = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.Unauthorized ||
                        response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        ClearSettings();
                        return SettingsData.Default._419;
                    }

                    if (response.IsSuccessStatusCode)
                        return SettingsData.Default._200;

                    return ObtenerMensajeError(json, response.ReasonPhrase);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error moderando reporte: " + ex.Message);
                    return $"{SettingsData.Default._503}\nDetalle: {ex.Message}";
                }
            }
        }

        private async Task ConfirmarEliminarPerfil(Reporte reporte)
        {
            MessageBoxResult confirmacion = MessageBox.Show(
                "Se eliminara el perfil reportado por administracion y se limpiaran sus reportes. Esta accion no se puede deshacer. Deseas continuar?",
                "Eliminar perfil",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            string resultado = await EliminarEntidadReporte(reporte.IDReporte);

            if (resultado == SettingsData.Default._200)
            {
                MessageBox.Show("Perfil eliminado correctamente.", "Reportes", MessageBoxButton.OK, MessageBoxImage.Information);
                await CargarReportesPorTipo(_tipoEntidadActual);
                ReporteSeleccionado = null;
                return;
            }

            if (resultado == SettingsData.Default._419)
            {
                MostrarError("Session Terminada", "Por favor inicie session.");
                return;
            }

            MostrarError("Error", resultado);
        }

        private async Task<string> EliminarEntidadReporte(string idReporte)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SettingsData.Default.token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.DeleteAsync($"{ApiRouteUsuarios.Reporte.EliminarEntidad}/{idReporte}/entidad");
                    string json = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.Unauthorized ||
                        response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        ClearSettings();
                        return SettingsData.Default._419;
                    }

                    if (response.IsSuccessStatusCode)
                        return SettingsData.Default._200;

                    return ObtenerMensajeError(json, response.ReasonPhrase);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error eliminando entidad reportada: " + ex.Message);
                    return $"{SettingsData.Default._503}\nDetalle: {ex.Message}";
                }
            }
        }

        public void MostrarError(string titulo, string mensaje)
        {
            Notificacion notificacion = new Notificacion(titulo, mensaje);
            notificacion.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            notificacion.ShowDialog();
        }

        private void ClearSettings()
        {
            SettingsData.Default.token = "";
            SettingsData.Default.appToken = "";
            SettingsData.Default.idPerfil = "";
            SettingsData.Default.rol = "";
            SettingsData.Default.nombre = "";
            SettingsData.Default.Save();
        }
    }
}
