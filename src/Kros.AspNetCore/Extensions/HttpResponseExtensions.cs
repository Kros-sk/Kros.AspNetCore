using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Extensions for <see cref="HttpResponse"/>.
    /// </summary>
    public static class HttpResponseExtensions
    {
        private static HashSet<string> _preservedHeaders = new HashSet<string>{ "Access-Control-Allow-Credentials",
            "Access-Control-Allow-Headers", "Access-Control-Allow-Origin", "Access-Control-Allow-Methods" };

        /// <summary>
        /// Clears <paramref name="response"/> and preserves Cors headers.
        /// </summary>
        /// <param name="response">Response object.</param>
        public static void ClearExceptCorsHeaders(this HttpResponse response)
        {
            var headers = response.Headers.Where(kv => _preservedHeaders.Contains(kv.Key)).ToList();

            response.Clear();

            foreach (KeyValuePair<string, StringValues> header in headers)
            {
                response.Headers.Add(header);
            }
        }
    }
}
