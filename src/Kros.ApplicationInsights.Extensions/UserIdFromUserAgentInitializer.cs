using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Configuration of user id in Application Insights telemetry.
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer" />
    class UserIdFromUserAgentInitializer : ITelemetryInitializer
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
            if (telemetry is RequestTelemetry requestTelemetry)
            {
                requestTelemetry.Context.User.Id = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];
            }
        }
    }
}
