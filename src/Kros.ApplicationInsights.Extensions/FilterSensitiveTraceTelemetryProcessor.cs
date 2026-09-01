using OpenTelemetry;
using OpenTelemetry.Logs;
using System;
using System.Collections.Generic;

namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Log processor which redacts log records containing sensitive data.
    /// </summary>
    /// <remarks>
    /// OpenTelemetry log records cannot be dropped from a processor, so a matching record has its message
    /// and attributes replaced by <see cref="RedactedMessage"/>. The sensitive values therefore never leave
    /// the process, which is what this processor is for.
    /// </remarks>
    /// <seealso cref="BaseProcessor{T}" />
    public sealed class FilterSensitiveTraceTelemetryProcessor : BaseProcessor<LogRecord>
    {
        /// <summary>
        /// Message which replaces the original one when sensitive data is detected.
        /// </summary>
        public const string RedactedMessage = "[Redacted: log record contained sensitive data.]";

        private static readonly string[] _sensitivePatterns =
        [
            "Request Headers:\nAuthorization:",
            "Request Headers:\r\nAuthorization:",
            "Request Headers:\nx-functions-key:",
            "Request Headers:\r\nx-functions-key:"
        ];

        private static readonly IReadOnlyList<KeyValuePair<string, object>> _emptyAttributes = [];

        /// <summary>
        /// Redacts the log record when it contains sensitive data.
        /// </summary>
        /// <param name="data">Log record which has just been emitted.</param>
        public override void OnEnd(LogRecord data)
        {
            if (HasSensitiveData(data))
            {
                Redact(data);
            }
        }

        private static bool HasSensitiveData(LogRecord data)
            => IsSensitive(data.FormattedMessage) || IsSensitive(data.Body);

        private static bool IsSensitive(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            foreach (string pattern in _sensitivePatterns)
            {
                if (message.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void Redact(LogRecord data)
        {
            data.FormattedMessage = RedactedMessage;
            data.Body = RedactedMessage;

            // The exporter reads the values from the attributes, so the original message has to be
            // removed from there as well.
            data.Attributes = _emptyAttributes;
        }
    }
}
