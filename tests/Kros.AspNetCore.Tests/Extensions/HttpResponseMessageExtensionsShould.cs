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
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<ResourceIsForbiddenException>();
        }

        [Fact]
        public void ReturnsNotFoundExceptionForStatusCode404()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<NotFoundException>();
        }

        [Fact]
        public void ReturnsUnathorizedxceptionForStatusCode401()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized };
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<UnauthorizedAccessException>();
        }

        [Fact]
        public void ReturnsBadRequestExceptionForStatusCode400()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<BadRequestException>();
        }

        [Fact]
        public void ReturnsRequestConflictExceptionForStatusCode409()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict };
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<RequestConflictException>();
        }

        [Fact]
        public void ReturnUnknownStatusCodeExceptionWithCodeInMessageForUnsupportedStatusCode()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };
            Exception ex = response.GetExceptionForStatusCode();
            ex.Should().BeOfType<UnknownStatusCodeException>();
            ex.Message.Should().Contain(((int) HttpStatusCode.NoContent).ToString());
        }

        [Fact]
        public void ReturnsGivenDefaultExceptionForUnsupportedStatusCode()
        {
            HttpResponseMessage response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };
            Exception ex = response.GetExceptionForStatusCode(new WebException());
            ex.Should().BeOfType<WebException>();
        }
    }
}
