using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace Kros.ApplicationInsights.Extensions
{
    internal class RoutePatternInitializer : ITelemetryInitializer
    {
        private const string RoutePatternClaimType = "route_pattern";
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutePatternInitializer"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Instance of IHttpContextAccessor.</param>
        public RoutePatternInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is RequestTelemetry request)
            {
                AddProperty(request, RoutePatternClaimType);
            }
        }

        private void AddProperty(RequestTelemetry requestTelemetry, string claimType)
        {
            string claimValue = GetClaimValue(claimType);
            if (!string.IsNullOrEmpty(claimValue))
            {
                requestTelemetry.Properties.Add(claimType, claimValue);
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

            return claim != null ? claim.Value : string.Empty;
        }
    }
}
