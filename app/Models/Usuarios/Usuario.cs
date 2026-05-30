 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.Models.Usuarios
{
    public class Usuario
    {
        public string _id { get; set; }
        public string email { get; set; }
        public string emailApp { get; set; }
        public string password { get; set; }
        public string registro { get; set; }
        public string verificationCode { get; set; }
        public string codeExpiresAt { get; set; }
        public string isVerified { get; set; }
        public dynamic privileges { get; set; }


    }
}
