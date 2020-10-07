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

        public PasswordManagerConnector(IMemoryCache cache)
        {
            _cache = cache;
        }

        public class Secret
        {
            public static string UserName { get; set; }
            public static string Password { get; set; }
            public static string APIKey { get; set; }
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
}
