using Microsoft.AspNetCore.Authentication;

namespace Kros.AspNetCore.Authentication
{
    /// <summary>
    /// Options to configure the ApiKeyBasicAuthenticationHandler.
    /// </summary>
    public class ApiKeyBasicAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// The API key, that will be checked during authentication.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Auth scheme name.
        /// </summary>
        public string Scheme { get; set; }
    }
}
