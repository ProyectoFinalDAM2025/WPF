using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Models.ApiRouteUsuario
{
    public static class ApiRouteUsuarios
    {
        private static readonly string BaseUrl = "http://127.0.0.1:8000/api";

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
    }
}
