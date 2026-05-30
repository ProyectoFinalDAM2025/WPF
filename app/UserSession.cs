using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntermodularWPF
{
    public class UserSession
    {
        private static UserSession instance = null;
        public string Token { get; set; }

        public string AppToken { get; set; }
        public JObject Data { get; set; }

        public string CurrentId { get; set; }

        private UserSession() { }

        public static UserSession Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserSession();
                }
                return instance;
            }
        }
    }

}
