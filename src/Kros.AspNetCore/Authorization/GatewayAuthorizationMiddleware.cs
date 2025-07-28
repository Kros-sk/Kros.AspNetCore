using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Middleware for user authorization.
/// </summary>
internal class GatewayAuthorizationMiddleware
{
    /// <summary>
    /// HttpClient name used for communication between ApiGateway and Authorization service.
    /// </summary>
    public const string AuthorizationHttpClientName = "JwtAuthorizationClientName";

    private readonly RequestDelegate _next;
    private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="next">Next middleware.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    public GatewayAuthorizationMiddleware(
        RequestDelegate next,
        GatewayJwtAuthorizationOptions jwtAuthorizationOptions)
    {
        _next = Check.NotNull(next, nameof(next));
        _jwtAuthorizationOptions = Check.NotNull(jwtAuthorizationOptions, nameof(jwtAuthorizationOptions));
    }

    /// <summary>
    /// HttpContext pipeline processing.
    /// </summary>
    /// <param name="httpContext">Http context.</param>
    /// <param name="jwtProvider">JWT provider service.</param>
    public async Task Invoke(
        HttpContext httpContext,
        IJwtTokenProvider jwtProvider)
    {
        string userJwt = await GetUserAuthorizationJwtAsync(httpContext, jwtProvider);

        if (!string.IsNullOrEmpty(userJwt))
        {
            AddUserProfileClaimsToIdentityAndHttpHeaders(httpContext, userJwt);
        }

        await _next(httpContext);
    }

    private async Task<string> GetUserAuthorizationJwtAsync(
        HttpContext httpContext,
        IJwtTokenProvider jwtProvider)
    {
        if (JwtAuthorizationHelper.TryGetTokenValue(httpContext.Request.Headers, out string token))
        {
            return await jwtProvider.GetJwtTokenAsync(token);
        }
        else if (!string.IsNullOrEmpty(_jwtAuthorizationOptions.HashParameterName)
            && httpContext.Request.Query.TryGetValue(_jwtAuthorizationOptions.HashParameterName, out StringValues hashValue))
        {
            return await jwtProvider.GetJwtTokenForHashAsync(hashValue);
        }

        return string.Empty;
    }

    private static void AddUserProfileClaimsToIdentityAndHttpHeaders(HttpContext httpContext, string userJwtToken)
        => httpContext.Request.Headers[HeaderNames.Authorization] = $"{JwtAuthorizationHelper.AuthTokenPrefix} {userJwtToken}";
}
