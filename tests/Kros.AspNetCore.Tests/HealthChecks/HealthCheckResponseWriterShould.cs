using FluentAssertions;
using Kros.AspNetCore.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Kros.AspNetCore.Tests.HealthChecks
{
    public class HealthCheckResponseWriterShould
    {
        private static HealthReport CreateHealthReport(HealthStatus status)
        {
            Dictionary<string, HealthReportEntry> entries = new();
            entries.Add("Health_Test_Key", new HealthReportEntry(status, null, new TimeSpan(0, 0, 5), null, null));

            HealthReport report = new(entries, new TimeSpan(0, 0, 5));

            return report;
        }

        [Fact]
        public async void WriteToHttpResponse()
        {
            HealthReport report = CreateHealthReport(HealthStatus.Healthy);

            DefaultHttpContext context = new();
            context.Response.Body = new MemoryStream();

            await HealthCheckResponseWriter.WriteHealthCheckResponseAsync(context, report);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseBody = new StreamReader(context.Response.Body).ReadToEnd();

            UIHealthCheckReport uiHealthReport = JsonConvert.DeserializeObject<UIHealthCheckReport>(responseBody);

            uiHealthReport.Status.Should().Be(HealthStatus.Healthy);
            uiHealthReport.TotalDuration.Seconds.Should().Be(5);
            uiHealthReport.Entries.Count.Should().Be(report.Entries.Count);
        }
    }
}
