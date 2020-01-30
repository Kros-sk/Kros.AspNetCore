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
        /// Returns exception object, depends on status code in http response.
        /// </summary>
        /// <param name="response">Http response message.</param>
        /// <param name="defaultException">Optional default exception to be returned on unsupported http code. Default is System.Exception</param>
        public static Exception GetExceptionForStatusCode(this HttpResponseMessage response, Exception defaultException = null)
        {
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Forbidden:
                    return new ResourceIsForbiddenException();
                case System.Net.HttpStatusCode.NotFound:
                    return new NotFoundException();
                case System.Net.HttpStatusCode.Unauthorized:
                    return new UnauthorizedAccessException(Properties.Resources.AuthorizationServiceForbiddenRequest);
                case System.Net.HttpStatusCode.BadRequest:
                    return new BadRequestException();
                default:
                    if (defaultException != null)
                    {
                        return defaultException;
                    }
                    else
                    {
                        return new Exception();
                    }
            }
        }
    }
}
