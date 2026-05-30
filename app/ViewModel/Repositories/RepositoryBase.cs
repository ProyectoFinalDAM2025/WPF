using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace app.ViewModel.Repositories
{
    public class RepositoryBase
    {
        private readonly HttpClient _httpClient;

        public RepositoryBase() 
        {
            _httpClient = new HttpClient(); 
        }

        public HttpClient API => _httpClient;
    }
}
