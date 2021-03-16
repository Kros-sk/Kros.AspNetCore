using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Helper class for working with cached http headers.
    /// </summary>
    public static class CacheHttpHeadersHelper
    {
        /// <summary>
        /// Gets header values from header associated with the specified keys from <paramref name="cachedHeaderNames"/>.
        /// </summary>
        /// <param name="headers">Http headers.</param>
        /// /// <param name="cachedHeaderNames">Cache header names.</param>
        /// <param name="values">When this method returns, values associated with cache header names,
        /// if the key is found; otherwise null.</param>
        /// <returns>True if the header contains any element with the specified keys from <paramref name="cachedHeaderNames"/>.
        /// Otherwise false.</returns>
        public static bool TryGetValue(IHeaderDictionary headers, List<string> cachedHeaderNames, out string values)
        {
            bool containsAnyHeader = false;
            values = null;

            foreach (string headerName in cachedHeaderNames)
            {
                if (headers.TryGetValue(headerName, out StringValues headerValues) && headerValues.Any())
                {
                    values += $"{headerName}:{headerValues.First()}";
                    containsAnyHeader = true;
                }
            }
            return containsAnyHeader;
        }
    }
}
