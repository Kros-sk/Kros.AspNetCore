using Microsoft.OpenApi;
using System.Collections.Generic;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Settings for Swagger documentation.
    /// </summary>
    public class SwaggerSettings : OpenApiInfo
    {
        /// <summary>
        /// OAuth client ID.
        /// </summary>
        public string OAuthClientId { get; set; } = string.Empty;

        /// <summary>
        /// OAuth client secret.
        /// </summary>
        public string OAuthClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// OAuth scopes. There is usually no need to set this property.
        /// If the property is empty, all scopes from OAuth security schemes are automatically added.
        /// </summary>
        public List<string> OAuthScopes { get; } = [];

        /// <summary>
        /// Authorization security schemes.
        /// </summary>
        public Dictionary<string, OpenApiSecurityScheme> Authorizations { get; } = [];
    }
}
