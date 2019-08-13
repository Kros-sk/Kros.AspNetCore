using System;

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
        /// Cache sliding expiration offset.
        /// </summary>
        /// <remarks>
        /// Default is <see cref="TimeSpan.Zero"/>.
        /// </remarks>
        public TimeSpan CacheSlidingExpirationOffset { get; set; } = TimeSpan.Zero;
    }
}
