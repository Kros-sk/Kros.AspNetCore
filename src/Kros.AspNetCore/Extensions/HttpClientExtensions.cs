using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Extensions
{
    public static class HttpClientExtensions
    {

        /// <summary>
        /// Get response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="defaultException">Default exception to by thrown, if failure result code is unsupported.</param>
        public static async Task<HttpResponseMessage> GetAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            Exception defaultException = null)
        {
            HttpResponseMessage response = await client.GetAsync(uri);
            response.ThrowIfNotSuccessStatusCode(defaultException);
            return response;
        }

        /// <summary>
        /// Get response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="defaultException">Default exception to by thrown, if failure result code is unsupported.</param>
        public static async Task<HttpResponseMessage> GetAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.ThrowIfNotSuccessStatusCode(defaultException);
            return response;
        }

        /// <summary>
        /// Get string response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="defaultException">Default exception to by thrown, if failure result code is unsupported.</param>
        public static async Task<string> GetStringAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            Exception defaultException = null)
            => await (await client.GetAndCheckResponseAsync(uri, defaultException)).Content.ReadAsStringAsync();

        /// <summary>
        /// Get string response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="defaultException">Default exception to by thrown, if failure result code is unsupported.</param>
        public static async Task<string> GetStringAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null)
            => await (await client.GetAndCheckResponseAsync(url, defaultException)).Content.ReadAsStringAsync();

        /// <summary>
        /// Get stream response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="defaultException">Default exception to by thrown, if failure result code is unsupported.</param>
        public static async Task<Stream> GetStreamAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            Exception defaultException = null)
            => await (await client.GetAndCheckResponseAsync(uri, defaultException)).Content.ReadAsStreamAsync();

        /// <summary>
        /// Get stream response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="defaultException">Default exception to by thrown, if failure result code is unsupported.</param>
        public static async Task<Stream> GetStreamAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null)
            => await (await client.GetAndCheckResponseAsync(url, defaultException)).Content.ReadAsStreamAsync();

        /// <summary>
        /// Get byte array response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="defaultException">Default exception to by thrown, if failure result code is unsupported.</param>
        public static async Task<byte[]> GetByteArrayAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            Exception defaultException = null)
            => await (await client.GetAndCheckResponseAsync(uri, defaultException)).Content.ReadAsByteArrayAsync();

        /// <summary>
        /// Get byte array response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="defaultException">Default exception to by thrown, if failure result code is unsupported.</param>
        public static async Task<byte[]> GetByteArrayAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null)
            => await (await client.GetAndCheckResponseAsync(url, defaultException)).Content.ReadAsByteArrayAsync();
    }
}
