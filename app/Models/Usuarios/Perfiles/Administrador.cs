using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Models.Usuarios.Perfiles
{
    public class Administrador : UsuarioBase
    {

        public string _nivelDeAcceso { get; set; }
        public string _responsableDeArea { get; set; }


    }
}
