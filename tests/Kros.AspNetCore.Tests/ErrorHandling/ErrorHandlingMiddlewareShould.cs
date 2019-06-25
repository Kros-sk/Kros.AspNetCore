using FluentAssertions;
using Kros.AspNetCore.Exceptions;
using Kros.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.ErrorHandling
{
    public class ErrorHandlingMiddlewareShould
    {
        [Fact]
        public void Return500StatusCodeAndThrowException()
        {
            ErrorHandlingMiddleware middleware = CreateMiddleware(new Exception("Exception"), StatusCodes.Status200OK);
            var context = new DefaultHttpContext();

            Func<Task> action = async () => await middleware.Invoke(context);

            action.Should().Throw<Exception>().WithMessage("Exception");
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public void DontChangeStatusCodeAndThrowException()
        {
            ErrorHandlingMiddleware middleware = CreateMiddleware(new Exception("Exception"), StatusCodes.Status400BadRequest);
            var context = new DefaultHttpContext();

            Func<Task> action = async () => await middleware.Invoke(context);

            action.Should().Throw<Exception>().WithMessage("Exception");
            context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async void CatchResourceIsForbiddenException()
        {
            ErrorHandlingMiddleware middleware = CreateMiddleware(new ResourceIsForbiddenException());
            var context = new DefaultHttpContext();

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async void CatchNotFoundException()
        {
            ErrorHandlingMiddleware middleware = CreateMiddleware(new NotFoundException());
            var context = new DefaultHttpContext();

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async void CatchTimeoutException()
        {
            ErrorHandlingMiddleware middleware = CreateMiddleware(new TimeoutException());
            var context = new DefaultHttpContext();

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be(StatusCodes.Status408RequestTimeout);
        }

        [Fact]
        public async void CatchUnauthorizedAccessException()
        {
            ErrorHandlingMiddleware middleware = CreateMiddleware(new UnauthorizedAccessException());
            var context = new DefaultHttpContext();

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
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
