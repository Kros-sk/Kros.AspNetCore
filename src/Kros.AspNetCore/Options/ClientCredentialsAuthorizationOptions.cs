using System;

namespace Kros.AspNetCore.Options
{
    /// <summary>
    /// Option for client credentials authorization middleware.
    /// </summary>
    public class ClientCredentialsAuthorizationOptions
    {
        /// <summary>
        /// Identity server url.
        /// </summary>
        public string AuthorityUrl { get; set; }

        /// <summary>
        /// Identity scope.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Clock skew to apply when validating a time.
        /// </summary>
        public TimeSpan ClockSkew { get; set; }
    }
}
