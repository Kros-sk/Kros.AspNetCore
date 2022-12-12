using FluentAssertions;
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

            action.Should().Throw<NotFoundException>();
        }

        [Fact]
        public void DoNotThrowExceptionWhenResponseIsNotNull()
        {
            NullCheckPostProcessor<IRequest<string>, string> postProcessor = new(NullCheckPostProcessorOptions.Default);

            Action action = () => postProcessor.Process(null, "response", CancellationToken.None);

            action.Should().NotThrow<NotFoundException>();
        }

        [Fact]
        public void DoNotCheckResponseIfNullCheckIgnoreIsSet()
        {
            NullCheckPostProcessorOptions options = new();
            options.IgnoreRequest<Request>();

            NullCheckPostProcessor<Request, string> postProcessor = new(options);

            Action action = () => postProcessor.Process(new Request(), null, CancellationToken.None);

            action.Should().NotThrow<NotFoundException>();
        }

        public class Request : IRequest<string> { }
    }
}
