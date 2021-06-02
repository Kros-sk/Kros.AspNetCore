using System.Net;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Class which represent settings for Identity server handler.
    /// </summary>
    internal class IdentityServerOptions
    {
        /// <summary>
        /// Name of the API.
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// Api secret.
        /// </summary>
        public string ApiSecret { get; set; }

        /// <summary>
        /// Identity server URL.
        /// </summary>
        public string AuthorityUrl { get; set; }

        /// <summary>
        /// Proxy configuration. If the value is <see langword="null"/> then proxy is not set.
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// Specifies whether HTTPS is required for the discovery endpoint.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = true;

        /// <summary>
        /// Authentication scheme. Required when configuring more than one handler.
        /// </summary>
        public string AuthenticationScheme { get; set; }
    }
}
