using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;

namespace Kros.AspNetCore.Authentication;

/// <summary> 
/// Options to configure the ApiKeyBasicAuthenticationHandler. 
/// </summary> 
public class ApiKeyBasicAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary> 
    /// The API key, that will be checked during authentication (single scheme mode).
    /// </summary> 
    public string ApiKey { get; set; }

    /// <summary> 
    /// Auth scheme name (single scheme mode).
    /// </summary> 
    public string Scheme { get; set; }

    /// <summary>
    /// Multiple authentication schemes with their respective API keys.
    /// If specified, this takes precedence over the single Scheme/ApiKey properties.
    /// Key: Scheme name, Value: API key.
    /// </summary>
    public Dictionary<string, string> Schemes { get; set; } = [];

    /// <summary>
    /// Retrieves the collection of authentication schemes and their associated keys. If specified, multiple schemes
    /// take precedence over the single Scheme/ApiKey properties. Key: Scheme name, Value: API key.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> GetSchemesWithApiKeys()
    {
        if (Schemes?.Count > 0)
        {
            return Schemes;
        }
        return [new KeyValuePair<string, string>(Scheme, ApiKey)];
    }

    /// <summary>
    /// Gets the API key for a specific scheme.
    /// </summary>
    /// <param name="schemeName">The name of the scheme to get the API key for.</param>
    /// <returns>The API key if found, otherwise null.</returns>
    public string GetApiKeyForScheme(string schemeName)
    {
        if (Schemes?.Count > 0)
        {
            return Schemes.TryGetValue(schemeName, out string key) ? key : null;
        }

        return string.Equals(Scheme, schemeName, StringComparison.Ordinal) ? ApiKey : null;
    }
}
