using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Kros.ApplicationInsights.Extensions.Tests")]
namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Configuration of user id in Application Insights telemetry.
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer" />
    internal class UserIdFromUserAgentInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserIdFromUserAgentInitializer"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Instance of IHttpContextAccessor.</param>
        public UserIdFromUserAgentInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Initializes properties of the specified <see cref="T:Microsoft.ApplicationInsights.Channel.ITelemetry" /> object.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            bool? contiansAgent = _httpContextAccessor?.HttpContext?.Request?.Headers?.TryGetValue(
                "User-Agent",
                out StringValues userAgent);
            if (telemetry is RequestTelemetry requestTelemetry
                && contiansAgent.HasValue
                && contiansAgent.Value)
            {
                requestTelemetry.Context.User.Id = userAgent;
            }
        }
    }
}
