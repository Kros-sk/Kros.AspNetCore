using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using MediatR.Pipeline;
using Kros.MediatR.PostProcessors;
using Kros.AspNetCore.Exceptions;
using System;
using System.Reflection;

namespace Kros.MediatR.Extensions.Tests
{
    public class ServiceCollectionExtensionsShould
    {
        #region Nested class

        public interface IFooRequest { }

        public interface IFooResponse { }

        public class FooRequest : IRequest<FooRequest.FooResponse>, IFooRequest
        {
            public class FooResponse : IFooResponse
            {
            }
        }

        public class FooPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IFooRequest, IRequest<TResponse>
            where TResponse : IFooResponse
        {
            public async Task<TResponse> Handle(
                TRequest request,
                RequestHandlerDelegate<TResponse> next,
                CancellationToken cancellationToken)
            {
                TResponse result = await next();
                return result;
            }
        }

        public interface IBarRequest { }

        public interface IBarResponse { }

        public class BarRequest : IRequest<BarRequest.BarResponse>, IBarRequest
        {
            public class BarResponse : IBarResponse
            {
            }
        }

        public class Bar1Request : IRequest<Bar1Request.BarResponse>, IBarRequest
        {
            public class BarResponse : IBarResponse
            {
            }
        }

        public class BarPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>, IBarRequest
            where TResponse : IBarResponse
        {
            public async Task<TResponse> Handle(
                TRequest request,
                RequestHandlerDelegate<TResponse> next,
                CancellationToken cancellationToken)
            {
                TResponse result = await next();
                return result;
            }
        }

        public interface ITestRequest { }

        public interface ITestResponse { }

        public class TestRequest : IRequest<TestRequest.TestResponse>, ITestRequest
        {
            public class TestResponse
            {
            }
        }

        public class TestPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : ITestRequest, IRequest<TResponse>
            where TResponse : ITestResponse
        {
            public async Task<TResponse> Handle(
                TRequest request,
                RequestHandlerDelegate<TResponse> next,
                CancellationToken cancellationToken)
            {
                TResponse result = await next();
                return result;
            }
        }

        public interface ICommandRequest { }

        public class TestCommand : IRequest<Unit>, ICommandRequest
        {
        }

        public class CommandPipelineBehavior<TRequest> : IPipelineBehavior<TRequest, Unit>
            where TRequest : ICommandRequest, IRequest<Unit>
        {
            public async Task<Unit> Handle(
                TRequest request,
                RequestHandlerDelegate<Unit> next,
                CancellationToken cancellationToken)
            {
                Unit result = await next();
                return result;
            }
        }

        #endregion

        public IServiceCollection Services { get; } = new ServiceCollection();

        private ServiceProvider _provider;

        public ServiceProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = Services.BuildServiceProvider();
                }

                return _provider;
            }
        }

        [Fact]
        public void RegisterPipelineBehaviorsForRequestType()
        {
            Services.AddPipelineBehaviorsForRequest<IFooRequest>();

            IPipelineBehavior<FooRequest, FooRequest.FooResponse> behavior =
                Provider.GetRequiredService<IPipelineBehavior<FooRequest, FooRequest.FooResponse>>();

            behavior.Should().BeAssignableTo<FooPipelineBehavior<FooRequest, FooRequest.FooResponse>>();
        }

        [Fact]
        public void RegisterPipelineBehaviorsForRequestTypeWhenMoreRequestsImplementInterface()
        {
            Services.AddPipelineBehaviorsForRequest<IBarRequest>();

            IPipelineBehavior<BarRequest, BarRequest.BarResponse> behavior =
                Provider.GetRequiredService<IPipelineBehavior<BarRequest, BarRequest.BarResponse>>();
            behavior.Should().BeAssignableTo<BarPipelineBehavior<BarRequest, BarRequest.BarResponse>>();

            IPipelineBehavior<Bar1Request, Bar1Request.BarResponse> behaviorBar1 =
                Provider.GetRequiredService<IPipelineBehavior<Bar1Request, Bar1Request.BarResponse>>();
            behaviorBar1.Should().BeAssignableTo<BarPipelineBehavior<Bar1Request, Bar1Request.BarResponse>>();
        }

        [Fact]
        public void RegisterNullCheckPostProcess()
        {
            Services.AddMediatRNullCheckPostProcessor();

            IRequestPostProcessor<BarRequest, BarRequest.BarResponse> behavior =
                Provider.GetRequiredService<IRequestPostProcessor<BarRequest, BarRequest.BarResponse>>();

            behavior.Should().BeAssignableTo<NullCheckPostProcessor<BarRequest, BarRequest.BarResponse>>();
        }

        [Fact]
        public void RegisterNullCheckPostProcessWithIgnoringType()
        {
            Services.AddMediatRNullCheckPostProcessor((o) => o.IgnoreRequest<IRequest<string>>());
            IRequestPostProcessor<IRequest<string>, string> behavior =
                Provider.GetRequiredService<IRequestPostProcessor<IRequest<string>, string>>();

            Action action = () => behavior.Process(null, null, CancellationToken.None);

            action.Should().NotThrow<NotFoundException>();
        }

        [Fact]
        public void RegisterBehaviorsPipelineForCommand()
        {
            Services.AddPipelineBehaviorsForRequest<ICommandRequest>();

            IPipelineBehavior<TestCommand, Unit> behavior = Provider.GetRequiredService<IPipelineBehavior<TestCommand, Unit>>();
            behavior.Should().BeAssignableTo<CommandPipelineBehavior<TestCommand>>();
        }

        [Fact]
        public void RegisterPipelineBehaviorsForRequestTypeFromConfiguredAssemblies()
        {
            Assembly myAssembly = GetType().Assembly;
            Services.AddPipelineBehaviorsForRequest<IFooRequest>(cfg =>
                cfg.AddPipelineBehaviorAssembly(myAssembly)
                    .AddRequestAssembly(myAssembly)
                    .AddRequestAssembly(typeof(string).Assembly));

            IPipelineBehavior<FooRequest, FooRequest.FooResponse> behavior =
                Provider.GetRequiredService<IPipelineBehavior<FooRequest, FooRequest.FooResponse>>();

            behavior.Should().BeAssignableTo<FooPipelineBehavior<FooRequest, FooRequest.FooResponse>>();
        }
    }
}
