using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Models.Usuarios
{
    public class UsuarioBase
    {

        public string _id { get; set; }
        public string idUsuario { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string dni { get; set; }
        public string rol { get; set; }
        public string date { get; set; }
        public string ciudad { get; set; }
        public string sexo { get; set; }
        public string registro { get; set; }
        public string rutaFoto { get; set; }
        public string baja { get; set; }
    }
}
