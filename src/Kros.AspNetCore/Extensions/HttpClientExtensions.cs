﻿using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
        public static async Task<HttpResponseMessage> GetAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            Exception defaultException = null)
        {
            HttpResponseMessage response = await client.GetAsync(uri);
            await response.ThrowIfNotSuccessStatusCodeAndKeepPayload(defaultException);

            return response;
        }

        /// <summary>
        /// Get response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        public static async Task<HttpResponseMessage> GetAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null)
        {
            HttpResponseMessage response = await client.GetAsync(url);
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
        public static async Task<HttpResponseMessage> GetAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            object body,
            Exception defaultException = null)
        {
            string jsonBody = JsonSerializer.Serialize(body);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri,
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await client.SendAsync(request);
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
        public static Task<HttpResponseMessage> GetAndCheckResponseAsync(
            this HttpClient client,
            string url,
            object body,
            Exception defaultException = null)
            => client.GetAndCheckResponseAsync(new Uri(url), body, defaultException);

        /// <summary>
        /// Get string response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
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
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
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
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
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
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        public static async Task<Stream> GetStreamAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null)
            => await (await client.GetAndCheckResponseAsync(url, defaultException)).Content.ReadAsStreamAsync();

        /// <summary>
        /// Get stream response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="body">Request body.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        public static async Task<Stream> GetStreamAndCheckResponseAsync(
            this HttpClient client,
            Uri uri,
            object body,
            Exception defaultException = null)
            => await (await client.GetAndCheckResponseAsync(uri, body, defaultException)).Content.ReadAsStreamAsync();

        /// <summary>
        /// Get stream response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="url">Url for request.</param>
        /// <param name="body">Request body.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        public static async Task<Stream> GetStreamAndCheckResponseAsync(
            this HttpClient client,
            string url,
            object body,
            Exception defaultException = null)
            => await (await client.GetAndCheckResponseAsync(url, body, defaultException)).Content.ReadAsStreamAsync();

        /// <summary>
        /// Get byte array response from http request and throws exception if request is unsuccessful.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="uri">Uri for request.</param>
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
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
        /// <param name="defaultException">Default exception to be thrown, if failure result code is unsupported.</param>
        public static async Task<byte[]> GetByteArrayAndCheckResponseAsync(
            this HttpClient client,
            string url,
            Exception defaultException = null)
            => await (await client.GetAndCheckResponseAsync(url, defaultException)).Content.ReadAsByteArrayAsync();
    }
}
