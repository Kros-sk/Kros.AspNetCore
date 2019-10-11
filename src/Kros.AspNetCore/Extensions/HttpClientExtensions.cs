using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Kros.AspNetCore.Net
{
    /// <summary>
    /// Extensions for HttpClient.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Copies authorization header from Http context <paramref name="httpContextAccessor"/>.
        /// </summary>
        public static void CopyAuthHeader(this HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            string authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != null)
            {
                AuthenticationHeaderValue.TryParse(authHeader, out AuthenticationHeaderValue newVal);
                client.DefaultRequestHeaders.Authorization = newVal;
            }
        }
    }
}
