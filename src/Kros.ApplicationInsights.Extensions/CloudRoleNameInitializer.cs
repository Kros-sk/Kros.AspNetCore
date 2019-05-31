using Kros.Utils;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Configuration of service name representation in Application Insights.
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer" />
    public class CloudRoleNameInitializer : ITelemetryInitializer
    {
        private readonly string _serviceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudRoleNameInitializer"/> class.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        public CloudRoleNameInitializer(string serviceName)
        {
            _serviceName = Check.NotNullOrWhiteSpace(serviceName, nameof(serviceName));
        }

        /// <summary>
        /// Initializes properties of the specified <see cref="T:Microsoft.ApplicationInsights.Channel.ITelemetry" /> object.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = _serviceName;
            telemetry.Context.Cloud.RoleInstance = _serviceName;
        }
    }
}
