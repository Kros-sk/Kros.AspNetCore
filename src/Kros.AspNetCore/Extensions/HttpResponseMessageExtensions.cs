using Kros.AspNetCore.Exceptions;
using System;
using System.Net.Http;

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
    }
}
