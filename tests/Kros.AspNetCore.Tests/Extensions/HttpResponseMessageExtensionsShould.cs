using FluentAssertions;
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
        public void ThrowsForbidenExceptionForStatusCode403()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<ResourceIsForbiddenException>();
        }

        [Fact]
        public void ThrowsPaymentRequiredExceptionForStatusCode402()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.PaymentRequired };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<ResourceIsForbiddenException>();
        }

        [Fact]
        public void ThrowsNotFoundExceptionForStatusCode404()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<NotFoundException>();
        }

        [Fact]
        public void ThrowsUnathorizedxceptionForStatusCode401()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<UnauthorizedAccessException>();
        }

        [Fact]
        public void ThrowsBadRequestExceptionForStatusCode400()
        {
            var response = new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad request reason")
            };

            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<BadRequestException>().WithMessage("Bad request reason");
        }

        [Fact]
        public void ThrowsRequestConflictExceptionForStatusCode409()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<RequestConflictException>();
        }

        [Fact]
        public void ThrowUnknownStatusCodeExceptionWithCodeInMessageForUnsupportedStatusCode()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InsufficientStorage };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<UnknownStatusCodeException>().WithMessage($"*{(int)HttpStatusCode.InsufficientStorage}*");
        }

        [Fact]
        public void ThrowsGivenDefaultExceptionForUnsupportedStatusCode()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InsufficientStorage };
            Action act = () => response.ThrowIfNotSuccessStatusCode(new WebException());
            act.Should().ThrowExactly<WebException>();
        }

        [Fact]
        public void NotThrowsExceptionForSuccessStatusCode()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().NotThrow();
        }
    }
}
