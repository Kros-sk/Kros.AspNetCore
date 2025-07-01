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
    [Obsolete("Use the Schemes property instead. This property will be removed in a future version.")]
    public string ApiKey { get; set; }

    /// <summary> 
    /// Auth scheme name (single scheme mode).
    /// </summary>
    [Obsolete("Use the Schemes property instead. This property will be removed in a future version.")]
    public string Scheme { get; set; }

    /// <summary>
    /// Multiple authentication schemes with their respective API keys.
    /// If specified, this takes precedence over the single Scheme/ApiKey properties.
    /// Key: Scheme name, Value: API key.
    /// </summary>
    public Dictionary<string, string> Schemes { get; set; } = [];
}
