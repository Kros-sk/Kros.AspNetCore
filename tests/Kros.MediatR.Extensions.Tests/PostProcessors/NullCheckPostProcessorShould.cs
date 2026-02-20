using Kros.AspNetCore.Exceptions;
using Kros.MediatR.PostProcessors;
using MediatR;
using System;
using System.Threading;
using Xunit;

namespace Kros.MediatR.Extensions.Tests.PostProcessors
{
    public class NullCheckPostProcessorShould
    {
        [Fact]
        public void ThrowExceptionWhenResponseIsNull()
        {
            NullCheckPostProcessor<IRequest<string>, string> postProcessor = new(NullCheckPostProcessorOptions.Default);

            Action action = () => postProcessor.Process(null, null, CancellationToken.None);

            Assert.Throws<NotFoundException>(action);
        }

        [Fact]
        public void DoNotThrowExceptionWhenResponseIsNotNull()
        {
            NullCheckPostProcessor<IRequest<string>, string> postProcessor = new(NullCheckPostProcessorOptions.Default);

            Action action = () => postProcessor.Process(null, "response", CancellationToken.None);

            Exception exception = Record.Exception(action);
            Assert.Null(exception);
        }

        [Fact]
        public void DoNotCheckResponseIfNullCheckIgnoreIsSet()
        {
            NullCheckPostProcessorOptions options = new();
            options.IgnoreRequest<Request>();

            NullCheckPostProcessor<Request, string> postProcessor = new(options);

            Action action = () => postProcessor.Process(new Request(), null, CancellationToken.None);

            Exception exception = Record.Exception(action);
            Assert.Null(exception);
        }

        public class Request : IRequest<string> { }
    }
}
