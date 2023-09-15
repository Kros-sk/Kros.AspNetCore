using Kros.Utils;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kros.ApplicationInsights.Extensions
{
    internal class HeadersTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HeadersToCaptureOptions _headersToCapture;

        public HeadersTelemetryInitializer(
            IHttpContextAccessor httpContextAccessor,
            IOptions<HeadersToCaptureOptions> headersToCapture)
        {
            _httpContextAccessor = Check.NotNull(httpContextAccessor, nameof(httpContextAccessor));
            _headersToCapture = Check.NotNull(headersToCapture.Value, nameof(headersToCapture));
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is RequestTelemetry requestTelemetry)
            {
                HttpContext context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    foreach (string headerKey in _headersToCapture)
                    {
                        if (context.Request.Headers.TryGetValue(headerKey, out StringValues headerValue))
                        {
                            requestTelemetry.Properties[_headersToCapture.PropertyNameResolver(headerKey)]
                                = headerValue.ToString();
                        }
                    }
                }
            }
        }

        internal class HeadersToCaptureOptions : IEnumerable<string>
        {
            private readonly HashSet<string> _headersToCapture = new(StringComparer.OrdinalIgnoreCase);

            public Func<string, string> PropertyNameResolver { get; set; } = (headerKey) => $"Header-{headerKey}";

            public IEnumerator<string> GetEnumerator() => _headersToCapture.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public HeadersToCaptureOptions Add(params string[] headersToCapture)
            {
                foreach (string headerKey in headersToCapture)
                {
                    Add(headerKey);
                }
                return this;
            }

            private void Add(string headerKey)
                => _headersToCapture.Add(Check.NotNullOrWhiteSpace(headerKey, nameof(headerKey)));
        }
    }
}
