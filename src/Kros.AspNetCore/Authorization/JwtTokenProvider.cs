using Kros.AspNetCore.Extensions;
using Kros.AspNetCore.ServiceDiscovery;
using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Service for fetching JWT tokens from authorization service.
/// </summary>
internal class JwtTokenProvider : IJwtTokenProvider
{
    /// <summary>
    /// HttpClient name used for communication between ApiGateway and Authorization service.
    /// </summary>
    public const string AuthorizationHttpClientName = "JwtAuthorizationClientName";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;
    private readonly IServiceDiscoveryProvider _serviceDiscoveryProvider;

    /// <summary>
    /// Initializes a new instance of the JwtProvider class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <param name="serviceDiscoveryProvider">The service discovery provider.</param>
    public JwtTokenProvider(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        GatewayJwtAuthorizationOptions jwtAuthorizationOptions,
        IServiceDiscoveryProvider serviceDiscoveryProvider)
    {
        _httpClientFactory = Check.NotNull(httpClientFactory, nameof(httpClientFactory));
        _httpContextAccessor = Check.NotNull(httpContextAccessor, nameof(httpContextAccessor));
        _jwtAuthorizationOptions = Check.NotNull(jwtAuthorizationOptions, nameof(jwtAuthorizationOptions));
        _serviceDiscoveryProvider = Check.NotNull(serviceDiscoveryProvider, nameof(serviceDiscoveryProvider));
    }

    /// <inheritdoc/>
    public async Task<string> GetJwtTokenAsync(StringValues token)
    {
        HttpContext httpContext = _httpContextAccessor.HttpContext;
        string authUrl = _jwtAuthorizationOptions.GetAuthorizationUrl(_serviceDiscoveryProvider)
            + httpContext.Request.Path.Value;
        return await GetJwtTokenAsync(httpContext, token, authUrl);
    }

    /// <summary>
    ///  <inheritdoc/>
    /// </summary>
    public async Task<string> GetJwtTokenForHashAsync(StringValues hashValue)
    {
        UriBuilder uriBuilder = new(_jwtAuthorizationOptions.GetHashAuthorization(_serviceDiscoveryProvider))
        {
            Query = QueryString
                .Create(_jwtAuthorizationOptions.HashParameterName, hashValue.ToString())
                .ToUriComponent()
        };
        return await GetJwtTokenAsync(_httpContextAccessor.HttpContext, StringValues.Empty, uriBuilder.Uri.ToString());
    }

    private async Task<string> GetJwtTokenAsync(
        HttpContext httpContext,
        StringValues authHeader,
        string authorizationUrl)
    {
        using (HttpClient client = _httpClientFactory.CreateClient(AuthorizationHttpClientName))
        {
            if (authHeader.Count != 0)
            {
                client.DefaultRequestHeaders.Add(HeaderNames.Authorization, authHeader.ToString());
            }
            if (_jwtAuthorizationOptions.ForwardedHeaders.Count != 0)
            {
                AddForwardedHeaders(client, httpContext.Request.Headers);
            }

            string jwtToken = await client.GetStringAndCheckResponseAsync(authorizationUrl,
                new UnauthorizedAccessException(Properties.Resources.AuthorizationServiceForbiddenRequest));

            return jwtToken;
        }
    }

    private void AddForwardedHeaders(HttpClient client, IHeaderDictionary headers)
    {
        foreach (string headerName in _jwtAuthorizationOptions.ForwardedHeaders)
        {
            if (headers.TryGetValue(headerName, out StringValues value))
            {
                client.DefaultRequestHeaders.Add(headerName, (IEnumerable<string>)value);
            }
        }
    }
}
