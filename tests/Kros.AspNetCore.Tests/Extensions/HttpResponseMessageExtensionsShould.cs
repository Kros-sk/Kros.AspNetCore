using Kros.AspNetCore.Exceptions;
using Kros.AspNetCore.Extensions;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class HttpResponseMessageExtensionsShould
    {
        [Fact]
        public void ThrowsForbiddenExceptionForStatusCode403()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.Forbidden };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            Assert.Throws<ResourceIsForbiddenException>(act);
        }

        [Fact]
        public void ThrowsPaymentRequiredExceptionForStatusCode402()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.PaymentRequired };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            Assert.Throws<PaymentRequiredException>(act);
        }

        [Fact]
        public void ThrowsNotFoundExceptionForStatusCode404()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.NotFound };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            Assert.Throws<NotFoundException>(act);
        }

        [Fact]
        public void ThrowsUnathorizedxceptionForStatusCode401()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.Unauthorized };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            Assert.Throws<UnauthorizedAccessException>(act);
        }

        [Fact]
        public void ThrowsBadRequestExceptionForStatusCode400()
        {
            HttpResponseMessage response = new()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad request reason")
            };

            Action act = () => response.ThrowIfNotSuccessStatusCode();
            BadRequestException exception = Assert.Throws<BadRequestException>(act);
            Assert.Contains("Bad request reason", exception.Message);
        }

        [Fact]
        public void ThrowsRequestConflictExceptionForStatusCode409()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.Conflict };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            Assert.Throws<RequestConflictException>(act);
        }

        [Fact]
        public void ThrowUnknownStatusCodeExceptionWithCodeInMessageForUnsupportedStatusCode()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.InsufficientStorage };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            UnknownStatusCodeException exception = Assert.Throws<UnknownStatusCodeException>(act);
            Assert.Contains(((int)HttpStatusCode.InsufficientStorage).ToString(), exception.Message);
        }

        [Fact]
        public void ThrowsGivenDefaultExceptionForUnsupportedStatusCode()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.InsufficientStorage };
            Action act = () => response.ThrowIfNotSuccessStatusCode(new WebException());
            Assert.Throws<WebException>(act);
        }

        [Fact]
        public void NotThrowsExceptionForSuccessStatusCode()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.OK };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            Exception exception = Record.Exception(act);
            Assert.Null(exception);
        }
    }
}
