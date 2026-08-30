using Microsoft.AspNetCore.Http;
using OpenTelemetry;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Adds the route pattern claim of the current user to request telemetry.
    /// </summary>
    /// <seealso cref="BaseProcessor{T}" />
    internal sealed class RoutePatternProcessor : BaseProcessor<Activity>
    {
        private const string RoutePatternClaimType = "route_pattern";
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutePatternProcessor"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Instance of IHttpContextAccessor.</param>
        public RoutePatternProcessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public override void OnEnd(Activity activity)
        {
            if (!activity.IsRequest())
            {
                return;
            }

            string claimValue = GetClaimValue(RoutePatternClaimType);
            if (!string.IsNullOrEmpty(claimValue))
            {
                activity.SetTag(RoutePatternClaimType, claimValue);
            }
        }

        private string GetClaimValue(string claimType)
        {
            Claim claim = _httpContextAccessor?.
                HttpContext?.
                User?.
                Identities?.
                FirstOrDefault(i => i.Claims.Any(c => c.Type == claimType))?.
                Claims?.
                FirstOrDefault(c => c.Type == claimType);

            return claim?.Value;
        }
    }
}
