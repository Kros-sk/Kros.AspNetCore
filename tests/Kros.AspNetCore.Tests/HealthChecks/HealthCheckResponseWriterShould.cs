using Kros.AspNetCore.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
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
        public async Task WriteToHttpResponse()
        {
            HealthReport report = CreateHealthReport(HealthStatus.Healthy);

            DefaultHttpContext context = new();
            context.Response.Body = new MemoryStream();

            await HealthCheckResponseWriter.WriteHealthCheckResponseAsync(context, report);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseBody = new StreamReader(context.Response.Body).ReadToEnd();

            UIHealthCheckReport uiHealthReport = JsonSerializer.Deserialize<UIHealthCheckReport>(responseBody,
                HealthCheckResponseWriter._serializeOptions);

            Assert.Equal(HealthStatus.Healthy, uiHealthReport.Status);
            Assert.Equal(5, uiHealthReport.TotalDuration.Seconds);
            Assert.Equal(report.Entries.Count, uiHealthReport.Entries.Count);
        }
    }
}
