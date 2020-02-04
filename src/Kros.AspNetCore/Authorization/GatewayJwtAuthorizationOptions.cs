using System;
using System.Collections.Generic;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// JWT authorization options for api gateway.
    /// </summary>
    public class GatewayJwtAuthorizationOptions
    {
        /// <summary>
        /// Authorization service url.
        /// </summary>
        public string AuthorizationUrl { get; set; }

        /// <summary>
        /// Hash authorization url.
        /// </summary>
        public string HashAuthorizationUrl { get; set; }

        /// <summary>
        /// Hash authorization parameter name.
        /// </summary>
        public string HashParameterName { get; set; }

        /// <summary>
        /// Cache sliding expiration offset.
        /// </summary>
        /// <remarks>
        /// Default is <see cref="TimeSpan.Zero"/>.
        /// </remarks>
        public TimeSpan CacheSlidingExpirationOffset { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Paths that do not use JWT authorization caching 
        /// </summary>
        public List<string> IgnoredPathForCache { get; private set; } = new List<string>();
    }
}
