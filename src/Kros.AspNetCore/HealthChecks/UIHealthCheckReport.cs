using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Kros.AspNetCore.HealthChecks
{
    /// <summary>
    /// Models for health checks UI Client. This models represent a indirection between HealthChecks API and
    /// UI Client in order to implement some features not present on HealthChecks of substiture
    /// some properties etc.
    /// </summary>
    public class UIHealthCheckReport
    {
        /// <summary>
        /// Health check status.
        /// </summary>
        public HealthStatus Status { get; set; }

        /// <summary>
        /// Health check duration.
        /// </summary>
        public TimeSpan TotalDuration { get; set; }

        /// <summary>
        /// Health check entries.
        /// </summary>
        public Dictionary<string, HealthCheckReportEntry> Entries { get; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="totalDuration"></param>
        public UIHealthCheckReport(Dictionary<string, HealthCheckReportEntry> entries, TimeSpan totalDuration)
        {
            Entries = entries;
            TotalDuration = totalDuration;
        }

        /// <summary>
        /// Create health check report.
        /// </summary>
        /// <param name="report">Report.</param>
        public static UIHealthCheckReport CreateFrom(HealthReport report)
        {
            var uiReport = new UIHealthCheckReport(new Dictionary<string, HealthCheckReportEntry>(), report.TotalDuration)
            {
                Status = report.Status,
            };

            foreach (var item in report.Entries)
            {
                var entry = new HealthCheckReportEntry()
                {
                    Data = item.Value.Data,
                    Description = item.Value.Description,
                    Duration = item.Value.Duration,
                    Status = item.Value.Status
                };

                if (item.Value.Exception != null)
                {
                    var message = item.Value.Exception
                        .Message;

                    entry.Exception = message;
                    entry.Description = item.Value.Description ?? message;
                }

                uiReport.Entries.Add(item.Key, entry);
            }

            return uiReport;
        }
    }
}
