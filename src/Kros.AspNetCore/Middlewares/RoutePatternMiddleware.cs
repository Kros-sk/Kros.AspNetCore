using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Middlewares
{
    /// <summary>
    /// Middleware which adds route pattern claim.
    /// </summary>
    public class RoutePatternMiddleware
    {
        private const string RoutePatternClaimType = "route_pattern";

        private readonly RequestDelegate _next;

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="next">Delegate for next middleware.</param>
        public RoutePatternMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invoke.
        /// </summary>
        /// <param name="context">Current http context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            finally
            {
                string rawText = context.Request.Method;
                var endpointFeature = context.Features[typeof(Microsoft.AspNetCore.Http.Features.IEndpointFeature)]
                                               as Microsoft.AspNetCore.Http.Features.IEndpointFeature;
                var endpoint = endpointFeature?.Endpoint;

                if (endpoint is RouteEndpoint e && e?.RoutePattern != null)
                {
                    rawText += $" {e.RoutePattern.RawText}";
                }

                var claimsIdentity = new ClaimsIdentity(new[] { new Claim(RoutePatternClaimType, rawText) });
                context.User.AddIdentity(claimsIdentity);
            }
        }
    }
}
