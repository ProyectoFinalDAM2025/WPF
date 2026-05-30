using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using app.Models.ApiRouteUsuario;
using IntermodularWPF;

namespace app.ViewModel.Usuarios.RegistroUsuarios
{
    public class RegistroPerfilViewModel
    {
        public async Task<HttpResponseMessage> RegistrarAdministrador(string idUsuario, string nombre, string apellido, bool activo, byte[] fotoPerfil)
        {
            using (var cliente = new HttpClient())
            {
                try
                {
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SettingsData.Default.token);
                    cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new MultipartFormDataContent
                    {
                        { new StringContent(idUsuario), "IDUsuario" },
                        { new StringContent(nombre), "Nombre" },
                        { new StringContent(apellido), "Apellido" },
                        { new StringContent(activo ? "1" : "0"), "Activo" }
                    };

                    if (fotoPerfil != null)
                    {
                        var imageContent = new ByteArrayContent(fotoPerfil);
                        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                        content.Add(imageContent, "FotoPerfil", "foto-perfil.jpg");
                    }

                    return await cliente.PostAsync(ApiRouteUsuarios.Administrador.GetAll, content);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
