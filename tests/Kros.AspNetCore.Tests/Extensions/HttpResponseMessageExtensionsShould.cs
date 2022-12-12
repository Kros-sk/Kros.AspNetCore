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
        public void ThrowsForbiddenExceptionForStatusCode403()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.Forbidden };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<ResourceIsForbiddenException>();
        }

        [Fact]
        public void ThrowsPaymentRequiredExceptionForStatusCode402()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.PaymentRequired };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<PaymentRequiredException>();
        }

        [Fact]
        public void ThrowsNotFoundExceptionForStatusCode404()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.NotFound };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<NotFoundException>();
        }

        [Fact]
        public void ThrowsUnathorizedxceptionForStatusCode401()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.Unauthorized };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<UnauthorizedAccessException>();
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
            act.Should().ThrowExactly<BadRequestException>().WithMessage("Bad request reason");
        }

        [Fact]
        public void ThrowsRequestConflictExceptionForStatusCode409()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.Conflict };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<RequestConflictException>();
        }

        [Fact]
        public void ThrowUnknownStatusCodeExceptionWithCodeInMessageForUnsupportedStatusCode()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.InsufficientStorage };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().ThrowExactly<UnknownStatusCodeException>().WithMessage($"*{(int)HttpStatusCode.InsufficientStorage}*");
        }

        [Fact]
        public void ThrowsGivenDefaultExceptionForUnsupportedStatusCode()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.InsufficientStorage };
            Action act = () => response.ThrowIfNotSuccessStatusCode(new WebException());
            act.Should().ThrowExactly<WebException>();
        }

        [Fact]
        public void NotThrowsExceptionForSuccessStatusCode()
        {
            HttpResponseMessage response = new() { StatusCode = HttpStatusCode.OK };
            Action act = () => response.ThrowIfNotSuccessStatusCode();
            act.Should().NotThrow();
        }
    }
}
