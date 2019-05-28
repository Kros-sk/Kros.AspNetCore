using Kros.Utils;
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

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="responseMessage">Response which <see cref="HttpClient"/> return.</param>
        public FakeHttpMessageHandler(HttpResponseMessage responseMessage)
        {
            _fakeResponse = Check.NotNull(responseMessage, nameof(responseMessage));
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => await Task.FromResult(_fakeResponse);
    }
}
