using Kros.AspNetCore.Exceptions;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Extensions for HttpResponseMessage
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Throws exception, depends on status code in http response, if HTTP response was not successful.
        /// </summary>
        /// <param name="response">Http response message.</param>
        /// <param name="defaultException">Optional default exception to be returned on unsupported http code. Default is <see cref="UnknownStatusCodeException"/></param>
        public static void ThrowIfNotSuccessStatusCode(this HttpResponseMessage response, Exception defaultException = null)
        {
            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.Forbidden:
                        throw new ResourceIsForbiddenException();
                    case System.Net.HttpStatusCode.NotFound:
                        throw new NotFoundException();
                    case System.Net.HttpStatusCode.Unauthorized:
                        throw new UnauthorizedAccessException(Properties.Resources.AuthorizationServiceForbiddenRequest);
                    case System.Net.HttpStatusCode.PaymentRequired:
                        throw new PaymentRequiredException();
                    case System.Net.HttpStatusCode.BadRequest:
                        throw new BadRequestException(response.Content.ReadAsStringAsync().Result);
                    case System.Net.HttpStatusCode.Conflict:
                        throw new RequestConflictException();
                    default:
                        if (defaultException != null)
                        {
                            throw defaultException;
                        }
                        else
                        {
                            throw new UnknownStatusCodeException(response.StatusCode);
                        }
                }
            }
        }

        /// <summary>
        /// Throws exception depending on status code in HTTP response, if HTTP response was not successful.
        /// Persists response payload in exception.
        /// </summary>
        /// <param name="response">Http response message.</param>
        /// <param name="defaultException">Optional default exception to be returned on unsupported http code. Default is <see cref="UnknownStatusCodeException"/></param>
        public static async Task ThrowIfNotSuccessStatusCodeAndKeepPayload(this HttpResponseMessage response, Exception defaultException = null)
        {
            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.PaymentRequired:
                        throw (await GetExceptionWithResponseContent<PaymentRequiredException>(response));

                    default:
                        ThrowIfNotSuccessStatusCode(response, defaultException);
                        break;
                }
            }
        }

        private static async Task<T> GetExceptionWithResponseContent<T>(HttpResponseMessage response)
            where T : RequestUnsuccessfulException, new()
        {
            MediaTypeHeaderValue contentType = response.Content?.Headers.ContentType;
            string content = await response.Content?.ReadAsStringAsync();

            var exception = new T();
            if (!string.IsNullOrEmpty(content))
            {
                exception.AddPayload(content, contentType);
            }

            return exception;
        }
    }
}
