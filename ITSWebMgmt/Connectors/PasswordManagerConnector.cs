using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Connectors
{
    public class PasswordManagerConnector
    {
        private readonly IMemoryCache _cache;

        public PasswordManagerConnector(IMemoryCache cache = null)
        {
            _cache = cache;
        }

        public Secret GetSecret(string name)
        {
            Secret secret;
            if (!_cache.TryGetValue(name, out secret))
            {
                secret = RequestSecret(name);
            }

            return secret;
        }

        private Secret RequestSecret(string name)
        {
            var api = Environment.GetEnvironmentVariable("API");
            var key = Environment.GetEnvironmentVariable("KEY");
            //https://clickstudios.com.au/about/application-programming-interface.aspx
            //API dokumentation kan findes under Help, når man er logget ind
            return new Secret();
        }
    }

    public class Secret
    {
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string APIKey { get; set; }
    }
}
