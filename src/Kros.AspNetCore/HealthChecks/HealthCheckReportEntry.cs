using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Kros.AspNetCore.HealthChecks
{
    /// <summary>
    /// Info about health check report.
    /// </summary>
    public class HealthCheckReportEntry
    {
        /// <summary>
        /// Health check data.
        /// </summary>
        public IReadOnlyDictionary<string, object> Data { get; set; }

        /// <summary>
        /// Health check description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Health check duration.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Health check exception.
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// Health check status.
        /// </summary>
        public HealthStatus Status { get; set; }
    }
}
