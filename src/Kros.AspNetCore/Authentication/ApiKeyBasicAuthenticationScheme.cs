using Microsoft.AspNetCore.Authentication;

namespace Kros.AspNetCore.Authentication;

/// <summary>
/// Internally used settings for the API key authentication scheme.
/// </summary>
public class ApiKeyBasicAuthenticationScheme : AuthenticationSchemeOptions
{
    /// <summary> 
    /// The API key, that will be checked during authentication.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary> 
    /// Auth scheme name (single scheme mode).
    /// </summary>
    public string SchemeName { get; set; }
}
