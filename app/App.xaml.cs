using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using app.View.Home;
using app.View.Usuarios.InicioDeSesion;

namespace app
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string ApiHost = "api.officium.es";
        private const string ApiCertificatePath = "Certificates\\officium_selfsigned.crt";
        private const string FallbackApiCertificateThumbprint = "924A31CDD0B1FFCB51D383EF63428DC6D9C7284C";
        private static string _apiCertificateThumbprint = FallbackApiCertificateThumbprint;

        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureApiSsl();
            base.OnStartup(e);
        }

        private static void ConfigureApiSsl()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _apiCertificateThumbprint = LoadApiCertificateThumbprint();
            ServicePointManager.ServerCertificateValidationCallback += ValidateApiCertificate;
        }

        private static string LoadApiCertificateThumbprint()
        {
            try
            {
                string certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApiCertificatePath);
                if (!File.Exists(certificatePath))
                    return FallbackApiCertificateThumbprint;

                var certificate = new X509Certificate2(certificatePath);
                return certificate.Thumbprint?.Replace(" ", "") ?? FallbackApiCertificateThumbprint;
            }
            catch
            {
                return FallbackApiCertificateThumbprint;
            }
        }

        private static bool ValidateApiCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            var request = sender as HttpWebRequest;
            if (request == null || !string.Equals(request.RequestUri.Host, ApiHost, StringComparison.OrdinalIgnoreCase))
                return false;

            var certificate2 = certificate as X509Certificate2 ?? new X509Certificate2(certificate);
            return string.Equals(
                certificate2.Thumbprint?.Replace(" ", ""),
                _apiCertificateThumbprint,
                StringComparison.OrdinalIgnoreCase);
        }

        //protected void ApplicationStart(object sender, StartupEventArgs e)
        //{

        //    //Se instacia la vista de inicio de sesión
        //    var logInView = new LogIn();
        //    logInView.Show();
        //    logInView.IsVisibleChanged += (s, ev) =>
        //    {
        //        //Si la vista de inicio de sesion no esta visible y esta cargado, creamos la instacia de la vista principal
        //        if (logInView.IsVisible == false && logInView.IsLoaded)
        //        {
        //            var homeView = new Inicio();
        //            homeView.Show();
        //            logInView.Hide();

        //        }
        //    };

            //Aqui deberia de generar el evento de cerrar sesion subscribiendo el evento de cerrado de ventana 
        //}
    }
}
