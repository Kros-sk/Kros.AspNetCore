using FluentAssertions;
using Kros.AspNetCore.Exceptions;
using Kros.AspNetCore.Extensions;
using NSubstitute;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class HttpResponseMessageExtensionsShould
    {
        [Fact]
        public void ReturnsForbideenExceptionForStatusCode403()
        {
            HttpResponseMessage response = CreateHttpResponseMessageSubstituteWithCode(HttpStatusCode.Forbidden);
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<ResourceIsForbiddenException>();
        }

        [Fact]
        public void ReturnsNotFoundExceptionForStatusCode404()
        {
            HttpResponseMessage response = CreateHttpResponseMessageSubstituteWithCode(HttpStatusCode.NotFound);
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<NotFoundException>();
        }

        [Fact]
        public void ReturnsUnathorizedxceptionForStatusCode401()
        {
            HttpResponseMessage response = CreateHttpResponseMessageSubstituteWithCode(HttpStatusCode.Unauthorized);
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<UnauthorizedAccessException>();
        }

        [Fact]
        public void ReturnsBadRequestExceptionForStatusCode400()
        {
            HttpResponseMessage response = CreateHttpResponseMessageSubstituteWithCode(HttpStatusCode.BadRequest);
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<BadRequestException>();
        }

        [Fact]
        public void ReturnsSystemExceptionForUnsupportedStatusCode()
        {
            HttpResponseMessage response = CreateHttpResponseMessageSubstituteWithCode(HttpStatusCode.NoContent);
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<Exception>();
        }

        [Fact]
        public void ReturnsGivenDefaultExceptionForUnsupportedStatusCode()
        {
            HttpResponseMessage response = CreateHttpResponseMessageSubstituteWithCode(HttpStatusCode.NoContent);
            Exception ex = response.GetExceptionForStatusCode(new WebException());
            ex.Should().BeOfType<WebException>();
        }

        private static HttpResponseMessage CreateHttpResponseMessageSubstituteWithCode(HttpStatusCode code)
        {
            HttpResponseMessage response = Substitute.For<HttpResponseMessage>();
            response.StatusCode = code;
            return response;
        }
    }
}
