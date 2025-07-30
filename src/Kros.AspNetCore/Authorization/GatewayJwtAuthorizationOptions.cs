using Kros.AspNetCore.ServiceDiscovery;
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
        /// Gets or sets the authorization.
        /// </summary>
        public AuthorizationServiceOptions Authorization { get; set; }

        internal string GetAuthorizationUrl(IServiceDiscoveryProvider provider)
            => AuthorizationUrl ?? GetUrl(provider, Authorization);

        /// <summary>
        /// Hash authorization url.
        /// </summary>
        public string HashAuthorizationUrl { get; set; }

        /// <summary>
        /// Gets or sets the hash authorization.
        /// </summary>
        public AuthorizationServiceOptions HashAuthorization { get; set; }

        internal string GetHashAuthorization(IServiceDiscoveryProvider provider)
            => HashAuthorizationUrl ?? GetUrl(provider, HashAuthorization);

        private static string GetUrl(IServiceDiscoveryProvider provider, AuthorizationServiceOptions authorization)
            => provider.GetPath(authorization.ServiceName, authorization.PathName).ToString();

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
        /// Cache absolute expiration.
        /// </summary>
        /// <remarks>
        /// Default is <see cref="TimeSpan.Zero"/>.
        /// </remarks>
        public TimeSpan CacheAbsoluteExpiration { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Paths that do not use JWT authorization caching.
        /// </summary>
        public List<string> IgnoredPathForCache { get; private set; } = new List<string>();

        /// <summary>
        /// Headers which if are in request, will be forwarded to client.
        /// </summary>
        public List<string> ForwardedHeaders { get; private set; } = new List<string>();

        /// <summary>
        /// Headers which will be used as cache key for authorization token.
        /// </summary>
        public List<string> CacheKeyHttpHeaders { get; set; } = new List<string>();

        /// <summary>
        /// Part of url path which will be used as cache key for authorization token.
        /// </summary>
        public string CacheKeyUrlPathRegexPattern { get; set; }

        /// <summary>
        /// Cache key prefix for authorization token caching.
        /// </summary>
        /// <remarks>
        /// Default is "jwtToken".
        /// </remarks>
        public string CacheKeyPrefix { get; set; } = "jwtToken";
    }
}
