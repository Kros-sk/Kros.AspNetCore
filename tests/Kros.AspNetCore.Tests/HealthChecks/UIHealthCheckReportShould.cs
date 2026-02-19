using Kros.AspNetCore.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using Xunit;

namespace Kros.AspNetCore.Tests.HealthChecks
{
    public class UIHealthCheckReportShould
    {
        private static HealthReport CreateHealthReport(HealthStatus status)
        {
            Dictionary<string, HealthReportEntry> entries = new();
            entries.Add("Health_Test_Key", new HealthReportEntry(status, null, new TimeSpan(0, 0, 5), null, null));

            HealthReport report = new(entries, new TimeSpan(0, 0, 5));

            return report;
        }

        private static HealthReport CreateMultiStatusHealthReport(List<HealthStatus> statuses)
        {
            Dictionary<string, HealthReportEntry> entries = new();

            foreach (HealthStatus status in statuses)
            {
                entries.Add(
                    $"Health_Test_Key_{statuses.IndexOf(status)}",
                    new HealthReportEntry(status, null, new TimeSpan(0, 0, 5), null, null));
            }

            HealthReport report = new(entries, new TimeSpan(0, 0, 5));

            return report;
        }

        [Fact]
        public void CreateUIReportFromReport()
        {
            UIHealthCheckReport uiReport = UIHealthCheckReport.CreateFrom(CreateHealthReport(HealthStatus.Healthy));

            Assert.NotNull(uiReport);
        }

        [Theory()]
        [InlineData(HealthStatus.Degraded)]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.Unhealthy)]
        public void CreateUIReportFromSpecificReportStatus(HealthStatus healthStatus)
        {
            UIHealthCheckReport uiReport = UIHealthCheckReport.CreateFrom(CreateHealthReport(healthStatus));

            Assert.Equal(healthStatus, uiReport.Status);
        }

        [Fact]
        public void CreateUIReportFromReportWithDuration()
        {
            UIHealthCheckReport uiReport = UIHealthCheckReport.CreateFrom(CreateHealthReport(HealthStatus.Healthy));

            Assert.Equal(5, uiReport.TotalDuration.Seconds);
        }

        [Fact]
        public void CreateUIReportFromReportWithEntries()
        {
            HealthReport healthReport = CreateHealthReport(HealthStatus.Healthy);
            UIHealthCheckReport uiReport = UIHealthCheckReport.CreateFrom(healthReport);

            Assert.Equal(healthReport.Entries.Count, uiReport.Entries.Count);
        }

        [Fact]
        public void CreateUIReportFromReportWithDifferentStatusesHealthyFirst()
        {
            HealthReport healthReport = CreateMultiStatusHealthReport(new List<HealthStatus>() {
                HealthStatus.Healthy,
                HealthStatus.Unhealthy
            });
            UIHealthCheckReport uiReport = UIHealthCheckReport.CreateFrom(healthReport);

            Assert.Equal(HealthStatus.Unhealthy, uiReport.Status);
        }

        [Fact]
        public void CreateUIReportFromReportWithDifferentStatusesUnhealthyFirst()
        {
            HealthReport healthReport = CreateMultiStatusHealthReport(new List<HealthStatus>() {
                HealthStatus.Unhealthy,
                HealthStatus.Healthy
            });
            UIHealthCheckReport uiReport = UIHealthCheckReport.CreateFrom(healthReport);

            Assert.Equal(HealthStatus.Unhealthy, uiReport.Status);
        }
    }
}
