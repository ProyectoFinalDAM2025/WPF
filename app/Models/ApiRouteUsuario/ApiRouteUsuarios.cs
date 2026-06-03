using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Models.ApiRouteUsuario
{
    public static class ApiRouteUsuarios
    {
        private static readonly string BaseUrl = "https://api.officium.es/api";
        public static readonly string PublicBaseUrl = "https://api.officium.es";

        public static string ResolvePublicFileUrl(string ruta, string fallback = "")
        {
            if (string.IsNullOrWhiteSpace(ruta))
                return fallback;

            if (ruta.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                ruta.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return ruta;

            ruta = ruta.TrimStart('/', '\\').Replace("\\", "/");

            if (ruta.StartsWith("storage/", StringComparison.OrdinalIgnoreCase) ||
                ruta.StartsWith("assets/", StringComparison.OrdinalIgnoreCase))
                return $"{PublicBaseUrl}/{ruta}";

            return $"{PublicBaseUrl}/storage/{ruta}";
        }

        public static class Administrador
        {
            public static readonly string GetAll = $"{BaseUrl}/administrador";
            public static readonly string Eliminar = $"{BaseUrl}/administrador";
            public static readonly string Editar = $"{BaseUrl}/administrador";
            public static readonly string Buscar = $"{BaseUrl}/administrador";
        }

        public static class Usuario
        {
            public static readonly string EmailDisponible = $"{BaseUrl}/Usuario/emailDisponible";
            public static readonly string LogIn = $"{BaseUrl}/login";
            public static readonly string LogOut = $"{BaseUrl}/logout";
            public static readonly string AccessToken = $"{BaseUrl}/rolUsuario";
            public static readonly string CurrentUser = $"{BaseUrl}/user";
            public static readonly string Registro = $"{BaseUrl}/register";
            public static readonly string PreRegistro = $"{BaseUrl}/pre-register";
            public static readonly string Verificar = $"{BaseUrl}/verifyCode";
            public static readonly string RecuperarPassword = $"{BaseUrl}/recover";
            public static readonly string CambiarPassword = $"{BaseUrl}/change-password";
        }

        public static class Reporte
        {
            public static readonly string GetAll = $"{BaseUrl}/reportes";
            public static readonly string Eliminar = $"{BaseUrl}/reportes";
            public static readonly string Moderar = $"{BaseUrl}/reportes";
            public static readonly string EliminarEntidad = $"{BaseUrl}/reportes";
        }
    }
}
