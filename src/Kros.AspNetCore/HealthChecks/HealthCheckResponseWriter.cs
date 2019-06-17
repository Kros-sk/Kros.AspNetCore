using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace Kros.AspNetCore.HealthChecks
{
    /// <summary>
    /// Response writer for JSON output.
    /// </summary>
    public static class HealthCheckResponseWriter
    {
        private const string DefaultContentType = "application/json";

        /// <summary>
        /// Add health report as json to HttpContext response.
        /// </summary>
        /// <param name="httpContext">Http context.</param>
        /// <param name="report">Report for health.</param>
        public static Task WriteHealthCheckResponseAsync(HttpContext httpContext, HealthReport report)
        {
            var response = "{}";

            if (report != null)
            {
                var settings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };

                settings.Converters.Add(new StringEnumConverter());

                httpContext.Response.ContentType = DefaultContentType;

                var uiReport = UIHealthCheckReport
                    .CreateFrom(report);

                response = JsonConvert.SerializeObject(uiReport, settings);
            }

            return httpContext.Response.WriteAsync(response);
        }
    }
}
