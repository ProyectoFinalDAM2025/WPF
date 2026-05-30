using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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
