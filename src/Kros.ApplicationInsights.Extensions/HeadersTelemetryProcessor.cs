using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OpenTelemetry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Adds selected request headers to request telemetry.
    /// </summary>
    /// <seealso cref="BaseProcessor{T}" />
    internal sealed class HeadersTelemetryProcessor : BaseProcessor<Activity>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HeadersToCaptureOptions _headersToCapture;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeadersTelemetryProcessor"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Instance of IHttpContextAccessor.</param>
        /// <param name="headersToCapture">Headers which are added to the telemetry.</param>
        public HeadersTelemetryProcessor(
            IHttpContextAccessor httpContextAccessor,
            IOptions<HeadersToCaptureOptions> headersToCapture)
        {
            _httpContextAccessor = Check.NotNull(httpContextAccessor, nameof(httpContextAccessor));
            _headersToCapture = Check.NotNull(headersToCapture.Value, nameof(headersToCapture));
        }

        /// <inheritdoc />
        public override void OnEnd(Activity activity)
        {
            if (!activity.IsRequest())
            {
                return;
            }

            HttpContext context = _httpContextAccessor.HttpContext;
            if (context is null)
            {
                return;
            }

            foreach (string headerKey in _headersToCapture)
            {
                if (context.Request.Headers.TryGetValue(headerKey, out StringValues headerValue))
                {
                    activity.SetTag(_headersToCapture.PropertyNameResolver(headerKey), headerValue.ToString());
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
