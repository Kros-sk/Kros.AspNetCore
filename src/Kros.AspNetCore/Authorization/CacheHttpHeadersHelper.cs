using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Helper class for working with cached http headers.
    /// </summary>
    internal static class CacheHttpHeadersHelper
    {
        /// <summary>
        /// Gets header values from header associated with the specified keys from <paramref name="headerNames"/>.
        /// </summary>
        /// <param name="headers">Http headers.</param>
        /// /// <param name="headerNames">Cache header names.</param>
        /// <param name="values">When this method returns, values associated with cache header names,
        /// if the key is found; otherwise null.</param>
        /// <returns>True if the header contains any element with the specified keys from <paramref name="headerNames"/>.
        /// Otherwise false.</returns>
        public static bool TryGetValue(IHeaderDictionary headers, List<string> headerNames, out string values)
        {
            values = null;
            if ((headerNames is null) || (headerNames.Count == 0))
            {
                return false;
            }

            foreach (string headerName in headerNames)
            {
                if (headers.TryGetValue(headerName, out StringValues headerValues) && (headerValues.Count > 0))
                {
                    values += $"{headerName}:{headerValues}|";
                }
            }
            return values is not null;
        }
    }
}
