using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Extensions for <see cref="HttpResponse"/>.
    /// </summary>
    public static class HttpResponseExtensions
    {
        private static readonly HashSet<string> _preservedHeaders = new()
        {
            HeaderNames.AccessControlAllowCredentials, HeaderNames.AccessControlAllowHeaders,
            HeaderNames.AccessControlAllowOrigin, HeaderNames.AccessControlAllowMethods
        };

        /// <summary>
        /// Clears <paramref name="response"/> and preserves CORS headers.
        /// </summary>
        /// <param name="response">Response object.</param>
        public static void ClearExceptCorsHeaders(this HttpResponse response)
        {
            List<KeyValuePair<string, StringValues>> headers = response.Headers
                .Where(kv => _preservedHeaders.Contains(kv.Key)).ToList();

            response.Clear();

            foreach (KeyValuePair<string, StringValues> header in headers)
            {
                response.Headers.Add(header);
            }
        }
    }
}
