using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Kros.AspNetCore.Net
{
    /// <summary>
    /// Extensions for HttpClientFactory.
    /// </summary>
    public static class HttpClientFactoryExtensions
    {
        /// <summary>
        /// Creates HttpClient with authorization header from <paramref name="httpContextAccessor"/>.
        /// </summary>
        public static HttpClient CreateClientWithAuthorization(this IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            HttpClient client = httpClientFactory.CreateClient();
            try
            {
                string authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader != null)
                {
                    if (AuthenticationHeaderValue.TryParse(authHeader, out AuthenticationHeaderValue newVal))
                    {
                        client.DefaultRequestHeaders.Authorization = newVal;
                    }
                }
            }
            catch
            {
                client.Dispose();
                throw;
            }

            return client;
        }
    }
}
