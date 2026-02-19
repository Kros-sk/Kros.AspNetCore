using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kros.AspNetCore.HealthChecks
{
    /// <summary>
    /// Response writer for JSON output.
    /// </summary>
    public static class HealthCheckResponseWriter
    {
        private const string DefaultContentType = "application/json";

        internal static readonly JsonSerializerOptions _serializeOptions = CreateSerializerOptions();

        private static JsonSerializerOptions CreateSerializerOptions()
        {
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        /// <summary>
        /// Add health report as json to HttpContext response.
        /// </summary>
        /// <param name="httpContext">Http context.</param>
        /// <param name="report">Report for health.</param>
        public static Task WriteHealthCheckResponseAsync(HttpContext httpContext, HealthReport report)
        {
            string response = "{}";

            if (report != null)
            {
                httpContext.Response.ContentType = DefaultContentType;
                UIHealthCheckReport uiReport = UIHealthCheckReport.CreateFrom(report);
                response = JsonSerializer.Serialize(uiReport, _serializeOptions);
            }

            return httpContext.Response.WriteAsync(response);
        }
    }
}
