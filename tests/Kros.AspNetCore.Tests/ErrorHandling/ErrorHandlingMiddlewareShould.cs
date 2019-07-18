using FluentAssertions;
using Kros.AspNetCore.Exceptions;
using Kros.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.ErrorHandling
{
    public class ErrorHandlingMiddlewareShould
    {
        private static readonly (Exception ex, int statusCode)[] _knownExceptions = new (Exception, int)[]
        {
            (new BadRequestException(), StatusCodes.Status400BadRequest),
            (new UnauthorizedAccessException(), StatusCodes.Status401Unauthorized),
            (new ResourceIsForbiddenException(), StatusCodes.Status403Forbidden),
            (new NotFoundException(), StatusCodes.Status404NotFound),
            (new TimeoutException(), StatusCodes.Status408RequestTimeout)
        };

        [Theory]
        [InlineData(StatusCodes.Status200OK, StatusCodes.Status500InternalServerError)]
        [InlineData(StatusCodes.Status201Created, StatusCodes.Status500InternalServerError)]
        [InlineData(StatusCodes.Status204NoContent, StatusCodes.Status500InternalServerError)]
        [InlineData(StatusCodes.Status401Unauthorized, StatusCodes.Status401Unauthorized)]
        public void ChangeSuccessStatusCodeTo500AndRethrowException(int requestStatusCode, int responseStatusCode)
        {
            var context = new DefaultHttpContext();
            var middleware = new ErrorHandlingMiddleware(innerHttpContext =>
            {
                innerHttpContext.Response.StatusCode = requestStatusCode;
                throw new Exception("Exception");
            }, Substitute.For<ILogger<ErrorHandlingMiddleware>>());

            Func<Task> action = async () => await middleware.Invoke(context);

            action.Should().Throw<Exception>().WithMessage("Exception");
            context.Response.StatusCode.Should().Be(responseStatusCode);
        }

        [Theory]
        [InlineData(StatusCodes.Status200OK)]
        [InlineData(StatusCodes.Status201Created)]
        [InlineData(StatusCodes.Status204NoContent)]
        [InlineData(StatusCodes.Status401Unauthorized)]
        public void NotChangeSuccessStatusCodeIfResponseHasStartedAndRethrowException(int responseStatusCode)
        {
            HttpResponse response = Substitute.For<HttpResponse>();
            response.HasStarted.Returns(true);
            response.StatusCode = responseStatusCode;

            HttpContext context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var middleware = new ErrorHandlingMiddleware(innerHttpContext =>
            {
                throw new Exception();
            }, Substitute.For<ILogger<ErrorHandlingMiddleware>>());

            Func<Task> action = async () => await middleware.Invoke(context);
            action.Should().Throw<Exception>();

            context.Response.StatusCode.Should().Be(responseStatusCode);
        }

        [Theory]
        [MemberData(nameof(ReturnCorectStatusCodeForExceptionData))]
        public async Task ReturnCorectStatusCodeForException(Exception exception, int expectedStatusCode)
        {
            var context = new DefaultHttpContext();
            context.Response.StatusCode = StatusCodes.Status100Continue;

            var middleware = new ErrorHandlingMiddleware(innerHttpContext =>
            {
                throw exception;
            }, Substitute.For<ILogger<ErrorHandlingMiddleware>>());

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be(expectedStatusCode);
        }

        public static IEnumerable<object[]> ReturnCorectStatusCodeForExceptionData()
        {
            foreach ((Exception ex, int statusCode) in _knownExceptions)
            {
                yield return new object[] { ex, statusCode };
            }
        }

        [Theory]
        [MemberData(nameof(NotChangeStatusCodeIfResponseHasStartedData))]
        public async Task NotChangeStatusCodeIfResponseHasStarted(Exception exception)
        {
            HttpResponse response = Substitute.For<HttpResponse>();
            response.HasStarted.Returns(true);
            response.StatusCode = StatusCodes.Status100Continue;

            HttpContext context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var middleware = new ErrorHandlingMiddleware(innerHttpContext =>
            {
                throw exception;
            }, Substitute.For<ILogger<ErrorHandlingMiddleware>>());

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be(StatusCodes.Status100Continue);
        }

        public static IEnumerable<object[]> NotChangeStatusCodeIfResponseHasStartedData()
        {
            foreach ((Exception ex, int _) in _knownExceptions)
            {
                yield return new object[] { ex };
            }
        }
    }
}
