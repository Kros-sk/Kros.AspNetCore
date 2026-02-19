using Kros.AspNetCore.Exceptions;
using Kros.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.ErrorHandling
{
    public class ErrorHandlingMiddlewareShould
    {
        private static readonly (Exception ex, int statusCode)[] _knownExceptions = new (Exception, int)[]
        {
            (new JsonPatchException(), StatusCodes.Status400BadRequest),
            (new BadRequestException(), StatusCodes.Status400BadRequest),
            (new UnauthorizedAccessException(), StatusCodes.Status401Unauthorized),
            (new PaymentRequiredException(), StatusCodes.Status402PaymentRequired),
            (new ResourceIsForbiddenException(), StatusCodes.Status403Forbidden),
            (new NotFoundException(), StatusCodes.Status404NotFound),
            (new TimeoutException(), StatusCodes.Status408RequestTimeout),
            (new RequestConflictException(), StatusCodes.Status409Conflict)
        };

        [Theory]
        [InlineData(StatusCodes.Status200OK, StatusCodes.Status500InternalServerError)]
        [InlineData(StatusCodes.Status201Created, StatusCodes.Status500InternalServerError)]
        [InlineData(StatusCodes.Status204NoContent, StatusCodes.Status500InternalServerError)]
        [InlineData(StatusCodes.Status401Unauthorized, StatusCodes.Status401Unauthorized)]
        public async Task ChangeSuccessStatusCodeTo500AndRethrowException(int requestStatusCode, int responseStatusCode)
        {
            DefaultHttpContext context = new();
            ErrorHandlingMiddleware middleware = new(innerHttpContext =>
            {
                innerHttpContext.Response.StatusCode = requestStatusCode;
                throw new Exception("Exception");
            }, Substitute.For<ILogger<ErrorHandlingMiddleware>>());

            Func<Task> action = async () => await middleware.Invoke(context);

            Exception exception = await Assert.ThrowsAsync<Exception>(action);
            Assert.Equal("Exception", exception.Message);
            Assert.Equal(responseStatusCode, context.Response.StatusCode);
        }

        [Theory]
        [InlineData(StatusCodes.Status200OK)]
        [InlineData(StatusCodes.Status201Created)]
        [InlineData(StatusCodes.Status204NoContent)]
        [InlineData(StatusCodes.Status401Unauthorized)]
        public async Task NotChangeSuccessStatusCodeIfResponseHasStartedAndRethrowException(int responseStatusCode)
        {
            HttpResponse response = Substitute.For<HttpResponse>();
            response.HasStarted.Returns(true);
            response.StatusCode = responseStatusCode;

            HttpContext context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            ErrorHandlingMiddleware middleware = new(innerHttpContext =>
            {
                throw new Exception();
            }, Substitute.For<ILogger<ErrorHandlingMiddleware>>());

            Func<Task> action = async () => await middleware.Invoke(context);
            await Assert.ThrowsAsync<Exception>(action);

            Assert.Equal(responseStatusCode, context.Response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(ReturnCorectStatusCodeForExceptionData))]
        public async Task ReturnCorectStatusCodeForException(Exception exception, int expectedStatusCode)
        {
            DefaultHttpContext context = new();
            context.Response.StatusCode = StatusCodes.Status100Continue;

            ErrorHandlingMiddleware middleware = new(innerHttpContext =>
            {
                throw exception;
            }, Substitute.For<ILogger<ErrorHandlingMiddleware>>());

            await middleware.Invoke(context);

            Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        }

        public static TheoryData<Exception, int> ReturnCorectStatusCodeForExceptionData()
        {
            TheoryData<Exception, int> data = new();
            _knownExceptions.ForEach(item => data.Add(item.ex, item.statusCode));
            return data;
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

            ErrorHandlingMiddleware middleware = new(innerHttpContext =>
            {
                throw exception;
            }, Substitute.For<ILogger<ErrorHandlingMiddleware>>());

            await middleware.Invoke(context);

            Assert.Equal(StatusCodes.Status100Continue, context.Response.StatusCode);
        }

        public static TheoryData<Exception> NotChangeStatusCodeIfResponseHasStartedData()
        {
            TheoryData<Exception> data = new();
            _knownExceptions.ForEach(item => data.Add(item.ex));
            return data;
        }
    }
}
