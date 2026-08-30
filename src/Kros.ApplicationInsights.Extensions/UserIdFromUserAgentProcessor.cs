using Microsoft.AspNetCore.Http;
using OpenTelemetry;
using System.Diagnostics;

namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Sets the user id of request telemetry from the <c>User-Agent</c> header.
    /// </summary>
    /// <seealso cref="BaseProcessor{T}" />
    internal sealed class UserIdFromUserAgentProcessor : BaseProcessor<Activity>
    {
        private const string UserAgentHeaderName = "User-Agent";
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserIdFromUserAgentProcessor"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Instance of IHttpContextAccessor.</param>
        public UserIdFromUserAgentProcessor(IHttpContextAccessor httpContextAccessor)
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

            string userAgent = GetUserAgent();
            if (!string.IsNullOrEmpty(userAgent))
            {
                activity.SetTag(ActivityTags.EnduserId, userAgent);
            }
        }

        private string GetUserAgent()
            => _httpContextAccessor.HttpContext?.Request.Headers[UserAgentHeaderName].ToString();
    }
}
