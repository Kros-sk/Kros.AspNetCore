using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class FilterSensitiveTraceTelemetryProcessorShould
    {
        [Fact]
        public void PassTelemetryWhenMessageDoesNotContainSensitiveData()
        {
            ExportedRecord record = Log("This is a harmless message.");

            Assert.Equal("This is a harmless message.", record.Message);
        }

        [Theory]
        [InlineData("Request Headers:\r\nAuthorization: SecretValue")]
        [InlineData("Request Headers:\r\nx-functions-key: SecretValue")]
        [InlineData("Request Headers:\nAuthorization: SecretValue")]
        [InlineData("Request Headers:\nx-functions-key: SecretValue")]
        public void RedactTelemetryWhenMessageContainsSensitiveData(string sensitiveMessage)
        {
            ExportedRecord record = Log(sensitiveMessage);

            Assert.Equal(FilterSensitiveTraceTelemetryProcessor.RedactedMessage, record.Message);
            Assert.DoesNotContain("SecretValue", record.Message);
            Assert.DoesNotContain(record.Attributes, a => a.Contains("SecretValue"));
        }

        [Fact]
        public void PassTelemetryWhenSensitiveDataIsNotAtTheBeginning()
        {
            ExportedRecord record = Log("All good. Request Headers:\r\nAuthorization: SecretValue");

            Assert.NotEqual(FilterSensitiveTraceTelemetryProcessor.RedactedMessage, record.Message);
        }

        private static ExportedRecord Log(string message)
        {
            List<ExportedRecord> exported = new();

            using (ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder
                .AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.AddProcessor(new FilterSensitiveTraceTelemetryProcessor());
                    options.AddProcessor(new SimpleLogRecordExportProcessor(new CapturingExporter(exported)));
                })))
            {
                loggerFactory.CreateLogger("Test").LogInformation("{Message}", message);
            }

            return Assert.Single(exported);
        }

        private sealed record ExportedRecord(string Message, IReadOnlyList<string> Attributes);

        private sealed class CapturingExporter : BaseExporter<LogRecord>
        {
            private readonly List<ExportedRecord> _exported;

            public CapturingExporter(List<ExportedRecord> exported) => _exported = exported;

            public override ExportResult Export(in Batch<LogRecord> batch)
            {
                foreach (LogRecord record in batch)
                {
                    // Values have to be copied, log records are pooled and reused after the export.
                    _exported.Add(new ExportedRecord(
                        record.FormattedMessage ?? record.Body,
                        record.Attributes?.Select(a => $"{a.Key}={a.Value}").ToList() ?? new List<string>()));
                }

                return ExportResult.Success;
            }
        }
    }
}
