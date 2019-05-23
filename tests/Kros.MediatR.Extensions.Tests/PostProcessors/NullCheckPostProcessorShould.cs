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
            var postProcessor = new NullCheckPostProcessor<string, string>(NullCheckPostProcessorOptions.Default);

            Action action = () => postProcessor.Process("", null, CancellationToken.None);

            action.Should().Throw<NotFoundException>();
        }

        [Fact]
        public void DoNotThrowExceptionWhenResponseIsNotNull()
        {
            var postProcessor = new NullCheckPostProcessor<string, string>(NullCheckPostProcessorOptions.Default);

            Action action = () => postProcessor.Process("request", "response", CancellationToken.None);

            action.Should().NotThrow<NotFoundException>();
        }

        [Fact]
        public void DoNotCheckResponseIfNullCheckIgnoreIsSet()
        {
            var options = new NullCheckPostProcessorOptions();
            options.IgnoreRequest<Request>();

            var postProcessor = new NullCheckPostProcessor<Request, string>(options);

            Action action = () => postProcessor.Process(new Request(), null, CancellationToken.None);

            action.Should().NotThrow<NotFoundException>();
        }

        public class Request : IRequest<string> { }
    }
}
