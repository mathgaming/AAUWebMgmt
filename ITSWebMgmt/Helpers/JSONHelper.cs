using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ITSWebMgmt.Helpers
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content) =>
            JsonConvert.DeserializeObject<T>(await content.ReadAsStringAsync());
    }
}
