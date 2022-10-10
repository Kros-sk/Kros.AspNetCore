using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Extension methods for HttpClient class.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Get response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<HttpResponseMessage> GetAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await client.GetAsync(uri, cancellationToken);
            await response.ThrowIfNotSuccessStatusCodeAndKeepPayload(defaultException);

            return response;
        }

        /// <summary>
        /// Get response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<HttpResponseMessage> GetAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
            await response.ThrowIfNotSuccessStatusCodeAndKeepPayload(defaultException);

            return response;
        }

        /// <summary>
        /// Get response from http request (with a body) and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="body">Request body.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<HttpResponseMessage> GetAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            object body,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
        {
            string jsonBody = JsonSerializer.Serialize(body);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri,
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
            await response.ThrowIfNotSuccessStatusCodeAndKeepPayload(defaultException);

            return response;
        }

        /// <summary>
        /// Get response from http request (with a body) and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="body">Request body.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static Task<HttpResponseMessage> GetAndCheckResponseAsync(
            this HttpClient client,
            string url,
            object body,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
            => client.GetAndCheckResponseAsync(new Uri(url), body, defaultException, cancellationToken);

        /// <summary>
        /// Get string response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<string> GetStringAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
            => await (await client.GetAndCheckResponseAsync(uri, defaultException, cancellationToken))
                .Content.ReadAsStringAsync();

        /// <summary>
        /// Get string response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<string> GetStringAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
            => await (await client.GetAndCheckResponseAsync(url, defaultException, cancellationToken))
                .Content.ReadAsStringAsync();

        /// <summary>
        /// Get stream response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<Stream> GetStreamAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
            => await (await client.GetAndCheckResponseAsync(uri, defaultException, cancellationToken))
                .Content.ReadAsStreamAsync();

        /// <summary>
        /// Get stream response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<Stream> GetStreamAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
            => await (await client.GetAndCheckResponseAsync(url, defaultException, cancellationToken))
                .Content.ReadAsStreamAsync();

        /// <summary>
        /// Get stream response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="body">Request body.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<Stream> GetStreamAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            object body,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
            => await (await client.GetAndCheckResponseAsync(uri, body, defaultException, cancellationToken))
                .Content.ReadAsStreamAsync();

        /// <summary>
        /// Get stream response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="body">Request body.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<Stream> GetStreamAndCheckResponseAsync(
            this HttpClient client,
            string url,
            object body,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
            => await (await client.GetAndCheckResponseAsync(url, body, defaultException, cancellationToken))
                .Content.ReadAsStreamAsync();

        /// <summary>
        /// Get byte array response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<byte[]> GetByteArrayAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
            => await (await client.GetAndCheckResponseAsync(uri, defaultException, cancellationToken))
                .Content.ReadAsByteArrayAsync();

        /// <summary>
        /// Get byte array response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<byte[]> GetByteArrayAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null,
            CancellationToken cancellationToken = default)
            => await (await client.GetAndCheckResponseAsync(url, defaultException, cancellationToken))
                .Content.ReadAsByteArrayAsync();
    }
}
