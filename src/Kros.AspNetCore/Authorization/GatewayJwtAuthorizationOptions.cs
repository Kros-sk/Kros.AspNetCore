using Kros.AspNetCore.ServiceDiscovery;
using Kros.Extensions;
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
            => AuthorizationUrl ?? GetUrl(provider, HashAuthorization);

        /// <summary>
        /// Hash authorization url.
        /// </summary>
        public string HashAuthorizationUrl { get; set; }

        /// <summary>
        /// Gets or sets the authorization.
        /// </summary>
        public AuthorizationServiceOptions HashAuthorization { get; set; }

        internal string GetHashAuthorization(IServiceDiscoveryProvider provider)
            => HashAuthorizationUrl ?? GetUrl(provider, HashAuthorization);

        private string GetUrl(IServiceDiscoveryProvider provider, AuthorizationServiceOptions authorization)
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
        /// Paths that do not use JWT authorization caching
        /// </summary>
        public List<string> IgnoredPathForCache { get; private set; } = new List<string>();
    }
}
