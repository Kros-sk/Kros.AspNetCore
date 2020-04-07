using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Tests
{
    /// <summary>
    /// Fake HttpMessageHandler for testing methods, which sending request through <see cref="HttpClient"/>.
    /// </summary>
    public class FakeHttpMessageHandler : DelegatingHandler
    {
        private HttpResponseMessage _fakeResponse;
        private IEnumerable<KeyValuePair<HttpRequestFilter, HttpResponseMessage>> _specificResponses;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="responseMessage">Response which <see cref="HttpClient"/> return.</param>
        public FakeHttpMessageHandler(HttpResponseMessage responseMessage)
        {
            _fakeResponse = Check.NotNull(responseMessage, nameof(responseMessage));
        }

        public delegate bool HttpRequestFilter(HttpRequestMessage request);

        public FakeHttpMessageHandler(IEnumerable<KeyValuePair<HttpRequestFilter, HttpResponseMessage>> specificResponses)
        {
            _specificResponses = Check.NotNull(specificResponses, nameof(specificResponses));
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_fakeResponse != null)
            {
                return await Task.FromResult(_fakeResponse);
            }
            else
            {
                foreach (var requestResponsePair in _specificResponses)
                {
                    if (requestResponsePair.Key(request))
                        return await Task.FromResult(requestResponsePair.Value);
                }

                throw new ArgumentException();
            }
        }
    }
}
