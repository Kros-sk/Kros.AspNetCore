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
        [Theory]
        [MemberData(nameof(RethrowExceptionAndReturnCorectStatusCodeForExceptionData))]
        public void RethrowExceptionAndReturnCorectStatusCodeForException(int requestStatusCode, int responseStatusCode)
        {
            ErrorHandlingMiddleware middleware = CreateMiddleware(new Exception("Exception"), requestStatusCode);
            var context = new DefaultHttpContext();

            Func<Task> action = async () => await middleware.Invoke(context);

            action.Should().Throw<Exception>().WithMessage("Exception");
            context.Response.StatusCode.Should().Be(responseStatusCode);
        }

        public static IEnumerable<object[]> RethrowExceptionAndReturnCorectStatusCodeForExceptionData()
        {
            yield return new object[] { StatusCodes.Status200OK, StatusCodes.Status500InternalServerError };
            yield return new object[] { StatusCodes.Status201Created, StatusCodes.Status500InternalServerError };
            yield return new object[] { StatusCodes.Status204NoContent, StatusCodes.Status500InternalServerError };
            yield return new object[] { StatusCodes.Status400BadRequest, StatusCodes.Status400BadRequest };
        }

        [Theory]
        [MemberData(nameof(ReturnCorectStatusCodeForExceptionData))]
        public async void ReturnCorectStatusCodeForException(Exception exception, int statusCode)
        {
            ErrorHandlingMiddleware middleware = CreateMiddleware(exception);
            var context = new DefaultHttpContext();

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be(statusCode);
        }

        public static IEnumerable<object[]> ReturnCorectStatusCodeForExceptionData()
        {
            yield return new object[] { new ResourceIsForbiddenException(), StatusCodes.Status403Forbidden };
            yield return new object[] { new NotFoundException(), StatusCodes.Status404NotFound };
            yield return new object[] { new TimeoutException(), StatusCodes.Status408RequestTimeout };
            yield return new object[] { new UnauthorizedAccessException(), StatusCodes.Status401Unauthorized };
        }

        private static ErrorHandlingMiddleware CreateMiddleware(Exception exception, int responseStatusCode)
        {
            return new ErrorHandlingMiddleware((innerHttpContext) =>
            {
                innerHttpContext.Response.StatusCode = responseStatusCode;
                throw exception;
            }, Substitute.For<ILogger<ErrorHandlingMiddleware>>());
        }

        private static ErrorHandlingMiddleware CreateMiddleware(Exception exception)
        {
            return new ErrorHandlingMiddleware((innerHttpContext) =>
            {
                throw exception;
            }, Substitute.For<ILogger<ErrorHandlingMiddleware>>());
        }
    }
}
